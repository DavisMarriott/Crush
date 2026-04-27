using UnityEngine;

public class AnimationTriggerSpeechBubble : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private Animator animator;
    void Start()
    {
        // Get the Animator component attached to the GameObject
        animator = GetComponent<Animator>();
        
    }

    public void SpeechBubbleShow()
    {
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        
        if (stateInfo.IsName("SpeechBubble_Off_CYCLE"))
        {
            animator.Play("SpeechBubble_Show");
        }
        
    }
    
    public void SpeechBubbleHide()
    {
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        
        if (stateInfo.IsName("SpeechBubble_On_CYCLE"))
        {
            animator.Play("SpeechBubble_Hide");
        }
        
    }
}