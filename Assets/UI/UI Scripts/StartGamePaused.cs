using UnityEngine;

public class StartGamePaused : MonoBehaviour
{
    public bool paused = true;
    
    void Start()
    {
        Time.timeScale = 0;
        AudioListener.pause = true;
        // paused =  true;
        // menuCanvas.SetActive(true);
        
    }

    public void StartGameTime()
    {
        Time.timeScale = 1;
        AudioListener.pause = false;
        // paused = false;
        // menuCanvas.SetActive(false);
    }
}
