using UnityEngine;

public class AnimationTriggerPlayer : MonoBehaviour
{
    // I think this just let's us access the animator attached to the game object (our character)
    private Animator animator;
    // This lets us assign a ConfidenceIncrementer through inspector. This was a temp solution to demo the concept.
    // We'll probably want to replace this with our health/confidence script.
    //public ConfidenceIncrementer confidenceIncrementer;

    void Start()
    {
        // Get the Animator component attached to the GameObject
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    // void Update()
    // {
    //     // Everything in Udpate() is looking for a current confidence score and playing the associated animation.
    //     // The confidenceIncrementer is script I'd made, triggered by buttons in the scene. You can see that in the scene CharacterHealth_Test.
    //     
    //     if (confidenceIncrementer.confidence == 0)
    //     {
    //         animator.Play("PlayerConfidence_00", 0);
    //     }
    //     
    //     if (confidenceIncrementer.confidence == 1)
    //     {
    //         animator.Play("PlayerConfidence_01", 0);
    //     }
    //     
    //     if (confidenceIncrementer.confidence == 2)
    //     {
    //         animator.Play("PlayerConfidence_02", 0);
    //     }
    //     
    //     if (confidenceIncrementer.confidence == 3)
    //     {
    //         animator.Play("PlayerConfidence_03", 0);
    //     }
    //
    //     if (confidenceIncrementer.confidence == 4)
    //     {
    //         animator.Play("PlayerConfidence_04", 0);
    //     }
    //     
    //     if (confidenceIncrementer.confidence == 5)
    //     {
    //         animator.Play("PlayerConfidence_05", 0);
    //     }
    //     
    //     if (confidenceIncrementer.confidence == 6)
    //     {
    //         animator.Play("PlayerConfidence_06", 0);
    //     }
    //     
    // }

    
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
        if (stateInfo.IsName("Player_Default"))
        {
            animator.Play("Player_Default_to_Walk", 0);
        }

        // If we're not in Default, just play Walk directly.
        else
        {
            animator.Play("Player_Walk", 0);
        }
    }
    
    // DEFAULT //
    public void Default()
    {
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        
        if (stateInfo.IsName("Player_Walk"))
        {
            animator.Play("Player_Walk_to_Default", 0);
        }

        else
        {
            animator.Play("Player_Default", 0);
        }
    }
    
}
