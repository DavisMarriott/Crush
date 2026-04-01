using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

public class ConfidenceState : MonoBehaviour
{
    [SerializeField] private Transform boyTransform;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private DialogueBox dialogueBox;
    [SerializeField] private TMP_Text label;
    [SerializeField] private ThoughtSpawner thoughtSpawner;
    [SerializeField] private GameObject deathScreen;
    [SerializeField] private float deathScreenTimer;
    [SerializeField] private DraftUI draftUI;
    [SerializeField] private CharmState charmState;
    [SerializeField] private DeckManager deckManager;
    [SerializeField] private GameProgression gameProgression;

    [Header("Conversation State")]
    public bool introMade = false;
    public bool inConversation = false;

    [HideInInspector] public bool testMode = false;
    [HideInInspector] public DebugMenu debugMenu;

    private bool _isDead = false;
    public bool Dead => _isDead;

    // confidence is in thirds, 45 = 15 full hearts
    const int MinConfidenceFullGame = 0;
    const int MaxConfidenceFullGame = 45;

    [Header("Starting Confidence (in thirds)")]
    [SerializeField] private int startingConfidence = 3;

    public int confidence;

    void Start()
    {
        confidence = startingConfidence;
        deathScreen.SetActive(false);
    }

    private void Update()
    {
        if (label != null)
            label.text = $"{confidence}";

        // todo: only clamp when score actually changes
        ClampConfidence();
        label.text = $"{confidence}";

        if (confidence <= 0 && !_isDead)
        {
            _isDead = true;
            StartCoroutine(SpawnDeathScreen());
        }
    }

    private IEnumerator SpawnDeathScreen()
    {
        yield return new WaitForSeconds(2f);
        deathScreen.SetActive(true);
        dialogueBox.CloseDialogueBox();
        yield return new WaitForSeconds(deathScreenTimer);
        deathScreen.SetActive(false);

        if (testMode)
        {
            // in test mode, skip draft and reopen debug menu
            if (debugMenu != null)
                debugMenu.OpenMenu();
            yield break;
        }

        if (deckManager.draftPool.Length >0)
        {
            draftUI.ShowDraftOptions();
            yield return new WaitUntil(() => !draftUI.gameObject.activeSelf || !draftUI.enabled);
        }

        // respawn
        thoughtSpawner.SpawnButtons();
        boyTransform.position = spawnPoint.position;
        confidence = startingConfidence;
        confidence = startingConfidence;
        charmState.ResetCharm();
        deckManager.ResetDeck();
        _isDead = false;
        introMade = false;
        gameProgression.NextLoop();
    }

    void ClampConfidence()
    {
        confidence = Mathf.Clamp(confidence, MinConfidenceFullGame, MaxConfidenceFullGame);
    }

    public void EnterConversation()
    {
        inConversation = true;
    }

    public void ExitConversation()
    {
        inConversation = false;
    }

    public void ResetForNewGame(int newConfidence)
    {
        confidence = newConfidence;
        _isDead = false;
        introMade = false;
    }
}
