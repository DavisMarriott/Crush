using UnityEngine;

public class AnimationTriggerIcon : MonoBehaviour
{
    public Animator iconAnimator;
    public DeckSizeIndicator deckSizeIndicator;
    
    public void UseAbility()
    {
        iconAnimator.Play("Icon_UseAbility");
       
    }
    
    public void DeckSizeAddOne()
    {
        iconAnimator.Play("Icon_DeckSize_AddOneCard");
        
    }
    
    public void DeckSizeMinusOne()
    {
        iconAnimator.Play("Icon_DeckSize_PlayCard");
        
    }

    public void UpdateDeckSizeIndicator()
    {
        deckSizeIndicator.UpdateDeckSize();
    }
    
}
