using UnityEngine;
using System.Collections.Generic;

public class CardUpgradeTracker : MonoBehaviour
{
    private Dictionary<DialogueCard, int> useCount = new();
    private Dictionary<DialogueCard, DialogueCardUpgrade> appliedUpgrades = new();
    public AnimationTriggerIcon animationTriggerIcon;
    public GameProgression gameProgression;   // for the BranchTag condition (reads last loop's fired tags)
    public void NoteCardPlayed(DialogueCard card)
    {
        if (useCount.ContainsKey(card))
        {
            useCount[card]++;
        }
        else
        {
            useCount[card] = 1;
        }
        if (IsUpgradeAvailable(card))
        {
            Debug.Log($"Upgrade available for {card.name}");
        }
    }
    
    public bool IsUpgradeAvailable(DialogueCard card)
    {
        if (card.availableUpgrades == null || card.availableUpgrades.Length == 0) return false;
        // one upgrade per card (v1) - todo: additional upgrades/card
        if (appliedUpgrades.ContainsKey(card)) return false;

        switch (card.upgradeCondition.type)
        {
            case UpgradeConditionType.PlayThreshold:
                return useCount.ContainsKey(card) && useCount[card] >= card.upgradeCondition.playThreshold;

            case UpgradeConditionType.BranchTag:
                // unlocks in the draft right after a loop where the tag fired (same source milestones read)
                // lastLoop is a struct (never null); a default/unset one has tagsFired == null, which the next check catches
                return gameProgression != null
                       && gameProgression.lastLoop.tagsFired != null
                       && gameProgression.lastLoop.tagsFired.Contains(card.upgradeCondition.tag);
        }
        return false;
    }
    
    public void ApplyUpgrade(DialogueCard card, DialogueCardUpgrade upgrade)
    {
        appliedUpgrades[card] = upgrade;
        animationTriggerIcon.DraftUpgradedCard();
    }
    
    public DialogueCardUpgrade GetAppliedUpgrade(DialogueCard card)
    {
        if (appliedUpgrades.ContainsKey(card)) return appliedUpgrades[card];
        return null;
    }
    
    //grabs cost delta on upgrade to get new effective cost
    public int GetEffectiveCost(DialogueCard card)
    {
        var upgrade = GetAppliedUpgrade(card);
        if (upgrade == null) return card.cost;
        return card.cost + upgrade.costDelta;
    }
}
