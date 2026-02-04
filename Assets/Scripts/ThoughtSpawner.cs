using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ThoughtSpawner : MonoBehaviour
{
    [SerializeField] private Transform thoughtListContainer;
    [SerializeField] private Button thoughtButtonPrefab;
    [SerializeField] private DialogueBox dialogueBox;
    [SerializeField] private DialogueCard[] cards;

    private void Start()
    {
       SpawnButtons();
    }

    public void SpawnButtons()
    {
        for (int i = thoughtListContainer.childCount - 1; i >= 0; i--)
            Destroy(thoughtListContainer.GetChild(i).gameObject);

        foreach (var card in cards)
        {
            Debug.Log($"Spawning button for: {card.previewText}");
            var btn = Instantiate(thoughtButtonPrefab, thoughtListContainer);

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