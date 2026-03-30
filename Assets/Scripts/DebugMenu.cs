using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using Unity.Cinemachine;
using TMPro;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class DebugMenu : MonoBehaviour
{
    // test mode - check this to skip the hallway walk and jump to conversation
    [Header("Test Mode")]
    [SerializeField] private bool testMode = false;
    [SerializeField] private CinemachineCamera conversationCamera;
    [SerializeField] private CinemachineCamera hallwayCamera;
    [SerializeField] private GameObject dialogueUI;
    [SerializeField] private Transform testSpawnPoint;
    [SerializeField] private Transform normalSpawnPoint;
    [SerializeField] private Transform playerTransform;

    [Header("System References")]
    [SerializeField] private ConfidenceState confidenceState;
    [SerializeField] private CharmState charmState;
    [SerializeField] private DeckManager deckManager;
    [SerializeField] private ThoughtSpawner thoughtSpawner;
    [SerializeField] private DialogueBox dialogueBox;
    [SerializeField] private GameObject thoughtBubble;

    [Header("UI Panels")]
    [SerializeField] private GameObject debugMenuPanel;

    [Header("Global Settings")]
    [SerializeField] private TMP_InputField startingConfidenceInput;
    [SerializeField] private TMP_InputField startingCharmInput;
    [SerializeField] private TMP_InputField handSizeInput;

    [Header("Deck Management")]
    [SerializeField] private Transform deckListContainer;
    [SerializeField] private Transform availableCardsContainer;
    [SerializeField] private Button cardButtonPrefab;

    [Header("Card Editor")]
    [SerializeField] private GameObject cardEditorPanel;
    [SerializeField] private TMP_Text cardEditorTitle;
    [SerializeField] private TMP_InputField costInput;
    [SerializeField] private Button saveCardButton;

    [Header("Start")]
    [SerializeField] private Button startButton;

    private DialogueCard[] allCards;
    private DialogueCard selectedCard;
    private bool isOpen = false;
    private bool firstOpen = true;

    void Awake()
    {
        // grab all card assets
        allCards = Resources.LoadAll<DialogueCard>("");

        // fallback for editor - cards aren't in a Resources folder so grab them manually
        #if UNITY_EDITOR
        if (allCards == null || allCards.Length == 0)
        {
            string[] guids = AssetDatabase.FindAssets("t:DialogueCard", new[] { "Assets/Cards" });
            List<DialogueCard> cards = new List<DialogueCard>();
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                DialogueCard card = AssetDatabase.LoadAssetAtPath<DialogueCard>(path);
                if (card != null) cards.Add(card);
            }
            allCards = cards.ToArray();
        }
        #endif

        if (startButton != null)
            startButton.onClick.AddListener(StartGame);

        if (saveCardButton != null)
            saveCardButton.onClick.AddListener(SaveSelectedCard);

        if (cardEditorPanel != null)
            cardEditorPanel.SetActive(false);
    }

    void Start()
    {
        if (testMode)
        {
            // tell confidence state we're in test mode so death opens this menu instead of draft UI
            confidenceState.testMode = true;
            confidenceState.debugMenu = this;
            OpenMenu();
        }
    }

    void Update()
    {
    }

    public void OpenMenu()
    {
        isOpen = true;
        debugMenuPanel.SetActive(true);
        Time.timeScale = 0f;

        // only set defaults on first open - after death, keep whatever was typed in
        if (firstOpen)
        {
            if (startingConfidenceInput != null)
                startingConfidenceInput.text = confidenceState.confidence.ToString();
            if (startingCharmInput != null)
                startingCharmInput.text = charmState.charm.ToString();
            firstOpen = false;
        }
        if (handSizeInput != null)
            handSizeInput.text = deckManager.Hand.Count.ToString();

        RefreshDeckList();
        RefreshAvailableCards();
    }

    public void CloseMenu()
    {
        isOpen = false;
        debugMenuPanel.SetActive(false);
        Time.timeScale = 1f;
    }

    private void StartGame()
    {
        // read and apply settings from input fields
        int conf = 3;
        charmState.ResetCharm();
        if (int.TryParse(startingConfidenceInput.text, out int parsedConf))
            conf = parsedConf;
        confidenceState.ResetForNewGame(conf);

        if (int.TryParse(startingCharmInput.text, out int chrm))
            charmState.charm = chrm;

        if (int.TryParse(handSizeInput.text, out int parsedHand))
            deckManager.startingHandSize = parsedHand;
        deckManager.ResetDeck();



        // close dialogue if it's stuck open from last round
        if (dialogueBox != null)
            dialogueBox.CloseDialogueBox();
        if (thoughtBubble != null)
            thoughtBubble.SetActive(true);

        // in test mode, skip the walk and go straight to conversation
        if (testMode)
        {
            if (testSpawnPoint != null && playerTransform != null)
                playerTransform.position = testSpawnPoint.position;
            if (dialogueUI != null)
                dialogueUI.SetActive(true);
            if (conversationCamera != null)
                CameraManager.SwitchCamera(conversationCamera);
            confidenceState.inConversation = true;
        }

        CloseMenu();
        thoughtSpawner.SpawnButtons();
    }

    private void RefreshDeckList()
    {
        if (deckListContainer == null || cardButtonPrefab == null) return;

        // clear old buttons
        for (int i = deckListContainer.childCount - 1; i >= 0; i--)
            Destroy(deckListContainer.GetChild(i).gameObject);

        // deck + hand + discard = all owned cards
        List<DialogueCard> owned = new List<DialogueCard>();
        if (deckManager.Deck != null) owned.AddRange(deckManager.Deck);
        if (deckManager.Hand != null) owned.AddRange(deckManager.Hand);
        if (deckManager.Discard != null) owned.AddRange(deckManager.Discard);

        foreach (var card in owned)
        {
            var btn = Instantiate(cardButtonPrefab, deckListContainer);
            var label = btn.GetComponentInChildren<TMP_Text>();
            if (label != null) label.text = card.previewText;

            var capturedCard = card;
            btn.onClick.AddListener(() =>
            {
                OpenCardEditor(capturedCard);
            });
        }
    }

    private void RefreshAvailableCards()
    {
        if (availableCardsContainer == null || cardButtonPrefab == null) return;

        for (int i = availableCardsContainer.childCount - 1; i >= 0; i--)
            Destroy(availableCardsContainer.GetChild(i).gameObject);

        List<DialogueCard> owned = new List<DialogueCard>();
        if (deckManager.Deck != null) owned.AddRange(deckManager.Deck);
        if (deckManager.Hand != null) owned.AddRange(deckManager.Hand);
        if (deckManager.Discard != null) owned.AddRange(deckManager.Discard);

        foreach (var card in allCards)
        {
            if (owned.Contains(card)) continue;

            var btn = Instantiate(cardButtonPrefab, availableCardsContainer);
            var label = btn.GetComponentInChildren<TMP_Text>();
            if (label != null) label.text = card.previewText;

            var btnImage = btn.GetComponent<Image>();
            if (btnImage != null) btnImage.color = new Color(0.7f, 0.7f, 0.7f);

            var capturedCard = card;
            btn.onClick.AddListener(() =>
            {
                deckManager.AddCardToDeck(capturedCard);
                RefreshDeckList();
                RefreshAvailableCards();
            });
        }
    }

    private void OpenCardEditor(DialogueCard card)
    {
        selectedCard = card;
        if (cardEditorPanel != null) cardEditorPanel.SetActive(true);

        if (cardEditorTitle != null) cardEditorTitle.text = card.previewText;
        if (costInput != null) costInput.text = card.cost.ToString();
    }

    private void SaveSelectedCard()
    {
        if (selectedCard == null) return;

        if (int.TryParse(costInput.text, out int newCost))
            selectedCard.cost = newCost;

        // mark dirty so Unity saves the change to the asset file
        #if UNITY_EDITOR
        EditorUtility.SetDirty(selectedCard);
        AssetDatabase.SaveAssets();
        #endif

        RefreshDeckList();
    }

    public void RemoveSelectedCardFromDeck()
    {
        if (selectedCard == null) return;

        if (deckManager.Deck != null)
            deckManager.Deck.Remove(selectedCard);
        if (deckManager.Hand != null)
            deckManager.Hand.Remove(selectedCard);

        selectedCard = null;
        if (cardEditorPanel != null) cardEditorPanel.SetActive(false);

        RefreshDeckList();
        RefreshAvailableCards();
    }
}
