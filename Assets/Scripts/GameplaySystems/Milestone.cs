using UnityEngine;

public enum MilestoneConditionType
{
    PeakConfidenceReached,
    PeakCharmReached,
    CardsPlayedInLoopAtLeast,
    DialogueTagFired,
}

[System.Serializable]
public struct MilestoneCondition
{
    public MilestoneConditionType type;
    public int threshold;
    [Tooltip("Used only when type == DialogueTagFired")]
    public DialogueTag tag;
}

[CreateAssetMenu(menuName = "Crush/Milestone")]
public class Milestone : ScriptableObject
{
    public string milestoneName;
    public MilestoneCondition condition;

    [TextArea(2, 5)]
    public string[] reflectLines;

    [Tooltip("Optional. If set, applies this character upgrade when the milestone fires.")]
    public CharacterUpgrade characterUpgrade;
}
