using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class InventoryManagerTwo : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler

{
    
    public bool isAvailable = true;
    public bool isInInventory = false;
    public GameObject availableParent;
    public GameObject inventoryParent;
    public int nextIndex;
    
    public Image image;
    
    [HideInInspector] public Transform parentAfterDrag;
    
    
    // BEGIN DRAG //
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
    // END DRAG //
    
    // // BEGIN ADD TO INVENTORY //
    // public void AddToInventory()
    // {
    //     // This for loop goes though an index inventoryParent children until it finds a child without its own child
    //     // Then it sets nextIndex to that number.
    //     for (int i = 0; i < inventoryParent.transform.childCount; i++)
    //     {
    //         if (inventoryParent.transform.GetChild(i).childCount == 0)
    //         {
    //             nextIndex = i;
    //             Debug.Log(inventoryParent.transform.GetChild(i).name);
    //             break;
    //         }
    //     }
    //     
    //     
    //     if (isInInventory == false)
    //         
    //     {
    //         
    //         Transform nextInventorySlot = inventoryParent.transform.GetChild(nextIndex);
    //         
    //         if (nextInventorySlot.childCount == 0)
    //         {
    //             this.transform.SetParent(nextInventorySlot);
    //         
    //             isInInventory = true;
    //         
    //             Debug.Log(nextInventorySlot.name);
    //             
    //         }
    //     
    //     }
    // }
    //
    // // BEGIN REMOVE FROM INVENTORY //
    // public void RemoveFromInventory()
    // {
    //     
    //     for (int i = 0; i < availableParent.transform.childCount; i++)
    //     {
    //         if (availableParent.transform.GetChild(i).childCount == 0)
    //         {
    //             nextIndex = i;
    //             Debug.Log(availableParent.transform.GetChild(i).name);
    //             break;
    //         }
    //     }
    //     
    //     if (isInInventory == true)
    //         
    //     {
    //         
    //         Transform nextAvailableSlot = availableParent.transform.GetChild(nextIndex);
    //
    //         if (nextAvailableSlot.childCount == 0)
    //         {
    //             this.transform.SetParent(nextAvailableSlot);
    //             
    //             isInInventory = false;
    //                 
    //             Debug.Log(nextAvailableSlot.name);
    //                 
    //         }
    //         
    //     }
    //     
    // }
    

}
