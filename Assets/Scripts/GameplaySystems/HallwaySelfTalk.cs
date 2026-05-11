using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Random = UnityEngine.Random;

public class HallwaySelfTalk : MonoBehaviour
{
    [SerializeField] private DialogueTiming dialogueTiming;
    [SerializeField] private string[] genericHallwayLines;
    [SerializeField] private Animator letterBoxAnimator;
    [SerializeField] public AnimationTriggerThoughtBubble animationTriggerThoughtBubble;
    [SerializeField] public TMP_Text selfTalkText;
    [SerializeField] private float minFirstTimer = 1f;
    [SerializeField] private float maxFirstTimer = 4f;
    [SerializeField] private float minSecondTimer = 4f;
    [SerializeField] private float maxSecondTimer = 10f;
    [SerializeField] private float thoughtTimer = 3f;
    private Coroutine _hallwayRoutine;
    public bool draftLinesActive = false;

    private void OnEnable()
    {
        if (PhaseManager.Instance != null)
        {
            PhaseManager.Instance.OnPhaseChanged += HandlePhaseChanged;
            // Handle the "got enabled while already in Hallway" case —
            // the OnPhaseChanged event already fired before subscription, so
            // we'd miss the kickoff otherwise. (Common: post-respawn flow sets
            // PhaseManager → Hallway BEFORE BasicLoop flips hallwaySelfTalk.enabled = true.)
            if (PhaseManager.Instance.CurrentPhase == GamePhase.Hallway)
                StartHallwayTimer();
        }
    }

    private void OnDisable()
    {
        if (PhaseManager.Instance != null)
            PhaseManager.Instance.OnPhaseChanged -= HandlePhaseChanged;
        EndHallwayTimer();
    }

    private void HandlePhaseChanged(GamePhase oldPhase, GamePhase newPhase)
    {
        if (newPhase == GamePhase.Hallway)
            StartHallwayTimer();
        else if (oldPhase == GamePhase.Hallway)
            EndHallwayTimer();
    }

    public void StartHallwayTimer()
    {
        // Bail if no lines authored yet — otherwise HallwayTimer would throw on Random.Range
        if (genericHallwayLines == null || genericHallwayLines.Length == 0) return;
        if (_hallwayRoutine != null) StopCoroutine(_hallwayRoutine);
        _hallwayRoutine = StartCoroutine(HallwayTimer());
    }

    private IEnumerator HallwayTimer()
    {
        // First line
        yield return new WaitForSeconds(Random.Range(minFirstTimer, maxFirstTimer));
        animationTriggerThoughtBubble.ThoughtBubbleOn();
        yield return dialogueTiming.Run(genericHallwayLines[Random.Range(0, genericHallwayLines.Length)], selfTalkText);
        yield return new WaitForSeconds(thoughtTimer);
        selfTalkText.text = "";
        animationTriggerThoughtBubble.ThoughtBubbleHalf();

        // Second line
        yield return new WaitForSeconds(Random.Range(minSecondTimer, maxSecondTimer));
        animationTriggerThoughtBubble.ThoughtBubbleOn();
        yield return dialogueTiming.Run(genericHallwayLines[Random.Range(0, genericHallwayLines.Length)], selfTalkText);
        yield return new WaitForSeconds(thoughtTimer);
        selfTalkText.text = "";
        animationTriggerThoughtBubble.ThoughtBubbleHalf();
    }

    public void EndHallwayTimer()
    {
        if (_hallwayRoutine != null)
        {
            StopCoroutine(_hallwayRoutine);
            _hallwayRoutine = null;
        }
        selfTalkText.text = "";
    }

    public void TriggerDraftLines(DialogueCard.DraftLine[] draftLines)
    {
        StartCoroutine(PlayDraftLines(draftLines));
    }

    public void TriggerLetterBoxOut()
    {
        letterBoxAnimator.SetTrigger("LetterBoxOut");
    }
    public IEnumerator PlayDraftLines(DialogueCard.DraftLine[] draftLines)
    {
        // (bubble should already be Half from end of ReflectSelfTalk.PlayLines)
        draftLinesActive = true;
        foreach (DialogueCard.DraftLine draftLine in draftLines)
        {
            animationTriggerThoughtBubble.ThoughtBubbleOn();
            yield return new WaitForSeconds(0.5f);
            yield return dialogueTiming.Run(draftLine.line, selfTalkText);
            yield return new WaitForSeconds(1.5f);
        }
        letterBoxAnimator.SetTrigger("LetterBoxOut");
        animationTriggerThoughtBubble.ThoughtBubbleHalf();
        draftLinesActive = false;
    }
    
    
}
