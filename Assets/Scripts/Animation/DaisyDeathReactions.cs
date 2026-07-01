using UnityEngine;

public class DaisyDeathReactions : MonoBehaviour
{
    public AnimationTriggerCrush animationTriggerCrush;

    public void DaisyDeathReaction()
    {
        animationTriggerCrush.DeathReaction();
    }
}
