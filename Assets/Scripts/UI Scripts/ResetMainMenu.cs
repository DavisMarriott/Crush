using UnityEngine;
using UnityEngine.Playables;

public class ResetMainMenu : MonoBehaviour
{
    public PlayableDirector director;
    public GameObject menuCanvas;
    public AudioSource mainMenuMusic;
    public GameObject hud;
    
    public void RestartMenu()
    {
        menuCanvas.SetActive(false);
        menuCanvas.SetActive(true);
        mainMenuMusic.Stop();
        director.time = 0;
        director.Evaluate();
        mainMenuMusic.Play();
        hud.SetActive(true);
    }

}