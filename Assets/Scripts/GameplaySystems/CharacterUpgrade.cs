using UnityEngine;

public enum CharacterUpgradeEffect
{
    IncreaseStartingConfidence,
    IncreaseStartingHandSize,
}

[CreateAssetMenu(menuName = "Crush/Character Upgrade")]
public class CharacterUpgrade : ScriptableObject
{
    public string upgradeName;
    public CharacterUpgradeEffect effect;
    public int intValue;

    [TextArea(1, 3)]
    public string bannerText;
}
