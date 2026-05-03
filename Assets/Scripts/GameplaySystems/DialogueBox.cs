using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.InputSystem;


public class DialogueBox : MonoBehaviour
{
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private AnimationTriggerPlayer animationTriggerPlayer;
    [SerializeField] private AnimationTriggerCrush animationTriggerCrush;
    [SerializeField] private GameObject dialogueBox;
    [SerializeField] private InputActionReference nextLineAction;
    [SerializeField] private TMP_Text dialogueText;
    [SerializeField] private ConfidenceState confidenceState;
    [SerializeField] private DeckManager deckManager;
    [SerializeField] private ThoughtSpawner thoughtSpawner;
    [SerializeField] private CharmState charmState;
    [SerializeField] private DialogueTiming dialogueTiming;
    [SerializeField] private GameObject cardContainer;
    [SerializeField] private DialogueCard dialogueCard;
    [SerializeField] private GameProgression gameProgression;
    [SerializeField] private HallwaySelfTalk hallwaySelfTalk;
    [Header("Speaker Indicators")]
    [SerializeField] private GameObject leftArrow;
    [SerializeField] private GameObject rightArrow;
    [SerializeField] private Color boyTextColor = Color.white;
    [SerializeField] private Color girlTextColor = new Color(1f, 0.7f, 0.8f);
    [SerializeField] private Color boyInternalTextColor = Color.gray;



    public void Start()
    {
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
        StartCoroutine(SelectAnim());
        //wait to let the select animation play
        IEnumerator SelectAnim()
        {
            yield return new WaitForSeconds(.5f);
            cardContainer.SetActive(false);
            StartCoroutine(StepThroughDialogue(dialogueCard, lukeBranch));
        }
   
    }

    //this is where dialogue plays/bulk of conversation system lives
    private IEnumerator StepThroughDialogue(DialogueCard dialogueCard, DialogueCard.DialogueBranch lukeBranch)
    {
        //parse card data and play (including confidence/charm scores) - for Luke Branch
        foreach (DialogueCard.DialogueLine line in lukeBranch.dialogue)
        {
            SetSpeakerIndicator(line.character);
            if (line.character == DialogueCard.DialogueCharacter.BoyInternal)
            {
                yield return dialogueTiming.Run(line.line, hallwaySelfTalk.selfTalkText);
            }
            else
            {
                playerMovement.PlayerTalk();
                yield return dialogueTiming.Run(line.line, dialogueText);
            }
            confidenceState.confidence += line.confidenceImpact;
            charmState.charm += line.charmImpact;
            // Confidence particles 
            if (line.confidenceImpact > 0)
            {
                animationTriggerPlayer.ParticlesConfidenceUp();
            }
            if (line.confidenceImpact < 0)
            {
                animationTriggerPlayer.ParticlesConfidenceDown();
            }
            if (confidenceState.confidence > 2 && confidenceState.confidence < 0)
            {
                animationTriggerPlayer.ParticlesNervousStateTurnOff();
            }
            if (confidenceState.confidence == 0)
            {
                animationTriggerPlayer.ParticlesNervousStateTurnOff();
            }
            if (confidenceState.confidence <= 2)
            {
                animationTriggerPlayer.ParticlesNervousStateTurnOn();
            }
            
            playerMovement.GetConfidencePose();
            animationTriggerCrush.GetCharmPose();
            yield return new WaitUntil(() => nextLineAction.action.WasPerformedThisFrame());
            hallwaySelfTalk.selfTalkText.text = "";
            dialogueText.text = "";
        }
        

        // apply charm impact based on current charm state
        DialogueCard.CharmState currentCharmState = GetCurrentCharmState(charmState.charm);
        int charmImpact = lukeBranch.GetCharmImpact(currentCharmState);
        charmState.charm += charmImpact;
        
        // animate charm particles 
        if (charmImpact > 0)
        {
            animationTriggerCrush.ParticlesCharmUp();
        }
        
        if (charmState.charm >= 10)
        {
            animationTriggerCrush.ParticlesCharmedStateTurnOn();
        }
        
        if (charmState.charm < 10)
        {
            animationTriggerCrush.ParticlesCharmedStateTurnOff();
        }
        

        // Daisy's response (picked by charm score after Luke's impact)
        Debug.Log($"Charm after impact: {charmState.charm}, picking Daisy branch");
        var daisyBranch = lukeBranch.GetDaisyBranch(charmState.charm);

        if (daisyBranch != null && daisyBranch.dialogue != null && daisyBranch.dialogue.Length > 0)
        {
            foreach (DialogueCard.DialogueLine line in daisyBranch.dialogue)
            {
                SetSpeakerIndicator(line.character);
                animationTriggerCrush.GetCharmPose();
                if (line.character == DialogueCard.DialogueCharacter.BoyInternal)
                {
                    yield return dialogueTiming.Run(line.line, hallwaySelfTalk.selfTalkText);
                }
                else
                {
                    yield return dialogueTiming.Run(line.line, dialogueText);
                }
                confidenceState.confidence += line.confidenceImpact;
                
                // Confidence particles 
                if (line.confidenceImpact > 0)
                {
                    animationTriggerPlayer.ParticlesConfidenceUp();
                }
                if (line.confidenceImpact < 0)
                {
                    animationTriggerPlayer.ParticlesConfidenceDown();
                }
                if (confidenceState.confidence <= 2)
                {
                    animationTriggerPlayer.ParticlesNervousStateTurnOn();
                }
                if (confidenceState.confidence > 2)
                {
                    animationTriggerPlayer.ParticlesNervousStateTurnOff();
                }
                
                charmState.charm += line.charmImpact;
                playerMovement.GetConfidencePose();
                yield return new WaitUntil(() => nextLineAction.action.WasPerformedThisFrame());
                hallwaySelfTalk.selfTalkText.text = "";
                dialogueText.text = "";
            }
        }

        confidenceState.introMade = true;
        CloseDialogueBox();
        if (dialogueCard.isDance)
        {
            gameProgression.AskedToDance();
        }
        else
        {
            deckManager.DrawCard();
            thoughtSpawner.SpawnButtons();

            cardContainer.SetActive(true);
        }
        
    }

    // figure out which charm state bucket the current charm value falls into
    private DialogueCard.CharmState GetCurrentCharmState(int charm)
    {
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
        }
    }

    public void CloseDialogueBox()
    {
        StopAllCoroutines();
        dialogueBox.SetActive(false);
        dialogueText.text = string.Empty;
    }
    

}
