namespace InputVisualizer.Core.App;

public sealed class AppConfig
{
    public OverlayConfig Overlay { get; set; } = new();

    public SafetyConfig Safety { get; set; } = new();

    public InputConfig Input { get; set; } = new();

    public List<string> AllowedProcesses { get; set; } = [];

    public AllowedKeysConfig AllowedKeys { get; set; } = new();

    public static AppConfig SafeDefaults()
    {
        return new AppConfig
        {
            Overlay = new OverlayConfig(),
            Safety = new SafetyConfig(),
            Input = new InputConfig(),
            AllowedProcesses = ["example-game.exe"],
            AllowedKeys = AllowedKeysConfig.SafeDefaults()
        };
    }

    public AppConfig Clone()
    {
        return new AppConfig
        {
            Overlay = Overlay.Clone(),
            Safety = Safety.Clone(),
            Input = Input.Clone(),
            AllowedProcesses = [.. AllowedProcesses],
            AllowedKeys = AllowedKeys.Clone()
        };
    }
}

public sealed class OverlayConfig
{
    public bool EnabledByDefault { get; set; }

    public bool ManualResumeRequired { get; set; } = true;

    public bool ShowStatusLabel { get; set; } = true;

    public string PanicHotkey { get; set; } = "Ctrl+Alt+F12";

    public OverlayConfig Clone()
    {
        return new OverlayConfig
        {
            EnabledByDefault = EnabledByDefault,
            ManualResumeRequired = ManualResumeRequired,
            ShowStatusLabel = ShowStatusLabel,
            PanicHotkey = PanicHotkey
        };
    }
}

public sealed class SafetyConfig
{
    public bool WhitelistOnly { get; set; } = true;

    public bool HideOnFocusLost { get; set; } = true;

    public bool HideOnUnknownWindow { get; set; } = true;

    public bool HideOnTextInputMode { get; set; } = true;

    public bool HideOnPasswordFieldDetected { get; set; } = true;

    public bool HideOnClipboardPaste { get; set; } = true;

    public bool DisableAutoResumeAfterRisk { get; set; } = true;

    public SafetyConfig Clone()
    {
        return new SafetyConfig
        {
            WhitelistOnly = WhitelistOnly,
            HideOnFocusLost = HideOnFocusLost,
            HideOnUnknownWindow = HideOnUnknownWindow,
            HideOnTextInputMode = HideOnTextInputMode,
            HideOnPasswordFieldDetected = HideOnPasswordFieldDetected,
            HideOnClipboardPaste = HideOnClipboardPaste,
            DisableAutoResumeAfterRisk = DisableAutoResumeAfterRisk
        };
    }
}

public sealed class InputConfig
{
    public bool StoreHistory { get; set; }

    public bool WriteKeyLogs { get; set; }

    public bool AllowTextKeys { get; set; }

    public bool AllowGamepad { get; set; } = true;

    public bool AllowMouseButtons { get; set; } = true;

    public bool AllowMouseMovement { get; set; }

    public InputConfig Clone()
    {
        return new InputConfig
        {
            StoreHistory = StoreHistory,
            WriteKeyLogs = WriteKeyLogs,
            AllowTextKeys = AllowTextKeys,
            AllowGamepad = AllowGamepad,
            AllowMouseButtons = AllowMouseButtons,
            AllowMouseMovement = AllowMouseMovement
        };
    }
}

public sealed class AllowedKeysConfig
{
    public List<string> Keyboard { get; set; } = [];

    public List<string> Mouse { get; set; } = [];

    public List<string> Gamepad { get; set; } = [];

    public static AllowedKeysConfig SafeDefaults()
    {
        return new AllowedKeysConfig
        {
            Keyboard =
            [
                "W",
                "A",
                "S",
                "D",
                "Space",
                "LeftShift",
                "LeftCtrl",
                "Q",
                "E",
                "R",
                "F"
            ],
            Mouse =
            [
                "Mouse1",
                "Mouse2",
                "Mouse3",
                "WheelUp",
                "WheelDown"
            ],
            Gamepad =
            [
                "A",
                "B",
                "X",
                "Y",
                "LB",
                "RB",
                "LT",
                "RT",
                "LeftStick",
                "RightStick"
            ]
        };
    }

    public AllowedKeysConfig Clone()
    {
        return new AllowedKeysConfig
        {
            Keyboard = [.. Keyboard],
            Mouse = [.. Mouse],
            Gamepad = [.. Gamepad]
        };
    }
}
