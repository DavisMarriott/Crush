using UnityEngine;

// Content pools for the base-loop (non-scripted) reflect formula:
//   1. reflect on death  -> genericDeathReactions (fallback when the card/branch has none left)
//   2. comment on progress -> milestone lines or conditional branches (not in this SO)
//   3. plan/draft        -> draftIntroLines (one random line before the draft opens)
//   4. commit            -> commitLineGroups (random group after draft, before hallway)
// Create via Crush > Base Reflect Pools and assign to ReflectSelfTalk. All pools optional -
// empty/missing pools just skip their beat.
[CreateAssetMenu(menuName = "Crush/Base Reflect Pools")]
public class BaseReflectPools : ScriptableObject
{
    [Header("Generic death reactions — random pick when the death card/branch has no (unexhausted) specific reaction.")]
    public ReflectLineGroup[] genericDeathReactions;

    [Header("Draft intros — one random line right before the draft opens (\"Okay, what should I say this time…\").")]
    [TextArea(1, 2)]
    public string[] draftIntroLines;

    [Header("Base-loop commit lines — random group for the locker-close beat (scripted loops use their own commitLines).")]
    public ReflectLineGroup[] commitLineGroups;
}
