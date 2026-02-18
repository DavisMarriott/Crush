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
    [SerializeField] private DeckManager deckManager;
    [SerializeField] private ThoughtSpawner thoughtSpawner;
    private DialogueTiming _dialogueTiming;
    [SerializeField] private GameObject thoughtBubble;
    [Header("Speaker Indicators")]
    [SerializeField] private GameObject leftArrow;
    [SerializeField] private GameObject rightArrow;
    [SerializeField] private Color boyTextColor = Color.white;
    [SerializeField] private Color girlTextColor = new Color(1f, 0.7f, 0.8f);
    [SerializeField] private Color boyInternalTextColor = Color.gray;

    public void Start()
    {
        _dialogueTiming = GetComponent<DialogueTiming>();
        CloseDialogueBox();
    }
   
    private void OnEnable()  => nextLineAction.action.Enable();
    private void OnDisable() => nextLineAction.action.Disable();
   

    public void ShowDialogue(DialogueCard dialogueCard)
    {
        var branch = dialogueCard.GetBranchForConfidence(confidenceState.confidence, confidenceState.introMade);
      
        if (branch == null || branch.dialogue == null || branch.dialogue.Length == 0)
        {
            Debug.LogWarning($"No valid branch found for confidence {confidenceState.confidence} on card {dialogueCard.previewText}");
            return;
        }
      
        dialogueBox.SetActive(true);
        thoughtBubble.SetActive(false);
        StartCoroutine(StepThroughDialogue(branch));
    }
   
    private IEnumerator StepThroughDialogue(DialogueCard.DialogueBranch branch)
    {
        foreach (DialogueCard.DialogueLine line in branch.dialogue)
        {
            SetSpeakerIndicator(line.character);
    
            yield return _dialogueTiming.Run(line.line, dialogueText);
            confidenceState.confidence += line.confidenceImpact;
            yield return new WaitUntil(() => nextLineAction.action.WasPerformedThisFrame());
        }
        
        confidenceState.introMade = true;
        
        // Draw new card and refresh buttons
        deckManager.DrawCard();
        thoughtSpawner.SpawnButtons();
        
        thoughtBubble.SetActive(true);
        CloseDialogueBox();
    }
    
    private void SetSpeakerIndicator(DialogueCard.DialogueCharacter character)
    {
        switch (character)
        {
            case DialogueCard.DialogueCharacter.Boy:
                dialogueText.alignment = TMPro.TextAlignmentOptions.TopLeft;
                dialogueText.color = boyTextColor;
                leftArrow.SetActive(true);
                rightArrow.SetActive(false);
                break;
            case DialogueCard.DialogueCharacter.Girl:
                dialogueText.alignment = TMPro.TextAlignmentOptions.TopRight;
                dialogueText.color = girlTextColor;
                leftArrow.SetActive(false);
                rightArrow.SetActive(true);
                break;
            case DialogueCard.DialogueCharacter.BoyInternal:
                dialogueText.alignment = TMPro.TextAlignmentOptions.Left;
                dialogueText.color = boyInternalTextColor;
                leftArrow.SetActive(true);
                rightArrow.SetActive(false);
                break;
        }
    }

    public void CloseDialogueBox()
    {
        dialogueBox.SetActive(false);
        dialogueText.text = string.Empty;
    }
}