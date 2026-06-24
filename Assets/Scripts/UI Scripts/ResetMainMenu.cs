using UnityEngine;
using UnityEngine.Playables;

public class ResetMainMenu : MonoBehaviour
{
    public PlayableDirector director;
    public GameObject menuCanvas;
    public AudioSource mainMenuMusic;
    public GameObject hud;
    public AudioSource postFxAudio;
    public AnimationTriggerWinScreen animationTriggerWinScreen;
    public GameObject winScreen;

    
    public void RestartMenu()
    {
        menuCanvas.SetActive(true);
        mainMenuMusic.Stop();
        animationTriggerWinScreen.ResetWinScreen();
        director.time = 0;
        director.Evaluate();
        mainMenuMusic.Play();
        hud.SetActive(true);
        postFxAudio.enabled = true;
        winScreen.SetActive(false);
    }

}