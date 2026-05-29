using UnityEngine;
using TMPro;

public class DeckSizeIndicator : MonoBehaviour
{
    private TextMeshProUGUI textMeshPro;
    public DeckManager deckManager;
    //private int deckSize;

    void Start()
    {
        textMeshPro = GetComponent<TextMeshProUGUI>();
    }
    void Update()
    {
        //deckSize = deckManager.deckSize;
        textMeshPro.text = $"{deckManager.deckSize}";
        Debug.Log(deckManager.deckSize);
        Debug.Log($"Deck Size: {deckManager.deckSize}");
    }
}
