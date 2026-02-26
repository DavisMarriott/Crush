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
    
    [Header("Luke's Lines (confidence-based)")]
    [SerializeField] private DialogueBranch[] lukeBranches;
    
    [Header("Charm Impact Per State")]
    [SerializeField] private CharmImpactEntry[] charmImpacts;

    [Header("Daisy's Response (charm-based)")]
    [SerializeField] private DialogueBranch[] daisyBranches;

    public DialogueBranch[] LukeBranches => lukeBranches;
    public CharmImpactEntry[] CharmImpacts => charmImpacts;
    public DialogueBranch[] DaisyBranches => daisyBranches;

    [System.Serializable]
    public class CharmImpactEntry
    {
        public CharmState state;
        public int impact;
    }

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
        Positive,   // 6-7
        High        // 8-10
    }

    public static void GetCharmRange(CharmState state, out int min, out int max)
    {
        switch (state)
        {
            case CharmState.Death:    min = 0;  max = 0;  break;
            case CharmState.Low:      min = 1;  max = 2;  break;
            case CharmState.Neutral:  min = 3;  max = 5;  break;
            case CharmState.Positive: min = 6;  max = 7;  break;
            case CharmState.High:     min = 8;  max = 10; break;
            default:                  min = 0;  max = 10; break;
        }
    }

    [System.Serializable]
    public class DialogueBranch
    {
        public string branchName;
        public int minValue;
        public int maxValue;
        public bool requiresIntroFalse = false;
        public CharmState charmState;
        public DialogueLine[] dialogue = new DialogueLine[0];
    }

    public DialogueBranch GetLukeBranch(int confidence, bool introMade)
    {
        if (lukeBranches == null || lukeBranches.Length == 0)
            return null;
        
        for (int i = 0; i < lukeBranches.Length; i++)
        {
            var b = lukeBranches[i];
            
            bool aboveMin = confidence >= b.minValue;
            bool belowMax = confidence <= b.maxValue || b.maxValue < 0;
            bool introValid = !b.requiresIntroFalse || !introMade;

            if (aboveMin && belowMax && introValid)
                return b;
        }
        return null;
    }

    public DialogueBranch GetDaisyBranch(int charm)
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
