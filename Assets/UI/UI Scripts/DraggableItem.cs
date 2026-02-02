using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;


public class DraggableItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public Image image;
    
    [HideInInspector] public Transform parentAfterDrag;
    
    public void OnBeginDrag(PointerEventData eventData)
    {
        Debug.Log("OnBeginDrag");
        parentAfterDrag = transform.parent;
        transform.SetParent(transform.root);
        transform.SetAsLastSibling();
        image.raycastTarget = false;
    }

    
    public void OnDrag(PointerEventData eventData)
    {
        Debug.Log("OnDrag");
        transform.position = Input.mousePosition;
    }

    
    public void OnEndDrag(PointerEventData eventData)
    {
        Debug.Log("OnEndDrag");
        transform.SetParent(parentAfterDrag);
        image.raycastTarget = true;
    }
    

    // void Update()
    // {
    //     string UISection = transform.parent.parent.parent.name;
    //
    //     if (UISection == "Pool")
    //     {
    //         image.color = new Color(1.0f, 1.0f, 1.0f, 0.75f);
    //     }
    //     else
    //     {
    //         image.color = new Color(1.0f, 1.0f, 1.0f, 01.0f);
    //     }
    // }
    
}
