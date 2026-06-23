using InputVisualizer.Core.Input;
using InputVisualizer.Core.Safety;

namespace InputVisualizer.Core.Overlay;

public sealed class OverlayFrame
{
    private OverlayFrame(bool isVisible, HiddenReason hiddenReason, IReadOnlyList<InputAction> visibleActions, string statusText)
    {
        IsVisible = isVisible;
        HiddenReason = hiddenReason;
        VisibleActions = visibleActions;
        StatusText = statusText;
    }

    public bool IsVisible { get; }

    public HiddenReason HiddenReason { get; }

    public IReadOnlyList<InputAction> VisibleActions { get; }

    public string StatusText { get; }

    public static OverlayFrame Visible(IReadOnlyList<InputAction> visibleActions, bool showStatusLabel)
    {
        ArgumentNullException.ThrowIfNull(visibleActions);
        return new OverlayFrame(true, HiddenReason.None, visibleActions, showStatusLabel ? HiddenReason.None.ToStatusText() : string.Empty);
    }

    public static OverlayFrame Hidden(HiddenReason reason)
    {
        return new OverlayFrame(false, reason, [], reason.ToStatusText());
    }
}
