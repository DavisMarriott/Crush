using UnityEngine;

// NOTE: name is legacy from when hallway triggers were first-loop-only. It's now the
// holder for all per-loop hallway trigger data + the generic fallback pool.
public class FirstLoopManager : MonoBehaviour
{
    [SerializeField] public string[] firstLoopHallwayLines; // LEGACY — superseded by loopHallways. Safe to remove once verified.

    [Header("Per-loop hallway trigger lines (populated by the Hallway importer)")]
    [SerializeField] public LoopHallway[] loopHallways;

    [Header("Generic fallback pool — random lines fired at triggers during base loops (no scripted hallway)")]
    [SerializeField] public HallwayGenericPool genericPool;

    /// <summary>Returns the LoopHallway whose loop matches, or null if none (= base loop).</summary>
    public LoopHallway GetForLoop(int loop)
    {
        if (loopHallways == null) return null;
        foreach (var lh in loopHallways)
            if (lh != null && lh.loop == loop) return lh;
        return null;
    }
}
