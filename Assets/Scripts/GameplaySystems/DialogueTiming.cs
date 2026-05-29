using UnityEngine;
using TMPro;
using System.Collections;
using System.Text;

public class DialogueTiming : MonoBehaviour
{
   [SerializeField] float typeSpeed = 30f;
   public Coroutine Run(string textToType, TMP_Text textLabel)
   {
      return StartCoroutine(TypeText(textToType, textLabel));
   }

   private IEnumerator TypeText(string textToType, TMP_Text textLabel)
   {
      //bake line breaks in up front so the typewriter doesn't reflow mid-word
      string baked = PreBakeLineBreaks(textToType, textLabel);

      //clears any preview/placeholder text
      textLabel.text = string.Empty;

      //variables for speed and indexing single characters in lines
      float t = 0;
      int charIndex = 0;

      while (charIndex < baked.Length)
      {
         //sets speed
         t += Time.deltaTime * typeSpeed;

         //sets timer to round down to pair up with charIndex
         charIndex = Mathf.FloorToInt(t);

         charIndex = Mathf.Clamp(charIndex, 0, baked.Length);

         textLabel.text = baked.Substring(0, charIndex);

         yield return null;
      }
      textLabel.text = baked;
   }

   //lay it out once, ask TMP where it wraps, rebuild with explicit \n at those points
   private string PreBakeLineBreaks(string source, TMP_Text label)
   {
      if (string.IsNullOrEmpty(source)) return source;

      label.text = source;
      label.ForceMeshUpdate();

      var info = label.textInfo;
      if (info.lineCount <= 1) return source;

      var sb = new StringBuilder(source.Length + info.lineCount);
      int cursor = 0;

      for (int line = 0; line < info.lineCount; line++)
      {
         //end of this line in source-string coords
         int lastVisible = info.lineInfo[line].lastCharacterIndex;
         int srcEnd = info.characterInfo[lastVisible].index + 1;

         sb.Append(source, cursor, srcEnd - cursor);
         cursor = srcEnd;

         if (line < info.lineCount - 1)
         {
            //jump past the space TMP wrapped on, drop in our own \n
            int nextFirst = info.lineInfo[line + 1].firstCharacterIndex;
            cursor = info.characterInfo[nextFirst].index;
            sb.Append('\n');
         }
      }

      //tail end (trailing tags etc.)
      if (cursor < source.Length) sb.Append(source, cursor, source.Length - cursor);

      return sb.ToString();
   }

}
