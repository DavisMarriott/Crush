using UnityEngine;

public class AnimationTriggerThoughtBubble : MonoBehaviour
{
    public GameObject thoughtBubble;
    public Animator thoughtBubbleAnimator;
    

    void Start()
    {
        // Get the Animator component attached to the GameObject
        thoughtBubbleAnimator = thoughtBubble.GetComponent<Animator>();
    }

    public void ThoughtBubbleOn()
    {
        AnimatorStateInfo stateInfo = thoughtBubbleAnimator.GetCurrentAnimatorStateInfo(0);

        if (stateInfo.IsName("ThoughtBubble_Half_CYCLE"))
        {
            thoughtBubbleAnimator.Play("ThoughtBubble_Half_to_Full");
        }
        
        else if (stateInfo.IsName("ThoughtBubble_Off_CYCLE"))
        {
            thoughtBubbleAnimator.Play("ThoughtBubble_Off_to_Full");
        }
    }
    
    public void ThoughtBubbleHalf()
    {
        AnimatorStateInfo stateInfo = thoughtBubbleAnimator.GetCurrentAnimatorStateInfo(0);
        
        if (stateInfo.IsName("ThoughtBubble_Off_CYCLE"))
        {
            thoughtBubbleAnimator.Play("ThoughtBubble_Off_to_Half");
        }
        
        if (stateInfo.IsName("ThoughtBubble_Full_CYCLE"))
        {
            thoughtBubbleAnimator.Play("ThoughtBubble_Full_to_Half");
        }
    }
    
    
    public void ThoughtBubbleOff()
    {
        AnimatorStateInfo stateInfo = thoughtBubbleAnimator.GetCurrentAnimatorStateInfo(0);

        if (stateInfo.IsName("ThoughtBubble_Full_CYCLE"))
        {
            thoughtBubbleAnimator.Play("ThoughtBubble_Full_to_Off");
        }
        
        if (stateInfo.IsName("ThoughtBubble_Half_CYCLE"))
        {
            thoughtBubbleAnimator.Play("ThoughtBubble_Half_to_Off");
        }
        
    }
}
