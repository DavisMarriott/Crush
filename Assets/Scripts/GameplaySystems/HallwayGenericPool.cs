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
    [Header("Conditional clusters (loop-gated). One cluster = lines for one base loop, played one per trigger (line 0 → trigger 1, …). Picked random + once-per-run.")]
    public ConditionalReflectGroups[] clusterPools;

    [Header("Generic backup clusters — used when no conditional cluster is eligible or all eligible are exhausted this run.")]
    public ReflectLineGroup[] genericClusters;

    [Header("LEGACY flat lines (pre-cluster). No longer written by the importer.")]
    [TextArea(1, 3)]
    public string[] lines;

    /// <summary>Returns a random line, or null if the pool is empty. (Legacy.)</summary>
    public string GetRandomLine()
    {
        if (lines == null || lines.Length == 0) return null;
        return lines[Random.Range(0, lines.Length)];
    }
}
