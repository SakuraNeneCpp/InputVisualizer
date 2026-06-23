namespace InputVisualizer.Core.Input;

public interface IInputProvider
{
    InputSnapshot ReadCurrent();
}
