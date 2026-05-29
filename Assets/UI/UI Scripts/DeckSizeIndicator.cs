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
    public void UpdateDeckSize()
    {

        if (deckManager.deckSize >= 0)
        {
            textMeshPro.text = $"{deckManager.deckSize}";
            //Debug.Log($"Deck Size: {deckManager.deckSize}");
        }
        
        else
        {
            textMeshPro.text = "0";
            //Debug.Log($"Deck Size: {deckManager.deckSize}");
        }
        
    }
}
