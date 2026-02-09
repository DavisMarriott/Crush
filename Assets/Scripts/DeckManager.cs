using UnityEngine;
using System.Collections.Generic;

public class DeckManager : MonoBehaviour
{
    [SerializeField] private DialogueCard[] draftPool;
    [SerializeField] private DialogueCard[] startingDeck;
    private List<DialogueCard> deck;
    public List<DialogueCard> Deck => deck;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        deck = new List<DialogueCard>(startingDeck);
    }

    public void AddCardToDeck(DialogueCard card)
    {
        deck.Add(card);
    }
    
    public List<DialogueCard> GetDraftOptions(int count)
    {
        List<DialogueCard> options = new List<DialogueCard>();
        List<DialogueCard> pool = new List<DialogueCard>(draftPool);
        
        // Remove cards already in deck
        pool.RemoveAll(card => deck.Contains(card));
    
        for (int i = 0; i < count && pool.Count > 0; i++)
        {
            int randomIndex = Random.Range(0, pool.Count);
            options.Add(pool[randomIndex]);
            pool.RemoveAt(randomIndex);
        }
    
        return options;
    }
}
