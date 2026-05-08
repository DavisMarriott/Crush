using UnityEngine;

public class AnimationTriggerThoughtBubble : MonoBehaviour
{
    public GameObject thoughtBubble;
    public Animator thoughtBubbleAnimator;
    

    void Start()
    {
        // Get the Animator component attached to the GameObject
        thoughtBubbleAnimator = thoughtBubble.GetComponent<Animator>();

        // Force a known starting state so the first On/Half/Off call has a valid _CYCLE state to transition from.
        // (Without this, the first loop's bubble can stay invisible because the default Animator state
        //  isn't named with the _CYCLE suffix the state-guards in this class look for.)
        thoughtBubbleAnimator.Play("ThoughtBubble_Half_CYCLE");
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
