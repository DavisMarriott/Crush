using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class AnimationTriggerThoughtBubble : MonoBehaviour
{
    public GameObject thoughtBubble;
    public Animator thoughtBubbleAnimator;
    public GameObject thoughtBubbleConversationLocator;
    public GameObject thoughtBubbleHallwayLocator;
    
    public RectTransform thoughtBubbleTransform;
    public RectTransform hallwayTransform;
    public RectTransform conversationTransform;
    
    public float duration;
    
    // private float elapsedTime = 0f;
    

    void Start()
    {
        // Get the Animator component attached to the GameObject
        thoughtBubbleAnimator = thoughtBubble.GetComponent<Animator>();

        thoughtBubbleTransform = thoughtBubble.GetComponent<RectTransform>();
        hallwayTransform = thoughtBubbleHallwayLocator.GetComponent<RectTransform>();
        conversationTransform = thoughtBubbleConversationLocator.GetComponent<RectTransform>();
        
        // Force a known starting state so the first On/Half/Off call has a valid _CYCLE state to transition from.
        // (Without this, the first loop's bubble can stay invisible because the default Animator state
        //  isn't named with the _CYCLE suffix the state-guards in this class look for.)
        // thoughtBubbleAnimator.Play("ThoughtBubble_Half_CYCLE");
    }

    // Wired to debug buttons via UnityEvent (OnClick) - keep this void signature so the wiring
    // stays valid. Code that needs to know if it actually opened calls TryThoughtBubbleOn.
    public void ThoughtBubbleOn()
    {
        TryThoughtBubbleOn();
    }

    // Same open, but true when it actually starts an open transition (from Half or Off) so
    // callers can wait for the bubble to finish opening before text starts typing.
    public bool TryThoughtBubbleOn()
    {
        AnimatorStateInfo stateInfo = thoughtBubbleAnimator.GetCurrentAnimatorStateInfo(0);

        if (stateInfo.IsName("ThoughtBubble_Half_CYCLE"))
        {
            thoughtBubbleAnimator.Play("ThoughtBubble_Half_to_Full");
            return true;
        }

        else if (stateInfo.IsName("ThoughtBubble_Off_CYCLE"))
        {
            thoughtBubbleAnimator.Play("ThoughtBubble_Off_to_Full");
            return true;
        }

        return false;
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
    
    
    
    // Reposition ThoughtBubble in conversation
    public void ThoughtBubblePositionConversation()
    {
        // thoughtBubbleTransform.anchoredPosition3D = conversationTransform.anchoredPosition3D;
        // thoughtBubbleTransform.localScale = conversationTransform.localScale;
        StartCoroutine(HallwayToConversation());
        
    }

    public void ThoughtBubblePositionHallway()
    {
        // thoughtBubbleTransform.anchoredPosition3D = hallwayTransform.anchoredPosition3D;
        // thoughtBubbleTransform.localScale = hallwayTransform.localScale;
        StartCoroutine(ConversationToHallway());
    }
    
    
    IEnumerator HallwayToConversation()
    {
        float timeElapsed = 0;

        while (timeElapsed < duration)
        {
            // Calculate progress fraction (0.0 to 1.0)
            float t = timeElapsed / duration; 
            
            // Apply interpolation
            thoughtBubbleTransform.anchoredPosition3D = Vector3.Lerp(hallwayTransform.anchoredPosition3D, conversationTransform.anchoredPosition3D, Mathf.SmoothStep(0f, 1f, t));
            thoughtBubbleTransform.localScale = Vector3.Lerp(hallwayTransform.localScale, conversationTransform.localScale, Mathf.SmoothStep(0f, 1f, t));
            
            // Advance time based on frame completion rate
            timeElapsed += Time.deltaTime;
            yield return null;
        }

        // Ensure the object snaps exactly to the destination at the end
        thoughtBubbleTransform.anchoredPosition3D = conversationTransform.anchoredPosition3D; 
    }
    
    IEnumerator ConversationToHallway()
    {
        float timeElapsed = 0;

        while (timeElapsed < duration)
        {
            // Calculate progress fraction (0.0 to 1.0)
            float t = timeElapsed / duration; 
            
            // Apply interpolation
            thoughtBubbleTransform.anchoredPosition3D = Vector3.Lerp(conversationTransform.anchoredPosition3D, hallwayTransform.anchoredPosition3D, Mathf.SmoothStep(0f, 1f, t));
            thoughtBubbleTransform.localScale = Vector3.Lerp(conversationTransform.localScale, hallwayTransform.localScale, Mathf.SmoothStep(0f, 1f, t));
            
            // Advance time based on frame completion rate
            timeElapsed += Time.deltaTime;
            yield return null;
        }

        // Ensure the object snaps exactly to the destination at the end
        thoughtBubbleTransform.anchoredPosition3D = hallwayTransform.anchoredPosition3D; 
    }
    
    
}
