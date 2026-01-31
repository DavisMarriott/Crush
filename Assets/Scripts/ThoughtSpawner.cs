using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ThoughtSpawner : MonoBehaviour
{
    [SerializeField] private Transform thoughtListContainer;
    [SerializeField] private Button thoughtButtonPrefab;

    [Header("Cards to show in the thought bubble")]
    [SerializeField] private DialogueCard[] cards;

    [Header("Where to send the chosen card")]
    [SerializeField] private DialogueBox dialogueBox; // your existing script

    private void Start()
    {
        // Clear container
        for (int i = thoughtListContainer.childCount - 1; i >= 0; i--)
            Destroy(thoughtListContainer.GetChild(i).gameObject);

        // Spawn buttons
        foreach (var card in cards)
        {
            if (card == null) continue;

            var btn = Instantiate(thoughtButtonPrefab, thoughtListContainer);

            var label = btn.GetComponentInChildren<TMP_Text>();
            if (label) label.text = card.previewText;

            btn.onClick.AddListener(() =>
            {
                if (dialogueBox != null && card.dialogue != null)
                    dialogueBox.ShowDialogue(card.dialogue);
            });
        }
    }
}