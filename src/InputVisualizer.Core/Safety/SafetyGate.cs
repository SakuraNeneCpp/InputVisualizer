using InputVisualizer.Core.App;

namespace InputVisualizer.Core.Safety;

public sealed class SafetyGate
{
    private readonly AppConfig _config;
    private readonly IWindowSafetyProbe _windowSafetyProbe;
    private readonly ITextInputState _textInputState;
    private readonly IPasswordFieldGuard _passwordFieldGuard;
    private readonly PanicMode _panicMode;
    private readonly ProcessWhitelist _processWhitelist;

    public SafetyGate(
        AppConfig config,
        IWindowSafetyProbe windowSafetyProbe,
        ITextInputState textInputState,
        IPasswordFieldGuard passwordFieldGuard,
        PanicMode panicMode)
    {
        _config = config.Clone();
        _windowSafetyProbe = windowSafetyProbe;
        _textInputState = textInputState;
        _passwordFieldGuard = passwordFieldGuard;
        _panicMode = panicMode;
        _processWhitelist = new ProcessWhitelist(_config);
    }

    public SafetyDecision Evaluate()
    {
        if (_panicMode.IsActive)
        {
            return SafetyDecision.Hidden(HiddenReason.PanicMode);
        }

        var foregroundWindow = _windowSafetyProbe.GetForegroundWindow();
        if (_config.Safety.HideOnUnknownWindow &&
            (!foregroundWindow.HasWindow || string.IsNullOrWhiteSpace(foregroundWindow.ProcessName)))
        {
            return SafetyDecision.Hidden(HiddenReason.ForegroundWindowUnavailable);
        }

        if (_config.Safety.HideOnFocusLost && foregroundWindow.IsMinimized)
        {
            return SafetyDecision.Hidden(HiddenReason.FocusLost);
        }

        if (_config.Safety.WhitelistOnly && !_processWhitelist.IsAllowed(foregroundWindow))
        {
            return SafetyDecision.Hidden(HiddenReason.UnsafeApp);
        }

        if (_config.Safety.HideOnTextInputMode && _textInputState.IsTextInputMode)
        {
            return SafetyDecision.Hidden(HiddenReason.TextInputMode);
        }

        if (_config.Safety.HideOnPasswordFieldDetected &&
            _passwordFieldGuard.GetPasswordFieldState() == PasswordFieldState.Detected)
        {
            return SafetyDecision.Hidden(HiddenReason.PasswordFieldDetected);
        }

        return SafetyDecision.Visible;
    }
}
