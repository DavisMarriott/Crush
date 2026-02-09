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
    [SerializeField] private ThoughtSpawner thoughtSpawner;

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
        
        foreach (var card in options)
        {
            var btn = Instantiate(thoughtButtonPrefab, draftContainer);
            
            var label = btn.GetComponentInChildren<TMP_Text>();
            label.text = card.previewText;
            
            btn.onClick.AddListener(() =>
            {
                deckManager.AddCardToDeck(card);
                thoughtSpawner.AddCardToHand(card);
                CloseDraftUI();
            });
        }
    }

    public void CloseDraftUI()
    {
        draftUI.SetActive(false);
    }
}
