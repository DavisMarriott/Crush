using System;
using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.InputSystem;

public class DialogueBox : MonoBehaviour
{
   [SerializeField] private GameObject dialogueBox;
   [SerializeField] private InputActionReference nextLineAction;
   [SerializeField] private TMP_Text textLabel;
   [SerializeField] private DialogueObject testDialogue;
   private DialogueTiming dialogueTiming;

   public void Start()
   {
      dialogueTiming = GetComponent<DialogueTiming>();
      CloseDialogueBox();
      ShowDialogue(testDialogue);
   }
   
   private void OnEnable()  => nextLineAction.action.Enable();
   private void OnDisable() => nextLineAction.action.Disable();
   

   public void ShowDialogue(DialogueObject dialogueObject)
   {
      dialogueBox.SetActive(true);
      StartCoroutine(StepTrhoughDialogue(dialogueObject));
   }
   
   private IEnumerator StepTrhoughDialogue(DialogueObject dialogueObject)
   {
      foreach (string dialogue in dialogueObject.Dialogue)
      {
         yield return dialogueTiming.Run(dialogue, textLabel);

         // wait for Space / "Next Line" action
         yield return new WaitUntil(() => nextLineAction.action.WasPerformedThisFrame());
      }

      CloseDialogueBox();
   }

   private void CloseDialogueBox()
   {
      dialogueBox.SetActive(false);
      textLabel.text = string.Empty;
   }
}
