using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.InputSystem;

public class DialogueBox : MonoBehaviour
{
    [SerializeField] private GameObject dialogueBox;
    [SerializeField] private InputActionReference nextLineAction;
    [SerializeField] private TMP_Text dialogueText;
    [SerializeField] private ConfidenceState confidenceState; 
    private DialogueTiming _dialogueTiming;

    public void Start()
    {
        _dialogueTiming = GetComponent<DialogueTiming>();
        CloseDialogueBox();
    }
   
    private void OnEnable()  => nextLineAction.action.Enable();
    private void OnDisable() => nextLineAction.action.Disable();
   

    public void ShowDialogue(DialogueCard dialogueCard)
    {
        //this is where we get the dialogue branch that fits current confidence
        var branch = dialogueCard.GetBranchForConfidence(confidenceState.confidence);
      
        if (branch == null || branch.dialogue == null || branch.dialogue.Length == 0)
        {
            Debug.LogWarning($"No valid branch found for confidence {confidenceState.confidence} on card {dialogueCard.previewText}");
            return;
        }
      
        dialogueBox.SetActive(true);
        StartCoroutine(StepThroughDialogue(branch));
    }
   
    // this is where we get and play the dialogue lines
    private IEnumerator StepThroughDialogue(DialogueCard.DialogueBranch branch)
    {
        foreach (DialogueCard.DialogueLine line in branch.dialogue)
        {
            yield return _dialogueTiming.Run(line.line, dialogueText);  // ← use line.line directly

            yield return new WaitUntil(() => nextLineAction.action.WasPerformedThisFrame());
        }

        CloseDialogueBox();
    }

    private void CloseDialogueBox()
    {
        dialogueBox.SetActive(false);
        dialogueText.text = string.Empty;
    }
}