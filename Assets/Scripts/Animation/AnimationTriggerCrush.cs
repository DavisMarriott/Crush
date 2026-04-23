using UnityEngine;

public class AnimationTriggerCrush : MonoBehaviour
{

    private Animator animator;
    [SerializeField] private CharmState charmState;
    [SerializeField] private ConfidenceState confidenceState;
    public Animator particlesCharmUp;
    public Animator particlesCharmedState;

    void Start()
    {
        animator = GetComponent<Animator>();
    }
    
    // BEGINNING ANIMATION //
    public void Begin()
    {
            animator.Play("Crush_Start_CYCLE", 0);
    }
    
    // NEUTRAL //
    public void Neutral()
    {
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);

        if (stateInfo.IsName("Crush_Start_CYCLE"))
        {
            animator.Play("Crush_Start_to_Neutral", 0);
        }

        else if (stateInfo.IsName("Crush_Negative01_CYCLE"))
        {
            animator.Play("Crush_Negative01_to_Neutral", 0);
        }

        else if (stateInfo.IsName("Crush_Positive01_CYCLE"))
        {
            animator.Play("Crush_Positive01_to_Neutral", 0);
        }

        else
        {
        animator.Play("Crush_Neutral_CYCLE", 0);
        }
}
    
    public void NegativeOne()
    {
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        
        if (stateInfo.IsName("Crush_Neutral_CYCLE"))
        {
            animator.Play("Crush_Neutral_to_Negative01", 0);
        }
        
        else if (stateInfo.IsName("Crush_Negative02_CYCLE")) 
        {
            animator.Play("Crush_Negative02_to_Negative01", 0);
        }

        else
        {
            animator.Play("Crush_Negative01_CYCLE", 0); 
        }

    }
    
    public void NegativeTwo()
    {
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        
        if (stateInfo.IsName("Crush_Negative01_CYCLE")) 
        {
            animator.Play("Crush_Negative01_to_Negative02", 0);
        }
        
        else
        {
            animator.Play("Crush_Negative02_CYCLE", 0);
        }
    }
    
    public void PositiveOne()
    {
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        
        if (stateInfo.IsName("Crush_Neutral_CYCLE"))
        {
            animator.Play("Crush_Neutral_to_Positive01", 0);
        }
        
        else if (stateInfo.IsName("Crush_Positive02_CYCLE")) 
        {
            animator.Play("Crush_Positive02_to_Positive01", 0);
        }
        
        else
        {
            animator.Play("Crush_Positive01_CYCLE", 0);
        }
    }
    
    public void PositiveTwo()
    {
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        
        if (stateInfo.IsName("Crush_Positive01_CYCLE")) 
        {
            animator.Play("Crush_Positive01_to_Positive02", 0);
        }
        
        else
        {
            animator.Play("Crush_Positive02_CYCLE", 0);
        }
    }
    
    
    // CHARM POSE //
    
    public void GetCharmPose()
    {
        //tweak these numbers to edit charm range/pose pairing
        //grabs first true condition
        if (charmState.charm <= 0)
            NegativeTwo();
            
        else if (charmState.charm <= 2)
            NegativeOne();
        
        else if (charmState.charm <= 5)
            Neutral();
             
        else if (charmState.charm <= 8)
            PositiveOne();
        else
            PositiveTwo();
    }
    
    
    // Particle Systems //
    
    public void ParticlesCharmUp()
    {
        particlesCharmUp.Play("Particles_CharmUp_Burst", 0);
    }
    
    public void ParticlesCharmedStateTurnOn()
    {
        particlesCharmedState.Play("Particles_CharmedState_TurnOn", 0);
    }
    
    public void ParticlesCharmedStateTurnOff()
    {
        particlesCharmedState.Play("Particles_CharmedState_TurnOff", 0);
    }
    
    
}