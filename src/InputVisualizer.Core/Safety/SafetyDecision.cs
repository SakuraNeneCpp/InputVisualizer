namespace InputVisualizer.Core.Safety;

public readonly record struct SafetyDecision(bool CanDisplay, HiddenReason Reason)
{
    public static SafetyDecision Visible { get; } = new(true, HiddenReason.None);

    public static SafetyDecision Hidden(HiddenReason reason)
    {
        if (reason == HiddenReason.None)
        {
            throw new ArgumentException("Hidden decisions require a non-visible reason.", nameof(reason));
        }

        return new SafetyDecision(false, reason);
    }
}
