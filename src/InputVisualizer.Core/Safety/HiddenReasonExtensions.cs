namespace InputVisualizer.Core.Safety;

public static class HiddenReasonExtensions
{
    public static string ToStatusText(this HiddenReason reason)
    {
        return reason switch
        {
            HiddenReason.None => "Input visible",
            HiddenReason.OverlayDisabled => "Input hidden: overlay disabled",
            HiddenReason.ManualResumeRequired => "Input hidden: manual resume required",
            HiddenReason.PanicMode => "Input hidden: panic mode",
            HiddenReason.ForegroundWindowUnavailable => "Input hidden: unknown window",
            HiddenReason.FocusLost => "Input hidden: focus lost",
            HiddenReason.UnsafeApp => "Input hidden: unsafe app",
            HiddenReason.TextInputMode => "Input hidden: text input mode",
            HiddenReason.PasswordFieldDetected => "Input hidden: password field",
            HiddenReason.ClipboardPaste => "Input hidden: clipboard paste",
            _ => "Input hidden: unknown risk"
        };
    }
}
