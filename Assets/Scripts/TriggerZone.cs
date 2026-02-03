using UnityEngine;
using UnityEngine.Events;

public class TriggerZone : MonoBehaviour
{
   public UnityEvent onTriggerEnter;
   public UnityEvent onTriggerExit;
   private void OnTriggerEnter2D(Collider2D collision)
   {
      //checks if collision is with object tagged player
      if(collision.CompareTag("Player"))
         onTriggerEnter.Invoke();
   }
   private void OnTriggerExit2D(Collider2D collision)
   {
      //checks if collision is with object tagged player
      if(collision.CompareTag("Player"))
         onTriggerExit.Invoke();
   }
}
