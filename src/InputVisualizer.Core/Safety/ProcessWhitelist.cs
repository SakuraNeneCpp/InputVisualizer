using InputVisualizer.Core.App;

namespace InputVisualizer.Core.Safety;

public sealed class ProcessWhitelist
{
    private readonly HashSet<string> _allowedProcessNames;

    public ProcessWhitelist(IEnumerable<string> allowedProcessNames)
    {
        ArgumentNullException.ThrowIfNull(allowedProcessNames);
        _allowedProcessNames = new HashSet<string>(
            allowedProcessNames.Where(name => !string.IsNullOrWhiteSpace(name)),
            StringComparer.OrdinalIgnoreCase);
    }

    public ProcessWhitelist(AppConfig config)
        : this(config.AllowedProcesses)
    {
    }

    public bool IsAllowed(ForegroundWindowInfo foregroundWindow)
    {
        if (!foregroundWindow.HasWindow || string.IsNullOrWhiteSpace(foregroundWindow.ProcessName))
        {
            return false;
        }

        return _allowedProcessNames.Contains(foregroundWindow.ProcessName);
    }
}
