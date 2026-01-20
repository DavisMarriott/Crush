using System;
using Unity.VisualScripting;
using UnityEngine;

public class TalkApproach : MonoBehaviour
{

    public GameObject DialogueUI;

    private void Start() 
    {
        // Hide UI at the beginning
        DialogueUI.SetActive(false);
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if(other.CompareTag("Player"))
        {
            DialogueUI.SetActive(true);
        }
    }
    
    private void OnTriggerExit2D(Collider2D other)
    {
        if(other.CompareTag("Player"))
        {
            DialogueUI.SetActive(false);
        }
    }
    
}
