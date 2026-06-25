using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class DraftUI : MonoBehaviour
{
    [SerializeField] private DeckManager deckManager;
    [SerializeField] private Button thoughtButtonPrefab;
    [SerializeField] private GameObject draftUI;
    [SerializeField] private Transform draftContainer;
    [SerializeField] private Vector2 draftButtonSize = new Vector2(200, 60);
    [SerializeField] private HallwaySelfTalk hallwaySelfTalk;
    [SerializeField] private CardUpgradeTracker upgradeTracker;

    [Header("Loop-2 \"starting deck\" draft")]
    [Tooltip("On loop 2 the draft becomes a multi-pick: the player drafts this many cards to seed a starting deck.")]
    [SerializeField] private int loop2DraftPickCount = 3;
    [Tooltip("How many card options to show per pick on the loop-2 multi-pick draft. 3 = same width as the normal draft.")]
    [SerializeField] private int multiPickSlateSize = 3;

    // Multi-pick draft state (loop 2 only). For a normal draft _multiPick is false and _picksRemaining is 1.
    private int _picksRemaining = 1;
    private bool _multiPick = false;

    // Cards currently on the multi-pick slate, in slot order. Picking refreshes only the picked
    // slot; the others keep their card AND position when the slate comes back.
    private readonly List<DialogueCard> _slateCards = new();

    // (Was: Start() { draftUI.SetActive(false); } — fired on first activation, which during a real
    // draft meant DraftUI deactivated itself a frame after ShowSingleCardDraft, killing the draft.
    // Intent is now handled by leaving the DraftUI GO inactive in the scene at edit time.)

    // loopCount is passed in by DeathRespawn so the draft can special-case loop 2.
    public void ShowDraftOptions(int loopCount)
    {
        draftUI.SetActive(true);

        // Loop 2 is the "starting deck" draft: the player picks several cards instead of one.
        _multiPick = (loopCount == 2);
        _picksRemaining = _multiPick ? loop2DraftPickCount : 1;

        RenderDraftOptions();
    }

    private void RenderDraftOptions()
    {
        for (int i = draftContainer.childCount - 1; i >= 0; i--)
            Destroy(draftContainer.GetChild(i).gameObject);

        List<DialogueCard> cardOptions = deckManager.GetDraftOptions(10);

        if (_multiPick)
        {
            // Loop-2 "starting deck" draft: pick one of the slate, that slot refills with a fresh
            // card, the other slots stay put - like playing a card and drawing a replacement.
            int slots = Mathf.Min(multiPickSlateSize, cardOptions.Count);

            // Nothing left to offer — finish cleanly so the Upgrade/Draft phase doesn't hang.
            if (slots == 0)
            {
                deckManager.ResetDeck();
                CloseDraftUI();
                return;
            }

            _slateCards.Clear();
            for (int i = 0; i < slots; i++)
            {
                int idx = Random.Range(0, cardOptions.Count);
                _slateCards.Add(cardOptions[idx]);
                cardOptions.RemoveAt(idx);
            }
            RenderMultiPickSlate();
            return;
        }

        // Normal draft: a mix of cards + available upgrades across 3 slots.
        List<DeckManager.DraftableUpgrade> upgradeOptions = deckManager.GetAvailableUpgrades();

        // Nothing to draft AND nothing to upgrade — close cleanly so the phase doesn't hang on an
        // empty screen (DeathRespawn waits on the draft UI closing). Falls through to commit/hallway.
        if (cardOptions.Count == 0 && upgradeOptions.Count == 0)
        {
            deckManager.ResetDeck();
            CloseDraftUI();
            return;
        }

        for (int slot = 0; slot < 3; slot++)
        {
            if (cardOptions.Count == 0 && upgradeOptions.Count == 0) break;

            int totalAvailable = cardOptions.Count + upgradeOptions.Count;
            bool pickUpgrade = Random.Range(0, totalAvailable) >= cardOptions.Count;

            if (pickUpgrade)
            {
                int idx = Random.Range(0, upgradeOptions.Count);
                SpawnUpgradeButton(upgradeOptions[idx]);
                upgradeOptions.RemoveAt(idx);
            }
            else
            {
                int idx = Random.Range(0, cardOptions.Count);
                SpawnCardButton(cardOptions[idx]);
                cardOptions.RemoveAt(idx);
            }
        }
    }

    // Forced single-card draft (e.g. the loop-1 DANCE draft): offers exactly one card, one pick.
    // Reuses the normal single-pick path (SpawnCardButton -> OnCardPicked). Call this instead of
    // ShowDraftOptions when you want to hand the player one specific card.
    public void ShowSingleCardDraft(DialogueCard card)
    {
        draftUI.SetActive(true);
        _multiPick = false;
        _picksRemaining = 1;

        for (int i = draftContainer.childCount - 1; i >= 0; i--)
            Destroy(draftContainer.GetChild(i).gameObject);

        if (card == null)
        {
            CloseDraftUI();
            return;
        }

        SpawnCardButton(card);
    }

    private void SpawnCardButton(DialogueCard card)
    {
        // per-card prefab (e.g. DANCE) wins over the default
        var btn = Instantiate(card.visualPrefab != null ? card.visualPrefab : thoughtButtonPrefab, draftContainer);

        // button is centered in draft slot
        var rectTransform = btn.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.anchoredPosition = Vector2.zero;
        rectTransform.sizeDelta = draftButtonSize;
    
        var label = btn.transform.Find("Card_Art/PreviewText").GetComponent<TextMeshProUGUI>();
        label.text = card.previewText;

        // cost indicator - same population as the hand (ThoughtSpawner). Draft spawns were
        // skipping this entirely, so cost never showed on draft cards even when revealed.
        var costNode = btn.transform.Find("Card_Art/CostIndicator/CostText");
        if (costNode != null)
        {
            costNode.gameObject.SetActive(card.revealed);
            var costText = costNode.GetComponentInChildren<TMP_Text>();
            if (costText != null)
                costText.text = (upgradeTracker != null ? upgradeTracker.GetEffectiveCost(card) : card.cost).ToString();
        }

        btn.onClick.AddListener(() => OnCardPicked(card));
    }

    // Normal single-pick draft (and the loop-1 DANCE single draft): pick one, draft closes.
    private void OnCardPicked(DialogueCard card)
    {
        deckManager.AddCardToDeck(card);
        hallwaySelfTalk.TriggerDraftLines(card.draftLines);
        deckManager.ResetDeck();
        CloseDraftUI();
    }

    // Renders the multi-pick slate from _slateCards (clears the row first). Slot order = list order,
    // so unpicked cards always come back in the same place.
    private void RenderMultiPickSlate()
    {
        for (int i = draftContainer.childCount - 1; i >= 0; i--)
            Destroy(draftContainer.GetChild(i).gameObject);

        for (int s = 0; s < _slateCards.Count; s++)
            SpawnMultiPickButton(_slateCards[s], s);
    }

    private void SpawnMultiPickButton(DialogueCard card, int slotIndex)
    {
        // per-card prefab (e.g. DANCE) wins over the default
        var btn = Instantiate(card.visualPrefab != null ? card.visualPrefab : thoughtButtonPrefab, draftContainer);

        var rt = btn.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta = draftButtonSize;

        var label = btn.transform.Find("Card_Art/PreviewText").GetComponent<TextMeshProUGUI>();
        label.text = card.previewText;

        // cost indicator - same as SpawnCardButton above
        var costNode = btn.transform.Find("Card_Art/CostIndicator/CostText");
        if (costNode != null)
        {
            costNode.gameObject.SetActive(card.revealed);
            var costText = costNode.GetComponentInChildren<TMP_Text>();
            if (costText != null)
                costText.text = (upgradeTracker != null ? upgradeTracker.GetEffectiveCost(card) : card.cost).ToString();
        }

        btn.onClick.AddListener(() => OnMultiPickPicked(slotIndex));
    }

    private void OnMultiPickPicked(int slotIndex)
    {
        DialogueCard picked = _slateCards[slotIndex];
        deckManager.AddCardToDeck(picked);          // now owned -> excluded from the refill draw
        hallwaySelfTalk.TriggerDraftLines(picked.draftLines);
        _picksRemaining--;

        // Done seeding the starting deck.
        if (_picksRemaining <= 0)
        {
            deckManager.ResetDeck();
            CloseDraftUI();
            return;
        }

        // Replace ONLY the picked slot. Owned cards are already dropped by GetDraftOptions; also
        // skip the other slate cards so we don't double up. Other slots keep their card.
        List<DialogueCard> pool = deckManager.GetDraftOptions(10);
        for (int s = 0; s < _slateCards.Count; s++)
            if (s != slotIndex) pool.Remove(_slateCards[s]);

        if (pool.Count > 0)
            _slateCards[slotIndex] = pool[Random.Range(0, pool.Count)];
        else
            _slateCards.RemoveAt(slotIndex);  // pool exhausted - drop the slot

        // Clear the whole row and let the draft self-talk play, then bring the slate back with the
        // unpicked cards in their same spots + the new card in the picked slot.
        StartCoroutine(RefillAfterDraftLines());
    }

    // Empties the row, waits for the just-played draft line, then re-shows the slate.
    private IEnumerator RefillAfterDraftLines()
    {
        for (int i = draftContainer.childCount - 1; i >= 0; i--)
            Destroy(draftContainer.GetChild(i).gameObject);

        yield return new WaitUntil(() => !hallwaySelfTalk.draftLinesActive);

        RenderMultiPickSlate();
    }

    private void SpawnUpgradeButton(DeckManager.DraftableUpgrade dup)
    {
        var btn = Instantiate(dup.upgrade.visualPrefab, draftContainer);
    
        var rectTransform = btn.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.anchoredPosition = Vector2.zero;
        rectTransform.sizeDelta = draftButtonSize;
    
        var label = btn.transform.Find("Card_Art/PreviewText").GetComponent<TextMeshProUGUI>();
        label.text = dup.card.previewText;
    
        btn.onClick.AddListener(() =>
        {
            upgradeTracker.ApplyUpgrade(dup.card, dup.upgrade);
            hallwaySelfTalk.TriggerDraftLines(dup.upgrade.draftLines);   // same draft self-talk a card gets
            hallwaySelfTalk.TriggerLetterBoxOut();
            deckManager.ResetDeck();
            CloseDraftUI();
        });
    }


    public void CloseDraftUI()
    {
        draftUI.SetActive(false);
    }
}
