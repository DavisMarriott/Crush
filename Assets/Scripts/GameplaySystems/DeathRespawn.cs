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
        
       
        
        // first half of respawn + drafting
        PhaseManager.Instance.TransitionTo(GamePhase.Reflect);
        letterBoxAnimator.SetTrigger("LetterBoxIn");
        animationTriggerPlayer.EnterStart();
        playerTransform.position = spawnPoint.position;
        confidenceState.confidence = confidenceState.startingConfidence;
        charmState.ResetCharm();
        deckManager.ResetDeck();
        confidenceState.peakConfidence = 0;
        charmState.peakCharm = 0;
        yield return new WaitForSeconds(reflectDuration);
        
        if (deckManager.draftPool.Length >0)
        {
            PhaseManager.Instance.TransitionTo(GamePhase.UpgradeDraft);
            draftUI.ShowDraftOptions();
            yield return new WaitUntil(() => !draftUI.gameObject.activeSelf || !draftUI.enabled);
        }
        
        //full respawn, ready for hallway walk
        PhaseManager.Instance.TransitionTo(GamePhase.Hallway);
        thoughtSpawner.SpawnButtons();
        yield return new WaitUntil(() => (!hallwaySelfTalk.draftLinesActive));
        gameProgression.SetLoopConditions();
        isDead = false;
        
        
    }
    
    
    
}
