namespace InputVisualizer.Core.Safety;

public sealed class NoPasswordFieldGuard : IPasswordFieldGuard
{
    public PasswordFieldState GetPasswordFieldState()
    {
        return PasswordFieldState.NotDetected;
    }
}
