using UnityEngine;
using System.Collections.Generic;

public class DeckManager : MonoBehaviour
{
    [SerializeField] private DialogueCard[] draftPool;
    [SerializeField] private DialogueCard[] startingDeck;
    [SerializeField] private int startingHandSize = 4;
    
    private List<DialogueCard> deck;
    private List<DialogueCard> hand;
    private List<DialogueCard> discard;
    
    public List<DialogueCard> Deck => deck;
    public List<DialogueCard> Hand => hand;

    void Start()
    {
        deck = new List<DialogueCard>(startingDeck);
        hand = new List<DialogueCard>();
        discard = new List<DialogueCard>();
        
        Shuffle();
        DrawHand(startingHandSize);
    }

    public void AddCardToDeck(DialogueCard card)
    {
        deck.Add(card);
    }

    public void DiscardCard(DialogueCard card)
    {
        hand.Remove(card);
        discard.Add(card);
    }

    public void DrawCard()
    {
        if (deck.Count == 0) return;
        
        int randomIndex = Random.Range(0, deck.Count);
        hand.Add(deck[randomIndex]);
        deck.RemoveAt(randomIndex);
    }

    public void DrawHand(int count)
    {
        for (int i = 0; i < count; i++)
        {
            DrawCard();
        }
    }

    public void ResetDeck()
    {
        // Hand and discard go back to deck
        deck.AddRange(hand);
        deck.AddRange(discard);
        hand.Clear();
        discard.Clear();
        
        Shuffle();
        DrawHand(startingHandSize);
    }

    private void Shuffle()
    {
        for (int i = deck.Count - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);
            (deck[i], deck[randomIndex]) = (deck[randomIndex], deck[i]);
        }
    }
    
    void Awake()
    {
        deck = new List<DialogueCard>(startingDeck);
        hand = new List<DialogueCard>();
        discard = new List<DialogueCard>();
    
        Shuffle();
        DrawHand(startingHandSize);
    }

    public List<DialogueCard> GetDraftOptions(int count)
    {
        List<DialogueCard> options = new List<DialogueCard>();
        List<DialogueCard> pool = new List<DialogueCard>(draftPool);
        
        // Remove cards already owned (deck + hand + discard)
        pool.RemoveAll(card => deck.Contains(card) || hand.Contains(card) || discard.Contains(card));
    
        for (int i = 0; i < count && pool.Count > 0; i++)
        {
            int randomIndex = Random.Range(0, pool.Count);
            options.Add(pool[randomIndex]);
            pool.RemoveAt(randomIndex);
        }
    
        return options;
    }
}