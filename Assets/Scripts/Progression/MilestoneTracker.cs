using UnityEngine;
using System.Collections.Generic;

public class MilestoneTracker : MonoBehaviour
{
    [SerializeField] private Milestone[] milestones;
    [SerializeField] private ConfidenceState confidenceState;
    [SerializeField] private DeckManager deckManager;
    [SerializeField] private UpgradeBanner upgradeBanner;

    private HashSet<Milestone> completed = new HashSet<Milestone>();

    public Milestone GetTriggeredMilestone(LoopSnapshot snapshot, int loopCount)
    {
        if (milestones == null) return null;
        foreach (var m in milestones)
        {
            if (m == null) continue;
            if (completed.Contains(m)) continue;
            if (Matches(m.condition, snapshot, loopCount)) return m;
        }
        return null;
    }

    private bool Matches(MilestoneCondition cond, LoopSnapshot snapshot, int loopCount)
    {
        switch (cond.type)
        {
            case MilestoneConditionType.PeakConfidenceReached:
                return snapshot.lastPeakConfidence >= cond.threshold;
            case MilestoneConditionType.PeakCharmReached:
                return snapshot.lastPeakCharm >= cond.threshold;
            case MilestoneConditionType.CardsPlayedInLoopAtLeast:
                return snapshot.cardsPlayed != null && snapshot.cardsPlayed.Count >= cond.threshold;
        }
        return false;
    }

    public void ApplyCharacterUpgrade(CharacterUpgrade upgrade)
    {
        if (upgrade == null) return;

        switch (upgrade.effect)
        {
            case CharacterUpgradeEffect.IncreaseStartingConfidence:
                confidenceState.startingConfidence += upgrade.intValue;
                break;
            case CharacterUpgradeEffect.IncreaseStartingHandSize:
                deckManager.startingHandSize += upgrade.intValue;
                break;
        }

        if (upgradeBanner != null)
        {
            upgradeBanner.Show(string.IsNullOrEmpty(upgrade.bannerText) ? upgrade.upgradeName : upgrade.bannerText);
        }
    }

    public void MarkComplete(Milestone m)
    {
        if (m != null) completed.Add(m);
    }
}
