using UnityEngine;

public class StartGamePaused : MonoBehaviour
{
    public bool paused = true;
    public GameObject pauseMenu;
    
    void Awake()
    {
        Time.timeScale = 0;
        AudioListener.pause = true;
        pauseMenu.SetActive(false);
        // paused =  true;
        // menuCanvas.SetActive(true);
        
    }

    public void StartGameTime()
    {
        Time.timeScale = 1;
        AudioListener.pause = false;
        pauseMenu.SetActive(true);
        // paused = false;
        // menuCanvas.SetActive(false);
    }
    
    
}
