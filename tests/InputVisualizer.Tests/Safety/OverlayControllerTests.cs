using InputVisualizer.Core.App;
using InputVisualizer.Core.Filter;
using InputVisualizer.Core.Input;
using InputVisualizer.Core.Overlay;
using InputVisualizer.Core.Safety;

namespace InputVisualizer.Tests.Safety;

public sealed class OverlayControllerTests
{
    [Fact]
    public void Render_DoesNotAutoResumeAfterRisk()
    {
        var config = AppConfig.SafeDefaults();
        config.AllowedProcesses = ["game.exe"];

        var window = new MutableWindowProbe
        {
            Current = new ForegroundWindowInfo(true, "game.exe")
        };
        var gate = new SafetyGate(config, window, new MutableTextState(), new NoPasswordFieldGuard(), new PanicMode());
        var controller = new OverlayController(config, gate, new AllowedKeyFilter(config));
        controller.Enable();

        var visible = controller.Render(new InputSnapshot([new InputAction(InputActionKind.Keyboard, "W", true)]));
        Assert.True(visible.IsVisible);

        window.Current = new ForegroundWindowInfo(true, "browser.exe");
        var unsafeFrame = controller.Render(InputSnapshot.Empty);
        Assert.False(unsafeFrame.IsVisible);
        Assert.Equal(HiddenReason.UnsafeApp, unsafeFrame.HiddenReason);

        window.Current = new ForegroundWindowInfo(true, "game.exe");
        var stillHidden = controller.Render(new InputSnapshot([new InputAction(InputActionKind.Keyboard, "W", true)]));
        Assert.False(stillHidden.IsVisible);
        Assert.Equal(HiddenReason.ManualResumeRequired, stillHidden.HiddenReason);

        Assert.True(controller.TryResume());
        var resumed = controller.Render(new InputSnapshot([new InputAction(InputActionKind.Keyboard, "W", true)]));
        Assert.True(resumed.IsVisible);
    }

    private sealed class MutableWindowProbe : IWindowSafetyProbe
    {
        public ForegroundWindowInfo Current { get; set; } = new(false, null);

        public ForegroundWindowInfo GetForegroundWindow()
        {
            return Current;
        }
    }

    private sealed class MutableTextState : ITextInputState
    {
        public bool IsTextInputMode => false;
    }
}
