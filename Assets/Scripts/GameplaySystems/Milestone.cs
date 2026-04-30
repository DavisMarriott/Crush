using UnityEngine;

public enum MilestoneConditionType
{
    PeakConfidenceReached,
    PeakCharmReached,
    CardsPlayedInLoopAtLeast,
}

[System.Serializable]
public struct MilestoneCondition
{
    public MilestoneConditionType type;
    public int threshold;
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
