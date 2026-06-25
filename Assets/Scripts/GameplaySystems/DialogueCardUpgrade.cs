using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(menuName = "Crush/Card Upgrade")]
public class DialogueCardUpgrade : ScriptableObject
{
    public int costDelta;
    public DialogueCard.DialogueBranch[] branchOverrides;
    public DialogueCard.DialogueBranch[] branchAdditions;
    public string previewTextOverride;
    //overrides the cardprefab used once upgraded
    public Button visualPrefab;

    // self-talk lines that play when you draft THIS upgrade (same as DialogueCard.draftLines, but for the upgrade)
    public DialogueCard.DraftLine[] draftLines;
}
