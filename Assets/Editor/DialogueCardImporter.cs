using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

public class DialogueCardImporter : EditorWindow
{
    private string csvPath = "";

    // Adds a button to the Unity menu bar: Crush > Import Dialogue CSV
    [MenuItem("Crush/Import Dialogue CSV")]
    public static void ShowWindow()
    {
        GetWindow<DialogueCardImporter>("Dialogue Importer");
    }

    void OnGUI()
    {
        GUILayout.Label("Dialogue Card CSV Importer", EditorStyles.boldLabel);
        GUILayout.Space(10);

        // Show the current file path
        EditorGUILayout.LabelField("CSV File:", csvPath);

        // Button to pick a file
        if (GUILayout.Button("Select CSV File"))
        {
            string path = EditorUtility.OpenFilePanel("Select Dialogue CSV", "", "csv");
            if (!string.IsNullOrEmpty(path))
                csvPath = path;
        }

        GUILayout.Space(10);

        // The import button - only enabled if we have a file selected
        GUI.enabled = !string.IsNullOrEmpty(csvPath);
        if (GUILayout.Button("Import"))
        {
            ImportCSV(csvPath);
        }
        GUI.enabled = true;
    }

    private void ImportCSV(string path)
    {
        string[] lines = File.ReadAllLines(path);
        List<CardData> cards = ParseCSV(lines);

        int created = 0;
        int updated = 0;

        foreach (var card in cards)
        {
            bool isNew;
            DialogueCard asset = FindOrCreateAsset(card.cardName, out isNew);

            ApplyCardData(asset, card);

            EditorUtility.SetDirty(asset);

            if (isNew) created++;
            else updated++;
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"Dialogue import complete: {created} created, {updated} updated.");
    }

    // ─── CSV PARSING ───────────────────────────────────────────────

    private List<CardData> ParseCSV(string[] lines)
    {
        List<CardData> cards = new List<CardData>();
        CardData current = null;

        // Tracks what section we're in (Luke, CharmImpact, Daisy)
        string section = "";
        // Tracks the current branch we're adding lines to
        BranchData currentBranch = null;

        for (int i = 0; i < lines.Length; i++)
        {
            string[] cols = ParseCSVLine(lines[i]);

            // Skip completely empty rows
            if (IsEmpty(cols)) continue;

            string colA = Get(cols, 0).Trim();
            string colB = Get(cols, 1).Trim();
            string colC = Get(cols, 2).Trim();
            string colD = Get(cols, 3).Trim();
            string colE = Get(cols, 4).Trim();

            // ── Card-level data (Column A has content) ──
            if (!string.IsNullOrEmpty(colA))
            {
                switch (colA)
                {
                    case "Card Name":
                        current = new CardData { cardName = colB };
                        cards.Add(current);
                        section = "";
                        currentBranch = null;
                        break;
                    case "Preview Text":
                        if (current != null) current.previewText = colB;
                        break;
                    case "Cost":
                        if (current != null) int.TryParse(colB, out current.cost);
                        break;
                }
                continue;
            }

            if (current == null) continue;

            // ── Section/branch headers (Column B has content) ──
            if (!string.IsNullOrEmpty(colB))
            {
                if (colB == "Luke")
                {
                    section = "Luke";
                    currentBranch = new BranchData { branchName = colC };
                    current.lukeBranches.Add(currentBranch);
                    continue;
                }

                if (colB == "Daisy")
                {
                    section = "Daisy";
                    currentBranch = new BranchData { branchName = colC };
                    current.daisyBranches.Add(currentBranch);
                    continue;
                }

                if (colB == "CharmImpact")
                {
                    section = "CharmImpact";
                    currentBranch = null;

                    // colC = state name, colD = impact value
                    int impact = 0;
                    int.TryParse(colD, out impact);
                    current.charmImpacts.Add(new CharmImpactData
                    {
                        stateName = colC,
                        impact = impact
                    });
                    continue;
                }
            }

            // ── Dialogue lines (Column C has a character name) ──
            if (!string.IsNullOrEmpty(colC) && currentBranch != null)
            {
                if (colC == "Boy" || colC == "Girl" || colC == "BoyInternal")
                {
                    var line = new LineData { character = colC };

                    if (section == "Luke")
                    {
                        // Luke lines: C=character, D=text
                        line.text = colD;
                    }
                    else if (section == "Daisy")
                    {
                        // Daisy lines: C=character, D=text, E=confidence impact
                        line.text = colD;
                        if (!string.IsNullOrEmpty(colE))
                            int.TryParse(colE, out line.confidenceImpact);
                    }

                    currentBranch.lines.Add(line);
                }
            }
        }

        return cards;
    }

    // Handles CSV quoting (fields with commas wrapped in quotes)
    private string[] ParseCSVLine(string line)
    {
        List<string> fields = new List<string>();
        bool inQuotes = false;
        string field = "";

        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];

            if (c == '"')
            {
                // Handle escaped quotes ("")
                if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                {
                    field += '"';
                    i++;
                }
                else
                {
                    inQuotes = !inQuotes;
                }
            }
            else if (c == ',' && !inQuotes)
            {
                fields.Add(field);
                field = "";
            }
            else
            {
                field += c;
            }
        }
        fields.Add(field);

        return fields.ToArray();
    }

    private string Get(string[] cols, int index)
    {
        return index < cols.Length ? cols[index] : "";
    }

    private bool IsEmpty(string[] cols)
    {
        for (int i = 0; i < cols.Length; i++)
        {
            if (!string.IsNullOrWhiteSpace(cols[i])) return false;
        }
        return true;
    }

    // ─── ASSET CREATION ────────────────────────────────────────────

    private DialogueCard FindOrCreateAsset(string cardName, out bool isNew)
    {
        // Look for existing asset by name in Assets/Cards/
        string[] guids = AssetDatabase.FindAssets(cardName + " t:DialogueCard", new[] { "Assets/Cards" });

        foreach (string guid in guids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            DialogueCard existing = AssetDatabase.LoadAssetAtPath<DialogueCard>(assetPath);
            if (existing != null && existing.name == cardName)
            {
                isNew = false;
                return existing;
            }
        }

        // Create new asset
        if (!AssetDatabase.IsValidFolder("Assets/Cards"))
            AssetDatabase.CreateFolder("Assets", "Cards");

        DialogueCard card = ScriptableObject.CreateInstance<DialogueCard>();
        string newPath = $"Assets/Cards/{cardName}.asset";
        AssetDatabase.CreateAsset(card, newPath);

        isNew = true;
        return card;
    }

    private void ApplyCardData(DialogueCard asset, CardData data)
    {
        // We use SerializedObject to write to private [SerializeField] fields
        // This is the proper Unity way to modify assets from editor code
        SerializedObject so = new SerializedObject(asset);

        so.FindProperty("previewText").stringValue = data.previewText;
        so.FindProperty("cost").intValue = data.cost;

        // ── Luke branches ──
        SerializedProperty lukeProp = so.FindProperty("lukeBranches");
        lukeProp.arraySize = data.lukeBranches.Count;

        for (int i = 0; i < data.lukeBranches.Count; i++)
        {
            var branch = data.lukeBranches[i];
            SerializedProperty branchProp = lukeProp.GetArrayElementAtIndex(i);

            branchProp.FindPropertyRelative("branchName").stringValue = branch.branchName;

            // Derive requiresIntroFalse from branch name
            bool reqIntro = branch.branchName.Contains("NoIntro");
            branchProp.FindPropertyRelative("requiresIntroFalse").boolValue = reqIntro;

            // Luke branches use full confidence range by default
            branchProp.FindPropertyRelative("minValue").intValue = 0;
            branchProp.FindPropertyRelative("maxValue").intValue = 45;

            WriteBranchDialogue(branchProp, branch, false);
        }

        // ── Charm impacts ──
        SerializedProperty charmProp = so.FindProperty("charmImpacts");
        charmProp.arraySize = data.charmImpacts.Count;

        for (int i = 0; i < data.charmImpacts.Count; i++)
        {
            SerializedProperty entry = charmProp.GetArrayElementAtIndex(i);
            DialogueCard.CharmState state = ParseCharmState(data.charmImpacts[i].stateName);
            entry.FindPropertyRelative("state").enumValueIndex = (int)state;
            entry.FindPropertyRelative("impact").intValue = data.charmImpacts[i].impact;
        }

        // ── Daisy branches ──
        SerializedProperty daisyProp = so.FindProperty("daisyBranches");
        daisyProp.arraySize = data.daisyBranches.Count;

        for (int i = 0; i < data.daisyBranches.Count; i++)
        {
            var branch = data.daisyBranches[i];
            SerializedProperty branchProp = daisyProp.GetArrayElementAtIndex(i);

            branchProp.FindPropertyRelative("branchName").stringValue = branch.branchName;

            // Set charm state from branch name (Death, Low, Neutral, Positive, High)
            DialogueCard.CharmState state = ParseCharmState(branch.branchName);
            branchProp.FindPropertyRelative("charmState").enumValueIndex = (int)state;

            WriteBranchDialogue(branchProp, branch, true);
        }

        so.ApplyModifiedProperties();
    }

    private void WriteBranchDialogue(SerializedProperty branchProp, BranchData branch, bool isDaisy)
    {
        SerializedProperty dialogueProp = branchProp.FindPropertyRelative("dialogue");
        dialogueProp.arraySize = branch.lines.Count;

        for (int j = 0; j < branch.lines.Count; j++)
        {
            var line = branch.lines[j];
            SerializedProperty lineProp = dialogueProp.GetArrayElementAtIndex(j);

            // Parse character enum
            DialogueCard.DialogueCharacter character = DialogueCard.DialogueCharacter.Boy;
            switch (line.character)
            {
                case "Boy": character = DialogueCard.DialogueCharacter.Boy; break;
                case "Girl": character = DialogueCard.DialogueCharacter.Girl; break;
                case "BoyInternal": character = DialogueCard.DialogueCharacter.BoyInternal; break;
            }

            lineProp.FindPropertyRelative("character").enumValueIndex = (int)character;
            lineProp.FindPropertyRelative("line").stringValue = line.text;

            // Daisy lines can have confidence impact
            lineProp.FindPropertyRelative("confidenceImpact").intValue =
                isDaisy ? line.confidenceImpact : 0;
            lineProp.FindPropertyRelative("charmImpact").intValue = 0;
        }
    }

    private DialogueCard.CharmState ParseCharmState(string name)
    {
        switch (name)
        {
            case "Death": return DialogueCard.CharmState.Death;
            case "Low": return DialogueCard.CharmState.Low;
            case "Neutral": return DialogueCard.CharmState.Neutral;
            case "Positive": return DialogueCard.CharmState.Positive;
            case "High": return DialogueCard.CharmState.High;
            default:
                Debug.LogWarning($"Unknown charm state: '{name}', defaulting to Neutral");
                return DialogueCard.CharmState.Neutral;
        }
    }

    // ─── TEMP DATA STRUCTURES ──────────────────────────────────────
    // These only exist during import to hold parsed CSV data
    // before we write it into the ScriptableObjects

    private class CardData
    {
        public string cardName = "";
        public string previewText = "";
        public int cost = 1;
        public List<BranchData> lukeBranches = new List<BranchData>();
        public List<CharmImpactData> charmImpacts = new List<CharmImpactData>();
        public List<BranchData> daisyBranches = new List<BranchData>();
    }

    private class BranchData
    {
        public string branchName = "";
        public List<LineData> lines = new List<LineData>();
    }

    private class CharmImpactData
    {
        public string stateName = "";
        public int impact = 0;
    }

    private class LineData
    {
        public string character = "";
        public string text = "";
        public int confidenceImpact = 0;
    }
}
