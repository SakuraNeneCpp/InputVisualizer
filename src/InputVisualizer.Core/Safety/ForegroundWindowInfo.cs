namespace InputVisualizer.Core.Safety;

public sealed record ForegroundWindowInfo(
    bool HasWindow,
    string? ProcessName,
    bool IsMinimized = false);
