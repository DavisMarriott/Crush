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
    /// Dedicated pass that scans the Cards + Reflect docs for dialogue tags and appends any NEW ones
    /// to the DialogueTag enum (DialogueTag.cs). Run this FIRST when you've authored a new tag in a doc,
    /// let Unity recompile, THEN run the content importers — which now see the new tag compiled in.
    ///
    /// Why it's separate: adding an enum value forces a recompile, and a single importer pass can't use a
    /// value that wasn't compiled when it started. Splitting "add the tag" from "use the tag" sidesteps that.
    ///
    /// Tag sources scanned:
    ///   - Cards doc: [tag] markers on branch H1 headers + Daisy state headers (e.g. "Normal branch [funny]",
    ///     "**High** (charm 9-10) [funny]").
    ///   - Reflect doc: "&lt;Tag&gt; tag fires" milestone triggers + any [tag] on H1 headers.
    /// Non-tag brackets ([!], [1st]/[last], [+N conf]) are ignored: they only live on dialogue lines (never
    /// scanned), and the identifier check drops anything with spaces/digits/symbols anyway.
    /// </summary>
    public static class TagSyncImporter
    {
        const string API_URL = "https://script.google.com/macros/s/AKfycbwSvEH1QAUYxIkz2yQHD61Cszg8vKpCNrL85pf2f3ILFObTq1NNkjD-ZbVAxWX7Y6zz/exec";
        const string API_KEY = "claude062312atwater";
        const string CARDS_DOC_ID = "100TgRUnJ8_t9myFjorPp9p_enMh3LG6ZPvBf8afQ--Y";
        const string REFLECT_DOC_ID = "11IkbHj2FHUrDmRbG-JX-nHl2KzbCTdYNx6mgT1XhZTY";
        const string ENUM_PATH = "Assets/Scripts/GameplaySystems/DialogueTag.cs";

        static readonly Regex BoldStrip = new Regex(@"\*\*(.+?)\*\*");

        [MenuItem("Crush/Sync Dialogue Tags from Docs")]
        public static void SyncTags()
        {
            try
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                // raw tag candidates spotted in the docs (case-insensitive dedup)
                var found = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                Debug.Log("[TagSync] Scanning Cards doc…");
                ScanDoc(CARDS_DOC_ID, scanStateHeaders: true, found);
                Debug.Log("[TagSync] Scanning Reflect doc…");
                ScanDoc(REFLECT_DOC_ID, scanStateHeaders: false, found);

                // what's already in the enum
                var existing = new HashSet<string>(Enum.GetNames(typeof(DialogueTag)), StringComparer.OrdinalIgnoreCase);

                var toAdd = new List<string>();
                var addedSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                foreach (var raw in found)
                {
                    if (!IsValidIdentifier(raw))
                    {
                        Debug.LogWarning($"[TagSync] Skipping '{raw}' — not a valid tag (needs to be a single word: letters/digits/underscore, no leading digit).");
                        continue;
                    }
                    var norm = Normalize(raw);
                    if (existing.Contains(norm) || addedSet.Contains(norm)) continue;
                    toAdd.Add(norm);
                    addedSet.Add(norm);
                }

                if (toAdd.Count == 0)
                {
                    Debug.Log("[TagSync] No new tags — DialogueTag is already up to date.");
                    EditorUtility.DisplayDialog("Sync Dialogue Tags", "No new tags found. DialogueTag is up to date.", "OK");
                    return;
                }

                AppendTagsToEnum(toAdd);
                AssetDatabase.Refresh();
                var msg = $"Added {toAdd.Count} new tag(s): {string.Join(", ", toAdd)}.\n\nUnity is recompiling. Once it's done, run your content importers to wire them up.";
                Debug.Log($"[TagSync] {msg}");
                EditorUtility.DisplayDialog("Sync Dialogue Tags", msg, "OK");
            }
            catch (Exception e)
            {
                Debug.LogError($"[TagSync] Failed: {e.Message}\n{e.StackTrace}");
            }
        }

        // ── doc scan ───────────────────────────────────────────────

        static void ScanDoc(string docId, bool scanStateHeaders, HashSet<string> found)
        {
            foreach (var tab in ListTabs(docId))
            {
                var content = FetchTab(docId, tab.id);
                if (content.structure == null) continue;

                foreach (var item in content.structure)
                {
                    var type = (item.type ?? "").ToLowerInvariant();
                    var text = StripBold(item.text ?? "").Trim();
                    if (text.Length == 0) continue;

                    // milestone trigger: "<Tag> tag fires"
                    var trig = Regex.Match(text, @"^(.+?)\s+tag fires$", RegexOptions.IgnoreCase);
                    if (trig.Success) found.Add(trig.Groups[1].Value.Trim());

                    // [tag] markers — only on branch H1 headers and (cards only) Daisy state headers.
                    // Dialogue lines (where [1st]/[last]/[+N conf] live) are never scanned for brackets.
                    bool isStateHeader = scanStateHeaders &&
                        Regex.IsMatch(text, @"^(death|low|neutral|positive|high)\b", RegexOptions.IgnoreCase);
                    if (type == "heading1" || isStateHeader)
                    {
                        foreach (Match m in Regex.Matches(text, @"\[([^\]]+)\]"))
                            foreach (var piece in m.Groups[1].Value.Split(','))
                            {
                                var t = piece.Trim();
                                if (t.Length > 0) found.Add(t);
                            }
                    }
                }
            }
        }

        // ── enum file edit (append-only — never reorder; serialized data is the int index) ──

        static void AppendTagsToEnum(List<string> tags)
        {
            if (!File.Exists(ENUM_PATH))
                throw new Exception($"Enum file not found at {ENUM_PATH}");

            var lines = new List<string>(File.ReadAllLines(ENUM_PATH));
            int closeIdx = lines.FindLastIndex(l => l.Trim() == "}");
            if (closeIdx < 0)
                throw new Exception($"Couldn't find the enum's closing brace in {ENUM_PATH}");

            var insert = new List<string>();
            foreach (var t in tags) insert.Add("    " + t + ",");
            lines.InsertRange(closeIdx, insert);   // before the closing brace = append to the enum

            File.WriteAllText(ENUM_PATH, string.Join("\n", lines) + "\n");
        }

        // ── helpers ────────────────────────────────────────────────

        static string StripBold(string s) => BoldStrip.Replace(s ?? "", m => m.Groups[1].Value).Trim();
        static bool IsValidIdentifier(string s) => Regex.IsMatch(s ?? "", @"^[A-Za-z_][A-Za-z0-9_]*$");
        static string Normalize(string s) => char.ToUpper(s[0]) + s.Substring(1);

        // ── doc fetch (same pipeline the content importers use) ─────

        static string FetchUrl(string url)
        {
            using (var client = new WebClient())
            {
                client.Encoding = System.Text.Encoding.UTF8;
                return client.DownloadString(url);
            }
        }

        static List<TabRef> ListTabs(string docId)
        {
            var raw = FetchUrl($"{API_URL}?key={API_KEY}&action=list_tabs&doc_id={docId}");
            var resp = JsonUtility.FromJson<ListTabsResponse>(raw);
            if (!resp.success) throw new Exception($"list_tabs failed: {raw}");
            return resp.tabs != null ? new List<TabRef>(resp.tabs) : new List<TabRef>();
        }

        static TabContent FetchTab(string docId, string tabId)
        {
            var raw = FetchUrl($"{API_URL}?key={API_KEY}&action=get_tab&doc_id={docId}&tab_id={tabId}");
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
