using UnityEngine;

[CreateAssetMenu(menuName = "Crush/Card Upgrade")]
public class DialogueCardUpgrade : ScriptableObject
{
    public int costDelta;
    public DialogueCard.DialogueBranch[] branchOverrides;
    public DialogueCard.DialogueBranch[] branchAdditions;
    public string previewTextOverride;


}
