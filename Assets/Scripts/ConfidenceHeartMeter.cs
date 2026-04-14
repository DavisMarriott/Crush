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

            // This line pauses the loop execution for 1 second
            yield return new WaitForSeconds(0.3f);
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

   public void DestroyHeart()
   {
      
      List<GameObject> children = new List<GameObject>();
      foreach (Transform child in transform) 
      {
         children.Add(child.gameObject);
      }
      
      if (children.Count > 0)
      {
         // Get  the last index
         int lastIndex = children.Count - 1;

         // Destroy last object in list
         Destroy(children[lastIndex]);

         // Removes the null reference from the list
         children.RemoveAt(lastIndex);
      }\
   }
   
   
}
