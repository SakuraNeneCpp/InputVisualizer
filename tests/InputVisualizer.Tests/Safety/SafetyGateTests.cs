using InputVisualizer.Core.App;
using InputVisualizer.Core.Safety;

namespace InputVisualizer.Tests.Safety;

public sealed class SafetyGateTests
{
    [Fact]
    public void Evaluate_AllowsWhitelistedForegroundProcess()
    {
        var fixture = SafetyFixture.Create();

        var decision = fixture.Gate.Evaluate();

        Assert.True(decision.CanDisplay);
        Assert.Equal(HiddenReason.None, decision.Reason);
    }

    [Fact]
    public void Evaluate_HidesUnsafeForegroundProcess()
    {
        var fixture = SafetyFixture.Create();
        fixture.Window.Current = new ForegroundWindowInfo(true, "browser.exe");

        var decision = fixture.Gate.Evaluate();

        Assert.False(decision.CanDisplay);
        Assert.Equal(HiddenReason.UnsafeApp, decision.Reason);
    }

    [Fact]
    public void Evaluate_HidesTextInputMode()
    {
        var fixture = SafetyFixture.Create();
        fixture.Text.IsTextInputMode = true;

        var decision = fixture.Gate.Evaluate();

        Assert.False(decision.CanDisplay);
        Assert.Equal(HiddenReason.TextInputMode, decision.Reason);
    }

    [Fact]
    public void Evaluate_HidesPanicMode()
    {
        var fixture = SafetyFixture.Create();
        fixture.Panic.Activate();

        var decision = fixture.Gate.Evaluate();

        Assert.False(decision.CanDisplay);
        Assert.Equal(HiddenReason.PanicMode, decision.Reason);
    }

    private sealed class SafetyFixture
    {
        private SafetyFixture(
            MutableWindowProbe window,
            MutableTextState text,
            MutablePasswordGuard password,
            PanicMode panic,
            SafetyGate gate)
        {
            Window = window;
            Text = text;
            Password = password;
            Panic = panic;
            Gate = gate;
        }

        public MutableWindowProbe Window { get; }

        public MutableTextState Text { get; }

        public MutablePasswordGuard Password { get; }

        public PanicMode Panic { get; }

        public SafetyGate Gate { get; }

        public static SafetyFixture Create()
        {
            var config = AppConfig.SafeDefaults();
            config.AllowedProcesses = ["game.exe"];

            var window = new MutableWindowProbe
            {
                Current = new ForegroundWindowInfo(true, "game.exe")
            };
            var text = new MutableTextState();
            var password = new MutablePasswordGuard();
            var panic = new PanicMode();
            var gate = new SafetyGate(config, window, text, password, panic);

            return new SafetyFixture(window, text, password, panic, gate);
        }
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
        public bool IsTextInputMode { get; set; }
    }

    private sealed class MutablePasswordGuard : IPasswordFieldGuard
    {
        public PasswordFieldState State { get; set; } = PasswordFieldState.NotDetected;

        public PasswordFieldState GetPasswordFieldState()
        {
            return State;
        }
    }
}
