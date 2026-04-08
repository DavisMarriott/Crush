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
        if (triggerIndex == 1)
        {
            selfTalkText.text = firstLoopManager.firstLoopHallwayLines[0];
            confidenceState.confidence -= 1;

        }
  
        else if (triggerIndex == 2)
        {
            selfTalkText.text = firstLoopManager.firstLoopHallwayLines[1];
            confidenceState.confidence -= 1;
        }
        else if (triggerIndex == 3)
        {
            selfTalkText.text = firstLoopManager.firstLoopHallwayLines[2];
            confidenceState.confidence -= 1;
        }
    }

    public void AskedToDance()
    {
        Debug.Log("YOU DID IT!");
    }
}
