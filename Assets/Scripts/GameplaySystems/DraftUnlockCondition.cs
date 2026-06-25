using UnityEngine;

// What unlocks a Progress-Gated card into the draft pool. Mirrors UpgradeCondition's shape.
public enum DraftUnlockType
{
    BranchTag,     // a branch with this tag has fired this run
    LoopReached,   // reached this loop number
}

[System.Serializable]
public struct DraftUnlockCondition
{
    public DraftUnlockType type;

    [Tooltip("BranchTag: unlocks once a branch with this tag fires (sticky for the rest of the run).")]
    public DialogueTag tag;

    [Tooltip("LoopReached: unlocks once you reach this loop.")]
    public int loop;
}
