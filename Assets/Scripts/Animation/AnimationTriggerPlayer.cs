using UnityEngine;

public class AnimationTriggerPlayer : MonoBehaviour
{
    // I think this just lets us access the animator attached to the game object (our character)
    private Animator animator;
    public Animator particlesConfidenceUp;
    public Animator particlesConfidenceDown;
    public Animator particlesNervousState;

    void Start()
    {
        // Get the Animator component attached to the GameObject
        animator = GetComponent<Animator>();
    }
    
    // WALK //
    public void Walk()
    {

        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        
        if (stateInfo.IsName("Player_Start_CYCLE"))
        {
            animator.Play("Player_Start_to_Walk", 0);
        }
        
        else if (stateInfo.IsName("Player_State01_CYCLE"))
        {
            animator.Play("Player_State01_to_Walk", 0);
        }
        
        else if ( (stateInfo.IsName("Player_State01_to_Walk")) || (stateInfo.IsName("Player_Walk_to_State01")) || (stateInfo.IsName("Player_Start_to_Walk")) )
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
        
        if ( (stateInfo.IsName("Player_Walk_CYCLE")) || (stateInfo.IsName("Player_Start_to_Walk")) || (stateInfo.IsName("Player_State01_to_Walk")) )
        {
            animator.Play("Player_Walk_to_State01", 0);
        }
        
        else if ( stateInfo.IsName("Player_State02_CYCLE") )
        {
            animator.Play("Player_State02_to_State01", 0);
        }
        
        else if ( stateInfo.IsName("Player_State03_CYCLE") )
        {
            animator.Play("Player_State03_to_State01", 0);
        }
        else
        {
            animator.Play("Player_State01_CYCLE");
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
        
        else if ( stateInfo.IsName("Player_State03_CYCLE") )
        {
            animator.Play("Player_State03_to_State02", 0);
        }
        else
        {
            animator.Play("Player_State02_CYCLE");
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
        
        else if ( stateInfo.IsName("Player_State02_CYCLE") )
        {
            animator.Play("Player_State02_to_State03", 0);
        }
        
        else
        {
            animator.Play("Player_State03_CYCLE");
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
        
        else if ( stateInfo.IsName("Player_State02_CYCLE") )
        {
            animator.Play("Player_State02_to_Death01", 0);
        }
        
        else if ( stateInfo.IsName("Player_State03_CYCLE") )
        {
            animator.Play("Player_State03_to_Death01", 0);
        }
        
        else
        {
            animator.Play("Player_Death01_CYCLE", 0);
        }
        
    }
    
    public void EnterStart()
    {
        animator.Play("Player_Start", 0);
    }
    
    
    // Particle Systems //
    
    public void ParticlesConfidenceUp()
    {
        particlesConfidenceUp.Play("Particles_ConfidenceUp_Burst", 0);
    }
    
    public void ParticlesConfidenceDown()
    {
        particlesConfidenceDown.Play("Particles_ConfidenceDown_Burst", 0);
    }
    
    public void ParticlesNervousStateTurnOn()
    {
        particlesNervousState.Play("Particles_NervousState_TurnOn", 0);
    }
    
    public void ParticlesNervousStateTurnOff()
    {
        particlesNervousState.Play("Particles_NervousState_TurnOff", 0);
    }
    
    

}
