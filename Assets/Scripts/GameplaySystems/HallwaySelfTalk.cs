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
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    
    private IEnumerator HallwayTimer()
    {
        animationTriggerThoughtBubble.ThoughtBubbleOn();
        yield return new WaitForSeconds(Random.Range(minFirstTimer, maxFirstTimer));
        yield return dialogueTiming.Run(genericHallwayLines[Random.Range(0, genericHallwayLines.Length)], selfTalkText);
        yield return new WaitForSeconds(thoughtTimer);
        selfTalkText.text = "";
        yield return new WaitForSeconds(Random.Range(minSecondTimer, maxSecondTimer));
        yield return dialogueTiming.Run(genericHallwayLines[Random.Range(0, genericHallwayLines.Length)], selfTalkText);;
        yield return new WaitForSeconds(thoughtTimer);
        selfTalkText.text = "";
    }

    public void EndHallwayTimer()
    {
        if (_hallwayRoutine != null)
            StopCoroutine(_hallwayRoutine);
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
        animationTriggerThoughtBubble.ThoughtBubbleHalf();
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
