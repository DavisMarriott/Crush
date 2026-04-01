using UnityEngine;
using TMPro;

public class FirstLoopManager : MonoBehaviour
{
 //gameprogression has loop count
 [SerializeField] private GameProgression gameProgresion;
 [SerializeField] private HallwaySelfTalk hallwaySelfTalk;
 [SerializeField] private GameObject hallwayTriggers;
 [SerializeField] private TMP_Text selfTalkText;
 [SerializeField] private string[] firstLoopHallwayLines;
 

 public void Start()
 {
  if (gameProgresion.loopCount == 1)
  {
   hallwaySelfTalk.enabled = false;
   FirstLoopActive();
  }
 }
 public void FirstLoopActive()
 {
  hallwayTriggers.SetActive(true);
 }

 public void HallwayTriggerHit(int  triggerIndex)
 {
  Debug.Log("Trigger hit: " + triggerIndex);
  if (triggerIndex == 1)
  {
   selfTalkText.text = firstLoopHallwayLines[0];
  }
  
  else if (triggerIndex == 2)
   {
   selfTalkText.text = firstLoopHallwayLines[1];
   }
  else if (triggerIndex == 3)
  {
   selfTalkText.text = firstLoopHallwayLines[2];
  }
 }
 
 
}
