using UnityEngine;
using System.Collections.Generic;
using Random = UnityEngine.Random;

public class InventoryManager : MonoBehaviour

{
    
    public bool isAvailable = true;
    public bool isInInventory = false;
    public GameObject availableParent;
    public GameObject inventoryParent;

    public void AddToInventory()
    {
        
        if (isInInventory == false)
            
        {
            
            int randomIndex = Random.Range(0, inventoryParent.transform.childCount);
            
            Transform randomInventorySlot = inventoryParent.transform.GetChild(randomIndex);

            if (randomInventorySlot.childCount == 0)
                
            {
                this.transform.SetParent(randomInventorySlot);
            
                isInInventory = true;
            
                Debug.Log(randomInventorySlot.name);
                
            }
            
            if (randomInventorySlot.childCount > 0)

            {
                Debug.Log("Slot is full!");
            }
            
        
        }
    }
    
    public void RemoveFromInventory()
    {
        
        if (isInInventory == true)
            
        {
            
            int randomIndex = Random.Range(0, availableParent.transform.childCount);
            
            Transform randomAvailableSlot = availableParent.transform.GetChild(randomIndex);

            if (randomAvailableSlot.childCount == 0)
                
            {
                isInInventory = false;
            
                this.transform.SetParent(randomAvailableSlot);
                
                Debug.Log(randomAvailableSlot.name);
                
            }

            if (randomAvailableSlot.childCount > 0)

            {
                Debug.Log("Slot is full!");
            }

        }
    }

}
