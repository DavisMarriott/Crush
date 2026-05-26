using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

// Debug-only fast-forward for testing. Z toggles 3x, X toggles 5x (pressing the same key again
// returns to 1x; the keys cross over — at 3x, X jumps to 5x, etc.).
//
// It just drives Time.timeScale, which the whole game scales off (nothing in the project uses
// unscaled/real-time timing), so dialogue, coroutine waits, the Invoke-delayed transitions,
// animations and physics all speed up together — nothing desyncs.
//
// Time.timeScale itself is the source of truth (no cached speed field), so this never drifts out
// of sync with the DebugMenu, which also drives timeScale for pausing. While paused (timeScale 0)
// the keys are ignored so we can't accidentally un-pause the debug menu.
//
// This is a debug component — attach it to an always-on object (e.g. Systems). Don't ship it on a
// live GameObject in a release build (or ask me to add a UNITY_EDITOR/DEVELOPMENT_BUILD guard).
public class FastForward : MonoBehaviour
{
    [SerializeField] private float speed3 = 3f;
    [SerializeField] private float speed5 = 5f;

    [Tooltip("Optional HUD label that shows the current fast-forward speed. Hidden at normal speed.")]
    [SerializeField] private TMP_Text speedIndicator;

    private float _lastShownScale = -1f;

    void Update()
    {
        Keyboard kb = Keyboard.current;

        // Ignore the keys while paused (e.g. debug menu open) so we don't un-pause the game.
        if (kb != null && Time.timeScale > 0f)
        {
            if (kb.zKey.wasPressedThisFrame)
                Time.timeScale = Mathf.Approximately(Time.timeScale, speed3) ? 1f : speed3;

            if (kb.xKey.wasPressedThisFrame)
                Time.timeScale = Mathf.Approximately(Time.timeScale, speed5) ? 1f : speed5;
        }

        UpdateIndicator();
    }

    private void UpdateIndicator()
    {
        if (speedIndicator == null) return;

        // Only refresh when the scale actually changes.
        if (Mathf.Approximately(Time.timeScale, _lastShownScale)) return;
        _lastShownScale = Time.timeScale;

        bool fast = Mathf.Approximately(Time.timeScale, speed3) || Mathf.Approximately(Time.timeScale, speed5);

        if (speedIndicator.gameObject.activeSelf != fast)
            speedIndicator.gameObject.SetActive(fast);

        if (fast)
            speedIndicator.text = $"{Mathf.RoundToInt(Time.timeScale)}x";
    }

    void OnDisable()
    {
        // Don't leak fast-forward if this component is disabled mid-FF (leave a paused 0 alone).
        if (Time.timeScale > 1f)
            Time.timeScale = 1f;
    }
}
