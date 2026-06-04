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
    [SerializeField] private float maxRevealWait = 3f;   // safety cap so the reveal-wait below can't hang
    [SerializeField] private float minHoldAfterReveal = 0.3f;  // extra hold after the reveal flag fires - catches cases where the EnableSelfTalk anim event fires before the bubble is visually open
    [SerializeField] private SelfTalkManager selfTalkManager;   // reveal signal — EnableSelfTalk sets its flag

    public IEnumerator Play(LoopSnapshot snapshot, int loopCount)
    {
        ReflectBranch chosen = SelectBranch(snapshot, loopCount);
        if (chosen == null)
        {
            // no reflect content this loop - skip straight to draft, don't park on a blank bubble
            Debug.Log("[ReflectSelfTalk] no matching branch — skipping reflect");
            yield break;
        }

        yield return PlayLines(chosen.lines);
    }

    /// <summary>
    /// Find a scripted (loop-keyed) reflect branch matching the given loop number.
    /// Used by DeathRespawn to prioritize the narrative-spine scripted reflects
    /// over milestone overrides and conditional branches.
    /// Returns null if no scripted branch matches this loop.
    /// </summary>
    public ReflectBranch FindScriptedBranchForLoop(int loopCount)
    {
        if (branches == null) return null;
        foreach (var branch in branches)
        {
            if (branch == null) continue;
            if (!branch.isScripted) continue;
            if (loopCount < branch.minLoop || loopCount > branch.maxLoop) continue;
            return branch;
        }
        return null;
    }

    //Plays lines once correct branch is passed in
    public IEnumerator PlayLines(string[] lines)
    {
        if (lines == null || lines.Length == 0)
        {
            // no lines - fall through immediately, no blank hold
            yield break;
        }

        // Loop that waits for EnableSelfTalk on ThoughtBubble to start PlayLines
        float waited = 0f;
        while (selfTalkManager != null && !selfTalkManager.SelfTalkRevealed && waited < maxRevealWait)
        {
            animationTriggerThoughtBubble.ThoughtBubbleOn();
            waited += Time.deltaTime;
            yield return null;
        }

        //extra hold once revealed - on some loops (loop 2 "What??") the EnableSelfTalk event fires
        //before the bubble is fully open visually, so short lines beat the open animation otherwise
        if (selfTalkManager != null && selfTalkManager.SelfTalkRevealed && minHoldAfterReveal > 0f)
            yield return new WaitForSeconds(minHoldAfterReveal);

        foreach (string line in lines)
        {
            animationTriggerThoughtBubble.ThoughtBubbleOn();
            yield return dialogueTiming.Run(line, selfTalkText);
            // manual advance (space/click) - reflect + commit lines now wait on the player like convo
            yield return new WaitUntil(() => DialogueAdvance.Pressed());
            selfTalkText.text = "";
        }

        // Reflect lines done — minimize bubble before draft / hallway resumes
        // animationTriggerThoughtBubble.ThoughtBubbleHalf();
    }

    private ReflectBranch SelectBranch(LoopSnapshot snapshot, int loopCount)
    {
        if (branches == null) return null;
        foreach (var branch in branches)
        {
            // skip empty inspector slots - same guard FindScriptedBranchForLoop has.
            // Without it a null element NREs inside Matches and kills the reflect coroutine (the loop-7 freeze).
            if (branch == null) continue;
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
                if (required != null && !snapshot.cardsPlayed.Contains(required.name)) return false;
        }
        if (branch.requiresCardsUnplayed != null)
        {
            foreach (var required in branch.requiresCardsUnplayed)
                if (required != null && !snapshot.cardsUnplayed.Contains(required.name)) return false;
        }

        return true;
    }
}