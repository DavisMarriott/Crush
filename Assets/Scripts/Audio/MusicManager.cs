using UnityEngine;

public class MusicManager : MonoBehaviour
{
    
    public AudioSource music;
    public PhaseManager phaseManager;
    

    // Update is called once per frame
    void Update()
    {
        if (phaseManager.CurrentPhase == GamePhase.Hallway)
        {
            music.enabled = true;
        }
        
        else
        {
            music.enabled = false;
        }
    }
}
