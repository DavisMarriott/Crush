using UnityEngine;
using System.Collections.Generic;

public class GetImmediateChildren : MonoBehaviour
{

    // public int randomChild;
    void Start()
    {
        // Create a List to store the child GameObjects
        List<GameObject> childrenList = new List<GameObject>();

        // Iterate through each child transform
        foreach (Transform child in transform)
        {
            childrenList.Add(child.gameObject);
        }

        // Convert the List to an array if needed
        GameObject[] childrenArray = childrenList.ToArray();

        // Example: Print the name of each child
        foreach (GameObject child in childrenArray)
        {
            Debug.Log("Child name: " + child.name);
        }
    }

    void Update()
    {
        
    }
}