using System.Collections;
using UnityEngine;

public class AudioFader : MonoBehaviour
{
    
    public AudioSource audioSource;
    // public float targetVolume;
    // public float duration;
    
    public void StartFade(float duration, float targetVolume)
        {
            StartCoroutine(FadeVolume(duration, targetVolume));
        }
    
    void Start()
    {
        StartCoroutine(FadeVolume(3, 1));
    }
    
    private IEnumerator FadeVolume(float duration, float targetVolume)
        {
            float currentTime = 0;
            float startVolume = audioSource.volume;
    
            while (currentTime < duration)
            {
                currentTime += Time.deltaTime;
                // Linearly interpolate volume over time
                audioSource.volume = Mathf.Lerp(startVolume, targetVolume, currentTime / duration);
                yield return null;
            }
    
            audioSource.volume = targetVolume;
        }
    
    
    // private Animator animator;
    //
    // void Start()
    // {
    //     animator = GetComponent<Animator>();
    //     FadeIn();
    // }
    //
    // public void FadeIn()
    // {
    //     animator.Play("Audio_Fade_In", 0);
    // }
    //
    // public void FadeOut()
    // {
    //     animator.Play("Audio_Fade_Out", 0);
    // }

}
