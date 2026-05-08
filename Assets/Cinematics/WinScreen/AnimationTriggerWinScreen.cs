using UnityEngine;
using UnityEngine.Playables;

public class AnimationTriggerWinScreen : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private PlayableDirector director;

    void Awake()
    {
        // Get the director component attached to this GameObject
        director = GetComponent<PlayableDirector>();
    }

    public void WinScreenPlay()
    {
        director.Play();
    }
    
}
