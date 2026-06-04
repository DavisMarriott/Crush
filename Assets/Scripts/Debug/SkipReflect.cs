using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

// Debug-only: N toggles skip-reflect mode. While active, death respawns jump straight from the
// death screen to the draft - no reflect or commit self-talk plays. Presentation skip only:
// milestone upgrades still apply, draft still shows, deck/confidence resets untouched.
//
// Same setup as FastForward - attach to an always-on object (e.g. Systems), optionally wire a
// TMP label (author it to say "skip") that shows while the mode is on. Debug component - don't
// ship it on a live GameObject in a release build.
public class SkipReflect : MonoBehaviour
{
    public static bool Active { get; private set; }

    [Tooltip("Optional HUD label shown while skip mode is on. Hidden when off.")]
    [SerializeField] private TMP_Text skipIndicator;

    void Update()
    {
        Keyboard kb = Keyboard.current;
        if (kb != null && kb.nKey.wasPressedThisFrame)
            Active = !Active;

        if (skipIndicator != null && skipIndicator.gameObject.activeSelf != Active)
            skipIndicator.gameObject.SetActive(Active);
    }

    void OnDisable()
    {
        // don't leave skip mode stuck on if the component gets disabled mid-run
        Active = false;
    }
}
