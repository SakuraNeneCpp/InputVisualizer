namespace InputVisualizer.Core.Safety;

public sealed class PanicMode
{
    public bool IsActive { get; private set; }

    public void Activate()
    {
        IsActive = true;
    }

    public void Reset()
    {
        IsActive = false;
    }
}
