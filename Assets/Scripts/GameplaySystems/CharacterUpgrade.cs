using UnityEngine;

public enum CharacterUpgradeEffect
{
    IncreaseStartingConfidence,
    IncreaseStartingHandSize,
    RemoveApproachTriggerDrain,
}

[CreateAssetMenu(menuName = "Crush/Character Upgrade")]
public class CharacterUpgrade : ScriptableObject
{
    public string upgradeName;
    public CharacterUpgradeEffect effect;
    public int intValue;

    [TextArea(1, 3)]
    public string bannerText;

    // The card-style icon that pops when this upgrade fires. Player clicks it to activate -
    // reuses the draft card's Button + hover rig. Activate animation lives on the prefab.
    public UnityEngine.UI.Button iconPrefab;
}
