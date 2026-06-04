using UnityEngine;

public class SoundManager : MonoBehaviour
{
    private AudioSource audioSource;
    public AudioClip soundEffect1;
    public AudioClip soundEffect2;
    public AudioClip soundEffect3;
    public AudioClip soundEffect4;
    public AudioClip soundEffect5;
    public AudioClip soundEffect6;
    

    void Start()
    {
        // Get the AudioSource component attached to this GameObject
        audioSource = GetComponent<AudioSource>();

        // Optional: ensure the AudioSource exists
        if (audioSource == null)
        {
            Debug.LogError("AudioSource component missing from GameObject!");
        }
        
    }

    // single guarded play path - bails instead of NRE'ing if the source is missing/disabled
    // or the clip is null (was crashing here on loop 7's death SFX - SoundManager.cs:50)
    private void Play(AudioClip clip)
    {
        if (audioSource == null) audioSource = GetComponent<AudioSource>();
        if (audioSource == null || !audioSource.enabled || clip == null) return;
        audioSource.PlayOneShot(clip);
    }

    // Example functions to play sounds
    public void PlaySoundEffect1() => Play(soundEffect1);
    public void PlaySoundEffect2() => Play(soundEffect2);
    public void PlaySoundEffect3() => Play(soundEffect3);
    public void PlaySoundEffect4() => Play(soundEffect4);
    public void PlaySoundEffect5() => Play(soundEffect5);
    public void PlaySoundEffect6() => Play(soundEffect6);

}