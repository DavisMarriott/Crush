using UnityEngine;

public class AnimationTriggerIcon : MonoBehaviour
{
    public Animator iconAnimator;
    public Animator confidentWalkAnimator;
    public GameProgression gameProgression;
    public DeckSizeIndicator deckSizeIndicator;
    public int confidentWalkLevelTemp;
    
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

    public void DraftUpgradedCard()
    {
        iconAnimator.Play("Icon_DeckSize_Upgrade01");
    }

    public void UpdateDeckSizeIndicator()
    {
        deckSizeIndicator.UpdateDeckSize();
    }
    
    
    // CONFIDENT WALK specific methods //
    
    // Set the correct state when icon appears //
    public void ConfidentWalkSet()
    {
        if /* (confidentWalkLevelTemp == 1) */ (gameProgression.approachDrainDisabledCount == 1)
        {
            confidentWalkAnimator.Play("Icon_ConfidentWalk_Level01_Start", 0);
        }
        
        else if /* (confidentWalkLevelTemp == 2) */ (gameProgression.approachDrainDisabledCount == 2)
        {
            confidentWalkAnimator.Play("Icon_ConfidentWalk_Level02_Start", 0);
        }
        
        else if /* (confidentWalkLevelTemp == 3) */ (gameProgression.approachDrainDisabledCount == 3)
        {
            confidentWalkAnimator.Play("Icon_ConfidentWalk_Level03_Start", 0);
        }
    }
    
    // Trigger correct upgrade animation //
    public void ConfidentWalkUpgrade()
    {
        confidentWalkAnimator.Play("Icon_ConfidentWalk_Level01_to_Level02", 0);
        confidentWalkAnimator.Play("Icon_ConfidentWalk_Level02_to_Level03", 0);
    }
    
}
