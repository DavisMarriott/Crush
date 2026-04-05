using UnityEngine;
using Unity.Cinemachine;

// Add this to a GameObject in the Test Environment scene.
// It overrides the normal game flow:
//   - Skips the walk-up, goes straight to conversation
//   - On death, opens the debug menu instead of the draft UI

public class TestSceneManager : MonoBehaviour
{
    [SerializeField] private ConfidenceState confidenceState;
    [SerializeField] private DebugMenu debugMenu;
    [SerializeField] private GameObject dialogueUI;
    [SerializeField] private ThoughtSpawner thoughtSpawner;
    [SerializeField] private DeathRespawn deathRespawn;

    [Header("Camera")]
    [SerializeField] private CinemachineCamera conversationCamera;

    [Header("Objects to disable in test mode")]
    [SerializeField] private GameObject playerMovementObject;
    [SerializeField] private GameObject talkApproachTrigger;

    void Start()
    {
        // Show dialogue UI immediately (normally triggered by walking up)
        if (dialogueUI != null)
            dialogueUI.SetActive(true);

        // Tell ConfidenceState we're in test mode (skips draft UI on death)
        deathRespawn.testMode = true;
        deathRespawn.debugMenu = debugMenu;

        // Switch to conversation camera immediately
        if (conversationCamera != null)
            CameraManager.SwitchCamera(conversationCamera);

        // Conversation state
        confidenceState.inConversation = true;
    }
}
