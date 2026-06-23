namespace InputVisualizer.Core.Logging;

public interface ISafeLogSink
{
    void Write(SafeLogLevel level, string message);
}
