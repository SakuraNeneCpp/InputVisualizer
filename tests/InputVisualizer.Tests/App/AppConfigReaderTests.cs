using InputVisualizer.Core.App;

namespace InputVisualizer.Tests.App;

public sealed class AppConfigReaderTests
{
    [Fact]
    public void Parse_UsesYamlValues()
    {
        const string yaml = """
            overlay:
              enabled_by_default: true
              manual_resume_required: true
              show_status_label: false
              panic_hotkey: "Ctrl+Alt+F12"
            safety:
              whitelist_only: true
              hide_on_focus_lost: true
              hide_on_unknown_window: true
              hide_on_text_input_mode: true
              hide_on_password_field_detected: true
              hide_on_clipboard_paste: true
              disable_auto_resume_after_risk: true
            input:
              store_history: false
              write_key_logs: false
              allow_text_keys: false
              allow_gamepad: true
              allow_mouse_buttons: true
              allow_mouse_movement: false
            allowed_processes:
              - "game.exe"
            allowed_keys:
              keyboard:
                - "W"
              mouse:
                - "Mouse1"
              gamepad:
                - "A"
            """;

        var config = AppConfigReader.Parse(yaml);

        Assert.True(config.Overlay.EnabledByDefault);
        Assert.False(config.Overlay.ShowStatusLabel);
        Assert.Equal(["game.exe"], config.AllowedProcesses);
        Assert.Equal(["W"], config.AllowedKeys.Keyboard);
        Assert.Equal(["Mouse1"], config.AllowedKeys.Mouse);
        Assert.Equal(["A"], config.AllowedKeys.Gamepad);
    }

    [Fact]
    public void Parse_RejectsUnknownSetting()
    {
        const string yaml = """
            overlay:
              unsafe_default: true
            """;

        Assert.Throws<ConfigurationException>(() => AppConfigReader.Parse(yaml));
    }
}
