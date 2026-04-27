using System.Collections;
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
    [SerializeField] private CardUpgradeTracker upgradeTracker;
    
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
            var appliedUpgrade = upgradeTracker.GetAppliedUpgrade(card);
            var prefabToUse = (appliedUpgrade != null && appliedUpgrade.visualPrefab != null)
                ? appliedUpgrade.visualPrefab
                : thoughtButtonPrefab;
            var btn = Instantiate(prefabToUse, thoughtListContainer);

            var btnImage = btn.GetComponent<Image>();
            if (btnImage != null)
                btnImage.color = card.buttonColor;

            var previewText = (appliedUpgrade != null && !string.IsNullOrEmpty(appliedUpgrade.previewTextOverride))
                ? appliedUpgrade.previewTextOverride
                : card.previewText;
            var label = btn.transform.Find("Card_Art/PreviewText").GetComponent<TextMeshProUGUI>();
            label.text = previewText;

            // if revealed = true, display cost
            var costIndicator = btn.transform.Find("Card_Art/CostIndicator/CostText").GetComponent<TextMeshProUGUI>();
            if (costIndicator != null)
            {
                costIndicator.gameObject.SetActive(card.revealed);
                var costText = costIndicator.GetComponentInChildren<TMP_Text>();
                if (costText != null)
                    costText.text = upgradeTracker.GetEffectiveCost(card).ToString();
            }

            btn.onClick.AddListener(() =>
            {  
                // Always deduct cost and play the card.
                // GetLukeBranch picks Death/Awkward/Normal based on
                // what confidence looks like AFTER cost is paid.
                confidenceState.confidence -= upgradeTracker.GetEffectiveCost(card);
                deckManager.DiscardCard(card);
                dialogueBox.ShowDialogue(card);
                //btn.gameObject.SetActive(false);
                //playing the card reveals it's confidence next time
                card.revealed = true;
            });
    }
    }
}