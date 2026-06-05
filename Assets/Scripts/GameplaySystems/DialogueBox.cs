using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.InputSystem;


public class DialogueBox : MonoBehaviour
{
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private AnimationTriggerPlayer animationTriggerPlayer;
    [SerializeField] private AnimationTriggerCrush animationTriggerCrush;
    [SerializeField] private AnimationTriggerThoughtBubble animationTriggerThoughtBubble;
    [SerializeField] private AnimationTriggerSpeechBubble animationTriggerSpeechBubblePlayer;
    [SerializeField] private AnimationTriggerSpeechBubble animationTriggerSpeechBubbleCrush;
    [SerializeField] private GameObject dialogueBox;
    [SerializeField] private InputActionReference nextLineAction;
    [SerializeField] private TMP_Text dialogueText;
    [SerializeField] private ConfidenceState confidenceState;
    [SerializeField] private DeckManager deckManager;
    [SerializeField] private CardUpgradeTracker upgradeTracker;
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

    [Header("Per-loop intro lines (play once on first card per loop)")]
    [SerializeField] private string lukeIntroLine;
    [SerializeField] private string daisyIntroLine;

    [Header("Death")]
    [Tooltip("Seconds the final Death-branch line holds on screen (no input) before Luke dies. Player shouldn't have to 'press space to die'.")]
    [SerializeField] private float deathFinalLineHold = 2f;



    public void Start()
    {
        CloseDialogueBox();
    }

    private void OnEnable()  => nextLineAction.action.Enable();
    private void OnDisable() => nextLineAction.action.Disable();


    public void ShowDialogue(DialogueCard dialogueCard)
    {
        // Fold in any applied card upgrade (branch overrides) and pass loop context so a
        // DanceCard can special-case loop 1. upgradeTracker may be unwired in older scenes.
        var appliedUpgrade = upgradeTracker != null ? upgradeTracker.GetAppliedUpgrade(dialogueCard) : null;
        var lukeBranch = dialogueCard.GetLukeBranch(confidenceState.confidence, appliedUpgrade, gameProgression.loopCount);

        if (lukeBranch == null || lukeBranch.dialogue == null || lukeBranch.dialogue.Length == 0)
        {
            // no death branch authored - slump now, since the line loop below won't run to do it
            playerMovement.GetConfidencePose();
            Debug.LogWarning($"No valid Luke branch for confidence {confidenceState.confidence} on card {dialogueCard.previewText}");
            return;
        }
        // Register tags from the Luke branch as fired this loop
        deckManager.RegisterTags(lukeBranch.tags);
        // Bring thought bubble to full while dialogue is showing
        animationTriggerThoughtBubble.ThoughtBubbleOn();
        dialogueBox.SetActive(true);
        //narrow defer-gate: only true for Death branch dialogues - lets the Death lines play
        //before DeathRespawn closes the dialogue UI. Normal/Awkward/etc. don't set this.
        _isPlayingDeathBranch = (lukeBranch.branchName == "Death");

        // pose reflects the just-paid cost BEFORE any line types (play card -> cost -> pose -> line).
        // Cost was deducted in the click handler, so confidence is already current here.
        // Death branches keep the delayed slump (held until the final line) - don't pre-pose those.
        if (!_isPlayingDeathBranch)
            playerMovement.GetConfidencePose();
        StartCoroutine(SelectAnim());
        //wait to let the select animation play
        IEnumerator SelectAnim()
        {
            yield return new WaitForSeconds(.5f);
            cardContainer.SetActive(false);
            StartCoroutine(StepThroughDialogue(dialogueCard, lukeBranch));
        }
   
    }

    // Plays a single intro line (no confidence/charm impact, no card data).
    // Used at the top of the first card-played per loop to play the character intro
    // before the card's own Luke/Daisy branches fire.
    private IEnumerator PlayIntroLine(string line, DialogueCard.DialogueCharacter speaker)
    {
        if (string.IsNullOrEmpty(line)) yield break;
        SetSpeakerIndicator(speaker);
        if (speaker == DialogueCard.DialogueCharacter.Boy)
            playerMovement.PlayerTalk();
        yield return dialogueTiming.Run(line, dialogueText);
        yield return new WaitUntil(() => DialogueAdvance.Pressed(nextLineAction.action));
        dialogueText.text = "";
    }

    // Daisy's intro - fired from the conversation TriggerZone's onTriggerEnter (the same trigger
    // that stops movement / swaps to the convo camera). Wire this on that trigger in the inspector.
    // Luke's intro still plays on the first card (top of StepThroughDialogue).
    private bool _daisyIntroRunning;

    [Tooltip("Small beat between hitting the convo trigger and Daisy's intro starting.")]
    [SerializeField] private float daisyIntroDelay = 0.25f;

    public void PlayDaisyIntroAtTrigger()
    {
        // always clear the lingering hallway self-talk a beat after convo entry (bug 86ba9uzau) -
        // runs BEFORE the intro gates so it fires even on loops where her intro is skipped/already made
        if (hallwaySelfTalk != null)
            hallwaySelfTalk.ClearHallwayLineOnConvoEntry();

        if (confidenceState.daisyIntroMade) return;
        // note: deliberately NOT gated on ShouldSkipIntrosThisLoop - Daisy always greets at the
        // trigger, every loop. The skipIntros override only applies to Luke's convo-side intro.
        confidenceState.daisyIntroMade = true;
        // the box object is inactive between conversations (CloseDialogueBox turns it off), and an
        // inactive GO can't StartCoroutine - reactivate BEFORE starting, same as ShowDialogue does.
        dialogueBox.SetActive(true);
        StartCoroutine(DaisyIntroAtTrigger());
    }

    private IEnumerator DaisyIntroAtTrigger()
    {
        _daisyIntroRunning = true;
        // tiny beat so her line doesn't start the same frame you cross the trigger
        yield return new WaitForSeconds(daisyIntroDelay);
        yield return PlayIntroLine(daisyIntroLine, DialogueCard.DialogueCharacter.Girl);
        // tuck just her bubble away. Deliberately NOT CloseDialogueBox() - that StopAllCoroutines()s,
        // which would kill this routine mid-call AND any card dialogue the player started meanwhile.
        animationTriggerSpeechBubbleCrush.SpeechBubbleHide();
        _daisyIntroRunning = false;
    }

    //this is where dialogue plays/bulk of conversation system lives
    private IEnumerator StepThroughDialogue(DialogueCard dialogueCard, DialogueCard.DialogueBranch lukeBranch)
    {
        // if Daisy's trigger-intro is mid-line, let it finish before the card dialogue takes the box
        yield return new WaitUntil(() => !_daisyIntroRunning);

        // First card per loop: play Luke's intro line ("Hey Daisy") before his card dialogue runs.
        // (Daisy's intro now fires earlier, at the convo trigger - see PlayDaisyIntroAtTrigger.)
        // confidenceState.introMade gets flipped to true at the end of this method,
        // so subsequent cards in the same loop skip Luke's intro.
        // Per-loop override: GameProgression.ShouldSkipIntrosThisLoop() reads from LoopHallway.skipIntros.
        // ...but never on a death branch - "Hey Daisy" makes no sense if he's dying
        if (!confidenceState.introMade && !gameProgression.ShouldSkipIntrosThisLoop() && !_isPlayingDeathBranch)
            yield return PlayIntroLine(lukeIntroLine, DialogueCard.DialogueCharacter.Boy);

        //parse card data and play (including confidence/charm scores) - for Luke Branch
        for (int i = 0; i < lukeBranch.dialogue.Length; i++)
        {
            DialogueCard.DialogueLine line = lukeBranch.dialogue[i];
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
            
            // death branch: hold the slump til the final line (below); other branches pose per line
            if (!_isPlayingDeathBranch)
                playerMovement.GetConfidencePose();
            animationTriggerCrush.GetCharmPose();

            // final death line - don't make the player press space to die. slump, hold a beat,
            // then the loop ends and Death() takes over. other lines advance on input.
            bool isFinalDeathLine = _isPlayingDeathBranch && i == lukeBranch.dialogue.Length - 1;
            if (isFinalDeathLine)
            {
                playerMovement.GetConfidencePose();
                yield return new WaitForSeconds(deathFinalLineHold);
            }
            else
            {
                yield return new WaitUntil(() => DialogueAdvance.Pressed(nextLineAction.action));
                hallwaySelfTalk.selfTalkText.text = "";
                dialogueText.text = "";
            }
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

        if (daisyBranch != null)
        {
            // Register tags from the Daisy branch as fired this loop
            deckManager.RegisterTags(daisyBranch.tags);
            // (Daisy's intro used to play here - it now fires at the convo trigger instead.)
        }

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
                yield return new WaitUntil(() => DialogueAdvance.Pressed(nextLineAction.action));
                hallwaySelfTalk.selfTalkText.text = "";
                dialogueText.text = "";
            }
        }

        // Card finished playing — minimize bubble between cards
        animationTriggerThoughtBubble.ThoughtBubbleHalf();
        confidenceState.introMade = true;

        // grab it before CloseDialogueBox clears the flag
        bool wasDeathBranch = _isPlayingDeathBranch;
        CloseDialogueBox();

        if (wasDeathBranch)
        {
            // died this card - don't deal a new hand. Death() was deferred til now and takes
            // over. drawing here flashed a stray "next turn" before the death screen showed.
        }
        else if (dialogueCard.isDance)
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
                animationTriggerSpeechBubblePlayer.SpeechBubbleShow();
                animationTriggerSpeechBubbleCrush.SpeechBubbleHide();                
                dialogueText.alignment = TMPro.TextAlignmentOptions.MidlineLeft;
                dialogueText.color = boyTextColor;
                leftArrow.SetActive(false);
                rightArrow.SetActive(false);
                break;
            case DialogueCard.DialogueCharacter.Girl:
                animationTriggerSpeechBubblePlayer.SpeechBubbleHide();
                animationTriggerSpeechBubbleCrush.SpeechBubbleShow(); 
                dialogueText.alignment = TMPro.TextAlignmentOptions.MidlineRight;
                dialogueText.color = girlTextColor;
                leftArrow.SetActive(false);
                rightArrow.SetActive(false);
                break;
        }
    }

    public void CloseDialogueBox()
    {
        StopAllCoroutines();
        dialogueBox.SetActive(false);
        _isPlayingDeathBranch = false;
        dialogueText.text = string.Empty;
        //also clear the internal-monologue text - if the card's last line was BoyInternal and
        //the per-line cleanup got skipped (coroutine killed, death race, etc.), it'd otherwise
        //linger into the next hallway phase under the minimized bubble.
        if (hallwaySelfTalk != null && hallwaySelfTalk.selfTalkText != null)
            hallwaySelfTalk.selfTalkText.text = string.Empty;
        //hide both speech bubbles - if the last line was Daisy's (or Luke's) the bubble is in
        //its "Show" animator state and won't go away on its own. SetSpeakerIndicator hides the
        //other one each line, but the FINAL line's bubble never gets a hide command otherwise.
        if (animationTriggerSpeechBubblePlayer != null) animationTriggerSpeechBubblePlayer.SpeechBubbleHide();
        if (animationTriggerSpeechBubbleCrush != null) animationTriggerSpeechBubbleCrush.SpeechBubbleHide();
    }

    //narrow gate - true ONLY while ShowDialogue is playing a card whose chosen branch is "Death".
    //DeathRespawn uses this to defer death-screen takeover until the Death branch dialogue finishes.
    //Other branches (Normal/Awkward/etc.) DON'T set this - if confidence drops to 0 mid-Normal,
    //Death() fires immediately and kills any remaining lines (acceptable - the remaining Normal
    //lines wouldn't make narrative sense post-death anyway).
    private bool _isPlayingDeathBranch;
    public bool IsPlayingDeathBranch => _isPlayingDeathBranch;
    

}
