using UnityEngine;
using Random = UnityEngine.Random;

/// <summary>
/// Ambient hallway lines played at trigger zones during "base" loops — loops with no
/// scripted LoopHallway. Currently one flat random pool; the design intent (per the Hallway
/// Phase doc) is to grow this into condition/progress-based pools later.
/// Populated by the Hallway importer from the "Base Loop Random Lines" tab.
/// </summary>
[CreateAssetMenu(menuName = "Crush/Hallway Generic Pool")]
public class HallwayGenericPool : ScriptableObject
{
    [Header("Ambient random hallway lines (base loops, no scripted hallway)")]
    [TextArea(1, 3)]
    public string[] lines;

    /// <summary>Returns a random line, or null if the pool is empty.</summary>
    public string GetRandomLine()
    {
        if (lines == null || lines.Length == 0) return null;
        return lines[Random.Range(0, lines.Length)];
    }
}
