using UnityEngine;

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

    [Header("Objects to disable in test mode")]
    [SerializeField] private GameObject playerMovementObject;
    [SerializeField] private GameObject talkApproachTrigger;

    private bool wasDead = false;

    void Start()
    {
        // Disable the walk-up — go straight to conversation
        if (playerMovementObject != null)
            playerMovementObject.SetActive(false);
        if (talkApproachTrigger != null)
            talkApproachTrigger.SetActive(false);

        // Show dialogue UI immediately (normally triggered by walking up)
        if (dialogueUI != null)
            dialogueUI.SetActive(true);

        // Tell ConfidenceState we're in test mode (skips draft UI on death)
        confidenceState.testMode = true;

        // Conversation state
        confidenceState.inConversation = true;
    }

    void Update()
    {
        // Watch for death — redirect to debug menu instead of draft UI
        if (confidenceState.Dead && !wasDead)
        {
            wasDead = true;
            // The debug menu will open after the death screen plays
            // We use Invoke to give the death screen time to show
            Invoke(nameof(OpenDebugOnDeath), 2.5f);
        }

        // Reset death tracker when player is alive again
        if (!confidenceState.Dead && wasDead)
        {
            wasDead = false;
        }
    }

    private void OpenDebugOnDeath()
    {
        debugMenu.OpenMenu();
    }
}
