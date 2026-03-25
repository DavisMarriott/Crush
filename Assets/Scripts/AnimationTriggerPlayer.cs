using UnityEngine;

public class AnimationTriggerPlayer : MonoBehaviour
{
    // I think this just lets us access the animator attached to the game object (our character)
    private Animator animator;
    // This lets us assign a ConfidenceIncrementer through inspector. This was a temp solution to demo the concept.
    // We'll probably want to replace this with our health/confidence script.
    //public ConfidenceIncrementer confidenceIncrementer;

    void Start()
    {
        // Get the Animator component attached to the GameObject
        animator = GetComponent<Animator>();
    }
    
    // Below is the start of writing custom methods to trigger animations with custom transition animations.
    // In the demo scene "CharacterSetup_Protagonist" only WALK and DEFAULT buttons use these methods.
    // The other buttons use standard Unity method "Animator.Play (String)" and the animation name is then typed into inspector.
    // That should ultimately be replaced with this new method.
    
    // WALK //
    public void Walk()
    {
        // This gets the current animation state info
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        
        // If we're in Default, Walk() will play the transition animation "Player_Default_to_Walk".
        // The way the animator is set up, Player_Walk will automatically play after.
        // We can use this method for all transition animations. 
        if (stateInfo.IsName("Player_Start_CYCLE"))
        {
            animator.Play("Player_Start_to_Walk", 0);
        }
        
        if (stateInfo.IsName("Player_State01_CYCLE"))
        {
            animator.Play("Player_State01_to_Walk", 0);
        }
        
        if ( (stateInfo.IsName("Player_State01_to_Walk")) || (stateInfo.IsName("Player_Walk_to_State01")) || (stateInfo.IsName("Player_Start_to_Walk")) )
        {
            animator.Play("Player_Walk_CYCLE", 0);
        }
    }
    
    // DEFAULT //
    public void Default()
    {
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        
        if ( (stateInfo.IsName("Player_Walk_CYCLE")) || (stateInfo.IsName("Player_Start_to_Walk")) || (stateInfo.IsName("Player_State01_to_Walk")) )
        {
            animator.Play("Player_Walk_to_State01", 0);
        }

        else
        {
            animator.Play("Player_State01_CYCLE", 0);
        }
    }
    
    public void EnterStateOne()
    {
        
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        
        // if ( stateInfo.IsName("Player_State01_CYCLE") )
        // {
        //     animator.Play("Player_State01_CYCLE", 0);
        // }
        
        if ( stateInfo.IsName("Player_State02_CYCLE") )
        {
            animator.Play("Player_State02_to_State01", 0);
        }
        
        if ( stateInfo.IsName("Player_State03_CYCLE") )
        {
            animator.Play("Player_State03_to_State01", 0);
        }
        
    }
    
    public void EnterStateTwo()
    {
        
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        
        // if ( stateInfo.IsName("Player_State02_CYCLE") )
        // {
        //     animator.Play("Player_State02_CYCLE", 0);
        // }
        
        if ( stateInfo.IsName("Player_State01_CYCLE") )
        {
            animator.Play("Player_State01_to_State02", 0);
        }
        
        if ( stateInfo.IsName("Player_State03_CYCLE") )
        {
            animator.Play("Player_State03_to_State02", 0);
        }
        
    }
    
    public void EnterStateThree()
    {
        
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        
        // if ( stateInfo.IsName("Player_State03_CYCLE") )
        // {
        //     animator.Play("Player_State03_CYCLE", 0);
        // }
        
        if ( stateInfo.IsName("Player_State01_CYCLE") )
        {
            animator.Play("Player_State01_to_State03", 0);
        }
        
        if ( stateInfo.IsName("Player_State02_CYCLE") )
        {
            animator.Play("Player_State02_to_State03", 0);
        }
        
    }
    
    public void EnterDeathOne()
    {
        
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        
        // if ( stateInfo.IsName("Player_Death01_CYCLE") )
        // {
        //     animator.Play("Player_Death01_CYCLE", 0);
        // }
        
        if ( stateInfo.IsName("Player_State01_CYCLE") )
        {
            animator.Play("Player_State01_to_Death01", 0);
        }
        
        if ( stateInfo.IsName("Player_State02_CYCLE") )
        {
            animator.Play("Player_State02_to_Death01", 0);
        }
        
        if ( stateInfo.IsName("Player_State03_CYCLE") )
        {
            animator.Play("Player_State03_to_Death01", 0);
        }
        
        
    }
    
}
