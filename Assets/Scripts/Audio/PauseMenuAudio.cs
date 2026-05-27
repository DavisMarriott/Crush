using UnityEngine;

public class PauseMenuAudio : MonoBehaviour
{
    void Awake()
    {
        AudioSource audioSource = GetComponent<AudioSource>();
        audioSource.ignoreListenerPause = true;
    }
}
