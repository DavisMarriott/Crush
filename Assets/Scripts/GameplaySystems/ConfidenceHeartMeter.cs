using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class ConfidenceHeartMeter : MonoBehaviour

{
   public GameObject heartPrefab;
   public GameObject parentObject;
   [SerializeField] ConfidenceState confidenceState;
   public Animator postFxAnimator;

   // // used only for testing
   public int confidenceLevel = 5;
   public int confidenceDelta = 3;
   
   private Animator animator;
   // [SerializeField] private Animator heartAnimator;

   
   // call SpawnHeartMeter() at start of sequence - spawns [starting confidence] hearts with ramping speed
   public void SpawnHeartMeter()
   {
      StartCoroutine(SpawnHeartWithDelay());

      IEnumerator SpawnHeartWithDelay()
      {
         // remove comment out here when implementing
         // for (int i = 0; i < confidenceState.startingConfidence; i++)
         for (int i = 0; i < confidenceLevel; i++)
         {
            SpawnHeart();
            Debug.Log("Iteration: " + i);
            // int delayTime = (i / (i * i));

            // This line pauses the loop execution
            yield return new WaitForSeconds(0.40f - (i *  0.04f));
         }
        
         Debug.Log("Loop Complete!");
      }
      
   }
   
   // call AddHearts() if confidence delta is positive. Spawns appropriate # hearts with ramping speed
   public void AddHearts()
   {
      StartCoroutine(SpawnHeartWithDelay());

      IEnumerator SpawnHeartWithDelay()
      {
         // Need to replace confidenceDelta, or redefine above
         for (int i = 0; i < confidenceDelta; i++)
         {
            SpawnHeart();
            Debug.Log("Iteration: " + i);
            // int delayTime = (i / (i * i));

            // This line pauses the loop execution
            yield return new WaitForSeconds(0.40f - (i *  0.04f));
         }
        
         Debug.Log("Loop Complete!");
      }
   }
   
   
   // call RemoveHearts() if confidence delta is negative. Breaks and Removes appropriate # hearts with ramping speed
   public void RemoveHearts()
   {
      StartCoroutine(BreakHeartWithDelay());

      IEnumerator BreakHeartWithDelay()
      {
         // Need to replace confidenceDelta, or redefine above
         for (int i = 0; i < confidenceDelta; i++)
         {
            BreakHeart();
            Debug.Log("Iteration: " + i);
            // int delayTime = (i / (i * i));

            // This line pauses the loop execution
            yield return new WaitForSeconds(0.40f - (i *  0.04f));
         }
        
         Debug.Log("Loop Complete!");
      }
   }
   
   
   
   // Instantiate a heart prefab
   public void SpawnHeart()
   {
      // Create Game Object
      Instantiate(heartPrefab, parentObject.transform);
      PositivePulse();

   }

   public void BreakHeart()
   {
      
      List<GameObject> totalHearts = new List<GameObject>();
      foreach (Transform child in transform) 
      {
         totalHearts.Add(child.gameObject);
      }
      
      if (totalHearts.Count > 1)
      { 
         // Get  the last index
         int lastIndex = totalHearts.Count - 1;
      // Declare animator and play animation. 
         Animator lastAnimator = totalHearts[lastIndex].GetComponent<Animator>();
      // Heart_Break animation has an event trigger that Destroys the game object after animation plays.
         lastAnimator.Play("Heart_Break", 0);
      }
      
      if (totalHearts.Count == 1)
      { 
         // Get  the last index
         int lastIndex = totalHearts.Count - 1;
         // Declare animator and play animation. 
         Animator lastAnimator = totalHearts[lastIndex].GetComponent<Animator>();
         // Heart_Break animation has an event trigger that Destroys the game object after animation plays.
         lastAnimator.Play("Heart_Break_Long", 0);
      }

      NegativePulse();

   }

   public void LowConfidenceCheck()
   {
      List<GameObject> totalHearts = new List<GameObject>();
      foreach (Transform child in transform) 
      {
         totalHearts.Add(child.gameObject);
      }
      
      if (totalHearts.Count > 3)
      {
         Animator heartOneAnimator = totalHearts[0].GetComponent<Animator>();
         Animator heartTwoAnimator = totalHearts[1].GetComponent<Animator>();
         Animator heartThreeAnimator = totalHearts[2].GetComponent<Animator>();
         
         heartOneAnimator.Play("Heart_Static", 0);
         heartTwoAnimator.Play("Heart_Static", 0);
         heartThreeAnimator.Play("Heart_Static", 0);
         postFxAnimator.Play("PostFX_Neutral_CYCLE", 0);
      }

      else if (totalHearts.Count == 3)
      {
         Animator heartOneAnimator = totalHearts[0].GetComponent<Animator>();
         Animator heartTwoAnimator = totalHearts[1].GetComponent<Animator>();
         Animator heartThreeAnimator = totalHearts[2].GetComponent<Animator>();
         
         heartOneAnimator.Play("Heart_Beat01", 0);
         heartTwoAnimator.Play("Heart_Beat01", 0);
         heartThreeAnimator.Play("Heart_Beat01", 0);
         postFxAnimator.Play("PostFX_Negative_LowHealth01_CYCLE", 0);
      }
      
      else if (totalHearts.Count == 2 )
      {
         Animator heartOneAnimator = totalHearts[0].GetComponent<Animator>();
         Animator heartTwoAnimator = totalHearts[1].GetComponent<Animator>();
         
         heartOneAnimator.Play("Heart_Beat02", 0);
         heartTwoAnimator.Play("Heart_Beat02", 0);
         postFxAnimator.Play("PostFX_Negative_LowHealth01_CYCLE", 0);
      }
      
      else if (totalHearts.Count == 1)
      {
         Animator heartOneAnimator = totalHearts[0].GetComponent<Animator>();

         heartOneAnimator.Play("Heart_Beat03", 0);
         postFxAnimator.Play("PostFX_Negative_LowHealth01_CYCLE", 0);
      }
      
      totalHearts.Clear();
   }

   private void NegativePulse()
   {
      AnimatorStateInfo postFxStateInfo = postFxAnimator.GetCurrentAnimatorStateInfo(0);

      if (postFxStateInfo.IsName("PostFX_Neutral_CYCLE"))
      {
         postFxAnimator.Play("PostFX_NegativePulse_01", 0);
      }
      
      if (postFxStateInfo.IsName("PostFX_Negative_LowHealth01_CYCLE"))
      {
         postFxAnimator.Play("PostFX_NegativePulse_02", 0);
      }
   }
   
   private void PositivePulse()
   {
      AnimatorStateInfo postFxStateInfo = postFxAnimator.GetCurrentAnimatorStateInfo(0);

      if (postFxStateInfo.IsName("PostFX_Neutral_CYCLE"))
      {
         postFxAnimator.Play("PostFX_PositivePulse_01", 0);
      }
      
      if (postFxStateInfo.IsName("PostFX_Negative_LowHealth01_CYCLE"))
      {
         postFxAnimator.Play("PostFX_PositivePulse_02", 0);
      }
   }

}
