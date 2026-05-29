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
    [Tooltip("How many card options to show on the loop-2 multi-pick draft.")]
    [SerializeField] private int multiPickSlateSize = 6;

    // Multi-pick draft state (loop 2 only). For a normal draft _multiPick is false and _picksRemaining is 1.
    private int _picksRemaining = 1;
    private bool _multiPick = false;

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
            // Loop-2 "starting deck" draft: cards only, chosen from a wider slate.
            int slots = Mathf.Min(multiPickSlateSize, cardOptions.Count);

            // Nothing left to offer — finish cleanly so the Upgrade/Draft phase doesn't hang.
            if (slots == 0)
            {
                deckManager.ResetDeck();
                CloseDraftUI();
                return;
            }

            for (int i = 0; i < slots; i++)
            {
                int idx = Random.Range(0, cardOptions.Count);
                SpawnCardButton(cardOptions[idx]);
                cardOptions.RemoveAt(idx);
            }
            return;
        }

        // Normal draft: a mix of cards + available upgrades across 3 slots.
        List<DeckManager.DraftableUpgrade> upgradeOptions = deckManager.GetAvailableUpgrades();

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
        var btn = Instantiate(thoughtButtonPrefab, draftContainer);
    
        // button is centered in draft slot
        var rectTransform = btn.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.anchoredPosition = Vector2.zero;
        rectTransform.sizeDelta = draftButtonSize;
    
        var label = btn.transform.Find("Card_Art/PreviewText").GetComponent<TextMeshProUGUI>();
        label.text = card.previewText;
    
        btn.onClick.AddListener(() => OnCardPicked(card));
    }

    // Handles a drafted card pick for both the normal (single-pick) and loop-2 (multi-pick) drafts.
    private void OnCardPicked(DialogueCard card)
    {
        deckManager.AddCardToDeck(card);

        // Play this card's draft self-talk on every pick (multi-pick included).
        hallwaySelfTalk.TriggerDraftLines(card.draftLines);

        if (_multiPick)
        {
            _picksRemaining--;
            if (_picksRemaining > 0)
            {
                // Hold the next options until this card's draft line finishes, then re-roll the slate.
                // (Serializes picks so the per-pick draft lines never overlap; GetDraftOptions also
                //  filters out cards we now own. ResetDeck is deferred until the last pick.)
                StartCoroutine(ShowNextAfterDraftLines());
            }
            else
            {
                deckManager.ResetDeck();
                CloseDraftUI();
            }
            return;
        }

        // Normal single-pick draft.
        deckManager.ResetDeck();
        CloseDraftUI();
    }

    // Clears the slate, waits for the just-played draft line to finish, then shows the next options.
    private IEnumerator ShowNextAfterDraftLines()
    {
        for (int i = draftContainer.childCount - 1; i >= 0; i--)
            Destroy(draftContainer.GetChild(i).gameObject);

        yield return new WaitUntil(() => !hallwaySelfTalk.draftLinesActive);

        RenderDraftOptions();
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
