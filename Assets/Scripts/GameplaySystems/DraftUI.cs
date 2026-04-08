using System;
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

    private void Start()
    {
        draftUI.SetActive(false);
    }

    public void ShowDraftOptions()
    {
        // Clear old buttons
        for (int i = draftContainer.childCount - 1; i >= 0; i--)
            Destroy(draftContainer.GetChild(i).gameObject);
        
        // Show the draft screen
        draftUI.SetActive(true);
        
        // Get random cards and spawn buttons
        List<DialogueCard> options = deckManager.GetDraftOptions(3);
        
        for (int i = 0; i < options.Count; i++)
        {
            var btn = Instantiate(thoughtButtonPrefab, draftContainer);
            
            // button is centered in draft slot
            var rectTransform = btn.GetComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            rectTransform.anchoredPosition = Vector2.zero;
            //set size of button
            rectTransform.sizeDelta = draftButtonSize;
            
            // Get button color from dialogue card
            var btnImage = btn.GetComponent<Image>();
            if (btnImage != null)
                btnImage.color = options[i].buttonColor;
        
            var label = btn.GetComponentInChildren<TMP_Text>();
            label.text = options[i].previewText;
        
            var card = options[i];
            btn.onClick.AddListener(() =>
            {
                hallwaySelfTalk.TriggerDraftLines(card.draftLines);
                deckManager.AddCardToDeck(card);
                CloseDraftUI();

            });

        }
    }

    public void CloseDraftUI()
    {
        draftUI.SetActive(false);
    }
}
