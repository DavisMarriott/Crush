using UnityEngine;
using UnityEngine.Playables;
public class AnimationTriggerStartScreen : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private PlayableDirector director;
    public GameObject blackout;

    void Awake()
    {
        // Get the director component attached to this GameObject
        director = GetComponent<PlayableDirector>();
    }

    public void StartScreenTransition()
    {
        director.Play();
        blackout.SetActive(false);
    }
    
    
    
}
