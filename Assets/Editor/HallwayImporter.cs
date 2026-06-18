using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Crush.EditorTools
{
    /// <summary>
    /// Imports the Hallway Phase Google Doc into Unity SOs.
    ///
    /// - "Loop N" tabs → LoopHallway_0N SOs. The paragraphs under the H1 become triggerLines
    ///   (index 0 → trigger zone 1, etc.). Added to FirstLoopManager.loopHallways.
    /// - "Base Loop Random Lines" tab → a single HallwayGenericPool SO. The bulleted lines become
    ///   the ambient/base-loop pool. Wired into FirstLoopManager.genericPool.
    /// - Explainer / Open Issues / "Loop N [Legacy]" tabs are skipped.
    ///
    /// Path-resilient: finds existing SOs by name, creates new ones next to existing ones.
    /// </summary>
    public static class HallwayImporter
    {
        const string API_URL = "https://script.google.com/macros/s/AKfycbwSvEH1QAUYxIkz2yQHD61Cszg8vKpCNrL85pf2f3ILFObTq1NNkjD-ZbVAxWX7Y6zz/exec";
        const string API_KEY = "claude062312atwater";
        const string HALLWAY_DOC_ID = "1Efr6HuIgOjZZY5Y6Fq1xf5XyxHEc3xTTeBmmjolXPvo";

        const string DEFAULT_DIR = "Assets/Crush Objects/Hallway";
        const string GENERIC_POOL_NAME = "HallwayGenericPool";

        [MenuItem("Crush/Import Hallway from Doc")]
        public static void ImportHallway()
        {
            try
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                Debug.Log("[HallwayImporter] Listing tabs…");
                var tabs = ListTabs();
                Debug.Log($"[HallwayImporter] Found {tabs.Count} tabs.");

                int updated = 0, warnings = 0;

                foreach (var tab in tabs)
                {
                    var title = tab.title ?? "";
                    if (title == "Base Loop Random Lines")
                    {
                        var (u, w) = ImportGenericPool(tab.id);
                        updated += u; warnings += w;
                    }
                    else if (Regex.IsMatch(title, @"^Loop \d+$"))   // excludes "Loop 1 [Legacy]"
                    {
                        var (u, w) = ImportLoopHallwayTab(tab.id, title);
                        updated += u; warnings += w;
                    }
                    else
                    {
                        Debug.Log($"[HallwayImporter] Skipping tab '{title}' (not a parseable section).");
                    }
                }

                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                Debug.Log($"[HallwayImporter] Import complete. {updated} SO(s) created/updated. {warnings} warning(s).");
                Debug.Log("[HallwayImporter] Scene marked dirty — save the scene (Cmd/Ctrl+S) to persist FirstLoopManager wiring.");
            }
            catch (Exception e)
            {
                Debug.LogError($"[HallwayImporter] Failed: {e}");
            }
        }

        // ============================================================
        // Per-loop trigger lines → LoopHallway_0N
        // ============================================================

        static (int updated, int warnings) ImportLoopHallwayTab(string tabId, string tabTitle)
        {
            var m = Regex.Match(tabTitle, @"^Loop (\d+)$");
            if (!m.Success) return (0, 1);
            int loopNum = int.Parse(m.Groups[1].Value);
            string soName = $"LoopHallway_{loopNum:D2}";

            var tab = FetchTab(tabId);
            var lines = ExtractLines(tab.structure);
            if (lines.Count == 0)
            {
                Debug.LogWarning($"[HallwayImporter] {tabTitle}: no trigger lines parsed. Skipping.");
                return (0, 1);
            }

            var so = FindByName<LoopHallway>(soName);
            bool created = false;
            if (so == null)
            {
                string folder = GetTargetFolder<LoopHallway>();
                if (!AssetDatabase.IsValidFolder(folder)) { Directory.CreateDirectory(folder); AssetDatabase.Refresh(); }
                so = ScriptableObject.CreateInstance<LoopHallway>();
                AssetDatabase.CreateAsset(so, $"{folder}/{soName}.asset");
                created = true;
            }
            so.loop = loopNum;
            so.triggerLines = lines.ToArray();
            EditorUtility.SetDirty(so);

            int warnings = EnsureLoopHallwayInManager(so) ? 0 : 1;
            Debug.Log($"[HallwayImporter] {tabTitle}: {(created ? "CREATED" : "Updated")} {soName} ({lines.Count} trigger line(s)).");
            return (1, warnings);
        }

        // ============================================================
        // Generic pool → HallwayGenericPool
        // ============================================================

        static (int updated, int warnings) ImportGenericPool(string tabId)
        {
            var tab = FetchTab(tabId);
            var sections = ParseConditionSections(tab.structure);

            // "Generic hallway pool" section → backup clusters; "if LoopCount …" → loop-gated cluster pools.
            var genericClusters = new List<List<string>>();
            var conditional = new List<(int min, int max, List<List<string>> groups)>();
            int warnings = 0;

            foreach (var section in sections)
            {
                var head = Regex.Replace((section.heading ?? "").Trim(), @"\s*\([^)]*\)\s*$", "").Trim();
                var groups = new List<List<string>>();
                foreach (var g in section.groups) if (g.Count > 0) groups.Add(g);

                if (Regex.IsMatch(head, @"^generic", RegexOptions.IgnoreCase))   // "Generic hallway pool"
                {
                    genericClusters.AddRange(groups);
                }
                else if (ReflectImporter.TryParsePoolCondition(section.heading, out int mn, out int mx, out string err))
                {
                    if (groups.Count > 0) conditional.Add((mn, mx, groups));
                }
                else
                {
                    Debug.LogWarning($"[HallwayImporter] Base Loop Random Lines: condition '{section.heading}' — {err}. Skipping.");
                    warnings++;
                }
            }

            var pool = FindByName<HallwayGenericPool>(GENERIC_POOL_NAME);
            bool created = false;
            if (pool == null)
            {
                string folder = GetTargetFolder<HallwayGenericPool>();
                if (!AssetDatabase.IsValidFolder(folder)) { Directory.CreateDirectory(folder); AssetDatabase.Refresh(); }
                pool = ScriptableObject.CreateInstance<HallwayGenericPool>();
                AssetDatabase.CreateAsset(pool, $"{folder}/{GENERIC_POOL_NAME}.asset");
                created = true;
            }

            var so = new SerializedObject(pool);
            ReflectImporter.WriteConditionalGroups(so.FindProperty("clusterPools"), conditional);
            ReflectImporter.WriteLineGroups(so.FindProperty("genericClusters"), genericClusters);
            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(pool);

            if (!WireGenericPoolInManager(pool)) warnings++;
            Debug.Log($"[HallwayImporter] Base Loop Random Lines: {(created ? "CREATED" : "Updated")} {GENERIC_POOL_NAME} ({conditional.Count} loop-gated pool(s), {genericClusters.Count} generic cluster(s)).");
            return (1, warnings);
        }

        // ============================================================
        // Condition-section parse (local copy — Hallway has its own TabStructureItem type;
        // the string/SerializedProperty helpers on ReflectImporter are reused directly).
        // ============================================================

        class ConditionSection { public string heading; public List<List<string>> groups = new List<List<string>>(); }

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

        // ============================================================
        // Line extraction — paragraphs + list items after the first H1
        // ============================================================

        static List<string> ExtractLines(TabStructureItem[] structure)
        {
            var lines = new List<string>();
            if (structure == null) return lines;
            bool started = false;
            foreach (var item in structure)
            {
                if (item.type == "heading1") { started = true; continue; }
                if (!started) continue;
                // Stop at a sub-heading (none expected in these tabs, but be safe)
                if (item.type == "heading2" || item.type == "heading3" || item.type == "title") break;
                var t = (item.text ?? "").Trim();
                if (string.IsNullOrEmpty(t)) continue;
                if (t.StartsWith("[!]")) continue;
                lines.Add(t);
            }
            return lines;
        }

        // ============================================================
        // FirstLoopManager wiring
        // ============================================================

        static bool EnsureLoopHallwayInManager(LoopHallway lh)
        {
            var managers = Resources.FindObjectsOfTypeAll<FirstLoopManager>();
            bool any = false;
            foreach (var mgr in managers)
            {
                if (mgr == null) continue;
                if (PrefabUtility.IsPartOfPrefabAsset(mgr)) continue;
                if (mgr.gameObject == null || !mgr.gameObject.scene.IsValid()) continue;

                var so = new SerializedObject(mgr);
                var prop = so.FindProperty("loopHallways");
                if (prop == null || !prop.isArray) continue;

                bool present = false;
                for (int i = 0; i < prop.arraySize; i++)
                    if (prop.GetArrayElementAtIndex(i).objectReferenceValue == lh) { present = true; break; }

                if (!present)
                {
                    prop.InsertArrayElementAtIndex(prop.arraySize);
                    prop.GetArrayElementAtIndex(prop.arraySize - 1).objectReferenceValue = lh;
                }
                so.ApplyModifiedProperties();
                EditorUtility.SetDirty(mgr);
                EditorSceneManager.MarkSceneDirty(mgr.gameObject.scene);
                any = true;
            }
            if (!any)
                Debug.LogWarning($"[HallwayImporter] No FirstLoopManager found in a loaded scene to register {lh.name}. Add it to loopHallways manually.");
            return any;
        }

        static bool WireGenericPoolInManager(HallwayGenericPool pool)
        {
            var managers = Resources.FindObjectsOfTypeAll<FirstLoopManager>();
            bool any = false;
            foreach (var mgr in managers)
            {
                if (mgr == null) continue;
                if (PrefabUtility.IsPartOfPrefabAsset(mgr)) continue;
                if (mgr.gameObject == null || !mgr.gameObject.scene.IsValid()) continue;

                var so = new SerializedObject(mgr);
                var prop = so.FindProperty("genericPool");
                if (prop == null) continue;
                prop.objectReferenceValue = pool;
                so.ApplyModifiedProperties();
                EditorUtility.SetDirty(mgr);
                EditorSceneManager.MarkSceneDirty(mgr.gameObject.scene);
                any = true;
            }
            if (!any)
                Debug.LogWarning($"[HallwayImporter] No FirstLoopManager found in a loaded scene to wire {pool.name}. Set genericPool manually.");
            return any;
        }

        // ============================================================
        // Asset lookups
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

        static string GetTargetFolder<T>() where T : ScriptableObject
        {
            var guids = AssetDatabase.FindAssets($"t:{typeof(T).Name}");
            if (guids.Length > 0)
            {
                var dir = Path.GetDirectoryName(AssetDatabase.GUIDToAssetPath(guids[0]));
                if (!string.IsNullOrEmpty(dir)) return dir.Replace("\\", "/");
            }
            return DEFAULT_DIR;
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
            var raw = FetchUrl($"{API_URL}?key={API_KEY}&action=list_tabs&doc_id={HALLWAY_DOC_ID}");
            var resp = JsonUtility.FromJson<ListTabsResponse>(raw);
            if (!resp.success) throw new Exception($"list_tabs failed: {raw}");
            return resp.tabs != null ? new List<TabRef>(resp.tabs) : new List<TabRef>();
        }

        static TabContent FetchTab(string tabId)
        {
            var raw = FetchUrl($"{API_URL}?key={API_KEY}&action=get_tab&doc_id={HALLWAY_DOC_ID}&tab_id={tabId}");
            var resp = JsonUtility.FromJson<TabContent>(raw);
            if (!resp.success) throw new Exception($"get_tab failed for {tabId}: {raw}");
            return resp;
        }

        [Serializable] class ListTabsResponse { public bool success; public string doc_id; public TabRef[] tabs; }
        [Serializable] class TabRef { public string id; public string title; public int index; public int depth; }
        [Serializable] class TabContent { public bool success; public string doc_id; public string tab_id; public string tab_title; public string text; public TabStructureItem[] structure; }
        [Serializable] class TabStructureItem { public int index; public string type; public string text; }
    }
}
