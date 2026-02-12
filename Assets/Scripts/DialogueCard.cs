using UnityEngine;

[CreateAssetMenu(menuName = "Crush/Dialogue Card")]
public class DialogueCard : ScriptableObject
{
    [Header("Display text on bubble")]
    public string previewText;
    
    [Header("Button BG Color")]
    public Color buttonColor = Color.black;
    
    [Header("Card Branches")]
    [SerializeField] private DialogueBranch[] branches;
    
    public DialogueBranch[] Branches => branches;
    

    public enum DialogueCharacter
    {
        Boy,
        Girl,
        BoyInternal
    }

    [System.Serializable]
    public class DialogueBranch
    {
        public string branchName;
        public int minConfidence;
        public int maxConfidence;
        public bool requiresIntroFalse = false;
        public DialogueLine[] dialogue = new DialogueLine[0];

    }

    public DialogueBranch GetBranchForConfidence(int confidence, bool introMade)
    {
        if (branches == null || branches.Length == 0)
            return null;
        
        for (int i = 0; i < branches.Length; i++)
        {
            var b = branches[i];
            
            bool aboveMin = confidence >= b.minConfidence;
            bool belowMax = confidence <= b.maxConfidence || confidence < 0;
            
            bool introValid = !b.requiresIntroFalse || !introMade;

            if (aboveMin && belowMax && introValid)
                //higher branch in array plays
                return b;
        }
        return null;
    }
    
    
    [System.Serializable]
    public class DialogueLine
    {
        [Header("Character")] 
        public DialogueCharacter character;

        [Header("Confidence Impact (in thirds)")]
        public int confidenceImpact;

        [Header("Dialogue Line")] public string line;
    }
    
}
