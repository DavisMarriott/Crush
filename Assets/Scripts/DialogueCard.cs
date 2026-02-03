using UnityEngine;

[CreateAssetMenu(menuName = "Crush/Dialogue Card")]
public class DialogueCard : ScriptableObject
{
    [Header("Display text on bubble")]
    public string previewText;
    
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
        public DialogueLine[] dialogue = new DialogueLine[0];

    }

    public DialogueBranch GetBranchForConfidence(int confidence)
    {
        if (branches == null || branches.Length == 0)
            return null;
        
        for (int i = 0; i < branches.Length; i++)
        {
            var b = branches[i];
            if (b == null) continue;
            
            bool aboveMin = confidence >= b.minConfidence;
            bool belowMax = confidence <= b.maxConfidence || confidence < 0;

            if (aboveMin && belowMax)
                return b;
        }
        return null;
    }
    
    
    [System.Serializable]
    public class DialogueLine
    {
        [Header("Character")] 
        public DialogueCharacter character;

        [Header("Dialogue Line")] public string line;
    }
    
}
