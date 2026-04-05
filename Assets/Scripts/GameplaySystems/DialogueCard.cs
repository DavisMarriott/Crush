using UnityEngine;

[CreateAssetMenu(menuName = "Crush/Dialogue Card")]
public class DialogueCard : ScriptableObject
{
    [Header("Display text on bubble")]
    public string previewText;

    [Header("Card Cost")]
    public int cost = 1;
    public bool revealed = false;

    [Header("Button BG Color")]
    public Color buttonColor = Color.black;

    [Header("Luke's Branches (confidence-based)")]
    [SerializeField] private DialogueBranch[] lukeBranches;

    public DialogueBranch[] LukeBranches => lukeBranches;

    public enum DialogueCharacter
    {
        Boy,
        Girl,
        BoyInternal
    }

    public enum CharmState
    {
        Death,      // 0
        Low,        // 1-2
        Neutral,    // 3-5
        Positive,   // 6-8
        High        // 9-10
    }

    public enum ConfidenceLevel
    {
        Low,        // 1-3
        Neutral,    // 4-6
        Positive,   // 7-9
        High        // 10+
    }


    public static void GetCharmRange(CharmState state, out int min, out int max)
    {
        switch (state)
        {
            case CharmState.Death:    min = 0;  max = 0;  break;
            case CharmState.Low:      min = 1;  max = 2;  break;
            case CharmState.Neutral:  min = 3;  max = 5;  break;
            case CharmState.Positive: min = 6;  max = 8;  break;
            case CharmState.High:     min = 9;  max = 10; break;
            default:                  min = 0;  max = 10; break;
        }
    }

    public static void GetConfidenceRange(ConfidenceLevel level, out int min, out int max)
    {
        switch (level)
        {
            case ConfidenceLevel.Low:      min = 1;  max = 3;  break;
            case ConfidenceLevel.Neutral:   min = 4;  max = 6;  break;
            case ConfidenceLevel.Positive:  min = 7;  max = 9;  break;
            case ConfidenceLevel.High:      min = 10; max = 45; break;
            default:                        min = 0;  max = 45; break;
        }
    }

    public static ConfidenceLevel GetConfidenceLevel(int confidence)
    {
        if (confidence <= 0) return ConfidenceLevel.Low;
        if (confidence <= 3) return ConfidenceLevel.Low;
        if (confidence <= 6) return ConfidenceLevel.Neutral;
        if (confidence <= 9) return ConfidenceLevel.Positive;
        return ConfidenceLevel.High;
    }


    [System.Serializable]
    public class CharmImpactEntry
    {
        public CharmState state;
        public int impact;
    }

    // separate class from DialogueBranch so Unity's serializer doesn't hit a
    // recursive depth limit (DialogueBranch used to contain DialogueBranch[])
    [System.Serializable]
    public class DaisyBranch
    {
        public CharmState charmState;
        public DialogueLine[] dialogue = new DialogueLine[0];
    }

    [System.Serializable]
    public class DialogueBranch
    {
        public string branchName;
        public bool requiresIntroFalse = false;
        public DialogueLine[] dialogue = new DialogueLine[0];

        [Header("Charm Impact Per State (for this branch)")]
        public CharmImpactEntry[] charmImpacts;

        [Header("Daisy's Response (charm-based, for this branch)")]
        public DaisyBranch[] daisyBranches;

        public int GetCharmImpact(CharmState currentState)
        {
            if (charmImpacts == null) return 0;
            for (int i = 0; i < charmImpacts.Length; i++)
            {
                if (charmImpacts[i].state == currentState)
                    return charmImpacts[i].impact;
            }
            return 0;
        }

        // find the daisy branch that matches the current charm score
        public DaisyBranch GetDaisyBranch(int charm)
        {
            if (daisyBranches == null || daisyBranches.Length == 0)
                return null;

            for (int i = 0; i < daisyBranches.Length; i++)
            {
                var b = daisyBranches[i];
                GetCharmRange(b.charmState, out int min, out int max);

                if (charm >= min && charm <= max)
                    return b;
            }
            return null;
        }
    }

    // confidence here is AFTER cost has been deducted by ThoughtSpawner
    public DialogueBranch GetLukeBranch(int confidence, bool introMade)
    {
        if (lukeBranches == null || lukeBranches.Length == 0)
            return null;

        // dead
        if (confidence <= 0)
            return FindBranchByName("Death", introMade);

        ConfidenceLevel level = GetConfidenceLevel(confidence);

        // awkward (only if the card has one)
        if (level == ConfidenceLevel.Low)
        {
            DialogueBranch awkward = FindBranchByName("Awkward", introMade);
            if (awkward != null)
                return awkward;
        }

        // normal (or fallback if no awkward)
        DialogueBranch normal = FindBranchByName("Normal", introMade);
        if (normal != null)
            return normal;

        // last resort - first branch that passes intro check
        for (int i = 0; i < lukeBranches.Length; i++)
        {
            var b = lukeBranches[i];
            bool introValid = !b.requiresIntroFalse || !introMade;
            if (introValid)
                return b;
        }
        return null;
    }

    private DialogueBranch FindBranchByName(string name, bool introMade)
    {
        for (int i = 0; i < lukeBranches.Length; i++)
        {
            var b = lukeBranches[i];
            if (b.branchName != name) continue;

            bool introValid = !b.requiresIntroFalse || !introMade;
            if (introValid)
                return b;
        }
        return null;
    }

    [System.Serializable]
    public class DialogueLine
    {
        [Header("Character")]
        public DialogueCharacter character;

        [Header("Confidence Impact")]
        public int confidenceImpact;

        [Header("Charm Impact")]
        public int charmImpact;

        [Header("Dialogue Line")]
        [TextArea(3,5)]
        public string line;
    }
}
