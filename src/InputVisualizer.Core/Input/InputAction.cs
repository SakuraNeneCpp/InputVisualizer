namespace InputVisualizer.Core.Input;

public sealed record InputAction
{
    public InputAction(InputActionKind kind, string id, bool isPressed, double value = 1.0)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);

        Kind = kind;
        Id = id;
        IsPressed = isPressed;
        Value = value;
    }

    public InputActionKind Kind { get; }

    public string Id { get; }

    public bool IsPressed { get; }

    public double Value { get; }
}
