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
    // ─── TEST MODE ───────────────────────────────────────────────
    // Check this box to skip the walk-up and go straight to conversation.
    // When unchecked, the debug menu still works (B key) but doesn't
    // override the normal game flow.
    [Header("Test Mode")]
    [SerializeField] private bool testMode = false;
    [SerializeField] private CinemachineCamera conversationCamera;
    [SerializeField] private CinemachineCamera hallwayCamera;
    [SerializeField] private GameObject dialogueUI;
    [SerializeField] private Transform testSpawnPoint;
    [SerializeField] private Transform normalSpawnPoint;
    [SerializeField] private Transform playerTransform;

    // ─── SYSTEM REFERENCES (drag in Inspector) ─────────────────
    [Header("System References")]
    [SerializeField] private ConfidenceState confidenceState;
    [SerializeField] private CharmState charmState;
    [SerializeField] private DeckManager deckManager;
    [SerializeField] private ThoughtSpawner thoughtSpawner;
    [SerializeField] private DialogueBox dialogueBox;
    [SerializeField] private GameObject thoughtBubble;

    // ─── UI PANELS ─────────────────────────────────────────────
    [Header("UI Panels")]
    [SerializeField] private GameObject debugMenuPanel;

    // ─── GLOBAL SETTINGS UI ────────────────────────────────────
    [Header("Global Settings")]
    [SerializeField] private TMP_InputField startingConfidenceInput;
    [SerializeField] private TMP_InputField startingCharmInput;
    [SerializeField] private TMP_InputField handSizeInput;

    // ─── DECK MANAGEMENT UI ────────────────────────────────────
    [Header("Deck Management")]
    [SerializeField] private Transform deckListContainer;
    [SerializeField] private Transform availableCardsContainer;
    [SerializeField] private Button cardButtonPrefab;

    // ─── CARD EDITOR UI ────────────────────────────────────────
    [Header("Card Editor")]
    [SerializeField] private GameObject cardEditorPanel;
    [SerializeField] private TMP_Text cardEditorTitle;
    [SerializeField] private TMP_InputField costInput;
    [SerializeField] private Button saveCardButton;

    // ─── START BUTTON ──────────────────────────────────────────
    [Header("Start")]
    [SerializeField] private Button startButton;

    private DialogueCard[] allCards;
    private DialogueCard selectedCard;
    private bool isOpen = false;
    private bool firstOpen = true;

    void Awake()
    {
        // Find all DialogueCard assets in the project
        allCards = Resources.LoadAll<DialogueCard>("");

        // If Resources.LoadAll finds nothing, try finding them manually
        // Cards need to be in a Resources folder, OR we load them in editor
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
            // Open the debug menu immediately so you can configure before playing
            // (EnterMode will run when you hit Start in the menu)
            confidenceState.testMode = true;
            confidenceState.debugMenu = this;
            OpenMenu();
        }
    }

    void Update()
    {
    }

    // ─── OPEN / CLOSE ──────────────────────────────────────────

    public void OpenMenu()
    {
        isOpen = true;
        debugMenuPanel.SetActive(true);
        Time.timeScale = 0f;

        // Only populate starting values on first open;
        // after death, keep whatever the player last typed in
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

    // ─── START GAME ────────────────────────────────────────────

    private void StartGame()
    {
        // Apply global settings and reset death state
        int conf = 3;
        if (int.TryParse(startingConfidenceInput.text, out int parsedConf))
            conf = parsedConf;
        confidenceState.ResetForNewGame(conf);

        if (int.TryParse(startingCharmInput.text, out int chrm))
            charmState.charm = chrm;

        // Reset deck and redraw hand
        deckManager.ResetDeck();
        charmState.ResetCharm();

        // Clean up any stuck dialogue state
        if (dialogueBox != null)
            dialogueBox.CloseDialogueBox();
        if (thoughtBubble != null)
            thoughtBubble.SetActive(true);

        // Test mode: skip walk-up, jump straight to conversation
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

        // Rebuild the hand UI
        thoughtSpawner.SpawnButtons();
    }

    // ─── DECK LIST (cards currently in your deck) ──────────────

    private void RefreshDeckList()
    {
        if (deckListContainer == null || cardButtonPrefab == null) return;

        // Clear old buttons
        for (int i = deckListContainer.childCount - 1; i >= 0; i--)
            Destroy(deckListContainer.GetChild(i).gameObject);

        // Combine deck + hand + discard to show all owned cards
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
                // Click to edit, long explanation: opens the card editor panel
                OpenCardEditor(capturedCard);
            });
        }
    }

    // ─── AVAILABLE CARDS (all cards not in deck) ───────────────

    private void RefreshAvailableCards()
    {
        if (availableCardsContainer == null || cardButtonPrefab == null) return;

        // Clear old buttons
        for (int i = availableCardsContainer.childCount - 1; i >= 0; i--)
            Destroy(availableCardsContainer.GetChild(i).gameObject);

        // Get all owned cards
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

    // ─── CARD EDITOR ───────────────────────────────────────────

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

        // Update cost
        if (int.TryParse(costInput.text, out int newCost))
            selectedCard.cost = newCost;

        // Save to asset file so changes persist
        #if UNITY_EDITOR
        EditorUtility.SetDirty(selectedCard);
        AssetDatabase.SaveAssets();
        #endif

        RefreshDeckList();
    }

    // ─── REMOVE CARD FROM DECK ─────────────────────────────────

    public void RemoveSelectedCardFromDeck()
    {
        if (selectedCard == null) return;

        // Remove from deck list
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
