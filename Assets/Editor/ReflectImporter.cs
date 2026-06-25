using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Crush.EditorTools
{
    /// <summary>
    /// Imports content from the Reflect Phase Google Doc into Unity SOs.
    /// Reads doc content via the Google Docs Editor Apps Script web app.
    ///
    /// v1 scope:
    /// - Milestones tab → updates Milestone.asset SOs (matched by milestoneName).
    /// - Loop N tabs → logs lines (does NOT yet write back to the engine; placeholder for a
    ///   future ReflectBranch SO or per-loop array on GameProgression).
    /// - DeathReactions tab → updates ReflectBranch.asset SOs (matched by SO file name).
    ///
    /// Templates for `Trigger:` line phrasings live in Assets/Editor/import_templates.json.
    /// v1 has them hardcoded in this file as a forward step; refactor to load from JSON
    /// once both the normalizer and the importer reference the file (TODO).
    /// </summary>
    public static class ReflectImporter
    {
        const string API_URL = "https://script.google.com/macros/s/AKfycbwSvEH1QAUYxIkz2yQHD61Cszg8vKpCNrL85pf2f3ILFObTq1NNkjD-ZbVAxWX7Y6zz/exec";
        const string API_KEY = "claude062312atwater";
        const string REFLECT_DOC_ID = "11IkbHj2FHUrDmRbG-JX-nHl2KzbCTdYNx6mgT1XhZTY";

        [MenuItem("Crush/Import Reflect from Doc")]
        public static void ImportReflect()
        {
            try
            {
                // SECURITY: TLS 1.2 — Apps Script requires it on some .NET versions
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                Debug.Log("[ReflectImporter] Listing tabs…");
                var tabs = ListTabs();
                Debug.Log($"[ReflectImporter] Found {tabs.Count} tabs.");

                int updated = 0;
                int warnings = 0;

                foreach (var tab in tabs)
                {
                    if (tab.title == "Milestones")
                    {
                        var (u, w) = ImportMilestonesTab(tab.id);
                        updated += u;
                        warnings += w;
                    }
                    else if (tab.title == "DeathReactions")
                    {
                        var (u, w) = ImportDeathReactionsTab(tab.id);
                        updated += u;
                        warnings += w;
                    }
                    else if (tab.title == "Progress Reflect")
                    {
                        var (u, w) = ImportProgressReflectTab(tab.id);
                        updated += u;
                        warnings += w;
                    }
                    else if (tab.title == "Death Reflect")
                    {
                        var (u, w) = ImportDeathReflectTab(tab.id);
                        updated += u;
                        warnings += w;
                    }
                    else if (tab.title == "Commit Lines")
                    {
                        var (u, w) = ImportPoolTab(tab.id, "commitPools", "Commit Lines");
                        updated += u;
                        warnings += w;
                    }
                    else if (tab.title == "Draft Intro")
                    {
                        var (u, w) = ImportPoolTab(tab.id, "draftIntroPools", "Draft Intro");
                        updated += u;
                        warnings += w;
                    }
                    else if (Regex.IsMatch(tab.title ?? "", @"^Loop \d+$"))
                    {
                        var (u, w) = ImportLoopTab(tab.id, tab.title);
                        updated += u;
                        warnings += w;
                    }
                    else
                    {
                        Debug.Log($"[ReflectImporter] Skipping tab '{tab.title}' (not a parseable section).");
                    }
                }

                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                Debug.Log($"[ReflectImporter] Import complete. {updated} SO(s) created/updated. {warnings} warning(s).");
                Debug.Log("[ReflectImporter] Scene marked dirty — save the scene (Cmd/Ctrl+S) to persist GameProgression / ReflectSelfTalk wiring to disk.");
            }
            catch (Exception e)
            {
                Debug.LogError($"[ReflectImporter] Failed: {e}");
            }
        }

        // ============================================================
        // HTTP fetch
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
            var url = $"{API_URL}?key={API_KEY}&action=list_tabs&doc_id={REFLECT_DOC_ID}";
            var raw = FetchUrl(url);
            var resp = JsonUtility.FromJson<ListTabsResponse>(raw);
            if (!resp.success) throw new Exception($"list_tabs failed: {raw}");
            return resp.tabs?.ToList() ?? new List<TabRef>();
        }

        static TabContent FetchTab(string tabId)
        {
            var url = $"{API_URL}?key={API_KEY}&action=get_tab&doc_id={REFLECT_DOC_ID}&tab_id={tabId}";
            var raw = FetchUrl(url);
            var resp = JsonUtility.FromJson<TabContent>(raw);
            if (!resp.success) throw new Exception($"get_tab failed for {tabId}: {raw}");
            return resp;
        }

        // ============================================================
        // Milestones tab
        // ============================================================

        /// <summary>
        /// Walks the structure of the Milestones tab. Each heading1 starts a new milestone.
        /// Subsequent paragraphs accumulate as Trigger:, Applies upgrade:, or reflect lines
        /// until the next heading1. Notes (lines starting with [!]) and empty paragraphs are skipped.
        /// </summary>
        static (int updated, int warnings) ImportMilestonesTab(string tabId)
        {
            var tab = FetchTab(tabId);
            var parsed = ParseEntityBlocks(tab.structure);
            Debug.Log($"[ReflectImporter] Milestones: parsed {parsed.Count} entities.");

            int updated = 0;
            int warnings = 0;
            foreach (var entity in parsed)
            {
                var ms = FindMilestoneByName(entity.name);
                if (ms == null)
                {
                    Debug.LogWarning($"[ReflectImporter] No Milestone SO found matching milestoneName='{entity.name}'. Skipping. (Doc-driven SO creation is not yet supported.)");
                    warnings++;
                    continue;
                }

                if (!TryParseMilestoneTrigger(entity.triggerLine, out var conditionType, out var threshold, out var tag, out var parseError))
                {
                    Debug.LogWarning($"[ReflectImporter] '{entity.name}': could not parse trigger '{entity.triggerLine}'. {parseError}");
                    warnings++;
                    continue;
                }

                var characterUpgrade = LookupCharacterUpgrade(entity.upgradeName);
                if (!string.IsNullOrEmpty(entity.upgradeName) && characterUpgrade == null)
                {
                    Debug.LogWarning($"[ReflectImporter] '{entity.name}': Applies upgrade '{entity.upgradeName}' — no CharacterUpgrade SO matches that upgradeName.");
                    warnings++;
                }

                ms.condition = new MilestoneCondition
                {
                    type = conditionType,
                    threshold = threshold,
                    tag = tag,
                };
                ms.reflectLines = entity.reflectLines.ToArray();
                ms.characterUpgrade = characterUpgrade;

                EditorUtility.SetDirty(ms);
                updated++;
                Debug.Log($"[ReflectImporter] Updated milestone '{entity.name}' — type={conditionType}, threshold={threshold}, tag={tag}, upgrade={(characterUpgrade != null ? characterUpgrade.name : "(none)")}, {entity.reflectLines.Count} reflect line(s).");
            }
            return (updated, warnings);
        }

        // ============================================================
        // DeathReactions tab — writes to ReflectBranch SOs
        // ============================================================

        /// <summary>
        /// Each heading1 is a ReflectBranch SO matched by its asset file name.
        /// Parses the Trigger line for "Death occurred on <CardName> card" to set requiresDeathCard.
        /// Reflect lines are everything after Trigger:.
        /// </summary>
        static (int updated, int warnings) ImportDeathReactionsTab(string tabId)
        {
            var tab = FetchTab(tabId);
            var parsed = ParseEntityBlocks(tab.structure);
            Debug.Log($"[ReflectImporter] DeathReactions: parsed {parsed.Count} entities.");

            int updated = 0;
            int warnings = 0;
            foreach (var entity in parsed)
            {
                var rb = FindReflectBranchByAssetName(entity.name);
                if (rb == null)
                {
                    Debug.LogWarning($"[ReflectImporter] No ReflectBranch asset found named '{entity.name}'. Skipping.");
                    warnings++;
                    continue;
                }

                // Parse trigger for death-card reference
                var m = Regex.Match(entity.triggerLine ?? "", @"^Death occurred on the ([\w ]+?) card\s*\(?[^)]*\)?\.?$");
                if (m.Success)
                {
                    var cardName = m.Groups[1].Value.Trim();
                    var card = FindDialogueCardByName(cardName);
                    if (card == null)
                    {
                        Debug.LogWarning($"[ReflectImporter] '{entity.name}': could not find DialogueCard SO matching '{cardName}'.");
                        warnings++;
                    }
                    rb.requiresDeathCard = card;
                }
                else
                {
                    Debug.LogWarning($"[ReflectImporter] '{entity.name}': trigger '{entity.triggerLine}' didn't match the v1 death-card pattern. Leaving requiresDeathCard untouched.");
                    warnings++;
                }

                rb.lines = entity.reflectLines.ToArray();
                EditorUtility.SetDirty(rb);
                updated++;
                Debug.Log($"[ReflectImporter] Updated ReflectBranch '{entity.name}' — {entity.reflectLines.Count} line(s), requiresDeathCard={(rb.requiresDeathCard != null ? rb.requiresDeathCard.name : "(none)")}.");
            }
            return (updated, warnings);
        }

        // ============================================================
        // Progress Reflect tab — beat 2 of the base-loop reflect (86baajqwe)
        // Each H1 is a condition ("if lastPeakConfidence > 10"); blank-line-separated
        // groups under it each become ONE conditional ReflectBranch whose range fields
        // encode the condition. Beat 2 (ReflectSelfTalk.PlayRandomEligible) then picks a
        // random eligible branch. Regenerated from scratch each import (prefix-namespaced).
        // ============================================================

        const string PROGRESS_REFLECT_PREFIX = "ProgressReflect_";

        static (int updated, int warnings) ImportProgressReflectTab(string tabId)
        {
            var tab = FetchTab(tabId);
            var sections = ParseConditionSections(tab.structure);

            // wipe last import's generated branches (and their ReflectSelfTalk refs) before rebuilding
            PurgeGeneratedBranches(PROGRESS_REFLECT_PREFIX);

            string folder = GetReflectBranchFolder();
            if (!AssetDatabase.IsValidFolder(folder))
            {
                Directory.CreateDirectory(folder);
                AssetDatabase.Refresh();
            }

            int created = 0, warnings = 0, idx = 0;
            foreach (var section in sections)
            {
                var apply = ParseConditionHeading(section.heading, out string err);
                if (apply == null)
                {
                    Debug.LogWarning($"[ReflectImporter] Progress Reflect: condition '{section.heading}' — {err}. Skipping its {section.groups.Count} group(s).");
                    warnings++;
                    continue;
                }
                foreach (var group in section.groups)
                {
                    if (group.Count == 0) continue;
                    idx++;
                    var rb = ScriptableObject.CreateInstance<ReflectBranch>();
                    apply(rb);                 // bake the condition into the range fields
                    rb.lines = group.ToArray();
                    rb.isScripted = false;
                    string name = $"{PROGRESS_REFLECT_PREFIX}{idx:D3}";
                    AssetDatabase.CreateAsset(rb, $"{folder}/{name}.asset");
                    EditorUtility.SetDirty(rb);
                    if (!EnsureBranchInReflectSelfTalk(rb))
                    {
                        Debug.LogWarning($"[ReflectImporter] Progress Reflect: created {name} but no ReflectSelfTalk in a loaded scene to wire it into.");
                        warnings++;
                    }
                    created++;
                }
            }
            Debug.Log($"[ReflectImporter] Progress Reflect: {created} conditional branch(es) from {sections.Count} condition(s).");
            return (created, warnings);
        }

        // ---- condition grammar: "if <field> <op> N [and <field> <op> N ...]" / "if deathFromCharm"
        // maps onto ReflectBranch's existing min/max range fields. Returns an apply-delegate or null+error.
        static System.Action<ReflectBranch> ParseConditionHeading(string heading, out string error)
        {
            error = null;
            var h = (heading ?? "").Trim();
            h = Regex.Replace(h, @"\s*\([^)]*\)\s*$", "").Trim();   // drop trailing (comment)
            h = Regex.Replace(h, @"^if\s+", "", RegexOptions.IgnoreCase).Trim();
            if (h.Length == 0) { error = "empty condition"; return null; }

            var actions = new List<System.Action<ReflectBranch>>();
            foreach (var raw in SplitAndClauses(h))
            {
                var c = raw.Trim();
                if (c.Equals("deathFromCharm", StringComparison.OrdinalIgnoreCase))
                {
                    actions.Add(rb => rb.requiresDeathFromCharm = true);
                    continue;
                }
                var m = Regex.Match(c, @"^(\w+)\s*(>=|<=|==|>|<)\s*(-?\d+)$");
                if (!m.Success) { error = $"clause '{c}' not understood"; return null; }
                var act = BuildClauseAction(m.Groups[1].Value.ToLowerInvariant(), m.Groups[2].Value, int.Parse(m.Groups[3].Value), out string ferr);
                if (act == null) { error = ferr; return null; }
                actions.Add(act);
            }
            return rb => { foreach (var a in actions) a(rb); };
        }

        static System.Action<ReflectBranch> BuildClauseAction(string field, string op, int val, out string error)
        {
            error = null;
            System.Action<ReflectBranch, int> setMin, setMax;
            switch (field)
            {
                case "finalconfidence":     setMin = (rb, v) => rb.minFinalConfidence = Mathf.Max(rb.minFinalConfidence, v); setMax = (rb, v) => rb.maxFinalConfidence = Mathf.Min(rb.maxFinalConfidence, v); break;
                case "lastpeakconfidence":  setMin = (rb, v) => rb.minPeakConfidence  = Mathf.Max(rb.minPeakConfidence, v);  setMax = (rb, v) => rb.maxPeakConfidence  = Mathf.Min(rb.maxPeakConfidence, v);  break;
                case "finalcharm":          setMin = (rb, v) => rb.minFinalCharm      = Mathf.Max(rb.minFinalCharm, v);      setMax = (rb, v) => rb.maxFinalCharm      = Mathf.Min(rb.maxFinalCharm, v);      break;
                case "lastpeakcharm":       setMin = (rb, v) => rb.minPeakCharm       = Mathf.Max(rb.minPeakCharm, v);       setMax = (rb, v) => rb.maxPeakCharm       = Mathf.Min(rb.maxPeakCharm, v);       break;
                case "loopcount":           setMin = (rb, v) => rb.minLoop            = Mathf.Max(rb.minLoop, v);            setMax = (rb, v) => rb.maxLoop            = Mathf.Min(rb.maxLoop, v);            break;
                default: error = $"unknown field '{field}'"; return null;
            }
            switch (op)
            {
                case ">":  return rb => setMin(rb, val + 1);
                case ">=": return rb => setMin(rb, val);
                case "<":  return rb => setMax(rb, val - 1);
                case "<=": return rb => setMax(rb, val);
                case "==": return rb => { setMin(rb, val); setMax(rb, val); };
                default: error = $"unknown op '{op}'"; return null;
            }
        }

        class ConditionSection { public string heading; public List<List<string>> groups = new List<List<string>>(); }

        // walk a tab into (condition heading -> blank-line-separated line groups). Shared shape
        // for Progress Reflect, Death Reflect, Commit/Draft pool tabs. (HallwayImporter has its own
        // copy since it has a distinct TabStructureItem type.)
        static List<ConditionSection> ParseConditionSections(TabStructureItem[] structure)
        {
            var result = new List<ConditionSection>();
            if (structure == null) return result;

            ConditionSection cur = null;
            List<string> group = null;
            foreach (var item in structure)
            {
                if (item.type == "heading1")
                {
                    cur = new ConditionSection { heading = (item.text ?? "").Trim() };
                    result.Add(cur);
                    group = null;
                    continue;
                }
                if (cur == null) continue;
                var text = item.text ?? "";
                if (string.IsNullOrWhiteSpace(text)) { group = null; continue; }   // blank = group break
                foreach (var sub in text.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    var line = sub.Trim();
                    if (line.Length == 0 || line.StartsWith("[!]")) continue;
                    if (group == null) { group = new List<string>(); cur.groups.Add(group); }
                    group.Add(line);
                }
            }
            return result;
        }

        // delete every ReflectBranch asset whose name starts with prefix, after unwiring it
        // from any ReflectSelfTalk. Lets each import regenerate the generated set cleanly.
        static void PurgeGeneratedBranches(string prefix)
        {
            var toDelete = new List<string>();
            foreach (var guid in AssetDatabase.FindAssets("t:ReflectBranch"))
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                if (!Path.GetFileNameWithoutExtension(path).StartsWith(prefix)) continue;
                var rb = AssetDatabase.LoadAssetAtPath<ReflectBranch>(path);
                if (rb != null) RemoveBranchFromReflectSelfTalk(rb);
                toDelete.Add(path);
            }
            foreach (var p in toDelete) AssetDatabase.DeleteAsset(p);
        }

        static void RemoveBranchFromReflectSelfTalk(ReflectBranch rb)
        {
            foreach (var rst in Resources.FindObjectsOfTypeAll<ReflectSelfTalk>())
            {
                if (rst == null || PrefabUtility.IsPartOfPrefabAsset(rst)) continue;
                if (rst.gameObject == null || !rst.gameObject.scene.IsValid()) continue;
                var so = new SerializedObject(rst);
                var prop = so.FindProperty("branches");
                if (prop == null || !prop.isArray) continue;
                for (int i = prop.arraySize - 1; i >= 0; i--)
                {
                    if (prop.GetArrayElementAtIndex(i).objectReferenceValue == rb)
                        prop.DeleteArrayElementAtIndex(i);
                }
                so.ApplyModifiedProperties();
                EditorUtility.SetDirty(rst);
                EditorSceneManager.MarkSceneDirty(rst.gameObject.scene);
            }
        }

        // accept either "and" or "&&" between clauses (Davis writes both)
        internal static string[] SplitAndClauses(string h) => Regex.Split(h, @"\s+and\s+|\s*&&\s*", RegexOptions.IgnoreCase);

        // ============================================================
        // Commit Lines / Draft Intro tabs — beats 4 & 3 loop-gated pools (86baajqwe)
        // Each "if LoopCount …" H1 → a ConditionalReflectGroups entry on BaseReflectPools.
        // Only loopCount conditions are supported here (per design); other fields warn.
        // ============================================================

        static (int updated, int warnings) ImportPoolTab(string tabId, string poolFieldName, string label)
        {
            var tab = FetchTab(tabId);
            var sections = ParseConditionSections(tab.structure);

            var entries = new List<(int min, int max, List<List<string>> groups)>();
            int warnings = 0;
            foreach (var section in sections)
            {
                if (!TryParsePoolCondition(section.heading, out int mn, out int mx, out string err))
                {
                    Debug.LogWarning($"[ReflectImporter] {label}: condition '{section.heading}' — {err}. Skipping its group(s).");
                    warnings++;
                    continue;
                }
                var groups = new List<List<string>>();
                foreach (var g in section.groups) if (g.Count > 0) groups.Add(g);
                if (groups.Count > 0) entries.Add((mn, mx, groups));
            }

            var pools = FindOrCreateBaseReflectPools();
            var so = new SerializedObject(pools);
            WriteConditionalGroups(so.FindProperty(poolFieldName), entries);
            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(pools);
            EnsureBasePoolsWired(pools);

            Debug.Log($"[ReflectImporter] {label}: {entries.Count} loop-gated pool(s).");
            return (entries.Count, warnings);
        }

        // loopCount-only condition → (minLoop, maxLoop). "Generic" / no clause = 0..999.
        internal static bool TryParsePoolCondition(string heading, out int minLoop, out int maxLoop, out string error)
        {
            minLoop = 0; maxLoop = 999; error = null;
            var h = Regex.Replace((heading ?? "").Trim(), @"\s*\([^)]*\)\s*$", "").Trim();
            if (Regex.IsMatch(h, @"^generic$", RegexOptions.IgnoreCase)) return true;
            h = Regex.Replace(h, @"^if\s+", "", RegexOptions.IgnoreCase).Trim();
            if (h.Length == 0) return true;   // bare/empty heading = all loops

            foreach (var raw in SplitAndClauses(h))
            {
                var m = Regex.Match(raw.Trim(), @"^loopcount\s*(>=|<=|==|>|<)\s*(\d+)$", RegexOptions.IgnoreCase);
                if (!m.Success) { error = $"only loopCount conditions are supported here (got '{raw.Trim()}')"; return false; }
                int v = int.Parse(m.Groups[2].Value);
                switch (m.Groups[1].Value)
                {
                    case ">":  minLoop = Mathf.Max(minLoop, v + 1); break;
                    case ">=": minLoop = Mathf.Max(minLoop, v); break;
                    case "<":  maxLoop = Mathf.Min(maxLoop, v - 1); break;
                    case "<=": maxLoop = Mathf.Min(maxLoop, v); break;
                    case "==": minLoop = v; maxLoop = v; break;
                }
            }
            return true;
        }

        internal static void WriteConditionalGroups(SerializedProperty arrayProp, List<(int min, int max, List<List<string>> groups)> entries)
        {
            arrayProp.arraySize = entries.Count;
            for (int i = 0; i < entries.Count; i++)
            {
                var e = arrayProp.GetArrayElementAtIndex(i);
                e.FindPropertyRelative("minLoop").intValue = entries[i].min;
                e.FindPropertyRelative("maxLoop").intValue = entries[i].max;
                var gp = e.FindPropertyRelative("groups");
                gp.arraySize = entries[i].groups.Count;
                for (int j = 0; j < entries[i].groups.Count; j++)
                {
                    var lp = gp.GetArrayElementAtIndex(j).FindPropertyRelative("lines");
                    lp.arraySize = entries[i].groups[j].Count;
                    for (int k = 0; k < entries[i].groups[j].Count; k++)
                        lp.GetArrayElementAtIndex(k).stringValue = entries[i].groups[j][k];
                }
            }
        }

        // ============================================================
        // Death Reflect tab — beat 1 generic/repeat pools (86baajqwe)
        // "Generic" + "if repeatDeaths >= 2" sections; blank-line groups → BaseReflectPools
        // (created + wired to ReflectSelfTalk if missing). Card-specific reactions still win at
        // runtime; these are the fallback pools.
        // ============================================================

        static (int updated, int warnings) ImportDeathReflectTab(string tabId)
        {
            var tab = FetchTab(tabId);
            var sections = ParseConditionSections(tab.structure);

            var generic = new List<List<string>>();
            var repeat = new List<List<string>>();
            int warnings = 0;

            foreach (var section in sections)
            {
                var head = Regex.Replace((section.heading ?? "").Trim(), @"\s*\([^)]*\)\s*$", "").Trim();
                List<List<string>> target;
                if (Regex.IsMatch(head, @"^generic$", RegexOptions.IgnoreCase))
                    target = generic;
                else if (Regex.IsMatch(head, @"^if\s+repeatDeaths\s*(>=|>)\s*2$", RegexOptions.IgnoreCase))
                    target = repeat;
                else
                {
                    Debug.LogWarning($"[ReflectImporter] Death Reflect: section '{section.heading}' isn't 'Generic' or 'if repeatDeaths >= 2' — skipping its {section.groups.Count} group(s).");
                    warnings++;
                    continue;
                }
                foreach (var g in section.groups)
                    if (g.Count > 0) target.Add(g);
            }

            var pools = FindOrCreateBaseReflectPools();
            var so = new SerializedObject(pools);
            WriteLineGroups(so.FindProperty("genericDeathReactions"), generic);
            WriteLineGroups(so.FindProperty("repeatDeathReactions"), repeat);
            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(pools);

            if (!EnsureBasePoolsWired(pools))
                Debug.LogWarning("[ReflectImporter] Death Reflect: wrote BaseReflectPools but no ReflectSelfTalk in a loaded scene to wire it into.");

            Debug.Log($"[ReflectImporter] Death Reflect: {generic.Count} generic + {repeat.Count} repeat death-reaction group(s) → BaseReflectPools.");
            return (generic.Count + repeat.Count, warnings);
        }

        // write List<group-of-lines> into a ReflectLineGroup[] serialized array
        internal static void WriteLineGroups(SerializedProperty arrayProp, List<List<string>> groups)
        {
            arrayProp.arraySize = groups.Count;
            for (int i = 0; i < groups.Count; i++)
            {
                var linesProp = arrayProp.GetArrayElementAtIndex(i).FindPropertyRelative("lines");
                linesProp.arraySize = groups[i].Count;
                for (int k = 0; k < groups[i].Count; k++)
                    linesProp.GetArrayElementAtIndex(k).stringValue = groups[i][k];
            }
        }

        static BaseReflectPools FindOrCreateBaseReflectPools()
        {
            var guids = AssetDatabase.FindAssets("t:BaseReflectPools");
            if (guids.Length > 0)
                return AssetDatabase.LoadAssetAtPath<BaseReflectPools>(AssetDatabase.GUIDToAssetPath(guids[0]));

            string folder = GetReflectBranchFolder();
            if (!AssetDatabase.IsValidFolder(folder)) { Directory.CreateDirectory(folder); AssetDatabase.Refresh(); }
            var pools = ScriptableObject.CreateInstance<BaseReflectPools>();
            AssetDatabase.CreateAsset(pools, $"{folder}/BaseReflectPools.asset");
            Debug.Log($"[ReflectImporter] Created BaseReflectPools at {folder}/BaseReflectPools.asset");
            return pools;
        }

        // wire the pools asset into ReflectSelfTalk.basePools on every loaded instance
        static bool EnsureBasePoolsWired(BaseReflectPools pools)
        {
            bool any = false;
            foreach (var rst in Resources.FindObjectsOfTypeAll<ReflectSelfTalk>())
            {
                if (rst == null || PrefabUtility.IsPartOfPrefabAsset(rst)) continue;
                if (rst.gameObject == null || !rst.gameObject.scene.IsValid()) continue;
                var so = new SerializedObject(rst);
                var prop = so.FindProperty("basePools");
                if (prop == null) continue;
                if (prop.objectReferenceValue != pools)
                {
                    prop.objectReferenceValue = pools;
                    so.ApplyModifiedProperties();
                    EditorUtility.SetDirty(rst);
                    EditorSceneManager.MarkSceneDirty(rst.gameObject.scene);
                }
                any = true;
            }
            return any;
        }

        // ============================================================
        // Loop N tabs — create-or-update LoopReflect_0N SOs
        // ============================================================

        // Used ONLY when no ReflectBranch SO exists yet to infer the folder from.
        // Otherwise the importer creates new loop reflects next to the existing ones —
        // so moving the folder (e.g. into "Crush Objects") doesn't break anything.
        const string REFLECT_BRANCH_DEFAULT_DIR = "Assets/Crush Objects/ReflectSelfTalk";

        /// <summary>
        /// Each "Loop N" tab maps to a ReflectBranch SO named LoopReflect_0N.
        /// Reflect lines are the paragraphs after the H1 (and before any H2 sub-section like
        /// "Conditional variant" — those are handled separately and ignored in v1).
        /// Notes prefixed with [!] are skipped.
        /// After create-or-update:
        ///   - Loop 1's SO is auto-wired into GameProgression.loop1ReflectBranch.
        ///   - Loops 2+ are prepended to ReflectSelfTalk.branches so they win branch selection
        ///     over any legacy branches (e.g. FirstReflect/SecondBranch).
        /// </summary>
        static (int updated, int warnings) ImportLoopTab(string tabId, string tabTitle)
        {
            var m = Regex.Match(tabTitle ?? "", @"^Loop (\d+)$");
            if (!m.Success)
            {
                Debug.LogWarning($"[ReflectImporter] Tab '{tabTitle}' didn't match 'Loop N' pattern.");
                return (0, 1);
            }
            int loopNum = int.Parse(m.Groups[1].Value);
            string soName = $"LoopReflect_{loopNum:D2}";

            // Extract reflect + commit sections from the tab structure.
            var tab = FetchTab(tabId);
            var (lines, commitLines) = ExtractLoopSections(tab.structure);
            if (lines.Count == 0)
            {
                Debug.LogWarning($"[ReflectImporter] {tabTitle}: no reflect lines parsed. Skipping.");
                return (0, 1);
            }

            // Find existing SO by name (path-independent — survives folder moves).
            // If absent, create it next to the existing ReflectBranch SOs.
            var rb = FindReflectBranchByAssetName(soName);
            bool created = false;
            if (rb == null)
            {
                string folder = GetReflectBranchFolder();
                if (!AssetDatabase.IsValidFolder(folder))
                {
                    Directory.CreateDirectory(folder);
                    AssetDatabase.Refresh();
                }
                rb = ScriptableObject.CreateInstance<ReflectBranch>();
                AssetDatabase.CreateAsset(rb, $"{folder}/{soName}.asset");
                created = true;
            }
            rb.lines = lines.ToArray();
            rb.commitLines = commitLines.ToArray();
            rb.minLoop = loopNum;
            rb.maxLoop = loopNum;
            rb.isScripted = true; // Mark as scripted (loop-keyed narrative spine) — see ReflectBranch.isScripted and DeathRespawn priority logic.
            EditorUtility.SetDirty(rb);

            int warnings = 0;
            if (loopNum == 1)
            {
                if (!WireLoop1ToGameProgression(rb))
                {
                    Debug.LogWarning($"[ReflectImporter] {tabTitle}: created/updated {soName} but no GameProgression component found in any loaded scene. Drag {soName} into GameProgression.loop1ReflectBranch manually.");
                    warnings++;
                }
            }
            else
            {
                if (!EnsureBranchInReflectSelfTalk(rb))
                {
                    Debug.LogWarning($"[ReflectImporter] {tabTitle}: created/updated {soName} but no ReflectSelfTalk component found in any loaded scene. Add {soName} to its branches array manually.");
                    warnings++;
                }
            }

            Debug.Log($"[ReflectImporter] {tabTitle}: {(created ? "CREATED" : "Updated")} {soName} ({lines.Count} reflect + {commitLines.Count} commit line(s)){(loopNum == 1 ? " — wired into GameProgression" : " — added to ReflectSelfTalk.branches")}.");
            return (1, warnings);
        }

        /// <summary>
        /// Walks the structure of a Loop tab and splits it into reflect + commit line sets.
        /// - Paragraphs after the loop H1 (and before any H2) are reflect lines.
        /// - A `### Commit` H2 switches collection to commit lines.
        /// - Any other H2/H3 (e.g. "Conditional variant") switches to an ignored section —
        ///   those are handled elsewhere (future task), not folded into this SO.
        /// Skips [!] notes and empty paragraphs throughout.
        /// </summary>
        static (List<string> reflect, List<string> commit) ExtractLoopSections(TabStructureItem[] structure)
        {
            var reflect = new List<string>();
            var commit = new List<string>();
            if (structure == null) return (reflect, commit);

            // section: "none" before the H1, "reflect" after it, "commit" after ### Commit,
            //          "ignore" after any other sub-heading.
            string section = "none";
            foreach (var item in structure)
            {
                if (item.type == "heading1")
                {
                    section = "reflect";
                    continue;
                }
                if (section == "none") continue;

                if (item.type == "heading2" || item.type == "heading3" || item.type == "title")
                {
                    var heading = (item.text ?? "").Trim().ToLowerInvariant();
                    section = (heading == "commit") ? "commit" : "ignore";
                    continue;
                }

                var t = (item.text ?? "").Trim();
                if (string.IsNullOrEmpty(t)) continue;
                if (t.StartsWith("[!]")) continue;

                //some doc paragraphs come back as one structure item with embedded newlines
                //- split so each visible line becomes its own entry
                foreach (var sub in t.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    var line = sub.Trim();
                    if (string.IsNullOrEmpty(line)) continue;
                    if (line.StartsWith("[!]")) continue;
                    if (section == "reflect") reflect.Add(line);
                    else if (section == "commit") commit.Add(line);
                    // "ignore" section: skip
                }
            }
            return (reflect, commit);
        }

        /// <summary>
        /// Set GameProgression.Loop1ReflectBranch on every loaded GameProgression instance.
        /// Returns true if at least one was wired.
        /// </summary>
        static bool WireLoop1ToGameProgression(ReflectBranch rb)
        {
            var components = Resources.FindObjectsOfTypeAll<GameProgression>();
            bool any = false;
            foreach (var gp in components)
            {
                if (gp == null) continue;
                // Skip prefab assets (only modify scene instances)
                if (PrefabUtility.IsPartOfPrefabAsset(gp)) continue;
                if (gp.gameObject == null || !gp.gameObject.scene.IsValid()) continue;

                gp.Loop1ReflectBranch = rb;
                EditorUtility.SetDirty(gp);
                EditorSceneManager.MarkSceneDirty(gp.gameObject.scene);
                any = true;
            }
            return any;
        }

        /// <summary>
        /// Ensure rb is present in every loaded ReflectSelfTalk.branches. Prepends at index 0
        /// so it wins branch selection over any legacy entries.
        /// Returns true if at least one ReflectSelfTalk was touched.
        /// </summary>
        static bool EnsureBranchInReflectSelfTalk(ReflectBranch rb)
        {
            var components = Resources.FindObjectsOfTypeAll<ReflectSelfTalk>();
            bool any = false;
            foreach (var rst in components)
            {
                if (rst == null) continue;
                if (PrefabUtility.IsPartOfPrefabAsset(rst)) continue;
                if (rst.gameObject == null || !rst.gameObject.scene.IsValid()) continue;

                var so = new SerializedObject(rst);
                var prop = so.FindProperty("branches");
                if (prop == null || !prop.isArray)
                {
                    Debug.LogWarning($"[ReflectImporter] ReflectSelfTalk on '{rst.gameObject.name}' missing 'branches' property — skipping.");
                    continue;
                }

                // Check if already present
                bool alreadyPresent = false;
                for (int i = 0; i < prop.arraySize; i++)
                {
                    var elem = prop.GetArrayElementAtIndex(i);
                    if (elem.objectReferenceValue == rb) { alreadyPresent = true; break; }
                }

                if (!alreadyPresent)
                {
                    prop.InsertArrayElementAtIndex(0);
                    prop.GetArrayElementAtIndex(0).objectReferenceValue = rb;
                }

                so.ApplyModifiedProperties();
                EditorUtility.SetDirty(rst);
                EditorSceneManager.MarkSceneDirty(rst.gameObject.scene);
                any = true;
            }
            return any;
        }

        // ============================================================
        // Shared entity-block parser
        // ============================================================

        class EntityBlock
        {
            public string name;
            public string triggerLine;
            public string upgradeName;
            public List<string> reflectLines = new List<string>();
        }

        static List<EntityBlock> ParseEntityBlocks(TabStructureItem[] structure)
        {
            var result = new List<EntityBlock>();
            if (structure == null) return result;

            EntityBlock current = null;
            foreach (var item in structure)
            {
                var text = (item.text ?? "").Trim();
                if (item.type == "heading1")
                {
                    if (current != null) result.Add(current);
                    current = new EntityBlock { name = text };
                    continue;
                }
                if (current == null) continue;
                if (string.IsNullOrEmpty(text)) continue;
                if (text.StartsWith("[!]")) continue;

                // Strip leading bold markdown if present (the markdown writer keeps **field**: format)
                var fieldMatch = Regex.Match(text, @"^\s*Trigger:\s*(.+?)\s*$");
                if (fieldMatch.Success)
                {
                    current.triggerLine = fieldMatch.Groups[1].Value.Trim().TrimEnd('.');
                    continue;
                }
                fieldMatch = Regex.Match(text, @"^\s*Applies upgrade:\s*(.+?)\s*$");
                if (fieldMatch.Success)
                {
                    current.upgradeName = fieldMatch.Groups[1].Value.Trim().TrimEnd('.');
                    continue;
                }

                //split multi-line paragraphs into one entry per line (same fix as ExtractLoopSections)
                foreach (var sub in text.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    var line = sub.Trim();
                    if (string.IsNullOrEmpty(line)) continue;
                    if (line.StartsWith("[!]")) continue;
                    current.reflectLines.Add(line);
                }
            }
            if (current != null) result.Add(current);
            return result;
        }

        // ============================================================
        // Trigger template matching (v1 — hardcoded; should load from import_templates.json)
        // ============================================================

        static bool TryParseMilestoneTrigger(string trigger, out MilestoneConditionType type, out int threshold, out DialogueTag tag, out string error)
        {
            type = default;
            threshold = 0;
            tag = default;
            error = null;

            if (string.IsNullOrEmpty(trigger))
            {
                error = "Trigger line is empty or missing.";
                return false;
            }

            // PeakConfidenceReached: "Peak confidence reaches N"
            var m = Regex.Match(trigger, @"^Peak confidence reaches (\d+)$");
            if (m.Success)
            {
                type = MilestoneConditionType.PeakConfidenceReached;
                threshold = int.Parse(m.Groups[1].Value);
                return true;
            }

            // PeakCharmReached: "Peak charm reaches N"
            m = Regex.Match(trigger, @"^Peak charm reaches (\d+)$");
            if (m.Success)
            {
                type = MilestoneConditionType.PeakCharmReached;
                threshold = int.Parse(m.Groups[1].Value);
                return true;
            }

            // CardsPlayedInLoopAtLeast: "Plays N or more cards in a loop"
            m = Regex.Match(trigger, @"^Plays (\d+) or more cards in a loop$");
            if (m.Success)
            {
                type = MilestoneConditionType.CardsPlayedInLoopAtLeast;
                threshold = int.Parse(m.Groups[1].Value);
                return true;
            }

            // DialogueTagFired: "<Tag> tag fires" — valid tags come straight from the DialogueTag enum,
            // so adding a value to DialogueTag.cs is the ONLY edit needed to support a new tag here.
            m = Regex.Match(trigger, @"^(.+) tag fires$");
            if (m.Success)
            {
                var tagName = m.Groups[1].Value.Trim();
                if (Enum.TryParse<DialogueTag>(tagName, true, out var parsedTag))
                {
                    type = MilestoneConditionType.DialogueTagFired;
                    threshold = 1;
                    tag = parsedTag;
                    return true;
                }
                error = $"Unknown dialogue tag '{tagName}'. Valid tags: {string.Join(", ", Enum.GetNames(typeof(DialogueTag)))}. Add it to DialogueTag.cs if it's new.";
                return false;
            }

            error = $"No template matched '{trigger}'. See Assets/Editor/import_templates.json for supported phrasings.";
            return false;
        }

        // ============================================================
        // SO lookups
        // ============================================================

        static Milestone FindMilestoneByName(string milestoneName)
        {
            var guids = AssetDatabase.FindAssets("t:Milestone");
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var ms = AssetDatabase.LoadAssetAtPath<Milestone>(path);
                if (ms != null && ms.milestoneName == milestoneName) return ms;
            }
            return null;
        }

        static CharacterUpgrade LookupCharacterUpgrade(string upgradeName)
        {
            if (string.IsNullOrEmpty(upgradeName)) return null;
            var guids = AssetDatabase.FindAssets("t:CharacterUpgrade");
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var up = AssetDatabase.LoadAssetAtPath<CharacterUpgrade>(path);
                if (up != null && up.upgradeName == upgradeName) return up;
            }
            return null;
        }

        static ReflectBranch FindReflectBranchByAssetName(string assetName)
        {
            var guids = AssetDatabase.FindAssets($"{assetName} t:ReflectBranch");
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                if (Path.GetFileNameWithoutExtension(path) == assetName)
                    return AssetDatabase.LoadAssetAtPath<ReflectBranch>(path);
            }
            return null;
        }

        /// <summary>
        /// Folder where new loop reflect SOs should be created. Infers it from where
        /// existing ReflectBranch SOs already live, so moving them (e.g. into Crush Objects)
        /// is transparent. Falls back to REFLECT_BRANCH_DEFAULT_DIR only if none exist yet.
        /// </summary>
        static string GetReflectBranchFolder()
        {
            var guids = AssetDatabase.FindAssets("t:ReflectBranch");
            if (guids.Length > 0)
            {
                var path = AssetDatabase.GUIDToAssetPath(guids[0]);
                var dir = Path.GetDirectoryName(path);
                if (!string.IsNullOrEmpty(dir)) return dir.Replace("\\", "/");
            }
            return REFLECT_BRANCH_DEFAULT_DIR;
        }

        static DialogueCard FindDialogueCardByName(string cardName)
        {
            var guids = AssetDatabase.FindAssets($"{cardName} t:DialogueCard");
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                if (Path.GetFileNameWithoutExtension(path) == cardName)
                    return AssetDatabase.LoadAssetAtPath<DialogueCard>(path);
            }
            return null;
        }

        // ============================================================
        // JSON DTOs (JsonUtility-compatible)
        // ============================================================

        [Serializable]
        class ListTabsResponse
        {
            public bool success;
            public string doc_id;
            public TabRef[] tabs;
        }

        [Serializable]
        class TabRef
        {
            public string id;
            public string title;
            public int index;
            public int depth;
        }

        [Serializable]
        class TabContent
        {
            public bool success;
            public string doc_id;
            public string tab_id;
            public string tab_title;
            public string text;
            public TabStructureItem[] structure;
        }

        [Serializable]
        class TabStructureItem
        {
            public int index;
            public string type;
            public string text;
        }
    }
}
