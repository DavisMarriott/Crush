using UnityEngine;

// One "reaction" / one beat of reflect lines - a small ordered set played together.
// Used by per-branch death reactions on DialogueCard and by the BaseReflectPools pools.
[System.Serializable]
public class ReflectLineGroup
{
    [TextArea(1, 3)]
    public string[] lines;
}
