using UnityEngine;
using System.Collections.Generic;
using TMPro;

public class DeckManager : MonoBehaviour
{
    [SerializeField] private CardUpgradeTracker upgradeTracker;
    [Tooltip("Master list of every card. The new-game deck + draft pools are derived from each card's Category.")]
    [SerializeField] public DialogueCard[] allCards;
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
        _deck = NewGameDeck();
        _hand = new List<DialogueCard>();
        _discard = new List<DialogueCard>();
        
        Shuffle();
        DrawHand(startingHandSize);
    }

    public void AddCardToDeck(DialogueCard card)
    {
        // Add an if statement that determines whether the card added is an upgraded card or not, then play the appropriate animation //
        _deck.Add(card);
        // animationTriggerIcon.DraftUpgradedCard();
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
            var eligible = _deck.FindAll(c => c.category != CardCategory.Dance);
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

        _deck = NewGameDeck();
        _hand = new List<DialogueCard>();
        _discard = new List<DialogueCard>();

        Shuffle();
        DrawHand(startingHandSize);
    }

    private GameProgression _gameProgression;

    // loop-3+ main draft: Starter + Basic cards (+ unlocked Progress-Gated — pass 2), minus what you own.
    public List<DialogueCard> GetDraftOptions(int count)
    {
        return PickFromPool(MainDraftPool(), count);
    }

    // loop-2 multi-draft ("starter deck"): Starter cards only.
    public List<DialogueCard> GetStarterDraftOptions(int count)
    {
        return PickFromPool(StarterPool(), count);
    }

    // a card you don't already own (deck + hand + discard)
    private bool IsDraftable(DialogueCard c)
        => c != null && !_deck.Contains(c) && !_hand.Contains(c) && !_discard.Contains(c);

    private List<DialogueCard> StarterPool()
    {
        var list = new List<DialogueCard>();
        if (allCards == null) return list;
        foreach (var c in allCards)
            if (IsDraftable(c) && c.category == CardCategory.StarterDeck) list.Add(c);
        return list;
    }

    private List<DialogueCard> MainDraftPool()
    {
        var list = new List<DialogueCard>();
        if (allCards == null) return list;
        foreach (var c in allCards)
        {
            if (!IsDraftable(c)) continue;
            if (c.category == CardCategory.StarterDeck || c.category == CardCategory.BasicDeck) list.Add(c);
            else if (c.category == CardCategory.ProgressGated && ProgressUnlocked(c)) list.Add(c);
        }
        return list;
    }

    // a Progress-Gated card's unlock condition is met (sticky — once true this run it stays true)
    private bool ProgressUnlocked(DialogueCard c)
    {
        if (_gameProgression == null) return false;
        switch (c.unlockCondition.type)
        {
            case DraftUnlockType.BranchTag:
                return _gameProgression.HasFiredTagThisRun(c.unlockCondition.tag);
            case DraftUnlockType.LoopReached:
                return _gameProgression.loopCount >= c.unlockCondition.loop;
        }
        return false;
    }

    private List<DialogueCard> PickFromPool(List<DialogueCard> pool, int count)
    {
        var options = new List<DialogueCard>();
        for (int i = 0; i < count && pool.Count > 0; i++)
        {
            int randomIndex = Random.Range(0, pool.Count);
            options.Add(pool[randomIndex]);
            pool.RemoveAt(randomIndex);
        }
        return options;
    }

    // cards that seed your deck at the start of a run
    private List<DialogueCard> NewGameDeck()
    {
        var list = new List<DialogueCard>();
        if (allCards == null) return list;
        foreach (var c in allCards)
            if (c != null && c.category == CardCategory.DeckOnNewGame) list.Add(c);
        return list;
    }

    // is any draftable card configured at all? (drives whether the draft phase runs — replaces the old draftPool.Length check)
    public bool HasDraftPool()
    {
        if (allCards == null) return false;
        foreach (var c in allCards)
            if (c != null && (c.category == CardCategory.StarterDeck
                              || c.category == CardCategory.BasicDeck
                              || c.category == CardCategory.ProgressGated)) return true;
        return false;
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