using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class GameProgression : MonoBehaviour
{
    public int loopCount = 1;
    [SerializeField] private HallwaySelfTalk hallwaySelfTalk;
    [SerializeField] private GameObject firstLoopManagerObject;
    [SerializeField] private TMP_Text selfTalkText;
    [SerializeField] ConfidenceState confidenceState;
    [SerializeField] private GameObject specialLoopConditions;
    [SerializeField] private FirstLoopManager firstLoopManager;
    [SerializeField] private Collider2D inConversationTrigger;
    [SerializeField] private AnimationTriggerThoughtBubble animationTriggerThoughtBubble;
    [SerializeField] private DialogueTiming dialogueTiming;
    [SerializeField] private ReflectSelfTalk reflectSelfTalk;
    [Header("Loop 1 reflect lines — placeholder for v1, will be replaced by SO-driven content from the Narrative Bible import (task 86b9z5x7z)")]
    [SerializeField, TextArea(2, 5)] private string[] loop1ReflectLines;
    public LoopSnapshot lastLoop;

    private void Start()
    {
        // Game starts in Reflect phase on loop 1 (per Narrative Bible's loop 1 opening monologue).
        // Loop 2+ never hit this path — Start() only runs once per scene load.
        StartCoroutine(StartGameOnReflect());
    }

    // Initial scene-load entry: play loop 1 reflect lines, then transition into the normal Hallway loop.
    // Distinct from DeathRespawn's reflect handling (which uses LoopSnapshot-driven branch selection).
    // Here loop 1 reflect is scripted, not branch-selected, so we use ReflectSelfTalk.PlayLines directly.
    private IEnumerator StartGameOnReflect()
    {
        // Wait one frame so every other component's Start() has completed —
        // otherwise AnimationTriggerThoughtBubble.thoughtBubbleAnimator (assigned in its Start)
        // and ReflectDraftCameraController's PhaseManager subscription (also in Start) can race
        // against this coroutine firing too early.
        yield return null;

        // Transition to Reflect — fires OnPhaseChanged so the camera controller cuts to DraftScreenCam
        // and other phase-aware systems init correctly.
        PhaseManager.Instance.TransitionTo(GamePhase.Reflect);

        // Play the scripted loop 1 reflect lines using the same PlayLines overload that milestones use.
        // Bypasses branch selection (no LoopSnapshot needed on game start — no prior loop exists).
        if (loop1ReflectLines != null && loop1ReflectLines.Length > 0)
            yield return reflectSelfTalk.PlayLines(loop1ReflectLines);

        // Now transition to Hallway and run the normal loop setup (BasicLoop + FirstLoopActive).
        PhaseManager.Instance.TransitionTo(GamePhase.Hallway);
        SetLoopConditions();
    }

    public void SetLoopConditions()
    {
        BasicLoop();
        
        if (loopCount == 1)
        {
   
            FirstLoopActive();
        }
    }
    
    public void NextLoop()
    {
        loopCount++;
    }
    
    //turns off all special loop conditions
    private void BasicLoop()
    {
        hallwaySelfTalk.enabled = true;
        inConversationTrigger.enabled = true;
        confidenceState.inConversation = false;
        // (removed redundant EndHallwayTimer() — HallwaySelfTalk's OnEnable / OnDisable /
        //  OnPhaseChanged lifecycle now handles timer cleanup and re-start.
        //  Calling End here was killing the timer right after OnEnable started it.)
        for (int i = specialLoopConditions.transform.childCount - 1; i >= 0; i--)
            specialLoopConditions.transform.GetChild(i).gameObject.SetActive(false);
    }
    
    public void FirstLoopActive()
    {
        hallwaySelfTalk.enabled = false;
        firstLoopManagerObject.SetActive(true);
    }
    
    public void HallwayTriggerHit(int  triggerIndex)
    {
        // Bring the thought bubble to Full so the first-loop trigger line is visible
        animationTriggerThoughtBubble.ThoughtBubbleOn();

        string line = null;
        if (triggerIndex == 1) line = firstLoopManager.firstLoopHallwayLines[0];
        else if (triggerIndex == 2) line = firstLoopManager.firstLoopHallwayLines[1];
        else if (triggerIndex == 3) line = firstLoopManager.firstLoopHallwayLines[2];

        if (line != null)
        {
            // Use the same typing effect as conversation/draft/reflect phases
            dialogueTiming.Run(line, selfTalkText);
            confidenceState.confidence -= 1;
        }
    }

    //this is our "trigger end game" success state. Need to build content
    public void AskedToDance()
    {
        Debug.Log("YOU DID IT!");
    }
}
