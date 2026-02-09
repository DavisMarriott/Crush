using UnityEngine;

public class AnimationTrigger : MonoBehaviour
{
    private Animator animator;

    void Start()
    {
        // Get the Animator component attached to the GameObject
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    
    // WALK //
    public void Walk()
    {
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        
        if (stateInfo.IsName("Protagonist_Default"))
        {
            animator.Play("Protagonist_Default_to_Walk", 0);
        }

        else
        {
            animator.Play("Protagonist_Walk", 0);
        }
    }
    
    // DEFAULT //
    public void Default()
    {
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        
        if (stateInfo.IsName("Protagonist_Walk"))
        {
            animator.Play("Protagonist_Walk_to_Default", 0);
        }

        else
        {
            animator.Play("Protagonist_Default", 0);
        }
    }
    
}
