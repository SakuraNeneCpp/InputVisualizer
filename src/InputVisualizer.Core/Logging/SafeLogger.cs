using InputVisualizer.Core.Safety;

namespace InputVisualizer.Core.Logging;

public sealed class SafeLogger
{
    private readonly ISafeLogSink _sink;

    public SafeLogger(ISafeLogSink sink)
    {
        _sink = sink;
    }

    public void OverlayEnabled()
    {
        _sink.Write(SafeLogLevel.Info, "Overlay enabled");
    }

    public void OverlayPaused()
    {
        _sink.Write(SafeLogLevel.Info, "Overlay paused");
    }

    public void OverlayHidden(HiddenReason reason)
    {
        _sink.Write(SafeLogLevel.Warning, reason.ToStatusText());
    }

    public void ForegroundProcessChanged(string? processName)
    {
        if (string.IsNullOrWhiteSpace(processName))
        {
            _sink.Write(SafeLogLevel.Warning, "Foreground process unavailable");
            return;
        }

        _sink.Write(SafeLogLevel.Info, $"Foreground process: {SanitizeProcessName(processName)}");
    }

    private static string SanitizeProcessName(string processName)
    {
        var fileName = Path.GetFileName(processName);
        return fileName.ReplaceLineEndings(string.Empty);
    }
}
