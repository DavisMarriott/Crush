using System.Collections;
using UnityEngine;
using TMPro;

public class ReflectSelfTalk : MonoBehaviour
{
    [SerializeField] private DialogueTiming dialogueTiming;
    [SerializeField] public AnimationTriggerThoughtBubble animationTriggerThoughtBubble;
    [SerializeField] private TMP_Text selfTalkText;
    [SerializeField] private ReflectBranch[] branches;
    [SerializeField] private float holdAfterLine = 2f;

    public IEnumerator Play(LoopSnapshot snapshot, int loopCount)
    {
        ReflectBranch chosen = SelectBranch(snapshot, loopCount);
        if (chosen == null)
        {
            Debug.Log("[ReflectSelfTalk] no matching branch — holding blank");
            yield return new WaitForSeconds(holdAfterLine);
            yield break;
        }

        yield return PlayLines(chosen.lines);
    }

    // Plays a fixed set of lines without branch selection.
    // Used for milestone reflects that bring their own line set.
    public IEnumerator PlayLines(string[] lines)
    {
        if (lines == null || lines.Length == 0)
        {
            yield return new WaitForSeconds(holdAfterLine);
            animationTriggerThoughtBubble.ThoughtBubbleHalf();
            yield break;
        }

        foreach (string line in lines)
        {
            animationTriggerThoughtBubble.ThoughtBubbleOn();
            yield return dialogueTiming.Run(line, selfTalkText);
            yield return new WaitForSeconds(holdAfterLine);
            selfTalkText.text = "";
        }
    }

    private ReflectBranch SelectBranch(LoopSnapshot snapshot, int loopCount)
    {
        foreach (var branch in branches)
        {
            if (Matches(branch, snapshot, loopCount)) return branch;
        }
        return null;
    }

    private bool Matches(ReflectBranch branch, LoopSnapshot snapshot, int loopCount)
    {
        if (loopCount < branch.minLoop || loopCount > branch.maxLoop) return false;

        if (snapshot.finalConfidence < branch.minFinalConfidence || snapshot.finalConfidence > branch.maxFinalConfidence) return false;
        if (snapshot.lastPeakConfidence < branch.minPeakConfidence || snapshot.lastPeakConfidence > branch.maxPeakConfidence) return false;
        if (snapshot.finalCharm < branch.minFinalCharm || snapshot.finalCharm > branch.maxFinalCharm) return false;
        if (snapshot.lastPeakCharm < branch.minPeakCharm || snapshot.lastPeakCharm > branch.maxPeakCharm) return false;

        if (branch.requiresDeathCard != null && snapshot.deathCard != branch.requiresDeathCard) return false;
        if (branch.requiresDeathFromCharm && !snapshot.deathFromCharm) return false;

        if (branch.requiresCardsPlayed != null)
        {
            foreach (var required in branch.requiresCardsPlayed)
                if (!snapshot.cardsPlayed.Contains(required.name)) return false;
        }
        if (branch.requiresCardsUnplayed != null)
        {
            foreach (var required in branch.requiresCardsUnplayed)
                if (!snapshot.cardsUnplayed.Contains(required.name)) return false;
        }

        return true;
    }
}