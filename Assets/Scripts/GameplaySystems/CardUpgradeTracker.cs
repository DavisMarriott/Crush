using UnityEngine;
using System.Collections.Generic;

public class CardUpgradeTracker : MonoBehaviour
{
    private Dictionary<DialogueCard, int> useCount = new();
    private Dictionary<DialogueCard, DialogueCardUpgrade> appliedUpgrades = new();
    
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
        if (!useCount.ContainsKey(card)) return false;
        if (useCount[card] < card.upgradeThreshold) return false;
        if (card.availableUpgrades == null || card.availableUpgrades.Length == 0) return false;
        //if card is already upgraded, return false (v1 of card upgrades is only one upgrade per card)
        //todo: build additional upgrades/card
        if (appliedUpgrades.ContainsKey(card)) return false;
        return true;
    }
    
    public void ApplyUpgrade(DialogueCard card, DialogueCardUpgrade upgrade)
    {
        appliedUpgrades[card] = upgrade;
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
