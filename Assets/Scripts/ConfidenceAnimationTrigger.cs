using UnityEngine;

public class ConfidenceAnimationTrigger : MonoBehaviour
{
    public Animator animator;
    public ConfidenceIncrementer confidenceIncrementer;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }
    

    // Update is called once per frame
    void Update()
    {
        
        if (confidenceIncrementer.confidence == 0)
        {
            animator.Play("PlayerConfidence_00", 0);
        }
        
        if (confidenceIncrementer.confidence == 1)
        {
            animator.Play("PlayerConfidence_01", 0);
        }
        
        if (confidenceIncrementer.confidence == 2)
        {
            animator.Play("PlayerConfidence_02", 0);
        }
        
        if (confidenceIncrementer.confidence == 3)
        {
            animator.Play("PlayerConfidence_03", 0);
        }

        if (confidenceIncrementer.confidence == 4)
        {
            animator.Play("PlayerConfidence_04", 0);
        }
        
        if (confidenceIncrementer.confidence == 5)
        {
            animator.Play("PlayerConfidence_05", 0);
        }
        
        if (confidenceIncrementer.confidence == 6)
        {
            animator.Play("PlayerConfidence_06", 0);
        }
        
    }
    
    // if (animator != null && !string.IsNullOrEmpty(nextAnimationName))
    // {
    //     animator.Play(nextAnimationName);
    // }
    
}
