using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class InventorySlotTwo : MonoBehaviour, IDropHandler
{
    
    public void OnDrop(PointerEventData eventData)
    
    {
        
        // if (transform.childCount == 1)
        // {
        //     Transform existingItem = GetComponentInChildren<Transform>();
        //     existingItem.gameObject.SetActive(false);
        // }
        
        if (transform.childCount == 0)
        {
            GameObject droppedItem = eventData.pointerDrag;
            InventoryManagerTwo draggableItem = droppedItem.GetComponent<InventoryManagerTwo>();
            draggableItem.parentAfterDrag = transform;
        }
        
        
        
    }
    
}
