namespace InputVisualizer.Core.App;

public static class AppConfigReader
{
    public static AppConfig Load(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        var config = AppConfig.SafeDefaults();
        if (!File.Exists(path))
        {
            return config;
        }

        Apply(File.ReadLines(path), config);
        return config;
    }

    public static AppConfig Parse(string yaml)
    {
        ArgumentNullException.ThrowIfNull(yaml);

        var config = AppConfig.SafeDefaults();
        Apply(ReadLines(yaml), config);
        return config;
    }

    private static void Apply(IEnumerable<string> lines, AppConfig config)
    {
        string? section = null;
        string? nestedList = null;

        foreach (var originalLine in lines)
        {
            var withoutComment = StripComment(originalLine);
            if (string.IsNullOrWhiteSpace(withoutComment))
            {
                continue;
            }

            var indent = CountLeadingSpaces(withoutComment);
            var trimmed = withoutComment.Trim();

            if (indent == 0 && trimmed.EndsWith(':'))
            {
                section = trimmed[..^1].Trim();
                nestedList = null;
                if (section == "allowed_processes")
                {
                    config.AllowedProcesses.Clear();
                }

                continue;
            }

            if (trimmed.StartsWith("- ", StringComparison.Ordinal))
            {
                AddListItem(config, section, nestedList, Unquote(trimmed[2..].Trim()));
                continue;
            }

            if (indent > 0 && trimmed.EndsWith(':'))
            {
                nestedList = trimmed[..^1].Trim();
                ClearNestedList(config, section, nestedList);
                continue;
            }

            var separator = trimmed.IndexOf(':');
            if (separator < 0)
            {
                throw new ConfigurationException($"Invalid YAML line: {trimmed}");
            }

            var key = trimmed[..separator].Trim();
            var value = Unquote(trimmed[(separator + 1)..].Trim());
            SetScalar(config, section, key, value);
        }
    }

    private static void SetScalar(AppConfig config, string? section, string key, string value)
    {
        switch (section, key)
        {
            case ("overlay", "enabled_by_default"):
                config.Overlay.EnabledByDefault = ParseBool(value, key);
                break;
            case ("overlay", "manual_resume_required"):
                config.Overlay.ManualResumeRequired = ParseBool(value, key);
                break;
            case ("overlay", "show_status_label"):
                config.Overlay.ShowStatusLabel = ParseBool(value, key);
                break;
            case ("overlay", "panic_hotkey"):
                config.Overlay.PanicHotkey = value;
                break;

            case ("safety", "whitelist_only"):
                config.Safety.WhitelistOnly = ParseBool(value, key);
                break;
            case ("safety", "hide_on_focus_lost"):
                config.Safety.HideOnFocusLost = ParseBool(value, key);
                break;
            case ("safety", "hide_on_unknown_window"):
                config.Safety.HideOnUnknownWindow = ParseBool(value, key);
                break;
            case ("safety", "hide_on_text_input_mode"):
                config.Safety.HideOnTextInputMode = ParseBool(value, key);
                break;
            case ("safety", "hide_on_password_field_detected"):
                config.Safety.HideOnPasswordFieldDetected = ParseBool(value, key);
                break;
            case ("safety", "hide_on_clipboard_paste"):
                config.Safety.HideOnClipboardPaste = ParseBool(value, key);
                break;
            case ("safety", "disable_auto_resume_after_risk"):
                config.Safety.DisableAutoResumeAfterRisk = ParseBool(value, key);
                break;

            case ("input", "store_history"):
                config.Input.StoreHistory = ParseBool(value, key);
                break;
            case ("input", "write_key_logs"):
                config.Input.WriteKeyLogs = ParseBool(value, key);
                break;
            case ("input", "allow_text_keys"):
                config.Input.AllowTextKeys = ParseBool(value, key);
                break;
            case ("input", "allow_gamepad"):
                config.Input.AllowGamepad = ParseBool(value, key);
                break;
            case ("input", "allow_mouse_buttons"):
                config.Input.AllowMouseButtons = ParseBool(value, key);
                break;
            case ("input", "allow_mouse_movement"):
                config.Input.AllowMouseMovement = ParseBool(value, key);
                break;

            default:
                throw new ConfigurationException($"Unknown setting: {section}.{key}");
        }
    }

    private static void AddListItem(AppConfig config, string? section, string? nestedList, string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return;
        }

        switch (section, nestedList)
        {
            case ("allowed_processes", null):
                config.AllowedProcesses.Add(value);
                break;
            case ("allowed_keys", "keyboard"):
                config.AllowedKeys.Keyboard.Add(value);
                break;
            case ("allowed_keys", "mouse"):
                config.AllowedKeys.Mouse.Add(value);
                break;
            case ("allowed_keys", "gamepad"):
                config.AllowedKeys.Gamepad.Add(value);
                break;
            default:
                throw new ConfigurationException($"List item is not allowed in section: {section}");
        }
    }

    private static void ClearNestedList(AppConfig config, string? section, string? nestedList)
    {
        switch (section, nestedList)
        {
            case ("allowed_keys", "keyboard"):
                config.AllowedKeys.Keyboard.Clear();
                break;
            case ("allowed_keys", "mouse"):
                config.AllowedKeys.Mouse.Clear();
                break;
            case ("allowed_keys", "gamepad"):
                config.AllowedKeys.Gamepad.Clear();
                break;
            default:
                throw new ConfigurationException($"Unknown list: {section}.{nestedList}");
        }
    }

    private static bool ParseBool(string value, string key)
    {
        return value.ToLowerInvariant() switch
        {
            "true" => true,
            "false" => false,
            _ => throw new ConfigurationException($"Expected true or false for {key}.")
        };
    }

    private static string StripComment(string line)
    {
        var inSingleQuote = false;
        var inDoubleQuote = false;

        for (var i = 0; i < line.Length; i++)
        {
            var current = line[i];
            if (current == '\'' && !inDoubleQuote)
            {
                inSingleQuote = !inSingleQuote;
            }
            else if (current == '"' && !inSingleQuote)
            {
                inDoubleQuote = !inDoubleQuote;
            }
            else if (current == '#' && !inSingleQuote && !inDoubleQuote)
            {
                return line[..i];
            }
        }

        return line;
    }

    private static int CountLeadingSpaces(string line)
    {
        var count = 0;
        while (count < line.Length && line[count] == ' ')
        {
            count++;
        }

        return count;
    }

    private static string Unquote(string value)
    {
        if (value.Length >= 2 &&
            ((value[0] == '"' && value[^1] == '"') || (value[0] == '\'' && value[^1] == '\'')))
        {
            return value[1..^1];
        }

        return value;
    }

    private static IEnumerable<string> ReadLines(string text)
    {
        using var reader = new StringReader(text);
        while (reader.ReadLine() is { } line)
        {
            yield return line;
        }
    }
}
