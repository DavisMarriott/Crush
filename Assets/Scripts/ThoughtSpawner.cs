using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ThoughtSpawner : MonoBehaviour
{
    [SerializeField] private Transform thoughtListContainer;
    [SerializeField] private Button thoughtButtonPrefab;
    [SerializeField] private DialogueBox dialogueBox;
    [SerializeField] private DeckManager deckManager;
    [SerializeField] private ConfidenceState confidenceState;
    
    private void Start()
    {
        SpawnButtons();
    }
    public void SpawnButtons()
    {
        for (int i = thoughtListContainer.childCount - 1; i >= 0; i--)
            Destroy(thoughtListContainer.GetChild(i).gameObject);

        foreach (var card in deckManager.Hand)
        {
            var btn = Instantiate(thoughtButtonPrefab, thoughtListContainer);
            
            var btnImage = btn.GetComponent<Image>();
            if (btnImage != null)
                btnImage.color = card.buttonColor;

            var label = btn.GetComponentInChildren<TMP_Text>();
            label.text = card.previewText;
            
            // if revealed = true, display cost
            var costIndicator = btn.transform.Find("CostIndicator");
            if (costIndicator != null)
            {
                costIndicator.gameObject.SetActive(card.revealed);
                var costText = costIndicator.GetComponentInChildren<TMP_Text>();
                if (costText != null)
                    costText.text = card.cost.ToString();
            }

            btn.onClick.AddListener(() =>
            {
                if (!card.revealed || confidenceState.confidence >= card.cost)
                {
                    confidenceState.confidence -= card.cost;
                    card.revealed = true;
                    deckManager.DiscardCard(card);
                    dialogueBox.ShowDialogue(card);
                    btn.gameObject.SetActive(false);
                }
               
            });
        }
    }
}