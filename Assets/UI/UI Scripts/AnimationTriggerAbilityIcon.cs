using UnityEngine;

public class AnimationTriggerAbilityIcon : MonoBehaviour
{
    public Animator abilityIconAnimator;
    
    public void UseAbility()
    {
        abilityIconAnimator.Play("Icon_UseAbility");
       
    }
}
