using InputVisualizer.Core.Input;

namespace InputVisualizer.Core.Safety;

public sealed class TextInputGuard : ITextInputState
{
    private static readonly IReadOnlySet<string> TextModeStartActions =
        new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "Enter",
            "T",
            "Slash"
        };

    private static readonly IReadOnlySet<string> TextModeEndActions =
        new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "Escape",
            "Enter"
        };

    public bool IsTextInputMode { get; private set; }

    public void EnterTextInputMode()
    {
        IsTextInputMode = true;
    }

    public void LeaveTextInputMode()
    {
        IsTextInputMode = false;
    }

    public void Observe(InputAction action)
    {
        ArgumentNullException.ThrowIfNull(action);

        if (action.Kind != InputActionKind.Keyboard || !action.IsPressed)
        {
            return;
        }

        if (IsTextInputMode)
        {
            if (TextModeEndActions.Contains(action.Id))
            {
                LeaveTextInputMode();
            }

            return;
        }

        if (TextModeStartActions.Contains(action.Id))
        {
            EnterTextInputMode();
        }
    }
}
