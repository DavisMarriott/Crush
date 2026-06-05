using UnityEngine;
using System.Collections.Generic;
using TMPro;

public class DeckManager : MonoBehaviour
{
    [SerializeField] private CardUpgradeTracker upgradeTracker;
    [SerializeField] public DialogueCard[] draftPool;
    [SerializeField] private DialogueCard[] startingDeck;
    public int startingHandSize = 4;
    [SerializeField] public int deckSize;
    public AnimationTriggerIcon animationTriggerIcon;
    
    public struct DraftableUpgrade
    {
        public DialogueCard card;
        public DialogueCardUpgrade upgrade;
    }
    private List<DialogueCard> _deck;
    private List<DialogueCard> _hand;
    private List<DialogueCard> _discard;
    public DialogueCard LastPlayedCard { get; private set; }

    private HashSet<DialogueTag> _tagsFiredThisLoop = new HashSet<DialogueTag>();

    public List<DialogueCard> Deck => _deck;
    public List<DialogueCard> Hand => _hand;
    public List<DialogueCard> Discard => _discard;
    public HashSet<DialogueTag> TagsFiredThisLoop => _tagsFiredThisLoop;

    public void RegisterTags(DialogueTag[] tags)
    {
        if (tags == null) return;
        foreach (var tag in tags)
        {
            _tagsFiredThisLoop.Add(tag);
        }
    }

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
        animationTriggerIcon.DeckSizeAddOne();
    }

    public void DiscardCard(DialogueCard card)
    {
        LastPlayedCard = card;
        upgradeTracker.NoteCardPlayed(card);
        _hand.Remove(card);
        _discard.Add(card);
        animationTriggerIcon.DeckSizeMinusOne();
    }

    public void DrawCard()
    {
        if (_deck.Count == 0) return;
        
        int randomIndex = Random.Range(0, _deck.Count);
        _hand.Add(_deck[randomIndex]);
        _deck.RemoveAt(randomIndex);
        // animationTriggerIcon.DeckSizeAddOne();
    }

    public void DrawHand(int count)
    {
        for (int i = 0; i < count; i++)
        {
            DrawCard();
        }
    }

    // Opening-hand draw - from loop 2 on, DANCE never starts in the hand (a 3-card hand with
    // DANCE stuck in it plays like a 2-card hand all loop - 06/04 note). It stays in the deck
    // and any later draw can pull it. Loop 1 is exempt (the loop-1 DANCE draft should land in hand).
    private void DrawOpeningHand(int count)
    {
        int loop = _gameProgression != null ? _gameProgression.loopCount : 1;
        if (loop < 2)
        {
            DrawHand(count);
            return;
        }

        for (int i = 0; i < count; i++)
        {
            if (_deck.Count == 0) return;
            var eligible = _deck.FindAll(c => !c.isDance);
            if (eligible.Count == 0) return;   // only DANCE left - short hand beats breaking the rule
            var card = eligible[Random.Range(0, eligible.Count)];
            _hand.Add(card);
            _deck.Remove(card);
        }
    }

    public void ResetDeck()
    {
        // Hand and discard go back to deck
        _deck.AddRange(_hand);
        _deck.AddRange(_discard);
        _hand.Clear();
        _discard.Clear();

        // Clear per-loop tag state so next loop starts fresh
        _tagsFiredThisLoop.Clear();

        Shuffle();
        DrawOpeningHand(startingHandSize);
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
        // found at runtime so no scene re-wiring needed - used by DrawOpeningHand's loop check
        _gameProgression = FindFirstObjectByType<GameProgression>();

        _deck = new List<DialogueCard>(startingDeck);
        _hand = new List<DialogueCard>();
        _discard = new List<DialogueCard>();

        Shuffle();
        DrawHand(startingHandSize);
    }

    private GameProgression _gameProgression;

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

    void Update()
    {
         deckSize = _deck.Count + _hand.Count;
    }
}