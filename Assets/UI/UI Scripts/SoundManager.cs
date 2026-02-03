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

    // Example functions to play sounds
    public void PlaySoundEffect1()
    {
        audioSource.PlayOneShot(soundEffect1);
    }

    public void PlaySoundEffect2()
    {
        audioSource.PlayOneShot(soundEffect2);
    }
    
    public void PlaySoundEffect3()
    {
        audioSource.PlayOneShot(soundEffect3);
    }
    
    public void PlaySoundEffect4()
    {
        audioSource.PlayOneShot(soundEffect4);
    }
    
    public void PlaySoundEffect5()
    {
        audioSource.PlayOneShot(soundEffect5);
    }
    
    public void PlaySoundEffect6()
    {
        audioSource.PlayOneShot(soundEffect6);
    }
    
}