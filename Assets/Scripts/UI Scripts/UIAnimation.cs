using UnityEngine;
using UnityEngine.UI;

public class UIAnimation : MonoBehaviour
{
    public Animator uiAnimator;
    public string triggerName = "PlayAnimation";

    // Call this function from the EventTrigger
    public void PlayUIAnimation()
    {
        if (uiAnimator != null)
        {
            uiAnimator.SetTrigger(triggerName);
        }
    }
}