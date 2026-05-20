using UnityEngine;

[CreateAssetMenu(menuName = "Crush/Reflect Branch")]
public class ReflectBranch : ScriptableObject
{
    [TextArea(2, 5)]
    public string[] lines;

    [Header("Commit lines — final hype-up before the hallway approach (locker-close beat). Plays after draft, before hallway. Per-loop scripted, usually 1-2 lines.")]
    [TextArea(1, 3)]
    public string[] commitLines;

    [Header("Scripted (loop-keyed) vs. conditional")]
    [Tooltip("True for the per-loop scripted reflects (LoopReflect_01..05). The importer sets this on Loop tabs. Scripted branches take priority over milestones and conditional branches in DeathRespawn — they're the narrative spine.")]
    public bool isScripted = false;

    [Header("Loop range (inclusive)")]
    public int minLoop = 0;
    public int maxLoop = 999;

    [Header("Final confidence range (inclusive)")]
    public int minFinalConfidence = 0;
    public int maxFinalConfidence = 999;

    [Header("Peak confidence range (inclusive)")]
    public int minPeakConfidence = 0;
    public int maxPeakConfidence = 999;

    [Header("Final charm range (inclusive)")]
    public int minFinalCharm = 0;
    public int maxFinalCharm = 999;

    [Header("Peak charm range (inclusive)")]
    public int minPeakCharm = 0;
    public int maxPeakCharm = 999;

    [Header("Death specifics (leave null / false to ignore)")]
    public DialogueCard requiresDeathCard;
    public bool requiresDeathFromCharm;

    [Header("Card history (leave empty to ignore)")]
    //require all listed cards must be in these fields
    public DialogueCard[] requiresCardsPlayed;
    public DialogueCard[] requiresCardsUnplayed;
}