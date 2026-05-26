using UnityEngine;

public class AnimationTriggerLockerInside : MonoBehaviour
{
    
    public Animator lockerInsideAnimator;

    public void LockerInsideOpen()
    {
        lockerInsideAnimator.Play("LockerInside_Open");
    }
    
    public void LockerInsideOpened()
    {
        lockerInsideAnimator.Play("LockerInside_Open_CYCLE");
    }
    
    public void LockerInsideClose()
    {
        lockerInsideAnimator.Play("LockerInside_Close");
    }
    
    public void LockerInsideClosed()
    {
        lockerInsideAnimator.Play("LockerInside_Closed_CYCLE");
    }
    
}
