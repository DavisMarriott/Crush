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
}


