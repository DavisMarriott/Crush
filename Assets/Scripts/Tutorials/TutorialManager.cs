using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// pauses the game and shows a tutorial overlay until the player hits continue. each step fires once.
public class TutorialManager : MonoBehaviour
{
    [SerializeField] private GameObject overlayRoot;     // whole overlay, off by default
    [SerializeField] private GameObject backdrop;        // dark Image - toggled per step
    [SerializeField] private TMP_Text tutorialText;
    [SerializeField] private Button continueButton;
    [SerializeField] private RectTransform tutorialBox;  // text box - snaps to the highlight target if one's given
    [SerializeField] private float continueDelay = 2f;
    [SerializeField] private PauseManager pauseManager;  // optional - switched off so Esc can't pause over us

    private readonly HashSet<string> _shown = new HashSet<string>();
    private bool _active;
    private CanvasGroup _continueGroup;  //grabbed in Awake if present - lets us hide button without reflowing layout

    private void Awake()
    {
        if (overlayRoot != null) overlayRoot.SetActive(false);
        if (continueButton != null)
        {
            _continueGroup = continueButton.GetComponent<CanvasGroup>();
            continueButton.onClick.AddListener(Dismiss);
        }
    }

    private void SetContinueVisible(bool on)
    {
        if (_continueGroup != null)
        {
            _continueGroup.alpha = on ? 1f : 0f;
            _continueGroup.interactable = on;
            _continueGroup.blocksRaycasts = on;
        }
        else if (continueButton != null)
        {
            continueButton.gameObject.SetActive(on);
        }
    }

    // fire a tutorial. ignored if this step already showed or one's already up.
    public void Show(TutorialStep step, RectTransform highlight = null)
    {
        if (step == null || _active || _shown.Contains(step.id)) return;
        _shown.Add(step.id);
        StartCoroutine(Run(step, highlight));
    }

    private IEnumerator Run(TutorialStep step, RectTransform highlight)
    {
        _active = true;

        if (tutorialText != null) tutorialText.text = step.text;

        //apply per-step layout to the box
        if (tutorialBox != null)
        {
            tutorialBox.sizeDelta = step.boxSize;
            tutorialBox.anchoredPosition = step.boxAnchoredPosition;
        }

        //per-step backdrop on/off
        if (backdrop != null) backdrop.SetActive(step.showBackdrop);

        SetContinueVisible(false);
        if (overlayRoot != null) overlayRoot.SetActive(true);

        //external-mode tutorials never show Continue - someone else calls DismissCurrent()
        if (step.dismissMode == TutorialStep.DismissMode.External) yield break;

        //small hold before continue shows so it's not instantly clickable
        yield return new WaitForSecondsRealtime(continueDelay);

        SetContinueVisible(true);
    }

    //public hook for external-mode tutorials (e.g. TutorialTrigger.OnDisable on the moment object)
    public void DismissCurrent()
    {
        Dismiss();
    }

    private void Dismiss()
    {
        if (!_active) return;
        if (overlayRoot != null) overlayRoot.SetActive(false);
        _active = false;
    }
}
