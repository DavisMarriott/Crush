using UnityEngine;

// Quit-to-desktop for builds. Wire a menu Quit button's OnClick -> QuitGame.Quit().
// In the editor it just stops play mode so the button is testable without a build.
public class QuitGame : MonoBehaviour
{
    public void Quit()
    {
        Debug.Log("[QuitGame] Quit requested.");
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
