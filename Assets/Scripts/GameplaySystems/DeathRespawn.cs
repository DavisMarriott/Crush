using System.Collections;
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
        isDead = true;
        inConversationTrigger.enabled = false;
        yield return new WaitForSeconds(2f);
        deathScreen.SetActive(true);
        dialogueBox.CloseDialogueBox();
        gameProgression.NextLoop();
        confidenceState.introMade = false;
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
        animationTriggerPlayer.EnterStart();
        playerTransform.position = spawnPoint.position;
        confidenceState.confidence = confidenceState.startingConfidence;
        charmState.ResetCharm();
        deckManager.ResetDeck();
        
        if (deckManager.draftPool.Length >0)
        {
            draftUI.ShowDraftOptions();
            yield return new WaitUntil(() => !draftUI.gameObject.activeSelf || !draftUI.enabled);
        }
        
        //full respawn, ready for hallway walk
        thoughtSpawner.SpawnButtons();
        yield return new WaitUntil(() => (!hallwaySelfTalk.draftLinesActive));
        gameProgression.SetLoopConditions();
        isDead = false;
        
        
    }
    
}
