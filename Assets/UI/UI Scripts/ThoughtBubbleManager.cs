using UnityEngine;

public class ThoughtBubbleManager : MonoBehaviour
{
    public AnimationTriggerThoughtBubble animationTriggerThoughtBubble;
    public GameObject thoughtBubble;


    public void EnableThoughtBubble()
    {
        thoughtBubble.SetActive(true);
    }
    
    public void ShowThoughtBubble()
    {
        animationTriggerThoughtBubble.ThoughtBubbleOn();
    }
    
    public void HideThoughtBubble()
    {
        animationTriggerThoughtBubble.ThoughtBubbleOff();
    }
    
    public void DisableThoughtBubble()
    {
        thoughtBubble.SetActive(false);
    }
    
    
    
}
