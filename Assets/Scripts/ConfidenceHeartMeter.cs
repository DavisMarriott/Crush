using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ConfidenceHeartMeter : MonoBehaviour
{
   public GameObject heartPrefab;
   public GameObject parentObject;
   [SerializeField] private ConfidenceState confidenceState;
   public int confidenceTemp = 4;
   private Animator animator;
   // [SerializeField] private Animator heartAnimator;

   private void Start()
   {
         // Start the coroutine (do not call it like a regular function)
         StartCoroutine(SpawnHeartWithDelay());

      IEnumerator SpawnHeartWithDelay()
      {
         for (int i = 0; i < confidenceTemp; i++)
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
   
   // Instantiate a heart prefab
   public void SpawnHeart()
   {
      // Create Game Object
      Instantiate(heartPrefab,  parentObject.transform);

   }

   public void BreakHeart()
   {
      
      List<GameObject> totalHearts = new List<GameObject>();
      foreach (Transform child in transform) 
      {
         totalHearts.Add(child.gameObject);
      }
      
      if (totalHearts.Count > 0)
      { 
      // Get  the last index
         int lastIndex = totalHearts.Count - 1;
      // Declare animator and play animation. 
         Animator lastAnimator = totalHearts[lastIndex].GetComponent<Animator>();
      // Heart_Break animation has an event trigger that Destroys the game object after animation plays.
         lastAnimator.Play("Heart_Break", 0);
      }
      
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
      }

      else if (totalHearts.Count == 3)
      {
         Animator heartOneAnimator = totalHearts[0].GetComponent<Animator>();
         Animator heartTwoAnimator = totalHearts[1].GetComponent<Animator>();
         Animator heartThreeAnimator = totalHearts[2].GetComponent<Animator>();
         
         heartOneAnimator.Play("Heart_Beat01", 0);
         heartTwoAnimator.Play("Heart_Beat01", 0);
         heartThreeAnimator.Play("Heart_Beat01", 0);
      }
      
      else if (totalHearts.Count == 2 )
      {
         Animator heartOneAnimator = totalHearts[0].GetComponent<Animator>();
         Animator heartTwoAnimator = totalHearts[1].GetComponent<Animator>();
         
         heartOneAnimator.Play("Heart_Beat02", 0);
         heartTwoAnimator.Play("Heart_Beat02", 0);
      }
      
      else if (totalHearts.Count == 1)
      {
         Animator heartOneAnimator = totalHearts[0].GetComponent<Animator>();

         heartOneAnimator.Play("Heart_Beat03", 0);
      }
      
      totalHearts.Clear();
   }

}
