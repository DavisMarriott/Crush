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
    [SerializeField] private CharmState charmState;
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
        var lukeBranch = dialogueCard.GetLukeBranch(confidenceState.confidence, confidenceState.introMade);
  
        if (lukeBranch == null || lukeBranch.dialogue == null || lukeBranch.dialogue.Length == 0)
        {
            Debug.LogWarning($"No valid Luke branch for confidence {confidenceState.confidence} on card {dialogueCard.previewText}");
            return;
        }
  
        dialogueBox.SetActive(true);
        thoughtBubble.SetActive(false);
        StartCoroutine(StepThroughDialogue(dialogueCard, lukeBranch));
    }

    private IEnumerator StepThroughDialogue(DialogueCard dialogueCard, DialogueCard.DialogueBranch lukeBranch)
    {
        // Phase 1-2: Luke's lines (Execution)
        foreach (DialogueCard.DialogueLine line in lukeBranch.dialogue)
        {
            SetSpeakerIndicator(line.character);
            yield return _dialogueTiming.Run(line.line, dialogueText);
            confidenceState.confidence += line.confidenceImpact;
            charmState.charm += line.charmImpact;
            yield return new WaitUntil(() => nextLineAction.action.WasPerformedThisFrame());
        }

        // Phase 4: Apply charm impact from this Luke branch
        DialogueCard.CharmState currentCharmState = GetCurrentCharmState(charmState.charm);
        int charmImpact = lukeBranch.GetCharmImpact(currentCharmState);
        charmState.charm += charmImpact;

        // Phase 5-6: Daisy's response (from this Luke branch, based on new charm)
        var daisyBranch = lukeBranch.GetDaisyBranch(charmState.charm);

        if (daisyBranch != null && daisyBranch.dialogue != null && daisyBranch.dialogue.Length > 0)
        {
            foreach (DialogueCard.DialogueLine line in daisyBranch.dialogue)
            {
                SetSpeakerIndicator(line.character);
                yield return _dialogueTiming.Run(line.line, dialogueText);
                confidenceState.confidence += line.confidenceImpact;
                charmState.charm += line.charmImpact;
                yield return new WaitUntil(() => nextLineAction.action.WasPerformedThisFrame());
            }
        }

        confidenceState.introMade = true;

        deckManager.DrawCard();
        thoughtSpawner.SpawnButtons();

        thoughtBubble.SetActive(true);
        CloseDialogueBox();
    }

    private DialogueCard.CharmState GetCurrentCharmState(int charm)
    {
        // Check each state's range to find which one the charm value falls into
        foreach (DialogueCard.CharmState state in System.Enum.GetValues(typeof(DialogueCard.CharmState)))
        {
            DialogueCard.GetCharmRange(state, out int min, out int max);
            if (charm >= min && charm <= max)
                return state;
        }
        return DialogueCard.CharmState.Neutral;
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
        StopAllCoroutines();
        dialogueBox.SetActive(false);
        dialogueText.text = string.Empty;
    }
}