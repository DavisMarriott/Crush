using UnityEngine;
using TMPro;
using System.Collections;
using System.Text;

public class DialogueTiming : MonoBehaviour
{
   [SerializeField] float typeSpeed = 30f;
   [SerializeField] RandomSoundPlayer textSound;
   [Tooltip("Beat held before text starts typing, but ONLY when the bubble is actually opening. Gives the open anim time to play so text doesn't beat it.")]
   [SerializeField] float openAnimDelay = 0.35f;

   public Coroutine Run(string textToType, TMP_Text textLabel)
   {
      return Run(textToType, textLabel, false);
   }

   // waitForOpen: true on the line where the bubble just opened - hold openAnimDelay before
   // typing. Continuation lines (bubble already open) pass false and type right away.
   public Coroutine Run(string textToType, TMP_Text textLabel, bool waitForOpen)
   {
      return StartCoroutine(TypeText(textToType, textLabel, waitForOpen));
   }

   private IEnumerator TypeText(string textToType, TMP_Text textLabel, bool waitForOpen)
   {
      //bake line breaks in up front so the typewriter doesn't reflow mid-word
      string baked = PreBakeLineBreaks(textToType, textLabel);

      //clears any preview/placeholder text
      textLabel.text = string.Empty;

      //hold a beat for the bubble's open anim before typing - only on a real open
      if (waitForOpen && openAnimDelay > 0f)
         yield return new WaitForSeconds(openAnimDelay);

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

         if (Time.time >= textSound.nextPlayTime)
         {
            textSound.PlayRandomSound();
         }

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
