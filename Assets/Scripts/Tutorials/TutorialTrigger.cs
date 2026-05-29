using UnityEngine;

// drop on an object, give it a step + pick when it fires
public class TutorialTrigger : MonoBehaviour
{
    public enum FireMode { OnEnable, Manual }

    [SerializeField] private TutorialManager tutorialManager;
    [SerializeField] private TutorialStep step;
    [SerializeField] private FireMode fireOn = FireMode.OnEnable;
    [SerializeField] private RectTransform highlightTarget;  // optional - box snaps here
    [Tooltip("For action-mode (External) tutorials: when this GameObject deactivates (e.g. player picked the card), dismiss any tutorial currently up.")]
    [SerializeField] private bool dismissOnDisable;

    private void OnEnable()
    {
        if (fireOn == FireMode.OnEnable) Fire();
    }

    private void OnDisable()
    {
        if (dismissOnDisable && tutorialManager != null) tutorialManager.DismissCurrent();
    }

    // hook to a UnityEvent or call from code for Manual moments
    public void Fire()
    {
        if (tutorialManager != null && step != null)
            tutorialManager.Show(step, highlightTarget);
    }
}
