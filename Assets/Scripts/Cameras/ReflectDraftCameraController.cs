using UnityEngine;
using Unity.Cinemachine;

// Drives Cinemachine camera switching for the Reflect and UpgradeDraft phases.
// Subscribes to PhaseManager.OnPhaseChanged and routes through CameraManager.SwitchCamera.
//
// Phase → camera mapping handled here:
//   Reflect       → DraftScreenCam
//   UpgradeDraft  → DraftScreenCam
//   Hallway       → HallCam
//
// Conversation transitions stay handled by the existing Cinemachine trigger zones
// (CameraBlendTrigger / InConversationTrigger → ConvoCam). Death phase is a no-op —
// the death screen overlays the currently-active camera.
public class ReflectDraftCameraController : MonoBehaviour
{
    [SerializeField] private CinemachineCamera draftScreenCam;
    [SerializeField] private CinemachineCamera hallCam;

    private void Awake()
    {
        //solo the draft/locker cam from frame 1 so HallCam doesn't render briefly
        //before the start-screen UI covers it (the menu lives in the locker shot)
        if (draftScreenCam != null)
            CinemachineCore.SoloCamera = draftScreenCam;
    }

    // Subscribe in Start (not OnEnable) — PhaseManager.Instance may not be assigned yet
    // at OnEnable time depending on script execution order across the scene's GameObjects.
    // Start runs after all Awake calls have completed, so Instance is guaranteed to be set.
    private void Start()
    {
        Debug.Log($"[ReflectDraftCameraController] Start — PhaseManager.Instance = {(PhaseManager.Instance == null ? "NULL" : "OK")}");
        if (PhaseManager.Instance != null)
            PhaseManager.Instance.OnPhaseChanged += HandlePhaseChanged;
    }

    private void OnDestroy()
    {
        if (PhaseManager.Instance != null)
            PhaseManager.Instance.OnPhaseChanged -= HandlePhaseChanged;
    }

    private void HandlePhaseChanged(GamePhase oldPhase, GamePhase newPhase)
    {
        if (newPhase == GamePhase.Reflect || newPhase == GamePhase.UpgradeDraft)
        {
            if (draftScreenCam != null && CinemachineCore.SoloCamera != (ICinemachineCamera)draftScreenCam)
            {
                Debug.Log($"[ReflectDraftCameraController] {oldPhase} -> {newPhase}: SOLO {draftScreenCam.name}");
                // Solo forces this camera live regardless of priority — the trigger system
                // can't override us with a HallCam priority bump during the respawn teleport.
                CinemachineCore.SoloCamera = draftScreenCam;
                CameraManager.SwitchCamera(draftScreenCam);
            }
        }
        else if (newPhase == GamePhase.Hallway)
        {
            Debug.Log($"[ReflectDraftCameraController] {oldPhase} -> {newPhase}: clear SOLO, switch to {hallCam.name}");
            // Release Solo so normal priority-based switching resumes (lets ConvoCam triggers
            // work normally on subsequent conversations).
            CinemachineCore.SoloCamera = null;
            if (hallCam != null)
                CameraManager.SwitchCamera(hallCam);
        }
        // Conversation / Death: intentionally no-op.
    }
}
