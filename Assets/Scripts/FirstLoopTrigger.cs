using TMPro;
using UnityEngine;

public class FirstLoopTrigger : MonoBehaviour
{
   [SerializeField] private FirstLoopManager firstLoopManager;
   [SerializeField] private int triggerIndex;

   public void OnTriggerEnter2D(Collider2D player)
   {
       if (!player.CompareTag("Player")) return;
         firstLoopManager.HallwayTriggerHit(triggerIndex);
         Debug.Log("Something entered trigger: " + player.gameObject.name);
        
   }
}
