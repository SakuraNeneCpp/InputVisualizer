using InputVisualizer.Core.Input;

namespace InputVisualizer.Core.Filter;

public static class ActionNormalizer
{
    private static readonly IReadOnlyDictionary<int, string> KeyboardMap = new Dictionary<int, string>
    {
        [0x08] = "Backspace",
        [0x09] = "Tab",
        [0x0D] = "Enter",
        [0x10] = "LeftShift",
        [0x11] = "LeftCtrl",
        [0x12] = "Alt",
        [0x1B] = "Escape",
        [0x20] = "Space",
        [0xBF] = "Slash",
        [0xA0] = "LeftShift",
        [0xA1] = "RightShift",
        [0xA2] = "LeftCtrl",
        [0xA3] = "RightCtrl",
        [0xA4] = "LeftAlt",
        [0xA5] = "RightAlt",
        [0x7B] = "F12"
    };

    private static readonly IReadOnlySet<string> AlwaysSuppressedKeyboardActions =
        new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "Backspace",
            "Enter",
            "Tab"
        };

    public static string? NormalizeKeyboardVirtualKey(int virtualKey)
    {
        if (KeyboardMap.TryGetValue(virtualKey, out var known))
        {
            return known;
        }

        if (virtualKey is >= 0x41 and <= 0x5A)
        {
            return ((char)virtualKey).ToString();
        }

        return null;
    }

    public static InputAction MouseButton(string id, bool isPressed)
    {
        return new InputAction(InputActionKind.MouseButton, id, isPressed);
    }

    public static InputAction MouseWheel(int delta)
    {
        return new InputAction(InputActionKind.MouseWheel, delta > 0 ? "WheelUp" : "WheelDown", true, delta);
    }

    public static bool IsAlwaysSuppressedKeyboardAction(string id)
    {
        return AlwaysSuppressedKeyboardActions.Contains(id);
    }
}
