using System.Collections;
using UnityEngine;

public enum SpeakingCharacter
{
    LukeSelf,
    Luke,
    Daisy
}
public class RandomSoundPlayer : MonoBehaviour
{
    [Header("Audio Components")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip[] soundClips;
    
    [Header("Clip Timing")]
    [SerializeField] private float minDelay = 0.05f;
    [SerializeField] private float maxDelay = 0.1f;
    public float timeBetweenClips;
    public float nextPlayTime;

    [Header("Pitch Variety")]
    [SerializeField] private float minPitchLuke;
    [SerializeField] private float maxPitchLuke;
    [SerializeField] private float minPitchLukeSelf;
    [SerializeField] private float maxPitchLukeSelf;
    [SerializeField] private float minPitchDaisy;
    [SerializeField] private float maxPitchDaisy;

    public SpeakingCharacter speakingCharacter;

    [Header("Temp: dialogue blips disabled by request 2026-06-16 — uncheck to bring them back")]
    [SerializeField] private bool muted = true;

    public void PlayRandomSound()
    {
        // master off-switch for the dialogue text blips (both internal + external). Flip in inspector.
        if (muted) return;

        if (soundClips == null || soundClips.Length == 0 )
        {
            return;
        }
        
        timeBetweenClips = Random.Range(minDelay, maxDelay);
        
        int randomIndex = Random.Range(0, soundClips.Length);
        AudioClip chosenClip = soundClips[randomIndex];
        
        if (speakingCharacter == SpeakingCharacter.Daisy)
        {
            audioSource.pitch = Random.Range(minPitchDaisy, maxPitchDaisy);
            audioSource.panStereo = 0.7f;
        }

        else if  (speakingCharacter == SpeakingCharacter.Luke)
        {
            audioSource.pitch = Random.Range(minPitchLuke, maxPitchLuke);
            audioSource.panStereo = -0.7f;
        }
        
        else if (speakingCharacter == SpeakingCharacter.LukeSelf)
        {
            audioSource.pitch = Random.Range(minPitchLukeSelf, maxPitchLukeSelf);
            audioSource.panStereo = -0.3f;
            
        }

        audioSource.PlayOneShot(chosenClip);
        nextPlayTime = Time.time + timeBetweenClips;
    }
    
}