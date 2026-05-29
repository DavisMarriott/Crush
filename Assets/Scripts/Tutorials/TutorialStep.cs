using UnityEngine;

[CreateAssetMenu(menuName = "Crush/Tutorial Step")]
public class TutorialStep : ScriptableObject
{
    public enum DismissMode { ContinueButton, External }

    public string id;              // fires once per id
    [TextArea(2, 5)] public string text;

    [Header("Box layout (per step)")]
    public Vector2 boxSize = new Vector2(600, 300);
    public Vector2 boxAnchoredPosition;  // 0,0 = center; negative Y moves down

    [Header("Behavior")]
    [Tooltip("ContinueButton: shows a Continue button after the delay. External: no button, dismissed by an outside signal (e.g. a TutorialTrigger with dismissOnDisable set).")]
    public DismissMode dismissMode = DismissMode.ContinueButton;
    [Tooltip("Off = no dark backdrop, just the floating text box. Useful when the player needs to see/interact with elements behind the tutorial.")]
    public bool showBackdrop = true;
}
