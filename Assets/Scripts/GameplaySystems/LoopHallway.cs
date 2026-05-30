using UnityEngine;

/// <summary>
/// Per-loop scripted hallway content. The triggerLines fire at the hallway trigger
/// zones in order (index 0 → trigger 1, index 1 → trigger 2, …). Populated by the
/// Hallway importer from the "Loop N" tabs of the Hallway Phase doc.
/// Mirrors the LoopReflect_0N pattern.
/// </summary>
[CreateAssetMenu(menuName = "Crush/Loop Hallway")]
public class LoopHallway : ScriptableObject
{
    [Header("Loop this set belongs to")]
    public int loop;

    [Header("Lines that fire at the hallway trigger zones (index 0 = trigger 1, etc.)")]
    [TextArea(1, 3)]
    public string[] triggerLines;

    [Header("Per-loop overrides")]
    [Tooltip("When true, DialogueBox skips Luke + Daisy intro lines for this loop (e.g. loop 1's nervous freeze).")]
    public bool skipIntros;
}
