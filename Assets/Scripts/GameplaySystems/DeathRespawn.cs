using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathRespawn : MonoBehaviour
{
    //Inspector Variables
    [SerializeField] private float deathScreenTimer;
    [SerializeField] private GameProgression gameProgression;
    [SerializeField] private ConfidenceState confidenceState;
    [SerializeField] private CharmState charmState;
    [SerializeField] private GameObject deathScreen;
    [SerializeField] private AnimationTriggerPlayer animationTriggerPlayer;
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
    
    //variables hidden in inspector
    [HideInInspector] public bool isDead = false;
    [HideInInspector] public bool testMode = false;
    [HideInInspector] public DebugMenu debugMenu;

    // Update is called once per frame
    void Update()
    {
        if (confidenceState.confidence <= 0 && !isDead)
        {
            StartCoroutine(Death());
        }
    }
    
    //currently does much more than death. Handles most of respawn/reflect phase as well. Might break up in teh future.
    public IEnumerator Death()
    {
        PhaseManager.Instance.TransitionTo(GamePhase.Death);
        isDead = true;
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
        //end of updating loopSnapshot: Next - game progression captures it
        gameProgression.lastLoop = loopSnapshot;
        
        
        //resume death sequence
        inConversationTrigger.enabled = false;
        yield return new WaitForSeconds(2f);
        deathScreen.SetActive(true);
        gameProgression.NextLoop();
        confidenceState.introMade = false;
        cardContainer.SetActive(false);
        yield return new WaitForSeconds(deathScreenTimer);
        deathScreen.SetActive(false);
        

        if (testMode)
        {
            // in test mode, skip draft and reopen debug menu
            if (debugMenu != null)
                debugMenu.OpenMenu();
            yield break;
        }
        
       
        
        // first half of respawn + drafting - ENTER Reflect Phase
        PhaseManager.Instance.TransitionTo(GamePhase.Reflect);
        // letterBoxAnimator.SetTrigger("LetterBoxIn"); — removed 2026-05-12: dedicated DraftScreenCam now handles the Reflect/Draft visual cut, no letterboxing needed.
        animationTriggerPlayer.EnterStart();
        playerTransform.position = spawnPoint.position;
        confidenceState.confidence = confidenceState.startingConfidence;
        charmState.ResetCharm();
        deckManager.ResetDeck();
        confidenceState.peakConfidence = 0;
        charmState.peakCharm = 0;
        // Reflect priority (per design 2026-05-19):
        //   1. Scripted loop branch (isScripted == true, loop-matched)  — the narrative spine, always wins
        //   2. Milestone override                                       — replaces conditional fallback
        //   3. Conditional / legacy ReflectBranch                       — picked by Play()'s SelectBranch
        // TODO (future task): scripted-vs-milestone overlap. When both match the same loop today,
        // the milestone's reflect lines are SKIPPED but the upgrade still applies + marks complete.
        // Davis flagged this for the first 4–5 loops (narrative intro) — we need both to play somehow.
        // Probably: chain them (scripted then milestone, or vice-versa), or queue the milestone for
        // the next non-scripted loop. Follow-up to the reflect import work.
        ReflectBranch scriptedBranch = reflectSelfTalk.FindScriptedBranchForLoop(gameProgression.loopCount);
        Milestone triggeredMilestone = milestoneTracker != null
            ? milestoneTracker.GetTriggeredMilestone(gameProgression.lastLoop, gameProgression.loopCount)
            : null;

        if (scriptedBranch != null)
        {
            // Scripted wins. Play its lines.
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
                }
                milestoneTracker.MarkComplete(triggeredMilestone);
            }
        }
        else if (triggeredMilestone != null)
        {
            // No scripted match — milestone wins over the conditional fallback.
            yield return reflectSelfTalk.PlayLines(triggeredMilestone.reflectLines);
            if (triggeredMilestone.characterUpgrade != null)
            {
                milestoneTracker.ApplyCharacterUpgrade(triggeredMilestone.characterUpgrade);
                // Re-apply respawn state so an upgrade that bumped startingConfidence
                // or startingHandSize is felt immediately on this respawn (not the next one).
                confidenceState.confidence = confidenceState.startingConfidence;
                deckManager.ResetDeck();
            }
            milestoneTracker.MarkComplete(triggeredMilestone);
        }
        else
        {
            // No scripted, no milestone — fall back to conditional / legacy branch selection.
            yield return reflectSelfTalk.Play(gameProgression.lastLoop, gameProgression.loopCount);
        }
        
        if (deckManager.draftPool.Length >0)
        {
            PhaseManager.Instance.TransitionTo(GamePhase.UpgradeDraft);
            draftUI.ShowDraftOptions();
            yield return new WaitUntil(() => !draftUI.gameObject.activeSelf || !draftUI.enabled);
        }

        // Wait for draft self-talk lines to finish typing BEFORE transitioning to Hallway —
        // otherwise the camera cuts back to HallCam mid-line, leaving the text playing in
        // the screen-space temp object over the wrong camera shot.
        yield return new WaitUntil(() => (!hallwaySelfTalk.draftLinesActive));

        // Commit lines — the loop's final hype-up before committing to the hallway approach
        // (the locker-close beat). Plays AFTER the draft phase, while the camera is still on the
        // reflect/draft close-up, then we cut to Hallway. Flow: reflect → draft → commit → hallway.
        if (scriptedBranch != null && scriptedBranch.commitLines != null && scriptedBranch.commitLines.Length > 0)
            yield return reflectSelfTalk.PlayLines(scriptedBranch.commitLines);

        //full respawn, ready for hallway walk
        PhaseManager.Instance.TransitionTo(GamePhase.Hallway);
        thoughtSpawner.SpawnButtons();
        gameProgression.SetLoopConditions();
        isDead = false;
        
        
    }
    
    
    
}
