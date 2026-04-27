using TMPro;
using UnityEngine;
using UnityEngine.UI;
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

    private void Start()
    {
        draftUI.SetActive(false);
    }

    public void ShowDraftOptions()
    {
        for (int i = draftContainer.childCount - 1; i >= 0; i--)
            Destroy(draftContainer.GetChild(i).gameObject);

        draftUI.SetActive(true);

        List<DialogueCard> cardOptions = deckManager.GetDraftOptions(10);
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
    
        btn.onClick.AddListener(() =>
        {
            hallwaySelfTalk.TriggerDraftLines(card.draftLines);
            deckManager.AddCardToDeck(card);
            deckManager.ResetDeck();
            CloseDraftUI();
        });
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
