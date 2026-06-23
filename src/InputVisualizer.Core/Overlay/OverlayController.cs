using InputVisualizer.Core.App;
using InputVisualizer.Core.Filter;
using InputVisualizer.Core.Input;
using InputVisualizer.Core.Safety;

namespace InputVisualizer.Core.Overlay;

public sealed class OverlayController
{
    private readonly AppConfig _config;
    private readonly SafetyGate _safetyGate;
    private readonly AllowedKeyFilter _allowedKeyFilter;

    public OverlayController(AppConfig config, SafetyGate safetyGate, AllowedKeyFilter allowedKeyFilter)
    {
        _config = config.Clone();
        _safetyGate = safetyGate;
        _allowedKeyFilter = allowedKeyFilter;
        IsEnabled = _config.Overlay.EnabledByDefault;
        RequiresManualResume = _config.Overlay.ManualResumeRequired && IsEnabled;
    }

    public bool IsEnabled { get; private set; }

    public bool RequiresManualResume { get; private set; }

    public void Enable()
    {
        IsEnabled = true;
        RequiresManualResume = false;
    }

    public void Pause()
    {
        IsEnabled = false;
        RequiresManualResume = false;
    }

    public bool TryResume()
    {
        IsEnabled = true;
        var decision = _safetyGate.Evaluate();
        if (!decision.CanDisplay)
        {
            RequiresManualResume = true;
            return false;
        }

        RequiresManualResume = false;
        return true;
    }

    public OverlayFrame Render(InputSnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(snapshot);

        if (!IsEnabled)
        {
            return OverlayFrame.Hidden(HiddenReason.OverlayDisabled);
        }

        if (RequiresManualResume)
        {
            return OverlayFrame.Hidden(HiddenReason.ManualResumeRequired);
        }

        var safetyDecision = _safetyGate.Evaluate();
        if (!safetyDecision.CanDisplay)
        {
            if (_config.Safety.DisableAutoResumeAfterRisk && IsRiskReason(safetyDecision.Reason))
            {
                RequiresManualResume = true;
            }

            return OverlayFrame.Hidden(safetyDecision.Reason);
        }

        return OverlayFrame.Visible(_allowedKeyFilter.Filter(snapshot), _config.Overlay.ShowStatusLabel);
    }

    private static bool IsRiskReason(HiddenReason reason)
    {
        return reason is not HiddenReason.None
            and not HiddenReason.OverlayDisabled
            and not HiddenReason.ManualResumeRequired;
    }
}
