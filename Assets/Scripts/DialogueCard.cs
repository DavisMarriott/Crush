using UnityEngine;

[CreateAssetMenu(menuName = "Crush/Dialogue Card")]
public class DialogueCard : ScriptableObject
{
    [Header("What the player sees in the thought bubble")]
    public string previewText;

    [Header("What gets spoken (for now)")]
    public DialogueObject dialogue; // your existing dialogue asset type
}
