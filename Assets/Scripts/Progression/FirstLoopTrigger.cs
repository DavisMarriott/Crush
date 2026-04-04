using TMPro;
using UnityEngine;

public class FirstLoopTrigger : MonoBehaviour
{
   [SerializeField] private GameProgression gameProgression;
   [SerializeField] private int triggerIndex;

   public void OnTriggerEnter2D(Collider2D player)
   {
       if (!player.CompareTag("Player")) return;
         gameProgression.HallwayTriggerHit(triggerIndex);
        
   }
}
