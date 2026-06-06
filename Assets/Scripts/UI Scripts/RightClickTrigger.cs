using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events; // Optional: For using UnityEvents in the Inspector

public class RightClickTrigger : MonoBehaviour, IPointerClickHandler
{
    // Optional: You can expose UnityEvents to the Inspector for easy setup
    public UnityEvent onRightClick;
    public UnityEvent onLeftClick;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            Debug.Log("Right click detected!");
            onRightClick.Invoke(); // Invoke the custom right click event
        }
        else if (eventData.button == PointerEventData.InputButton.Left)
        {
            Debug.Log("Left click detected!");
            onLeftClick.Invoke(); // Invoke the custom left click event
        }
        else if (eventData.button == PointerEventData.InputButton.Middle)
        {
            Debug.Log("Middle click detected!");
        }
    }
}