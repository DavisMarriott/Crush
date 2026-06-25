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
    /// Doesn't touch isDance / revealed / buttonColor / visualPrefab / availableUpgrades (programmer-side).
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
            string section = "preamble";       // preamble | branch | draftLines | upgrades | upgradeDraftLines
            BranchData currentBranch = null;
            DaisyBranchData currentDaisy = null;
            string daisyMode = "none";         // none | expectingState | inState
            bool deathReactionMode = false;    // inside a "Death reactions:" block (within a branch)
            LineData currentSlot = null;       // sequenced slot under construction ([1st]/[2nd]/[last])
            UpgradeDraftLinesData currentUpgradeDraft = null;  // active "Upgrade Draft Lines" block

            for (int i = 0; i < structure.Length; i++)
            {
                var item = structure[i];
                var type = (item.type ?? "").ToLowerInvariant();
                var rawText = (item.text ?? "").Trim();
                if (string.IsNullOrEmpty(rawText)) continue;

                // ── titles + H1 transitions ────────────────────────────────
                if (type == "title")
                {
                    // "Upgrades" as a centered Title is the section divider (the standard). Any other title
                    // is the redundant card-name title and is skipped.
                    if (Regex.IsMatch(StripBold(rawText), @"^upgrades$", RegexOptions.IgnoreCase))
                    {
                        section = "upgrades";
                        currentBranch = null;
                        currentDaisy = null;
                        daisyMode = "none";
                        currentSlot = null;
                    }
                    continue;
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
                        currentSlot = null;
                        continue;
                    }
                    if (Regex.IsMatch(h1, @"^upgrades$", RegexOptions.IgnoreCase))
                    {
                        section = "upgrades";
                        currentBranch = null;
                        currentDaisy = null;
                        daisyMode = "none";
                        currentSlot = null;
                        continue;
                    }
                    // "Upgrade Draft Lines" (optionally "...: <UpgradeName>") → lines for the card's upgrade SO.
                    var udlMatch = Regex.Match(h1, @"^upgrade draft lines(?::\s*(.+))?$", RegexOptions.IgnoreCase);
                    if (udlMatch.Success)
                    {
                        var upName = udlMatch.Groups[1].Success ? udlMatch.Groups[1].Value.Trim() : null;
                        currentUpgradeDraft = new UpgradeDraftLinesData { upgradeName = upName };
                        card.upgradeDraftLines.Add(currentUpgradeDraft);
                        section = "upgradeDraftLines";
                        currentBranch = null;
                        currentDaisy = null;
                        daisyMode = "none";
                        currentSlot = null;
                        continue;
                    }
                    // "BO: <Name> branch" H1 = a branch OVERRIDE for the card's upgrade (the standard).
                    // Authored exactly like a card branch; replaces the same-named branch when the upgrade applies.
                    var boMatch = Regex.Match(h1, @"^BO:\s*(.+)$", RegexOptions.IgnoreCase);
                    if (boMatch.Success)
                    {
                        var boBody = boMatch.Groups[1].Value;
                        var ovTags = new List<string>();
                        foreach (Match tagMatch in Regex.Matches(boBody, @"\[([^\]]+)\]"))
                            foreach (var piece in tagMatch.Groups[1].Value.Split(','))
                            {
                                var tagName = piece.Trim();
                                if (tagName.Length > 0) ovTags.Add(tagName);
                            }
                        var boClean = Regex.Replace(boBody, @"\s*\[[^\]]+\]", "").Trim();
                        var ovName = Regex.Replace(boClean, @"\s+branch$", "", RegexOptions.IgnoreCase).Trim();
                        currentBranch = new BranchData { branchName = ovName, tags = ovTags };
                        card.upgradeOverrideBranches.Add(currentBranch);
                        section = "branch";
                        currentDaisy = null;
                        daisyMode = "none";
                        deathReactionMode = false;
                        currentSlot = null;
                        continue;
                    }
                    // otherwise it's a new Luke branch. Extract optional [tag] markers first
                    // (e.g. "Normal branch [funny]"), then strip a trailing " branch".
                    var branchTags = new List<string>();
                    foreach (Match tagMatch in Regex.Matches(h1, @"\[([^\]]+)\]"))
                        foreach (var piece in tagMatch.Groups[1].Value.Split(','))
                        {
                            var tagName = piece.Trim();
                            if (tagName.Length > 0) branchTags.Add(tagName);
                        }
                    var h1Clean = Regex.Replace(h1, @"\s*\[[^\]]+\]", "").Trim();
                    var branchName = Regex.Replace(h1Clean, @"\s+branch$", "", RegexOptions.IgnoreCase).Trim();
                    currentBranch = new BranchData { branchName = branchName, tags = branchTags };
                    card.lukeBranches.Add(currentBranch);
                    section = "branch";
                    currentDaisy = null;
                    daisyMode = "none";
                    deathReactionMode = false;
                    currentSlot = null;
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

                    // explicit condition type (the standard): "Upgrade Condition: Play Threshold" / "Branch Tag"
                    m = Regex.Match(u, @"^Upgrade Condition:\s*(.+)$", RegexOptions.IgnoreCase);
                    if (m.Success)
                    {
                        var v = m.Groups[1].Value.Trim();
                        if (Regex.IsMatch(v, @"^branch\s*tag$", RegexOptions.IgnoreCase)) card.upgradeConditionType = "BranchTag";
                        else if (Regex.IsMatch(v, @"^play\s*threshold$", RegexOptions.IgnoreCase)) card.upgradeConditionType = "PlayThreshold";
                        continue;
                    }

                    // Play Threshold value ("Upgrade threshold" still accepted for un-migrated cards)
                    m = Regex.Match(u, @"^(?:Play Threshold|Upgrade threshold):\s*(\d+)$", RegexOptions.IgnoreCase);
                    if (m.Success) { int.TryParse(m.Groups[1].Value, out card.upgradeThreshold); continue; }

                    // Branch Tag value ("Upgrade tag" still accepted)
                    m = Regex.Match(u, @"^(?:Branch Tag|Upgrade tag):\s*(.+)$", RegexOptions.IgnoreCase);
                    if (m.Success) { card.upgradeTag = m.Groups[1].Value.Trim(); continue; }
                    continue;
                }

                // ── upgrade draft lines section: bullets are lines (same as draft lines, but for the upgrade)
                if (section == "upgradeDraftLines" && currentUpgradeDraft != null)
                {
                    var stripped = StripBold(rawText);
                    foreach (var sub in stripped.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        var trimmed = sub.Trim();
                        if (!string.IsNullOrEmpty(trimmed)) currentUpgradeDraft.lines.Add(trimmed);
                    }
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
                        deathReactionMode = false;
                        currentSlot = null;
                        continue;
                    }

                    if (Regex.IsMatch(stripped, @"^daisy responses:?$", RegexOptions.IgnoreCase))
                    {
                        currentBranch.expectingCharmBullets = false;
                        daisyMode = "expectingState";
                        deathReactionMode = false;
                        currentSlot = null;
                        continue;
                    }

                    if (Regex.IsMatch(stripped, @"^death reactions:?$", RegexOptions.IgnoreCase))
                    {
                        currentBranch.expectingCharmBullets = false;
                        daisyMode = "none";
                        deathReactionMode = true;
                        currentSlot = null;
                        continue;
                    }

                    // Death-reactions block: "1)" / "2." markers split reaction groups; other lines
                    // join the current group. A leading "Internal:" prefix is stripped (reflect lines
                    // render raw - the prefix would show on screen otherwise). Runs BEFORE the
                    // Luke/Internal matchers so reaction lines don't leak into the branch dialogue.
                    if (deathReactionMode)
                    {
                        if (Regex.IsMatch(stripped, @"^\d+[\)\.]?$"))
                        {
                            currentBranch.deathReactions.Add(new List<string>());
                            continue;
                        }
                        if (currentBranch.deathReactions.Count == 0)
                            currentBranch.deathReactions.Add(new List<string>());
                        var reactionText = stripped;
                        var internalPrefix = MatchDialogueLine(rawText, "Internal");
                        if (internalPrefix != null) reactionText = internalPrefix.text;
                        currentBranch.deathReactions[currentBranch.deathReactions.Count - 1].Add(reactionText);
                        continue;
                    }

                    // ---- sequenced discovery slots ([1st]/[2nd]/[last]/[once], 86baanp61) ----
                    // A marker line opens/extends a slot; a marker alone starts a multi-line group,
                    // inline text after the marker is a one-line group. Bare lines while a slot is
                    // open join its current group. A normal speaker line (no marker) closes the slot.
                    var slotMarker = MatchSlotMarker(rawText);
                    if (slotMarker != null)
                    {
                        if (currentSlot == null)
                        {
                            currentSlot = new LineData
                            {
                                character = "BoyInternal",   // discovery slots default to Internal
                                text = "",
                                variantGroups = new List<List<string>>()
                            };
                            var slotTarget = (daisyMode == "inState" && currentDaisy != null) ? currentDaisy.lines : currentBranch.luke;
                            slotTarget.Add(currentSlot);
                        }
                        if (slotMarker.last) currentSlot.lastSticks = true;
                        if (slotMarker.conf != 0) currentSlot.confidenceImpact = slotMarker.conf;
                        if (slotMarker.charm != 0) currentSlot.charmImpact = slotMarker.charm;
                        currentSlot.variantGroups.Add(new List<string>());

                        if (!string.IsNullOrEmpty(slotMarker.remainder))
                        {
                            var lineText = slotMarker.remainder;
                            var sp = Regex.Match(lineText, @"^(Luke|Internal|Daisy):\s*(.+)$", RegexOptions.IgnoreCase);
                            if (sp.Success)
                            {
                                currentSlot.character = SpeakerToCharacter(sp.Groups[1].Value);
                                lineText = sp.Groups[2].Value.Trim();
                            }
                            currentSlot.variantGroups[currentSlot.variantGroups.Count - 1].Add(StripBold(lineText));
                        }
                        continue;
                    }

                    if (currentSlot != null)
                    {
                        bool isNormalLine = MatchDialogueLine(rawText, "Luke") != null
                                         || MatchDialogueLine(rawText, "Internal") != null
                                         || MatchDialogueLine(rawText, "Daisy") != null;
                        bool isDaisyStateHeader = daisyMode != "none"
                                         && Regex.IsMatch(stripped, @"^(Death|Low|Neutral|Positive|High)(\s|\(|$)", RegexOptions.IgnoreCase)
                                         && !rawText.StartsWith("[", StringComparison.Ordinal);
                        if (!isNormalLine && !isDaisyStateHeader)
                        {
                            // bare line - belongs to the slot's current variant group
                            currentSlot.variantGroups[currentSlot.variantGroups.Count - 1].Add(stripped);
                            continue;
                        }
                        currentSlot = null;   // normal line / state header - close the slot, fall through
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
                            // optional [tag] / [tag1, tag2] markers on the state header → DaisyBranch.tags
                            // e.g. "**High** (charm 9–10) [funny]"
                            foreach (Match tagMatch in Regex.Matches(stripped, @"\[([^\]]+)\]"))
                                foreach (var piece in tagMatch.Groups[1].Value.Split(','))
                                {
                                    var tagName = piece.Trim();
                                    if (tagName.Length > 0) currentDaisy.tags.Add(tagName);
                                }
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

        // Matches a leading [..] bracket containing a slot token: an ordinal (1st/2nd/3...),
        // "last", or "once". Impacts may share the bracket ([1st, +1 conf]). Returns null for
        // non-slot lines (incl. normal [0 conf] annotations). Ordinal VALUES are ignored -
        // groups land in authored order; the numbers are for the writer's eyes.
        class SlotMarker { public bool last; public int conf; public int charm; public string remainder; }

        static SlotMarker MatchSlotMarker(string rawText)
        {
            var t = StripBold(rawText);
            var m = Regex.Match(t, @"^\[([^\]]+)\]\s*(.*)$", RegexOptions.Singleline);
            if (!m.Success) return null;

            bool isSlot = false, last = false;
            int conf = 0, charm = 0;
            foreach (var piece in m.Groups[1].Value.Split(','))
            {
                var p = piece.Trim();
                if (Regex.IsMatch(p, @"^\d+(st|nd|rd|th)?$", RegexOptions.IgnoreCase)) { isSlot = true; continue; }
                if (p.Equals("last", StringComparison.OrdinalIgnoreCase)) { isSlot = true; last = true; continue; }
                if (p.Equals("once", StringComparison.OrdinalIgnoreCase)) { isSlot = true; continue; }
                var im = Regex.Match(p, @"^([+\-]?\d+)\s*(conf|charm)$", RegexOptions.IgnoreCase);
                if (im.Success)
                {
                    int.TryParse(im.Groups[1].Value, out int val);
                    if (im.Groups[2].Value.Equals("conf", StringComparison.OrdinalIgnoreCase)) conf = val;
                    else charm = val;
                }
            }
            if (!isSlot) return null;
            return new SlotMarker { last = last, conf = conf, charm = charm, remainder = m.Groups[2].Value.Trim() };
        }

        static string SpeakerToCharacter(string speaker)
        {
            switch ((speaker ?? "").Trim().ToLowerInvariant())
            {
                case "luke": return "Boy";
                case "daisy": return "Girl";
                default: return "BoyInternal";
            }
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
            // upgrade condition: the explicit "Upgrade Condition:" line wins; else infer (a Branch Tag value => BranchTag)
            var condProp = so.FindProperty("upgradeCondition");
            bool branchTag = data.upgradeConditionType == "BranchTag"
                             || (data.upgradeConditionType == null && !string.IsNullOrEmpty(data.upgradeTag));
            if (branchTag)
            {
                condProp.FindPropertyRelative("type").enumValueIndex = (int)UpgradeConditionType.BranchTag;
                condProp.FindPropertyRelative("tag").enumValueIndex = (int)ParseDialogueTag(data.upgradeTag);
            }
            else
            {
                condProp.FindPropertyRelative("type").enumValueIndex = (int)UpgradeConditionType.PlayThreshold;
                condProp.FindPropertyRelative("playThreshold").intValue = data.upgradeThreshold > 0 ? data.upgradeThreshold : 3;
            }

            // luke branches
            var lukeProp = so.FindProperty("lukeBranches");
            lukeProp.arraySize = data.lukeBranches.Count;
            for (int i = 0; i < data.lukeBranches.Count; i++)
                WriteBranch(lukeProp.GetArrayElementAtIndex(i), data.lukeBranches[i]);

            // draft lines
            var dlProp = so.FindProperty("draftLines");
            dlProp.arraySize = data.draftLines.Count;
            for (int i = 0; i < data.draftLines.Count; i++)
            {
                dlProp.GetArrayElementAtIndex(i).FindPropertyRelative("line").stringValue = data.draftLines[i];
            }

            so.ApplyModifiedProperties();

            // upgrade draft lines + branch overrides live on the card's upgrade SO(s), not the card itself
            WriteUpgradeDraftLines(asset, data);
            WriteUpgradeOverrides(asset, data);
        }

        // Writes one DialogueBranch (branchName + luke lines + tags + charm impacts + daisy branches +
        // death reactions). Shared by the card's lukeBranches and an upgrade's branchOverrides — both are
        // DialogueCard.DialogueBranch arrays, so the same writer fills either.
        static void WriteBranch(SerializedProperty bp, BranchData branch)
        {
            bp.FindPropertyRelative("branchName").stringValue = branch.branchName;
            WriteLines(bp.FindPropertyRelative("dialogue"), branch.luke);

            // branch tags from [tag] header markers - only overwrite when the doc provides tags
            if (branch.tags.Count > 0)
            {
                var branchTagsProp = bp.FindPropertyRelative("tags");
                branchTagsProp.arraySize = branch.tags.Count;
                for (int j = 0; j < branch.tags.Count; j++)
                    branchTagsProp.GetArrayElementAtIndex(j).enumValueIndex = (int)ParseDialogueTag(branch.tags[j]);
            }

            var ciProp = bp.FindPropertyRelative("charmImpacts");
            ciProp.arraySize = branch.charmImpacts.Count;
            for (int j = 0; j < branch.charmImpacts.Count; j++)
            {
                var ci = branch.charmImpacts[j];
                var cep = ciProp.GetArrayElementAtIndex(j);
                cep.FindPropertyRelative("state").enumValueIndex = (int)ParseCharmState(ci.stateName);
                cep.FindPropertyRelative("impact").intValue = ci.impact;
            }

            var dbProp = bp.FindPropertyRelative("daisyBranches");
            dbProp.arraySize = branch.daisyBranches.Count;
            for (int j = 0; j < branch.daisyBranches.Count; j++)
            {
                var daisy = branch.daisyBranches[j];
                var dp = dbProp.GetArrayElementAtIndex(j);
                dp.FindPropertyRelative("charmState").enumValueIndex = (int)ParseCharmState(daisy.stateName);
                WriteLines(dp.FindPropertyRelative("dialogue"), daisy.lines);

                if (daisy.tags.Count > 0)
                {
                    var tagsProp = dp.FindPropertyRelative("tags");
                    tagsProp.arraySize = daisy.tags.Count;
                    for (int k = 0; k < daisy.tags.Count; k++)
                        tagsProp.GetArrayElementAtIndex(k).enumValueIndex = (int)ParseDialogueTag(daisy.tags[k]);
                }
            }

            var drProp = bp.FindPropertyRelative("deathReactions");
            drProp.arraySize = branch.deathReactions.Count;
            for (int j = 0; j < branch.deathReactions.Count; j++)
            {
                var linesProp = drProp.GetArrayElementAtIndex(j).FindPropertyRelative("lines");
                linesProp.arraySize = branch.deathReactions[j].Count;
                for (int k = 0; k < branch.deathReactions[j].Count; k++)
                    linesProp.GetArrayElementAtIndex(k).stringValue = branch.deathReactions[j][k];
            }
        }

        // Writes "BO:"-prefixed override branches (authored under the Upgrades section) onto the card's
        // upgrade SO's branchOverrides. One-per-card for now: all overrides go to the single availableUpgrade.
        static void WriteUpgradeOverrides(DialogueCard asset, CardData data)
        {
            if (data.upgradeOverrideBranches.Count == 0) return;
            var upgrades = asset.availableUpgrades;
            if (upgrades == null || upgrades.Length == 0)
            {
                Debug.LogWarning($"[CardsImporter] '{data.name}' has upgrade override branches but no availableUpgrades wired on the card (programmer-side). Skipping.");
                return;
            }
            var target = ResolveUpgrade(upgrades, null, data.name);   // unnamed → the card's single upgrade
            if (target == null) return;

            var uso = new SerializedObject(target);
            var ovProp = uso.FindProperty("branchOverrides");
            ovProp.arraySize = data.upgradeOverrideBranches.Count;
            for (int i = 0; i < data.upgradeOverrideBranches.Count; i++)
                WriteBranch(ovProp.GetArrayElementAtIndex(i), data.upgradeOverrideBranches[i]);
            uso.ApplyModifiedProperties();
            EditorUtility.SetDirty(target);
        }

        // Writes parsed "Upgrade Draft Lines" blocks onto the card's upgrade SO(s). One-per-card for now
        // (an unnamed block → the card's single availableUpgrade); a named block ("Upgrade Draft Lines: X")
        // targets that upgrade by asset name, so this extends cleanly when cards get multiple upgrades.
        static void WriteUpgradeDraftLines(DialogueCard asset, CardData data)
        {
            if (data.upgradeDraftLines.Count == 0) return;
            var upgrades = asset.availableUpgrades;
            if (upgrades == null || upgrades.Length == 0)
            {
                Debug.LogWarning($"[CardsImporter] '{data.name}' has Upgrade Draft Lines but no availableUpgrades wired on the card (programmer-side). Skipping.");
                return;
            }
            foreach (var block in data.upgradeDraftLines)
            {
                var target = ResolveUpgrade(upgrades, block.upgradeName, data.name);
                if (target == null) continue;
                var uso = new SerializedObject(target);
                var udl = uso.FindProperty("draftLines");
                udl.arraySize = block.lines.Count;
                for (int i = 0; i < block.lines.Count; i++)
                    udl.GetArrayElementAtIndex(i).FindPropertyRelative("line").stringValue = block.lines[i];
                uso.ApplyModifiedProperties();
                EditorUtility.SetDirty(target);
            }
        }

        static DialogueCardUpgrade ResolveUpgrade(DialogueCardUpgrade[] upgrades, string name, string cardName)
        {
            // unnamed block → the card's single upgrade (one-per-card default)
            if (string.IsNullOrEmpty(name))
            {
                if (upgrades.Length == 1) return upgrades[0];
                Debug.LogWarning($"[CardsImporter] '{cardName}' has {upgrades.Length} upgrades but an unnamed 'Upgrade Draft Lines' section — name it 'Upgrade Draft Lines: <UpgradeName>'. Skipping.");
                return null;
            }
            // named block → match an availableUpgrade by asset name
            foreach (var u in upgrades)
                if (u != null && string.Equals(u.name, name, StringComparison.OrdinalIgnoreCase)) return u;
            Debug.LogWarning($"[CardsImporter] '{cardName}': no availableUpgrade named '{name}'. Skipping.");
            return null;
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

                // sequenced slot payload
                var vgProp = lp.FindPropertyRelative("variantGroups");
                int groupCount = line.variantGroups?.Count ?? 0;
                vgProp.arraySize = groupCount;
                for (int g = 0; g < groupCount; g++)
                {
                    var linesProp = vgProp.GetArrayElementAtIndex(g).FindPropertyRelative("lines");
                    linesProp.arraySize = line.variantGroups[g].Count;
                    for (int k = 0; k < line.variantGroups[g].Count; k++)
                        linesProp.GetArrayElementAtIndex(k).stringValue = line.variantGroups[g][k];
                }
                lp.FindPropertyRelative("lastSticks").boolValue = line.lastSticks;
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

        static DialogueTag ParseDialogueTag(string s)
        {
            if (Enum.TryParse<DialogueTag>((s ?? "").Trim(), true, out var tag))
                return tag;
            Debug.LogWarning($"[CardsImporter] Unknown dialogue tag '{s}' — defaulting to {default(DialogueTag)}. Valid: {string.Join(", ", Enum.GetNames(typeof(DialogueTag)))}");
            return default;
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
            public string upgradeTag = null;            // Branch Tag value
            public string upgradeConditionType = null;  // "PlayThreshold" | "BranchTag" from the explicit "Upgrade Condition" line
            public List<BranchData> lukeBranches = new List<BranchData>();
            public List<string> draftLines = new List<string>();
            public List<string> upgradeNames = new List<string>(); // captured but not wired (programmer-side)
            public List<UpgradeDraftLinesData> upgradeDraftLines = new List<UpgradeDraftLinesData>();
            public List<BranchData> upgradeOverrideBranches = new List<BranchData>();   // H2 branches under "Upgrades"
        }

        // draft self-talk lines for a card's upgrade. upgradeName empty = the card's single upgrade
        // (one-per-card); a named block ("Upgrade Draft Lines: X") targets that upgrade once cards have several.
        class UpgradeDraftLinesData
        {
            public string upgradeName;
            public List<string> lines = new List<string>();
        }

        class BranchData
        {
            public string branchName;
            public List<LineData> luke = new List<LineData>();
            public List<CharmImpactData> charmImpacts = new List<CharmImpactData>();
            public List<DaisyBranchData> daisyBranches = new List<DaisyBranchData>();
            public bool expectingCharmBullets;  // set true after we see "Charm shift…" header
            public List<List<string>> deathReactions = new List<List<string>>();  // groups from "Death reactions:" blocks
            public List<string> tags = new List<string>();   // from [tag] markers on the branch H1
        }

        class DaisyBranchData
        {
            public string stateName;
            public List<LineData> lines = new List<LineData>();
            public List<string> tags = new List<string>();   // from [tag] markers on the state header
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
            // sequenced slot payload ([1st]/[2nd]/[last]) - null for normal lines
            public List<List<string>> variantGroups;
            public bool lastSticks;
        }

        class DialogueParseResult
        {
            public int conf;
            public int charm;
            public string text;
        }
    }
}
