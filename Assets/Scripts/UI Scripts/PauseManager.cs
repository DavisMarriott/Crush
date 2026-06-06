using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PauseManager : MonoBehaviour
{
    
    public bool paused = false;
    // Assign menu game object here
    public GameObject menuCanvas;
    public AudioSource mainMenuMusic;
    
    InputSystem_Actions.PauseActions action;

    private void Awake()
    {
        action = new InputSystem_Actions.PauseActions();
    }

    private void OnEnable()
    {
        action.Enable();
    }

    private void OnDisable()
    {
        action.Disable();
    }

    private void Start()
    {
        action.PauseGame.performed  += _ => DeterminePause();
    }
    
    public void Update()
    {
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            DeterminePause();
        }    
    }

    public void DeterminePause()
    {
        if (paused)
            ResumeGame();
        else
            PauseGame();
    }

    public void PauseGame()
    {
        Time.timeScale = 0;
        AudioListener.pause = true;
        paused =  true;
        menuCanvas.SetActive(true);
        mainMenuMusic.volume = 0f;

    }

    public void ResumeGame()
    {
        Time.timeScale = 1;
        AudioListener.pause = false;
        paused = false;
        menuCanvas.SetActive(false);
        mainMenuMusic.volume = 0.25f;
    }

   
}
