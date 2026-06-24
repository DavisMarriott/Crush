using UnityEngine;
using UnityEngine.Playables;

public class AnimationTriggerWinScreen : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private PlayableDirector director;
    public GameObject hud;
    public AudioSource hallwayMusic;
    public AudioSource postFxAudio;

    void Awake()
    {
        // Get the director component attached to this GameObject
        director = GetComponent<PlayableDirector>();
    }

    public void WinScreenPlay()
    {
        ResetWinScreen();
        director.Play();
        hud.SetActive(false);
        hallwayMusic.Stop();
        postFxAudio.enabled = false;
        
    }

    public void ResetWinScreen()
    {
        director.time = 0;
        director.Evaluate();
    }
    
}
