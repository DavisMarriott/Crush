using UnityEngine;

// Special card type for DANCE. DANCE accumulates one-off behaviors (the scripted first-loop
// "freeze" beat, a "confident" ask when confidence is comfortably high, and more to come), so it
// gets its own type instead of piling Dance-only flags/conditions onto the base DialogueCard.
[CreateAssetMenu(menuName = "Crush/Dance Card")]
public class DanceCard : DialogueCard
{
    [Header("DanceCard special branch selection")]
    [Tooltip("Luke branch played on loop 1 — the scripted first-encounter freeze. Author a branch with this name on the card.")]
    [SerializeField] private string firstLoopBranchName = "FirstLoop";

    [Tooltip("Luke branch played when confidence is comfortably above the card's drain. Author a branch with this name on the card.")]
    [SerializeField] private string confidentBranchName = "Confident";

    [Tooltip("Confidence must EXCEED this to use the 'confident' Dance dialogue. Placeholder rule (set by Davis): real intent is >5 over the card's total confidence drain.")]
    [SerializeField] private int confidentConfidenceThreshold = 17;

    // Selection priority: loop 1 (freeze) -> high confidence (confident ask) -> normal base selection.
    // Each special branch is optional: if it isn't authored yet we fall back to the base selector,
    // so DANCE behaves exactly as before until the FirstLoop / Confident branches exist.
    public override DialogueBranch GetLukeBranch(int confidence, DialogueCardUpgrade upgrade, int loopCount)
    {
        DialogueBranch[] branches = GetEffectiveLukeBranches(upgrade);

        if (loopCount == 1)
        {
            DialogueBranch firstLoop = FindBranchByName(branches, firstLoopBranchName);
            if (firstLoop != null) return firstLoop;
        }
        else if (confidence > confidentConfidenceThreshold)
        {
            DialogueBranch confident = FindBranchByName(branches, confidentBranchName);
            if (confident != null) return confident;
        }

        return base.GetLukeBranch(confidence, upgrade, loopCount);
    }
}
