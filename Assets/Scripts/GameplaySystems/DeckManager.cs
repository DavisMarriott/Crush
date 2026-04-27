using UnityEngine;
using System.Collections.Generic;

public class DeckManager : MonoBehaviour
{
    [SerializeField] private CardUpgradeTracker upgradeTracker;
    [SerializeField] public DialogueCard[] draftPool;
    [SerializeField] private DialogueCard[] startingDeck;
    public int startingHandSize = 4;
    
    public struct DraftableUpgrade
    {
        public DialogueCard card;
        public DialogueCardUpgrade upgrade;
    }
    private List<DialogueCard> _deck;
    private List<DialogueCard> _hand;
    private List<DialogueCard> _discard;
    public DialogueCard LastPlayedCard { get; private set; }
    
    public List<DialogueCard> Deck => _deck;
    public List<DialogueCard> Hand => _hand;
    public List<DialogueCard> Discard => _discard;

    void Start()
    {
        _deck = new List<DialogueCard>(startingDeck);
        _hand = new List<DialogueCard>();
        _discard = new List<DialogueCard>();
        
        Shuffle();
        DrawHand(startingHandSize);
    }

    public void AddCardToDeck(DialogueCard card)
    {
        _deck.Add(card);
    }

    public void DiscardCard(DialogueCard card)
    {
        LastPlayedCard = card;
        upgradeTracker.NoteCardPlayed(card);
        _hand.Remove(card);
        _discard.Add(card);
    }

    public void DrawCard()
    {
        if (_deck.Count == 0) return;
        
        int randomIndex = Random.Range(0, _deck.Count);
        _hand.Add(_deck[randomIndex]);
        _deck.RemoveAt(randomIndex);
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
        _deck.AddRange(_hand);
        _deck.AddRange(_discard);
        _hand.Clear();
        _discard.Clear();
        
        Shuffle();
        DrawHand(startingHandSize);
    }

    private void Shuffle()
    {
        for (int i = _deck.Count - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);
            (_deck[i], _deck[randomIndex]) = (_deck[randomIndex], _deck[i]);
        }
    }
    
    void Awake()
    {
        _deck = new List<DialogueCard>(startingDeck);
        _hand = new List<DialogueCard>();
        _discard = new List<DialogueCard>();
    
        Shuffle();
        DrawHand(startingHandSize);
    }

    public List<DialogueCard> GetDraftOptions(int count)
    {
        List<DialogueCard> options = new List<DialogueCard>();
        List<DialogueCard> pool = new List<DialogueCard>(draftPool);
        
        // Remove cards already owned (deck + hand + discard)
        pool.RemoveAll(card => _deck.Contains(card) || _hand.Contains(card) || _discard.Contains(card));
    
        for (int i = 0; i < count && pool.Count > 0; i++)
        {
            int randomIndex = Random.Range(0, pool.Count);
            options.Add(pool[randomIndex]);
            pool.RemoveAt(randomIndex);
        }
    
        return options;
    }
    public List<DraftableUpgrade> GetAvailableUpgrades()
    {
        var result = new List<DraftableUpgrade>();
        foreach (var card in _deck) AddIfAvailable(card, result);
        foreach (var card in _hand) AddIfAvailable(card, result);
        foreach (var card in _discard) AddIfAvailable(card, result);
        return result;
    }

    private void AddIfAvailable(DialogueCard card, List<DraftableUpgrade> list)
    {
        if (!upgradeTracker.IsUpgradeAvailable(card)) return;
        foreach (var upgrade in card.availableUpgrades)
        {
            list.Add(new DraftableUpgrade { card = card, upgrade = upgrade });
        }
    }
}