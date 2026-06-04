using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace Crush.EditorTools
{
    /// <summary>
    /// Imports the Dialogue Cards Google Doc into DialogueCard SOs.
    ///
    /// One tab per card. Each tab body:
    ///   - Title (centered) — redundant with tab title, skipped
    ///   - Preview / Cost paragraphs
    ///   - One H1 per Luke branch (Normal / Awkward / Death / Default / ...) with:
    ///       Luke: ... / Internal: ... lines  (with [+N conf] / [0 conf] / [-N conf] inline prefix)
    ///       "Charm shift from Luke's line:" bullets — `&lt;State&gt; → +/-N charm`
    ///       "Daisy responses:" then per-state blocks: `**&lt;State&gt;** (charm ...)` + Daisy: lines
    ///   - "Draft lines" H1 + bullets
    ///   - "Upgrades" H1 + bullets (optional, threshold + upgrade name)
    ///
    /// Doesn't touch isDance / revealed / buttonColor / availableUpgrades (programmer-side).
    /// Skips the "Explainer" tab.
    /// </summary>
    public static class DialogueCardDocImporter
    {
        const string API_URL = "https://script.google.com/macros/s/AKfycbwSvEH1QAUYxIkz2yQHD61Cszg8vKpCNrL85pf2f3ILFObTq1NNkjD-ZbVAxWX7Y6zz/exec";
        const string API_KEY = "claude062312atwater";
        const string CARDS_DOC_ID = "100TgRUnJ8_t9myFjorPp9p_enMh3LG6ZPvBf8afQ--Y";

        const string DEFAULT_DIR = "Assets/Crush Objects/Cards";

        [MenuItem("Crush/Import Dialogue Cards from Doc")]
        public static void ImportCards()
        {
            try
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                Debug.Log("[CardsImporter] Listing tabs…");
                var tabs = ListTabs();
                Debug.Log($"[CardsImporter] Found {tabs.Count} tabs.");

                int updated = 0, warnings = 0;

                foreach (var tab in tabs)
                {
                    var title = (tab.title ?? "").Trim();
                    //skip the Explainer tab + any tab whose title starts with `[` (convention for non-card content like "Open Issues" / "Soccer Outdated")
                    if (string.IsNullOrEmpty(title) || title == "Explainer" || title.StartsWith("[", StringComparison.Ordinal))
                    {
                        Debug.Log($"[CardsImporter] Skipping tab '{title}'.");
                        continue;
                    }

                    var content = FetchTab(tab.id);

                    var (u, w) = ImportCardTab(title, content);
                    updated += u; warnings += w;
                }

                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                Debug.Log($"[CardsImporter] Import complete. {updated} card(s) created/updated. {warnings} warning(s).");
            }
            catch (Exception e)
            {
                Debug.LogError($"[CardsImporter] Failed: {e}");
            }
        }

        // ============================================================
        // Per-card tab parse
        // ============================================================

        static (int updated, int warnings) ImportCardTab(string cardName, TabContent tab)
        {
            var data = ParseTab(cardName, tab.structure);
            if (data == null)
            {
                Debug.LogWarning($"[CardsImporter] {cardName}: parse failed, skipping.");
                return (0, 1);
            }

            var so = FindByName<DialogueCard>(cardName);
            bool created = false;
            if (so == null)
            {
                if (!AssetDatabase.IsValidFolder(DEFAULT_DIR))
                {
                    Directory.CreateDirectory(DEFAULT_DIR);
                    AssetDatabase.Refresh();
                }
                so = ScriptableObject.CreateInstance<DialogueCard>();
                AssetDatabase.CreateAsset(so, $"{DEFAULT_DIR}/{cardName}.asset");
                created = true;
            }

            ApplyToSO(so, data);
            EditorUtility.SetDirty(so);

            Debug.Log($"[CardsImporter] {cardName}: {(created ? "CREATED" : "Updated")} " +
                      $"({data.lukeBranches.Count} luke branch(es), {data.draftLines.Count} draft line(s)).");
            return (1, 0);
        }

        // ============================================================
        // Doc structure → CardData
        // ============================================================

        // Bold-marker patterns the Doc may produce. Strip them when matching prefixes/headers.
        static readonly Regex BoldStrip = new Regex(@"\*\*(.+?)\*\*");

        static CardData ParseTab(string cardName, TabStructureItem[] structure)
        {
            var card = new CardData { name = cardName };
            if (structure == null || structure.Length == 0) return card;

            // walker state
            string section = "preamble";       // preamble | branch | draftLines | upgrades
            BranchData currentBranch = null;
            DaisyBranchData currentDaisy = null;
            string daisyMode = "none";         // none | expectingState | inState

            for (int i = 0; i < structure.Length; i++)
            {
                var item = structure[i];
                var type = (item.type ?? "").ToLowerInvariant();
                var rawText = (item.text ?? "").Trim();
                if (string.IsNullOrEmpty(rawText)) continue;

                // ── titles + H1 transitions ────────────────────────────────
                if (type == "title")
                {
                    continue;  // centered card title is redundant with tab title
                }

                if (type == "heading1")
                {
                    var h1 = StripBold(rawText);
                    if (Regex.IsMatch(h1, @"^draft lines$", RegexOptions.IgnoreCase))
                    {
                        section = "draftLines";
                        currentBranch = null;
                        currentDaisy = null;
                        daisyMode = "none";
                        continue;
                    }
                    if (Regex.IsMatch(h1, @"^upgrades$", RegexOptions.IgnoreCase))
                    {
                        section = "upgrades";
                        currentBranch = null;
                        currentDaisy = null;
                        daisyMode = "none";
                        continue;
                    }
                    // otherwise it's a new Luke branch. Strip a trailing " branch" if the doc included it.
                    var branchName = Regex.Replace(h1, @"\s+branch$", "", RegexOptions.IgnoreCase).Trim();
                    currentBranch = new BranchData { branchName = branchName };
                    card.lukeBranches.Add(currentBranch);
                    section = "branch";
                    currentDaisy = null;
                    daisyMode = "none";
                    continue;
                }

                // ── preamble: Preview / Cost ───────────────────────────────
                if (section == "preamble")
                {
                    var p = StripBold(rawText);
                    var m = Regex.Match(p, @"^Preview:\s*(.+)$", RegexOptions.IgnoreCase);
                    if (m.Success) { card.previewText = m.Groups[1].Value.Trim(); continue; }

                    m = Regex.Match(p, @"^Cost:\s*(\d+)$", RegexOptions.IgnoreCase);
                    if (m.Success) { int.TryParse(m.Groups[1].Value, out card.cost); continue; }
                    // ignore other preamble paragraphs (could be notes)
                    continue;
                }

                // ── draft lines section: bullets are lines ─────────────────
                if (section == "draftLines")
                {
                    //some doc bullets come back as one structure item with embedded newlines
                    //- split so each visible line becomes its own DraftLine entry
                    var stripped = StripBold(rawText);
                    foreach (var sub in stripped.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        var trimmed = sub.Trim();
                        if (!string.IsNullOrEmpty(trimmed)) card.draftLines.Add(trimmed);
                    }
                    continue;
                }

                // ── upgrades section: parse "Available upgrades: X" + "Upgrade threshold: N"
                if (section == "upgrades")
                {
                    var u = StripBold(rawText);
                    var m = Regex.Match(u, @"^Available upgrades?:\s*(.+)$", RegexOptions.IgnoreCase);
                    if (m.Success) { card.upgradeNames.Add(m.Groups[1].Value.Trim()); continue; }

                    m = Regex.Match(u, @"^Upgrade threshold:\s*(\d+)$", RegexOptions.IgnoreCase);
                    if (m.Success) { int.TryParse(m.Groups[1].Value, out card.upgradeThreshold); continue; }
                    continue;
                }

                // ── inside a Luke branch ───────────────────────────────────
                if (section == "branch" && currentBranch != null)
                {
                    var stripped = StripBold(rawText);

                    // section transitions inside the branch
                    if (Regex.IsMatch(stripped, @"^charm shift from luke('s)? line:?$", RegexOptions.IgnoreCase))
                    {
                        currentBranch.expectingCharmBullets = true;
                        daisyMode = "none";
                        continue;
                    }

                    if (Regex.IsMatch(stripped, @"^daisy responses:?$", RegexOptions.IgnoreCase))
                    {
                        currentBranch.expectingCharmBullets = false;
                        daisyMode = "expectingState";
                        continue;
                    }

                    // Luke / Internal dialogue lines.
                    // If we're inside a Daisy state block, the line belongs to THAT daisy branch
                    // (it plays after her lines, in authored order) - not hoisted up to the pre-Daisy
                    // Luke array. This is what lets a "[..] Internal: That actually worked" sit after
                    // Daisy's response. Outside a daisy block it goes to the Luke array as before.
                    var lukeMatch = MatchDialogueLine(rawText, "Luke");
                    if (lukeMatch != null)
                    {
                        var ld = new LineData
                        {
                            character = "Boy",
                            text = lukeMatch.text,
                            confidenceImpact = lukeMatch.conf,
                            charmImpact = lukeMatch.charm
                        };
                        if (daisyMode == "inState" && currentDaisy != null) currentDaisy.lines.Add(ld);
                        else currentBranch.luke.Add(ld);
                        continue;
                    }
                    var internalMatch = MatchDialogueLine(rawText, "Internal");
                    if (internalMatch != null)
                    {
                        var ld = new LineData
                        {
                            character = "BoyInternal",
                            text = internalMatch.text,
                            confidenceImpact = internalMatch.conf,
                            charmImpact = internalMatch.charm
                        };
                        if (daisyMode == "inState" && currentDaisy != null) currentDaisy.lines.Add(ld);
                        else currentBranch.luke.Add(ld);
                        continue;
                    }
                    //Daisy lines interleaved in the main branch dialogue (before any "Daisy responses:" section)
                    //- get appended to the same array as Luke/Internal, with character = Girl
                    if (daisyMode == "none")
                    {
                        var daisyInMain = MatchDialogueLine(rawText, "Daisy");
                        if (daisyInMain != null)
                        {
                            currentBranch.luke.Add(new LineData
                            {
                                character = "Girl",
                                text = daisyInMain.text,
                                confidenceImpact = daisyInMain.conf,
                                charmImpact = daisyInMain.charm
                            });
                            continue;
                        }
                    }

                    // Charm shift bullets: "<State> → +/-N charm"
                    if (currentBranch.expectingCharmBullets)
                    {
                        var cm = Regex.Match(stripped, @"^(Death|Low|Neutral|Positive|High)\s*[→\-]+\s*([+\-]?\d+)\s*charm$",
                                             RegexOptions.IgnoreCase);
                        if (cm.Success)
                        {
                            int impact = 0; int.TryParse(cm.Groups[2].Value, out impact);
                            currentBranch.charmImpacts.Add(new CharmImpactData
                            {
                                stateName = Capitalize(cm.Groups[1].Value),
                                impact = impact
                            });
                            continue;
                        }
                    }

                    // Daisy state header: "**Death**" / "**Low** (charm 1-2)" / etc.
                    if (daisyMode == "expectingState" || daisyMode == "inState")
                    {
                        var stateMatch = Regex.Match(stripped, @"^(Death|Low|Neutral|Positive|High)(\s|\(|$)",
                                                    RegexOptions.IgnoreCase);
                        if (stateMatch.Success && !rawText.StartsWith("[", StringComparison.Ordinal))
                        {
                            currentDaisy = new DaisyBranchData
                            {
                                stateName = Capitalize(stateMatch.Groups[1].Value)
                            };
                            currentBranch.daisyBranches.Add(currentDaisy);
                            daisyMode = "inState";
                            continue;
                        }
                    }

                    // Daisy dialogue lines
                    if (daisyMode == "inState" && currentDaisy != null)
                    {
                        var dm = MatchDialogueLine(rawText, "Daisy");
                        if (dm != null)
                        {
                            currentDaisy.lines.Add(new LineData
                            {
                                character = "Girl",
                                text = dm.text,
                                confidenceImpact = dm.conf,
                                charmImpact = dm.charm
                            });
                            continue;
                        }
                    }

                    // Anything else inside a branch: ignore (could be a stray bold paragraph)
                    continue;
                }
            }

            return card;
        }

        // Matches "[+N conf] Luke: text" / "[0 conf, -1 charm] Daisy: text" / "Luke: text" (no annotation = 0 each)
        // The annotation supports one or more comma-separated impacts inside a single bracket, e.g.
        // [0 conf], [-1 charm], [+2 conf, -1 charm], [-1 charm, +2 conf] (order-flexible).
        // Returns null if the line doesn't match the expected speaker prefix.
        static DialogueParseResult MatchDialogueLine(string rawText, string speaker)
        {
            var t = rawText.Trim();
            int conf = 0;
            int charm = 0;

            //optional [N conf] / [N charm] / [N conf, N charm] / [N charm, N conf] prefix
            var annot = Regex.Match(t, @"^\[([^\]]+)\]\s*");
            if (annot.Success)
            {
                foreach (var piece in annot.Groups[1].Value.Split(','))
                {
                    var m = Regex.Match(piece.Trim(), @"^([+\-]?\d+)\s*(conf|charm)$", RegexOptions.IgnoreCase);
                    if (!m.Success) continue;
                    int.TryParse(m.Groups[1].Value, out int val);
                    if (m.Groups[2].Value.Equals("conf", StringComparison.OrdinalIgnoreCase)) conf = val;
                    else charm = val;
                }
                t = t.Substring(annot.Length);
            }

            // speaker prefix
            var sp = Regex.Match(t, $@"^{Regex.Escape(speaker)}:\s*(.+)$", RegexOptions.IgnoreCase);
            if (!sp.Success) return null;

            return new DialogueParseResult { conf = conf, charm = charm, text = sp.Groups[1].Value.Trim() };
        }

        static string StripBold(string s) => BoldStrip.Replace(s, m => m.Groups[1].Value).Trim();
        static string Capitalize(string s) => string.IsNullOrEmpty(s) ? s : char.ToUpper(s[0]) + s.Substring(1).ToLower();

        // ============================================================
        // CardData → DialogueCard (SerializedObject writes)
        // ============================================================

        static void ApplyToSO(DialogueCard asset, CardData data)
        {
            var so = new SerializedObject(asset);

            so.FindProperty("previewText").stringValue = data.previewText ?? "";
            so.FindProperty("cost").intValue = data.cost;
            so.FindProperty("upgradeThreshold").intValue = data.upgradeThreshold > 0 ? data.upgradeThreshold : 3;

            // luke branches
            var lukeProp = so.FindProperty("lukeBranches");
            lukeProp.arraySize = data.lukeBranches.Count;
            for (int i = 0; i < data.lukeBranches.Count; i++)
            {
                var branch = data.lukeBranches[i];
                var bp = lukeProp.GetArrayElementAtIndex(i);
                bp.FindPropertyRelative("branchName").stringValue = branch.branchName;

                // luke dialogue
                WriteLines(bp.FindPropertyRelative("dialogue"), branch.luke);

                // charm impacts on this branch
                var ciProp = bp.FindPropertyRelative("charmImpacts");
                ciProp.arraySize = branch.charmImpacts.Count;
                for (int j = 0; j < branch.charmImpacts.Count; j++)
                {
                    var ci = branch.charmImpacts[j];
                    var cep = ciProp.GetArrayElementAtIndex(j);
                    cep.FindPropertyRelative("state").enumValueIndex = (int)ParseCharmState(ci.stateName);
                    cep.FindPropertyRelative("impact").intValue = ci.impact;
                }

                // daisy branches on this luke branch
                var dbProp = bp.FindPropertyRelative("daisyBranches");
                dbProp.arraySize = branch.daisyBranches.Count;
                for (int j = 0; j < branch.daisyBranches.Count; j++)
                {
                    var daisy = branch.daisyBranches[j];
                    var dp = dbProp.GetArrayElementAtIndex(j);
                    dp.FindPropertyRelative("charmState").enumValueIndex = (int)ParseCharmState(daisy.stateName);
                    WriteLines(dp.FindPropertyRelative("dialogue"), daisy.lines);
                }
            }

            // draft lines
            var dlProp = so.FindProperty("draftLines");
            dlProp.arraySize = data.draftLines.Count;
            for (int i = 0; i < data.draftLines.Count; i++)
            {
                dlProp.GetArrayElementAtIndex(i).FindPropertyRelative("line").stringValue = data.draftLines[i];
            }

            so.ApplyModifiedProperties();
        }

        static void WriteLines(SerializedProperty arrayProp, List<LineData> lines)
        {
            arrayProp.arraySize = lines.Count;
            for (int i = 0; i < lines.Count; i++)
            {
                var line = lines[i];
                var lp = arrayProp.GetArrayElementAtIndex(i);
                lp.FindPropertyRelative("character").enumValueIndex = (int)ParseCharacter(line.character);
                lp.FindPropertyRelative("line").stringValue = line.text;
                lp.FindPropertyRelative("confidenceImpact").intValue = line.confidenceImpact;
                lp.FindPropertyRelative("charmImpact").intValue = line.charmImpact;
            }
        }

        static DialogueCard.CharmState ParseCharmState(string name)
        {
            switch ((name ?? "").Trim().ToLowerInvariant())
            {
                case "death": return DialogueCard.CharmState.Death;
                case "low": return DialogueCard.CharmState.Low;
                case "neutral": return DialogueCard.CharmState.Neutral;
                case "positive": return DialogueCard.CharmState.Positive;
                case "high": return DialogueCard.CharmState.High;
                default:
                    Debug.LogWarning($"[CardsImporter] Unknown charm state '{name}', defaulting to Neutral.");
                    return DialogueCard.CharmState.Neutral;
            }
        }

        static DialogueCard.DialogueCharacter ParseCharacter(string s)
        {
            switch (s)
            {
                case "Boy": return DialogueCard.DialogueCharacter.Boy;
                case "Girl": return DialogueCard.DialogueCharacter.Girl;
                case "BoyInternal": return DialogueCard.DialogueCharacter.BoyInternal;
                default: return DialogueCard.DialogueCharacter.Boy;
            }
        }

        // ============================================================
        // Asset lookup
        // ============================================================

        static T FindByName<T>(string assetName) where T : ScriptableObject
        {
            var guids = AssetDatabase.FindAssets($"{assetName} t:{typeof(T).Name}");
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                if (Path.GetFileNameWithoutExtension(path) == assetName)
                    return AssetDatabase.LoadAssetAtPath<T>(path);
            }
            return null;
        }

        // ============================================================
        // HTTP + JSON
        // ============================================================

        static string FetchUrl(string url)
        {
            using (var client = new WebClient())
            {
                client.Encoding = System.Text.Encoding.UTF8;
                return client.DownloadString(url);
            }
        }

        static List<TabRef> ListTabs()
        {
            var raw = FetchUrl($"{API_URL}?key={API_KEY}&action=list_tabs&doc_id={CARDS_DOC_ID}");
            var resp = JsonUtility.FromJson<ListTabsResponse>(raw);
            if (!resp.success) throw new Exception($"list_tabs failed: {raw}");
            return resp.tabs != null ? new List<TabRef>(resp.tabs) : new List<TabRef>();
        }

        static TabContent FetchTab(string tabId)
        {
            var raw = FetchUrl($"{API_URL}?key={API_KEY}&action=get_tab&doc_id={CARDS_DOC_ID}&tab_id={tabId}");
            var resp = JsonUtility.FromJson<TabContent>(raw);
            if (!resp.success) throw new Exception($"get_tab failed for {tabId}: {raw}");
            return resp;
        }

        // ============================================================
        // Wire types
        // ============================================================

        [Serializable] class ListTabsResponse { public bool success; public string doc_id; public TabRef[] tabs; }
        [Serializable] class TabRef { public string id; public string title; public int index; public int depth; }
        [Serializable] class TabContent { public bool success; public string doc_id; public string tab_id; public string tab_title; public string text; public TabStructureItem[] structure; }
        [Serializable] class TabStructureItem { public int index; public string type; public string text; }

        // ============================================================
        // Intermediate parsed data
        // ============================================================

        class CardData
        {
            public string name;
            public string previewText = "";
            public int cost = 1;
            public int upgradeThreshold = 3;
            public List<BranchData> lukeBranches = new List<BranchData>();
            public List<string> draftLines = new List<string>();
            public List<string> upgradeNames = new List<string>(); // captured but not wired (programmer-side)
        }

        class BranchData
        {
            public string branchName;
            public List<LineData> luke = new List<LineData>();
            public List<CharmImpactData> charmImpacts = new List<CharmImpactData>();
            public List<DaisyBranchData> daisyBranches = new List<DaisyBranchData>();
            public bool expectingCharmBullets;  // set true after we see "Charm shift…" header
        }

        class DaisyBranchData
        {
            public string stateName;
            public List<LineData> lines = new List<LineData>();
        }

        class CharmImpactData
        {
            public string stateName;
            public int impact;
        }

        class LineData
        {
            public string character; // "Boy" | "Girl" | "BoyInternal"
            public string text;
            public int confidenceImpact;
            public int charmImpact;
        }

        class DialogueParseResult
        {
            public int conf;
            public int charm;
            public string text;
        }
    }
}
