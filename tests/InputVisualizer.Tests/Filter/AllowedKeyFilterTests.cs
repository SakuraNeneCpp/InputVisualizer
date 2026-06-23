using InputVisualizer.Core.App;
using InputVisualizer.Core.Filter;
using InputVisualizer.Core.Input;

namespace InputVisualizer.Tests.Filter;

public sealed class AllowedKeyFilterTests
{
    [Fact]
    public void Filter_ReturnsOnlyAllowedActions()
    {
        var filter = new AllowedKeyFilter(AppConfig.SafeDefaults());
        var snapshot = new InputSnapshot(
        [
            new InputAction(InputActionKind.Keyboard, "W", true),
            new InputAction(InputActionKind.Keyboard, "G", true),
            new InputAction(InputActionKind.Keyboard, "Backspace", true),
            new InputAction(InputActionKind.MouseButton, "Mouse1", true),
            new InputAction(InputActionKind.MouseButton, "Mouse4", true),
            new InputAction(InputActionKind.GamepadButton, "A", true)
        ]);

        var visible = filter.Filter(snapshot);

        Assert.Equal(["W", "Mouse1", "A"], visible.Select(action => action.Id));
    }

    [Fact]
    public void Filter_IgnoresReleasedActions()
    {
        var filter = new AllowedKeyFilter(AppConfig.SafeDefaults());
        var snapshot = new InputSnapshot(
        [
            new InputAction(InputActionKind.Keyboard, "W", false),
            new InputAction(InputActionKind.MouseButton, "Mouse1", false)
        ]);

        Assert.Empty(filter.Filter(snapshot));
    }
}
