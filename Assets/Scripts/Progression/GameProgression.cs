using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class GameProgression : MonoBehaviour
{
    public int loopCount = 1;
    [SerializeField] private HallwaySelfTalk hallwaySelfTalk;
    [SerializeField] private GameObject firstLoopManagerObject;
    [SerializeField] private TMP_Text selfTalkText;
    [SerializeField] ConfidenceState confidenceState;
    [SerializeField] private GameObject specialLoopConditions;
    [SerializeField] private FirstLoopManager firstLoopManager;
    [SerializeField] private Collider2D inConversationTrigger;
    [SerializeField] private AnimationTriggerThoughtBubble animationTriggerThoughtBubble;
    [SerializeField] private AnimationTriggerPlayer animationTriggerPlayer;
    [SerializeField] private DialogueTiming dialogueTiming;
    [SerializeField] private ReflectSelfTalk reflectSelfTalk;
    [Header("Loop 1 DANCE draft (86ba2uadt) — wire DraftUI + the DANCE card + ThoughtSpawner, and set startingDeck size 0")]
    [SerializeField] private DraftUI draftUI;
    [SerializeField] private DialogueCard danceCard;
    [SerializeField] private ThoughtSpawner thoughtSpawner;
    [Header("Loop 1 reflect — populated by the doc importer (ReflectImporter creates/updates LoopReflect_01.asset and auto-wires it here).")]
    [SerializeField] private ReflectBranch loop1ReflectBranch;
    public ReflectBranch Loop1ReflectBranch { get { return loop1ReflectBranch; } set { loop1ReflectBranch = value; } }
    public LoopSnapshot lastLoop;

    // Tracks how many triggers (counting from 1 upward) have had their drain removed
    // by the RemoveApproachTriggerDrain character upgrade. e.g. 2 = triggers 1 and 2 skip drain.
    // Persists across loops within a run (set once by MilestoneTracker, never reset).
    private int approachDrainDisabledCount;

    private void Start()
    {
        // Game starts in Reflect phase on loop 1 (per Narrative Bible's loop 1 opening monologue).
        // Loop 2+ never hit this path — Start() only runs once per scene load.
        StartCoroutine(StartGameOnReflect());
    }

    // Initial scene-load entry: play loop 1 reflect lines, then transition into the normal Hallway loop.
    // Distinct from DeathRespawn's reflect handling (which uses LoopSnapshot-driven branch selection).
    // Here loop 1 reflect is scripted, not branch-selected, so we use ReflectSelfTalk.PlayLines directly.
    private IEnumerator StartGameOnReflect()
    {
        // Wait one frame so every other component's Start() has completed —
        // otherwise AnimationTriggerThoughtBubble.thoughtBubbleAnimator (assigned in its Start)
        // and ReflectDraftCameraController's PhaseManager subscription (also in Start) can race
        // against this coroutine firing too early.
        yield return null;

        // Transition to Reflect — fires OnPhaseChanged so the camera controller cuts to DraftScreenCam
        // and other phase-aware systems init correctly.
        PhaseManager.Instance.TransitionTo(GamePhase.Reflect);

        // Play the scripted loop 1 reflect lines using the same PlayLines overload that milestones use.
        // Bypasses branch selection (no LoopSnapshot needed on game start — no prior loop exists).
        if (loop1ReflectBranch != null && loop1ReflectBranch.lines != null && loop1ReflectBranch.lines.Length > 0)
            yield return reflectSelfTalk.PlayLines(loop1ReflectBranch.lines);

        // Loop-1 DANCE draft — DANCE is the only card offered (per design: DANCE is drafted, not pre-owned).
        // Null-guarded: until draftUI + danceCard are wired (and DANCE removed from startingDeck), this is
        // skipped and loop 1 stays reflect → commit → hallway. With it wired: reflect → DANCE draft → commit → hallway.
        if (draftUI != null && danceCard != null)
        {
            PhaseManager.Instance.TransitionTo(GamePhase.UpgradeDraft);
            draftUI.ShowSingleCardDraft(danceCard);
            yield return new WaitUntil(() => !draftUI.gameObject.activeSelf || !draftUI.enabled);
            // Wait for DANCE's draft self-talk to finish before the commit lines, so they don't overlap.
            yield return new WaitUntil(() => !hallwaySelfTalk.draftLinesActive);
            // Render the just-drafted DANCE into the hand (the pick's ResetDeck already drew it).
            if (thoughtSpawner != null) thoughtSpawner.SpawnButtons();
        }

        // Commit lines — final hype-up before committing to the hallway approach (locker-close beat).
        // Plays after the (optional) DANCE draft. Flow: reflect → [DANCE draft] → commit → hallway.
        if (loop1ReflectBranch != null && loop1ReflectBranch.commitLines != null && loop1ReflectBranch.commitLines.Length > 0)
            yield return reflectSelfTalk.PlayLines(loop1ReflectBranch.commitLines);

       
        // Now transition to Hallway and run the normal loop setup (BasicLoop + FirstLoopActive).
        // Trigger LockerClose Animation here //
        animationTriggerPlayer.LockerClose();
        Invoke(nameof(TransitionToHallway), 2.5f);
        // PhaseManager.Instance.TransitionTo(GamePhase.Hallway);
        SetLoopConditions();
    }

    public void TransitionToHallway()
    {
        PhaseManager.Instance.TransitionTo(GamePhase.Hallway);
    }
    
    public void SetLoopConditions()
    {
        BasicLoop();

        // Hallway triggers (drain + self-talk) now fire on every loop.
        // The firstLoopManagerObject holds the trigger GameObjects + FirstLoopManager line data.
        // (Name is legacy from when this was first-loop-only.)
        firstLoopManagerObject.SetActive(true);
    }
    
    public void NextLoop()
    {
        loopCount++;
    }
    
    //turns off all special loop conditions
    private void BasicLoop()
    {
        hallwaySelfTalk.enabled = false; // timer system on ice — triggers handle hallway beats now
        inConversationTrigger.enabled = true;
        confidenceState.inConversation = false;
        // (removed redundant EndHallwayTimer() — HallwaySelfTalk's OnEnable / OnDisable /
        //  OnPhaseChanged lifecycle now handles timer cleanup and re-start.
        //  Calling End here was killing the timer right after OnEnable started it.)
        for (int i = specialLoopConditions.transform.childCount - 1; i >= 0; i--)
            specialLoopConditions.transform.GetChild(i).gameObject.SetActive(false);
    }
    
    public void FirstLoopActive()
    {
        hallwaySelfTalk.enabled = false;
        firstLoopManagerObject.SetActive(true);
    }
    
    /// <summary>
    /// Called by MilestoneTracker when the RemoveApproachTriggerDrain upgrade is applied.
    /// Disables drain for the first N triggers (e.g. count=2 disables triggers 1 and 2).
    /// </summary>
    public void DisableApproachDrain(int count)
    {
        approachDrainDisabledCount = count;
    }

    public void HallwayTriggerHit(int triggerIndex)
    {
        // Bring the thought bubble to Full so the trigger line is visible
        animationTriggerThoughtBubble.ThoughtBubbleOn();

        // Per-loop priority: if this loop has a scripted LoopHallway, fire its line for this
        // trigger zone. Otherwise it's a "base loop" — pull a random line from the generic pool.
        // (Replaces the old timer-driven random self-talk; same "no script → random line" behavior.)
        string line = null;
        LoopHallway loopHallway = firstLoopManager.GetForLoop(loopCount);
        if (loopHallway != null && loopHallway.triggerLines != null
            && triggerIndex >= 1 && triggerIndex <= loopHallway.triggerLines.Length)
        {
            line = loopHallway.triggerLines[triggerIndex - 1];
        }
        else if (firstLoopManager.genericPool != null)
        {
            line = firstLoopManager.genericPool.GetRandomLine();
        }

        if (line != null)
        {
            dialogueTiming.Run(line, selfTalkText);

            // Only drain if this trigger hasn't been disabled by the upgrade.
            // approachDrainDisabledCount of 2 means triggers 1 and 2 skip drain.
            if (triggerIndex > approachDrainDisabledCount)
                confidenceState.confidence -= 1;
        }
    }

    //this is our "trigger end game" success state. Need to build content
    public void AskedToDance()
    {
        Debug.Log("YOU DID IT!");
    }
}
