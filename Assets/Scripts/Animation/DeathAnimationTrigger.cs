using UnityEngine;

public class DeathAnimationTrigger : MonoBehaviour
{
    public AnimationTriggerPlayer animationTriggerPlayer;

    public void TriggerPlayerDeathAnimation()
    {
        animationTriggerPlayer.EnterDeathOne();
    }
    
}
