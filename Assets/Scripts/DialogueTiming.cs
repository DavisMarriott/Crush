using UnityEngine;
using TMPro;
using System.Collections;

public class DialogueTiming : MonoBehaviour
{
   [SerializeField] private float typeSpeed = 50f;
   public Coroutine Run(string textToType, TMP_Text textLabel)
   {
      return StartCoroutine(TypeText(textToType, textLabel));
   }

   private IEnumerator TypeText(string textToType, TMP_Text textLabel)
   {
      //clears any preview/placeholder text
      textLabel.text = string.Empty;
      
      //variables for speed and indexing single characters in lines
      float t = 0;
      int charIndex = 0;

      while (charIndex < textToType.Length)
      {
         //sets speed
         t += Time.deltaTime * typeSpeed;
         
         //sets timer to round down to pair up with charIndex
         charIndex = Mathf.FloorToInt(t);
         
         charIndex = Mathf.Clamp(charIndex, 0, textToType.Length);
         
         textLabel.text = textToType.Substring(0, charIndex);

         yield return null;
      }
      textLabel.text = textToType;
   }
   
}
