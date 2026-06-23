namespace InputVisualizer.Core.Safety;

public enum HiddenReason
{
    None,
    OverlayDisabled,
    ManualResumeRequired,
    PanicMode,
    ForegroundWindowUnavailable,
    FocusLost,
    UnsafeApp,
    TextInputMode,
    PasswordFieldDetected,
    ClipboardPaste,
    Unknown
}
