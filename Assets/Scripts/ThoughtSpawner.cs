using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ThoughtSpawner : MonoBehaviour
{
    [SerializeField] private Transform thoughtListContainer;
    [SerializeField] private Button thoughtButtonPrefab;
    [SerializeField] private DialogueBox dialogueBox;
    [SerializeField] private DeckManager deckManager;

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

            btn.onClick.AddListener(() =>
            {
                deckManager.DiscardCard(card);
                dialogueBox.ShowDialogue(card);
                btn.gameObject.SetActive(false);
            });
        }
    }
}