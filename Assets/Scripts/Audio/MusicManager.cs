using System.Collections;
using UnityEngine;

public class MusicManager : MonoBehaviour
{
    
    public AudioSource musicHallway;
    public AudioSource musicLocker;
    public AudioSource musicMainMenu;
    public PhaseManager phaseManager;
    public GameProgression gameProgression;
    public GameObject mainMenu;
    public float mainMenuMusicVolume;
    public GameObject pauseMenu;

    void Awake()
    {
        musicMainMenu.enabled = true;
        mainMenuMusicVolume = musicMainMenu.volume;
        StartCoroutine(StartFadeIn(musicMainMenu, 3.0f, 0.35f));
    }
    
    
    void Update()
    {
        // Hallway Music //
        if (phaseManager.CurrentPhase == GamePhase.Hallway)
        {
            musicHallway.enabled = true;
            musicMainMenu.enabled = false;
            mainMenu.SetActive(false);
        }
        
        else
        {
            musicHallway.enabled = false;
        }
        
        
        // Locker Music //
        if ( gameProgression.loopCount > 1 && ( phaseManager.CurrentPhase == GamePhase.Reflect || phaseManager.CurrentPhase == GamePhase.UpgradeDraft ) )
        {
            StartCoroutine(StartLockerMusic(2.2f));
        }
        
        else
        {
            musicLocker.enabled = false;
        }

        // Mute Main menu music if paused
        if (pauseMenu.activeInHierarchy == true)
        {
            musicMainMenu.mute = true;
        }

        else
        {
            musicMainMenu.mute = false;
        }
        

    }
    
    
    // Fade Out //
    public static IEnumerator StartFadeIn(AudioSource audioSource, float duration, float targetVolume)
    {
        float currentTime = 0;
        float startVolume = audioSource.volume;

        while (currentTime < duration)
        {
            currentTime += Time.deltaTime;
            // Linearly interpolates volume over the given duration
            audioSource.volume = Mathf.Lerp(startVolume, targetVolume, currentTime / duration);
            yield return null;
        }

        // Ensure the final volume is explicitly set exactly to the target
        audioSource.volume = targetVolume;
        
    }
    
    
    public IEnumerator StartLockerMusic(float timeInSeconds)
    {
        yield return new WaitForSeconds(timeInSeconds);
        musicLocker.enabled = true;
    }
    
    
}
