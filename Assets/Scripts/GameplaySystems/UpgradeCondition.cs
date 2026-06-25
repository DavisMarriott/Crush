using UnityEngine;

// What unlocks a card's upgrade.
public enum UpgradeConditionType
{
    PlayThreshold,   // played this card N times
    BranchTag,       // a branch with this tag fired
}

[System.Serializable]
public struct UpgradeCondition
{
    public UpgradeConditionType type;

    [Tooltip("PlayThreshold: how many times this card must be played before the upgrade is offered.")]
    public int playThreshold;

    [Tooltip("BranchTag: the upgrade unlocks once a branch with this tag fires.")]
    public DialogueTag tag;
}
