using System.Windows.Automation;
using InputVisualizer.Core.Safety;

namespace InputVisualizer.Windows;

public sealed class UiAutomationPasswordFieldGuard : IPasswordFieldGuard
{
    public PasswordFieldState GetPasswordFieldState()
    {
        try
        {
            var focusedElement = AutomationElement.FocusedElement;
            if (focusedElement is null)
            {
                return PasswordFieldState.Unknown;
            }

            return focusedElement.Current.IsPassword
                ? PasswordFieldState.Detected
                : PasswordFieldState.NotDetected;
        }
        catch (ElementNotAvailableException)
        {
            return PasswordFieldState.Unknown;
        }
        catch (InvalidOperationException)
        {
            return PasswordFieldState.Unknown;
        }
    }
}
