using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Cinemachine;

public class DeathRespawn : MonoBehaviour
{
    //Inspector Variables
    [SerializeField] private float deathScreenTimer;
    [SerializeField] private GameProgression gameProgression;
    [SerializeField] private ConfidenceState confidenceState;
    [SerializeField] private CharmState charmState;
    [SerializeField] private GameObject deathScreen;
    [SerializeField] private AnimationTriggerPlayer animationTriggerPlayer;
    [SerializeField] private AnimationTriggerPlayer animationTriggerPlayerDraft;
    [SerializeField] private DeckManager deckManager;
    [SerializeField] private DialogueBox dialogueBox;
    [SerializeField] private GameObject cardContainer;
    [SerializeField] private Transform playerTransform;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private DraftUI draftUI;
    [SerializeField] private Collider2D inConversationTrigger;
    [SerializeField] ThoughtSpawner thoughtSpawner;
    [SerializeField] HallwaySelfTalk hallwaySelfTalk;
    [SerializeField] private float reflectDuration;
    [SerializeField] private Animator letterBoxAnimator;
    [SerializeField] private ReflectSelfTalk reflectSelfTalk;
    [SerializeField] private MilestoneTracker milestoneTracker;

    [Tooltip("TestMode only - nudge Luke's landing spot left(-)/right(+) if the computed stop point looks off.")]
    [SerializeField] private float testModeConvoXOffset = 0f;
    
    //variables hidden in inspector
    [HideInInspector] public bool isDead = false;

    // Update is called once per frame
    public void Update()
    {
        //narrow defer: only wait if the dialogue is actively playing a Death branch.
        //For non-Death branches (or no dialogue at all) Death fires immediately.
        bool deathBranchPlaying = dialogueBox != null && dialogueBox.IsPlayingDeathBranch;
        if (confidenceState.confidence <= 0 && !isDead && !deathBranchPlaying)
        {
            StartCoroutine(Death());
        }
    }
    
    //currently does much more than death. Handles most of respawn/reflect phase as well. Might break up in teh future.
    public IEnumerator Death()
    {
        PhaseManager.Instance.TransitionTo(GamePhase.Death);
        isDead = true;
        //safe now that DeathRespawn.Update defers on IsPlayingDeathBranch - Death() only fires here
        //after any Death-branch dialogue has reached its natural end (or it was never running).
        dialogueBox.CloseDialogueBox();
        
        //snapshot for branch tracking
        LoopSnapshot loopSnapshot = new LoopSnapshot();
        loopSnapshot.finalConfidence = confidenceState.confidence;
        loopSnapshot.finalCharm = charmState.charm;
        loopSnapshot.lastPeakConfidence = confidenceState.peakConfidence;
        loopSnapshot.lastPeakCharm = charmState.peakCharm;
        if (charmState.charm <= 0) loopSnapshot.deathFromCharm = true;
        loopSnapshot.cardsPlayed = new List<string>();
        foreach (DialogueCard card in deckManager.Discard)
        {
            loopSnapshot.cardsPlayed.Add(card.name);
        }
        loopSnapshot.cardsUnplayed = new List<string>();
        foreach (DialogueCard card in deckManager.Hand)
        {
            loopSnapshot.cardsUnplayed.Add(card.name);
        }
        loopSnapshot.deathCard = deckManager.LastPlayedCard;
        // Snapshot the tags fired during this loop's conversation(s)
        loopSnapshot.tagsFired = new HashSet<DialogueTag>(deckManager.TagsFiredThisLoop);
        gameProgression.NoteTagsFired(loopSnapshot.tagsFired);   // accumulate into the run-level set (Progress-Gated unlocks)
        // branch log + the branch you died on (last branch that fired) - drives death reactions.
        // Must copy BEFORE NextLoop() below, which clears the live log.
        loopSnapshot.branchesPlayed = new List<CardBranchRecord>(gameProgression.BranchLog);
        loopSnapshot.deathBranchName = loopSnapshot.branchesPlayed.Count > 0
            ? loopSnapshot.branchesPlayed[loopSnapshot.branchesPlayed.Count - 1].branchName
            : null;
        // bump the per-run death count for this card+branch combo (drives the repeatDeaths pool).
        // Counted here so the reflect beat below sees the up-to-date count incl. this death.
        if (loopSnapshot.deathCard != null && !string.IsNullOrEmpty(loopSnapshot.deathBranchName))
            gameProgression.NoteComboDeath(loopSnapshot.deathCard.name, loopSnapshot.deathBranchName);
        //end of updating loopSnapshot: Next - game progression captures it
        gameProgression.lastLoop = loopSnapshot;
        
        //resume death sequence
        inConversationTrigger.enabled = false;
        animationTriggerPlayer.EnterDeathOne();
        animationTriggerPlayer.ParticlesNervousStateTurnOff();
        yield return new WaitForSeconds(2f);
        deathScreen.SetActive(true);
        gameProgression.NextLoop();
        confidenceState.introMade = false;
        confidenceState.daisyIntroMade = false;
        cardContainer.SetActive(false);
        yield return new WaitForSeconds(deathScreenTimer);
        deathScreen.SetActive(false);
        

        if (TestMode.Active)
        {
            // test mode: skip reflect/draft/hallway entirely - straight back into the convo.
            // Milestones/upgrades deliberately skipped.
            TestModeRestartToConversation();
            yield break;
        }
        
       
        
        // first half of respawn + drafting - ENTER Reflect Phase
        PhaseManager.Instance.TransitionTo(GamePhase.Reflect);
        // letterBoxAnimator.SetTrigger("LetterBoxIn"); — removed 2026-05-12: dedicated DraftScreenCam now handles the Reflect/Draft visual cut, no letterboxing needed.
        animationTriggerPlayer.EnterStart();
        playerTransform.position = spawnPoint.position;
        charmState.ResetCharm();
        deckManager.ResetDeck();
        confidenceState.peakConfidence = 0;
        charmState.peakCharm = 0;
        // Reflect priority (per design 2026-05-19):
        //   1. Scripted loop branch (isScripted == true, loop-matched)  — the narrative spine, always wins
        //   2. Milestone override                                       — replaces conditional fallback
        //   3. Conditional / legacy ReflectBranch                       — picked by Play()'s SelectBranch
        // TODO (future task): scripted-vs-milestone overlap. When both match the same loop,
        // the milestone's reflect lines are SKIPPED but the upgrade still applies + marks complete.
        // Davis flagged this for the first 4–5 loops (narrative intro) — we need both to play somehow.
        // Probably: chain them (scripted then milestone, or vice-versa), or queue the milestone for
        // the next non-scripted loop. Follow-up to the reflect import work.
        ReflectBranch scriptedBranch = reflectSelfTalk.FindScriptedBranchForLoop(gameProgression.loopCount);
        Milestone triggeredMilestone = milestoneTracker != null
            ? milestoneTracker.GetTriggeredMilestone(gameProgression.lastLoop, gameProgression.loopCount)
            : null;
        
        //if statement to check for correct reflect branch
        if (scriptedBranch != null)
        {
            // SCRIPTED LOOP: plays ONLY its scripted lines - no death reactions, no milestone
            // text, no pools (2026-06-05 design, 86baajqwe). Skipped in skip-reflect mode (N key).
            if (!SkipReflect.Active)
                yield return reflectSelfTalk.PlayLines(scriptedBranch.lines);

            // Milestone overlap fallback — still apply the upgrade + mark complete so progression
            // doesn't break, even though the milestone's reflect lines didn't play this loop.
            if (triggeredMilestone != null)
            {
                Debug.Log($"[Reflect] Scripted (loop {gameProgression.loopCount}) overrode milestone '{triggeredMilestone.milestoneName}' reflect lines. Upgrade still applied — see TODO in DeathRespawn for overlap design.");
                if (triggeredMilestone.characterUpgrade != null)
                {
                    milestoneTracker.ApplyCharacterUpgrade(triggeredMilestone.characterUpgrade);
                    confidenceState.confidence = confidenceState.startingConfidence;
                    deckManager.ResetDeck();
                    if (!SkipReflect.Active)
                        yield return StartCoroutine(milestoneTracker.PlayUpgradeReveal(triggeredMilestone.characterUpgrade));
                }
                milestoneTracker.MarkComplete(triggeredMilestone);
            }
        }
        else
        {
            // BASE LOOP: the four-beat reflect formula (beats 3+4 live below, around the draft).
            // Beat 1 — reflect on death: card+branch-specific reaction, else generic pool.
            if (!SkipReflect.Active)
                yield return reflectSelfTalk.PlayDeathReactionBeat(gameProgression.lastLoop, gameProgression);

            // Beat 2 — comment on progress: milestone text if one fired, else a random
            // eligible conditional. Death reaction + milestone CAN both play in one loop (by design).
            if (triggeredMilestone != null)
            {
                if (!SkipReflect.Active)
                    yield return reflectSelfTalk.PlayLines(triggeredMilestone.reflectLines);
                if (triggeredMilestone.characterUpgrade != null)
                {
                    milestoneTracker.ApplyCharacterUpgrade(triggeredMilestone.characterUpgrade);
                    // Re-apply respawn state so an upgrade that bumped startingConfidence
                    // or startingHandSize is felt immediately on this respawn (not the next one).
                    confidenceState.confidence = confidenceState.startingConfidence;
                    deckManager.ResetDeck();
                    if (!SkipReflect.Active)
                        yield return StartCoroutine(milestoneTracker.PlayUpgradeReveal(triggeredMilestone.characterUpgrade));
                }
                milestoneTracker.MarkComplete(triggeredMilestone);
            }
            else if (!SkipReflect.Active)
            {
                yield return reflectSelfTalk.PlayRandomEligible(gameProgression.lastLoop, gameProgression.loopCount);
            }
        }

        // Beat 3 — draft intro (base loops only): one random "what should I say this time…" line
        // before the draft opens. Scripted loops stay scripted-only.
        if (scriptedBranch == null && !SkipReflect.Active && deckManager.HasDraftPool())
            yield return reflectSelfTalk.PlayDraftIntro(gameProgression.loopCount);

        if (deckManager.HasDraftPool())
        {
            PhaseManager.Instance.TransitionTo(GamePhase.UpgradeDraft);
            draftUI.ShowDraftOptions(gameProgression.loopCount);
            yield return new WaitUntil(() => !draftUI.gameObject.activeSelf || !draftUI.enabled);
        }

        // Wait for draft self-talk lines to finish typing BEFORE transitioning to Hallway —
        // otherwise the camera cuts back to HallCam mid-line, leaving the text playing in
        // the screen-space temp object over the wrong camera shot.
        yield return new WaitUntil(() => (!hallwaySelfTalk.draftLinesActive));

        // Commit lines — the loop's final hype-up before committing to the hallway approach
        // (the locker-close beat). Plays AFTER the draft phase, while the camera is still on the
        // reflect/draft close-up, then we cut to Hallway. Flow: reflect → draft → commit → hallway.
        if (!SkipReflect.Active && scriptedBranch != null && scriptedBranch.commitLines != null && scriptedBranch.commitLines.Length > 0)
            yield return reflectSelfTalk.PlayLines(scriptedBranch.commitLines);
        // Beat 4 — base loops pull a random commit group from the pool instead
        else if (!SkipReflect.Active && scriptedBranch == null)
            yield return reflectSelfTalk.PlayBaseCommit(gameProgression.loopCount);
        confidenceState.confidence = confidenceState.startingConfidence;

        //full respawn, ready for hallway walk
        animationTriggerPlayerDraft.LockerClose();
        Invoke(nameof(TransitionToHallway), 2.5f);
        // PhaseManager.Instance.TransitionTo(GamePhase.Hallway);
        thoughtSpawner.SpawnButtons();
        gameProgression.SetLoopConditions();
        isDead = false;
        
    }
    
    public void TransitionToHallway()
    {
        PhaseManager.Instance.TransitionTo(GamePhase.Hallway);
    }

    // TestMode - hard restart into the conversation from wherever the game currently is.
    // Called on T-activation (any phase) and after the death screen on test-mode deaths.
    // Lives here because this component already holds every ref the reset needs.
    public void TestModeRestartToConversation()
    {
        // kill whatever owns the game right now - reflect playback, draft waits, the loop-1
        // opening, mid-convo dialogue, pending hallway-transition Invokes. Restart, not transition.
        StopAllCoroutines();
        CancelInvoke();
        gameProgression.StopAllCoroutines();
        gameProgression.CancelInvoke();
        reflectSelfTalk.StopAllCoroutines();
        hallwaySelfTalk.StopAllCoroutines();
        dialogueBox.StopAllCoroutines();

        // clear leftover UI from the interrupted phase
        deathScreen.SetActive(false);
        draftUI.CloseDraftUI();
        dialogueBox.CloseDialogueBox();
        isDead = false;

        // same per-loop reset a normal respawn does (minus reflect/draft/milestones)
        animationTriggerPlayer.EnterStart();
        charmState.ResetCharm();
        deckManager.ResetDeck();
        confidenceState.peakConfidence = 0;
        charmState.peakCharm = 0;
        confidenceState.confidence = confidenceState.startingConfidence;
        confidenceState.introMade = false;
        confidenceState.daisyIntroMade = false;
        thoughtSpawner.SpawnButtons();

        // force the heart meter to match - covers T pressed before loop 1's SpawnHeartMeter
        // ever ran, and kills the slow-ramp AddHearts the confidence setter just queued
        gameProgression.confidenceHeartMeter.SyncHearts(confidenceState.confidence);

        // re-arm the convo entry: disable the trigger, put the player inside it, then
        // SetLoopConditions re-enables it -> physics refires the real entry (camera zone,
        // InConversation, Daisy's intro). Local-space math instead of bounds because the
        // collider may already be disabled here (death path) and disabled bounds lie.
        //
        // Landing spot = where a walking player ACTUALLY stops: the game locks movement the
        // frame his leading edge crosses the trigger's left boundary, so his center sits a
        // player-half-width LEFT of that edge (trigger center and edge+0.25 both parked him
        // too deep - 7/1 playtests). Tiny overlap kept so the re-enabled trigger still fires.
        float leftEdge;
        if (inConversationTrigger is BoxCollider2D box)
            leftEdge = box.transform.TransformPoint(new Vector3(box.offset.x - box.size.x * 0.5f, box.offset.y, 0f)).x;
        else
            leftEdge = inConversationTrigger.transform.TransformPoint(inConversationTrigger.offset).x;
        var playerCol = playerTransform.GetComponent<Collider2D>();
        if (playerCol == null) playerCol = playerTransform.GetComponentInChildren<Collider2D>();
        float playerHalf = playerCol != null ? playerCol.bounds.extents.x : 0f;
        float convoX = leftEdge - playerHalf + 0.15f + testModeConvoXOffset;
        inConversationTrigger.enabled = false;
        playerTransform.position = new Vector3(convoX, playerTransform.position.y, playerTransform.position.z);
        gameProgression.SetLoopConditions();
        PhaseManager.Instance.TransitionTo(GamePhase.Hallway);

        // that transition just made ReflectDraftCameraController cut to HallCam - and the convo
        // camera zone won't refire if we were already standing in it. Force ConvoCam back.
        var convoCam = FindConvoCam();
        if (convoCam != null)
            CameraManager.SwitchCamera(convoCam);
        else
            Debug.LogWarning("[TestMode] Couldn't find a camera named 'ConvoCam' - camera may be stuck on HallCam");

        var rb = playerTransform.GetComponent<Rigidbody2D>();
        if (rb != null) rb.WakeUp();   // sleeping body won't re-register the trigger overlap
    }

    // name-based lookup (no serialized ref to keep TestMode wiring-free) - if the convo
    // camera ever gets renamed, update this string
    private CinemachineCamera _convoCam;
    private CinemachineCamera FindConvoCam()
    {
        if (_convoCam != null) return _convoCam;
        foreach (var cam in FindObjectsByType<CinemachineCamera>(FindObjectsSortMode.None))
            if (cam.name == "ConvoCam") { _convoCam = cam; break; }
        return _convoCam;
    }

}
