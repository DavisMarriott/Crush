using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class ThoughtSpawner : MonoBehaviour
{
    [SerializeField] private Transform thoughtListContainer;
    [SerializeField] private Button thoughtButtonPrefab;
    [SerializeField] private DialogueBox dialogueBox;
    [SerializeField] private DeckManager deckManager;
    private List<DialogueCard> _hand;
    

    private void Start()
    {
        _hand = new List<DialogueCard>(deckManager.Deck);
        SpawnButtons();
    }

    public void AddCardToHand(DialogueCard card)
    {
        _hand.Add(card);
    }
    
    public void SpawnButtons()
    {
        for (int i = thoughtListContainer.childCount - 1; i >= 0; i--)
            Destroy(thoughtListContainer.GetChild(i).gameObject);

        foreach (var card in _hand)
        {
            Debug.Log($"Spawning button for: {card.previewText}");
            var btn = Instantiate(thoughtButtonPrefab, thoughtListContainer);
            
            // Set button bg color from Dialogue Card
            var btnImage = btn.GetComponent<Image>();
            if (btnImage != null)
                btnImage.color = card.buttonColor;

            var label = btn.GetComponentInChildren<TMP_Text>();
            label.text = card.previewText;

            btn.onClick.AddListener(() =>
            {
                dialogueBox.ShowDialogue(card);
                //removes dialogue card from options so it can't be picked again
                btn.gameObject.SetActive(false);
            });
        }
    }
}