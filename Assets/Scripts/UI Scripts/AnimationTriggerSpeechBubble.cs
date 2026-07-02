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

    // Wired to debug buttons via UnityEvent (OnClick) - keep this void signature so the wiring
    // stays valid. Code that needs to know if it actually opened calls TryShowSpeechBubble.
    public void SpeechBubbleShow()
    {
        TryShowSpeechBubble();
    }

    // Same open, but true only when it actually kicks off the open anim (from Off) - lets callers
    // hold the text a beat so it doesn't start typing before the bubble's done opening.
    public bool TryShowSpeechBubble()
    {
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);

        if (stateInfo.IsName("SpeechBubble_Off_CYCLE"))
        {
            animator.Play("SpeechBubble_Show");
            return true;
        }

        else if (stateInfo.IsName("SpeechBubble_On_CYCLE"))
        {
            animator.Play("SpeechBubble_On_CYCLE");
        }

        return false;
    }
    
    public void SpeechBubbleHide()
    {
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        
        if (stateInfo.IsName("SpeechBubble_On_CYCLE"))
        {
            animator.Play("SpeechBubble_Hide");
        }
        
        else if (stateInfo.IsName("SpeechBubble_Off_CYCLE"))
        {
            animator.Play("SpeechBubble_Off_CYCLE");
        }
        
    }
}