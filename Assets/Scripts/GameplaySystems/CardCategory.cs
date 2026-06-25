// How a card enters play. Separate from DialogueTag (which tags dialogue branches).
// Drives the starting deck and which draft a card shows up in.
public enum CardCategory
{
    BasicDeck,       // default — enters the normal draft pool from loop 3 (not the loop-2 multi-draft)
    StarterDeck,     // the loop-2 multi-draft pool; stays draftable in loop 3+ too
    DeckOnNewGame,   // seeds your deck at the start of a run
    Dance,           // the DANCE card (replaces the old isDance bool); loop-1 solo-DANCE draft unchanged
    ProgressGated,   // enters the draft pool once its own unlock condition is met (pass 2)
    Disabled,        // on ice / cut — never enters anything
}
