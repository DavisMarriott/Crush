using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ButtonController : MonoBehaviour
{

    public Button button;
    public EventTrigger eventTrigger;
    
    public void DisableButton()
    {
        button.interactable = false; 
        eventTrigger.enabled = false;
    }
    
    public void EnableButton()
    {
        button.interactable = true; 
        eventTrigger.enabled = true;
    }
}
