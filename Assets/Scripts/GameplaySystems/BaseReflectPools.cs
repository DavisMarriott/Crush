using UnityEngine;

// Content pools for the base-loop (non-scripted) reflect formula:
//   1. reflect on death  -> genericDeathReactions (fallback when the card/branch has none left)
//   2. comment on progress -> milestone lines or conditional branches (not in this SO)
//   3. plan/draft        -> draftIntroPools (loop-gated; random eligible group before the draft)
//   4. commit            -> commitPools (loop-gated; random eligible group after draft, before hallway)
// Create via Crush > Base Reflect Pools and assign to ReflectSelfTalk. All pools optional -
// empty/missing pools just skip their beat.
[CreateAssetMenu(menuName = "Crush/Base Reflect Pools")]
public class BaseReflectPools : ScriptableObject
{
    [Header("Generic death reactions — random pick when the death card/branch has no (unexhausted) specific reaction.")]
    public ReflectLineGroup[] genericDeathReactions;

    [Header("Repeat-death reactions — used instead of generic when you've died 2+ times on the SAME card+branch this run.")]
    public ReflectLineGroup[] repeatDeathReactions;

    [Header("Draft intros — loop-gated pools; a random eligible group plays right before the draft opens.")]
    public ConditionalReflectGroups[] draftIntroPools;

    [Header("Base-loop commit lines — loop-gated pools; a random eligible group plays at the locker-close beat (scripted loops use their own commitLines).")]
    public ConditionalReflectGroups[] commitPools;
}

// A loop-range-gated set of line groups. Importer fills minLoop/maxLoop from the tab's
// "if LoopCount …" heading (Generic / no condition = 0..999). Runtime pools all groups
// from every entry whose loop range matches, then picks one at random.
[System.Serializable]
public class ConditionalReflectGroups
{
    public int minLoop = 0;
    public int maxLoop = 999;
    public ReflectLineGroup[] groups;
}
