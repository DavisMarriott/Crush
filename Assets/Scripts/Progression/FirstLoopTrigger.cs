using System.Collections;
using UnityEngine;

public class FirstLoopTrigger : MonoBehaviour
{
   [SerializeField] private GameProgression gameProgression;
   [SerializeField] private int triggerIndex;

   [Tooltip("Max random delay (seconds) before the trigger fires. Actual delay is 0 to this value.")]
   [SerializeField] private float maxDelay = 1.5f;

   private bool hasFired;

   public void OnTriggerEnter2D(Collider2D player)
   {
       if (!player.CompareTag("Player")) return;
       if (hasFired) return;

       hasFired = true;
       StartCoroutine(DelayedFire());
   }

   private IEnumerator DelayedFire()
   {
       float delay = Random.Range(0f, maxDelay);
       yield return new WaitForSeconds(delay);
       gameProgression.HallwayTriggerHit(triggerIndex);
   }

   private void OnDisable()
   {
       StopAllCoroutines();
       hasFired = false;
   }
}
