namespace InputVisualizer.Core.Input;

public sealed class InputSnapshot
{
    public static InputSnapshot Empty { get; } = new([]);

    public InputSnapshot(IEnumerable<InputAction> actions)
    {
        ArgumentNullException.ThrowIfNull(actions);
        Actions = actions.ToArray();
    }

    public IReadOnlyList<InputAction> Actions { get; }
}
