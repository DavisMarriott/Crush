using System.Collections.Generic;

public struct LoopSnapshot
{
    public int finalConfidence;
    public int lastPeakConfidence;
    public int finalCharm;
    public int lastPeakCharm;
    public bool deathFromCharm;
    public List<string> cardsPlayed;
    public List<string> cardsUnplayed;
    public DialogueCard deathCard;
    public HashSet<DialogueTag> tagsFired;

    // which Luke branch fired for each card played this loop, in play order
    // (recorded by DialogueBox at branch-select time, copied here on death)
    public List<CardBranchRecord> branchesPlayed;
    // the branch that was playing when the death happened - drives per-branch death reactions
    public string deathBranchName;
}

[System.Serializable]
public struct CardBranchRecord
{
    public string cardName;
    public string branchName;

    public CardBranchRecord(string card, string branch)
    {
        cardName = card;
        branchName = branch;
    }
}
