using UnityEngine;

public class AnimationTriggerThoughtBubble : MonoBehaviour
{
    public GameObject thoughtBubble;
    private Animator thoughtBubbleAnimator;

    void Start()
    {
        // Get the Animator component attached to the GameObject
        thoughtBubbleAnimator = thoughtBubble.GetComponent<Animator>();
    }

    public void ThoughtBubbleOn()
    {
        thoughtBubbleAnimator.Play("ThoughtBubble_Off_to_Full");
    }
    
    public void ThoughtBubbleOff()
    {
        thoughtBubbleAnimator.Play("ThoughtBubble_Full_to_Off");
    }
}
