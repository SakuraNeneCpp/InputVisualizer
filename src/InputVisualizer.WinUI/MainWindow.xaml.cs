using InputVisualizer.Core.App;
using InputVisualizer.Core.Filter;
using InputVisualizer.Core.Input;
using InputVisualizer.Core.Overlay;
using InputVisualizer.Core.Safety;
using InputVisualizer.Windows;
using InputVisualizer.Windows.Interop;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using WinRT.Interop;

namespace InputVisualizer.WinUI;

public sealed partial class MainWindow : Window
{
    private readonly AppConfig _config;
    private readonly TextInputGuard _textInputGuard = new();
    private readonly PanicMode _panicMode = new();
    private readonly ForegroundWindowGuard _foregroundWindowGuard = new();
    private readonly UiAutomationPasswordFieldGuard _passwordFieldGuard = new();
    private readonly WindowsRawInputProvider _rawInputProvider = new();
    private readonly XInputGamepadProvider _gamepadProvider = new();
    private readonly Dictionary<string, InputAction> _pressedActions = new(StringComparer.OrdinalIgnoreCase);
    private readonly List<InputAction> _transientActions = [];
    private readonly DispatcherTimer _timer = new();
    private readonly OverlayController _overlayController;
    private HwndMessageHook? _messageHook;

    public MainWindow()
    {
        InitializeComponent();

        _config = LoadConfig();
        var safetyGate = new SafetyGate(
            _config,
            _foregroundWindowGuard,
            _textInputGuard,
            _passwordFieldGuard,
            _panicMode);

        _overlayController = new OverlayController(_config, safetyGate, new AllowedKeyFilter(_config));

        AttachRawInput();

        _timer.Interval = TimeSpan.FromMilliseconds(100);
        _timer.Tick += OnTick;
        _timer.Start();

        Closed += OnClosed;
        RenderFrame(OverlayFrame.Hidden(HiddenReason.OverlayDisabled));
    }

    private static AppConfig LoadConfig()
    {
        var configPath = Path.Combine(AppContext.BaseDirectory, "config", "default.yaml");
        return AppConfigReader.Load(configPath);
    }

    private void AttachRawInput()
    {
        try
        {
            var hwnd = WindowNative.GetWindowHandle(this);
            _rawInputProvider.RegisterKeyboardAndMouse(hwnd);
            _messageHook = HwndMessageHook.Attach(hwnd);
            _messageHook.MessageReceived += OnWindowMessage;
        }
        catch (Exception)
        {
            StatusTextBlock.Text = "Input hidden: raw input unavailable";
        }
    }

    private void OnWindowMessage(object? sender, WindowMessageEventArgs args)
    {
        if (args.Message != WindowsRawInputProvider.WmInput)
        {
            return;
        }

        foreach (var action in _rawInputProvider.ReadActionsFromMessage(args.LParam))
        {
            ObserveRawAction(action);
        }
    }

    private void ObserveRawAction(InputAction action)
    {
        _textInputGuard.Observe(action);

        if (action.Kind == InputActionKind.MouseWheel)
        {
            _transientActions.Add(action);
        }
        else
        {
            var key = StateKey(action);
            if (action.IsPressed)
            {
                _pressedActions[key] = action;
            }
            else
            {
                _pressedActions.Remove(key);
            }
        }

        if (IsPanicHotkeyPressed())
        {
            _panicMode.Activate();
            _pressedActions.Clear();
            _transientActions.Clear();
        }
    }

    private bool IsPanicHotkeyPressed()
    {
        var hasCtrl = IsPressed("LeftCtrl") || IsPressed("RightCtrl");
        var hasAlt = IsPressed("Alt") || IsPressed("LeftAlt") || IsPressed("RightAlt");
        var hasF12 = IsPressed("F12");
        return hasCtrl && hasAlt && hasF12;
    }

    private bool IsPressed(string id)
    {
        return _pressedActions.Values.Any(action =>
            action.Kind == InputActionKind.Keyboard &&
            string.Equals(action.Id, id, StringComparison.OrdinalIgnoreCase));
    }

    private void OnTick(object? sender, object e)
    {
        UpdateForegroundStatus();
        ModeTextBlock.Text = _textInputGuard.IsTextInputMode
            ? "Mode: text input hidden"
            : "Mode: action";

        RenderFrame(_overlayController.Render(BuildSnapshot()));
    }

    private InputSnapshot BuildSnapshot()
    {
        var actions = _pressedActions.Values.Where(action => action.IsPressed).ToList();
        actions.AddRange(_transientActions);
        _transientActions.Clear();

        try
        {
            actions.AddRange(_gamepadProvider.ReadCurrent().Actions);
        }
        catch (DllNotFoundException)
        {
        }
        catch (EntryPointNotFoundException)
        {
        }

        return new InputSnapshot(actions);
    }

    private void UpdateForegroundStatus()
    {
        var foreground = _foregroundWindowGuard.GetForegroundWindow();
        ForegroundTextBlock.Text = $"Foreground process: {foreground.ProcessName ?? "unknown"}";
    }

    private void RenderFrame(OverlayFrame frame)
    {
        StatusTextBlock.Text = frame.StatusText;
        ActionPanel.Children.Clear();

        if (!frame.IsVisible)
        {
            return;
        }

        foreach (var action in frame.VisibleActions.OrderBy(action => action.Kind).ThenBy(action => action.Id))
        {
            ActionPanel.Children.Add(CreateActionBadge(action));
        }
    }

    private static Border CreateActionBadge(InputAction action)
    {
        return new Border
        {
            MinWidth = 56,
            Padding = new Thickness(12, 8, 12, 8),
            Background = new SolidColorBrush(ColorHelper.FromArgb(255, 34, 34, 34)),
            BorderBrush = new SolidColorBrush(ColorHelper.FromArgb(255, 96, 96, 96)),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(8),
            Child = new TextBlock
            {
                Text = action.Id,
                Foreground = new SolidColorBrush(Colors.White),
                FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
                HorizontalAlignment = HorizontalAlignment.Center
            }
        };
    }

    private static string StateKey(InputAction action)
    {
        return $"{action.Kind}:{action.Id}";
    }

    private void EnableButton_Click(object sender, RoutedEventArgs e)
    {
        _overlayController.Enable();
    }

    private void PauseButton_Click(object sender, RoutedEventArgs e)
    {
        _overlayController.Pause();
        _pressedActions.Clear();
        _transientActions.Clear();
    }

    private void ResumeButton_Click(object sender, RoutedEventArgs e)
    {
        _ = _overlayController.TryResume();
    }

    private void PanicButton_Click(object sender, RoutedEventArgs e)
    {
        _panicMode.Activate();
        _pressedActions.Clear();
        _transientActions.Clear();
    }

    private void LeaveTextModeButton_Click(object sender, RoutedEventArgs e)
    {
        _textInputGuard.LeaveTextInputMode();
    }

    private void ResetPanicButton_Click(object sender, RoutedEventArgs e)
    {
        _panicMode.Reset();
        _ = _overlayController.TryResume();
    }

    private void OnClosed(object sender, WindowEventArgs args)
    {
        _timer.Stop();
        _messageHook?.Dispose();
    }
}
