using InputVisualizer.Core.App;
using InputVisualizer.Core.Input;

namespace InputVisualizer.Core.Filter;

public sealed class AllowedKeyFilter
{
    private readonly InputConfig _inputConfig;
    private readonly HashSet<string> _keyboard;
    private readonly HashSet<string> _mouse;
    private readonly HashSet<string> _gamepad;

    public AllowedKeyFilter(AppConfig config)
    {
        ArgumentNullException.ThrowIfNull(config);

        _inputConfig = config.Input.Clone();
        _keyboard = new HashSet<string>(config.AllowedKeys.Keyboard, StringComparer.OrdinalIgnoreCase);
        _mouse = new HashSet<string>(config.AllowedKeys.Mouse, StringComparer.OrdinalIgnoreCase);
        _gamepad = new HashSet<string>(config.AllowedKeys.Gamepad, StringComparer.OrdinalIgnoreCase);
    }

    public IReadOnlyList<InputAction> Filter(InputSnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(snapshot);

        var visible = new List<InputAction>();
        foreach (var action in snapshot.Actions)
        {
            if (!action.IsPressed && action.Kind != InputActionKind.MouseWheel)
            {
                continue;
            }

            if (IsAllowed(action))
            {
                visible.Add(action);
            }
        }

        return visible;
    }

    private bool IsAllowed(InputAction action)
    {
        return action.Kind switch
        {
            InputActionKind.Keyboard => IsAllowedKeyboardAction(action.Id),
            InputActionKind.MouseButton => _inputConfig.AllowMouseButtons && _mouse.Contains(action.Id),
            InputActionKind.MouseWheel => _inputConfig.AllowMouseButtons && _mouse.Contains(action.Id),
            InputActionKind.GamepadButton => _inputConfig.AllowGamepad && _gamepad.Contains(action.Id),
            InputActionKind.GamepadAxis => _inputConfig.AllowGamepad && _gamepad.Contains(action.Id),
            _ => false
        };
    }

    private bool IsAllowedKeyboardAction(string id)
    {
        if (ActionNormalizer.IsAlwaysSuppressedKeyboardAction(id))
        {
            return false;
        }

        return _keyboard.Contains(id);
    }
}
