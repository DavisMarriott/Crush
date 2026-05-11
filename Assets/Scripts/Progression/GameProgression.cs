using System;
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
    public LoopSnapshot lastLoop;

    private void Start()
    {
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
