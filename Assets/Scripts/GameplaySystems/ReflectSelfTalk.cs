using System.Collections;
using System.Collections.Generic;
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
    [SerializeField] private BaseReflectPools basePools;        // base-loop formula pools (generic death / draft intro / commit)

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

    // ============================================================
    // Base-loop reflect formula beats (2026-06-05 design, 86baajqwe)
    // ============================================================

    // Beat 1 — reflect on death. Card+branch-specific reaction first (in authored order,
    // per-run exhaustion tracked on GameProgression); else a random generic-pool reaction.
    public IEnumerator PlayDeathReactionBeat(LoopSnapshot snapshot, GameProgression progression)
    {
        var card = snapshot.deathCard;
        string branchName = snapshot.deathBranchName;

        if (card != null && !string.IsNullOrEmpty(branchName) && card.LukeBranches != null)
        {
            foreach (var b in card.LukeBranches)
            {
                if (b == null || b.branchName != branchName) continue;
                if (b.deathReactions == null || b.deathReactions.Length == 0) break;

                int used = progression.GetDeathReactionUse(card.name, branchName);
                if (used < b.deathReactions.Length)
                {
                    progression.NoteDeathReactionUsed(card.name, branchName);
                    yield return PlayLines(b.deathReactions[used].lines);
                    yield break;
                }
                break; // exhausted - fall through to the generic pool
            }
        }

        // No card-specific reaction left. If you've died 2+ times on this exact card+branch this
        // run, prefer the repeat-death pool ("doing the same thing and expecting different results");
        // otherwise the generic pool.
        if (basePools != null)
        {
            bool repeat = card != null && !string.IsNullOrEmpty(branchName)
                          && progression.GetComboDeaths(card.name, branchName) >= 2;

            var pool = (repeat && basePools.repeatDeathReactions != null && basePools.repeatDeathReactions.Length > 0)
                ? basePools.repeatDeathReactions
                : basePools.genericDeathReactions;

            if (pool != null && pool.Length > 0)
                yield return PlayLines(pool[Random.Range(0, pool.Length)].lines);
        }
        // no pool authored yet - beat just skips
    }

    // Beat 2 fallback — random among ALL eligible conditional branches (instead of first-match),
    // so repeated similar deaths don't always read the same conditional. Scripted branches excluded.
    public IEnumerator PlayRandomEligible(LoopSnapshot snapshot, int loopCount)
    {
        var eligible = new List<ReflectBranch>();
        if (branches != null)
        {
            foreach (var branch in branches)
            {
                if (branch == null || branch.isScripted) continue;
                if (Matches(branch, snapshot, loopCount)) eligible.Add(branch);
            }
        }

        if (eligible.Count == 0)
        {
            Debug.Log("[ReflectSelfTalk] no eligible conditional — skipping progress beat");
            yield break;
        }

        yield return PlayLines(eligible[Random.Range(0, eligible.Count)].lines);
    }

    // Beat 3 — a random eligible draft-intro group right before the draft opens.
    public IEnumerator PlayDraftIntro(int loopCount)
    {
        var g = PickEligiblePoolGroup(basePools != null ? basePools.draftIntroPools : null, loopCount);
        if (g != null) yield return PlayLines(g.lines);
    }

    // Beat 4 — a random eligible commit group for base loops (scripted loops keep their own commitLines).
    public IEnumerator PlayBaseCommit(int loopCount)
    {
        var g = PickEligiblePoolGroup(basePools != null ? basePools.commitPools : null, loopCount);
        if (g != null) yield return PlayLines(g.lines);
    }

    // pool every group from entries whose loop range includes loopCount, pick one at random
    private ReflectLineGroup PickEligiblePoolGroup(ConditionalReflectGroups[] pools, int loopCount)
    {
        if (pools == null) return null;
        var eligible = new List<ReflectLineGroup>();
        foreach (var p in pools)
        {
            if (p == null || p.groups == null) continue;
            if (loopCount < p.minLoop || loopCount > p.maxLoop) continue;
            foreach (var g in p.groups)
                if (g != null && g.lines != null && g.lines.Length > 0) eligible.Add(g);
        }
        return eligible.Count > 0 ? eligible[Random.Range(0, eligible.Count)] : null;
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