using UnityEngine;
using UnityEngine.InputSystem;

// Shared "advance to the next line" check for dialogue. True the frame the player asks to advance:
// the bound next-line action (if one's passed), OR space directly, OR a left-click anywhere.
// Hallway lines intentionally DON'T use this - they auto-advance on a timer.
public static class DialogueAdvance
{
    public static bool Pressed(InputAction nextLineAction = null)
    {
        if (nextLineAction != null && nextLineAction.WasPerformedThisFrame()) return true;

        var kb = Keyboard.current;
        if (kb != null && kb.spaceKey.wasPressedThisFrame) return true;

        var mouse = Mouse.current;
        if (mouse != null && mouse.leftButton.wasPressedThisFrame) return true;

        return false;
    }
}
