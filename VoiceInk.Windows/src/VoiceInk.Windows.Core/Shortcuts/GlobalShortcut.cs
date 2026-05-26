namespace VoiceInk.Windows.Core.Shortcuts;

public sealed record GlobalShortcut(
    bool Control,
    bool Alt,
    bool Shift,
    int VirtualKey,
    string KeyName,
    bool IsModifierOnly = false)
{
    public string DisplayText => string.Join("+", DisplayTokens());

    public static bool TryCreateFromKeyCapture(
        bool control,
        bool alt,
        bool shift,
        int virtualKey,
        out GlobalShortcut? shortcut,
        out string? error)
    {
        shortcut = null;
        error = null;

        if (virtualKey is 0x10 or 0x11 or 0x12)
        {
            error = "Press a non-modifier key with your shortcut.";
            return false;
        }

        if (virtualKey is 0x5B or 0x5C)
        {
            error = "Windows-key shortcuts are reserved by Windows.";
            return false;
        }

        if (!control && !alt && !shift)
        {
            error = "A global shortcut must include at least one modifier.";
            return false;
        }

        if (!TryGetKeyName(virtualKey, out var keyName))
        {
            error = $"Unsupported shortcut key: 0x{virtualKey:X2}.";
            return false;
        }

        shortcut = new GlobalShortcut(control, alt, shift, virtualKey, keyName);
        return true;
    }

    public static bool TryCreateModifierOnlyFromKeyCapture(
        bool control,
        bool alt,
        bool shift,
        int virtualKey,
        out GlobalShortcut? shortcut,
        out string? error)
    {
        shortcut = null;
        error = null;

        if (virtualKey is 0x5B or 0x5C)
        {
            error = "Windows-key shortcuts are reserved by Windows.";
            return false;
        }

        if (virtualKey is not 0x10 and not 0x11 and not 0x12)
        {
            error = "Press only modifier keys for a modifier-only shortcut.";
            return false;
        }

        if (!control && !alt && !shift)
        {
            error = "A modifier-only shortcut must include at least one modifier.";
            return false;
        }

        shortcut = CreateModifierOnly(control, alt, shift, virtualKey);
        return true;
    }

    public static bool TryParse(string? value, out GlobalShortcut? shortcut, out string? error)
    {
        shortcut = null;
        error = null;

        if (string.IsNullOrWhiteSpace(value))
        {
            error = "Shortcut is required.";
            return false;
        }

        var control = false;
        var alt = false;
        var shift = false;
        int? virtualKey = null;
        string? keyName = null;

        var tokens = value
            .Split('+', StringSplitOptions.TrimEntries)
            .ToArray();
        if (tokens.Any(token => token.Length == 0))
        {
            error = "A global shortcut must not contain empty parts.";
            return false;
        }

        foreach (var token in tokens)
        {
            switch (token.ToLowerInvariant())
            {
                case "ctrl":
                case "control":
                    control = true;
                    continue;
                case "alt":
                    alt = true;
                    continue;
                case "shift":
                    shift = true;
                    continue;
                case "win":
                case "windows":
                    error = "Windows-key shortcuts are reserved by Windows.";
                    return false;
            }

            if (virtualKey is not null)
            {
                error = "A global shortcut can contain only one key.";
                return false;
            }

            if (!TryParseKey(token, out var parsedVirtualKey, out var parsedKeyName))
            {
                error = $"Unsupported shortcut key: {token}.";
                return false;
            }

            virtualKey = parsedVirtualKey;
            keyName = parsedKeyName;
        }

        if (!control && !alt && !shift)
        {
            error = "A global shortcut must include at least one modifier.";
            return false;
        }

        if (virtualKey is null || keyName is null)
        {
            shortcut = CreateModifierOnly(control, alt, shift, virtualKey: null);
            return true;
        }

        shortcut = new GlobalShortcut(control, alt, shift, virtualKey.Value, keyName);
        return true;
    }

    private IEnumerable<string> DisplayTokens()
    {
        if (Control)
        {
            yield return "Ctrl";
        }

        if (Alt)
        {
            yield return "Alt";
        }

        if (Shift)
        {
            yield return "Shift";
        }

        if (!IsModifierOnly)
        {
            yield return KeyName;
        }
    }

    private static GlobalShortcut CreateModifierOnly(
        bool control,
        bool alt,
        bool shift,
        int? virtualKey)
    {
        var modifierCount = (control ? 1 : 0) + (alt ? 1 : 0) + (shift ? 1 : 0);
        var modifierVirtualKey = modifierCount == 1
            ? control ? 0x11 : alt ? 0x12 : 0x10
            : 0;

        if (virtualKey is 0x10 or 0x11 or 0x12 && modifierCount == 1)
        {
            modifierVirtualKey = virtualKey.Value;
        }

        return new GlobalShortcut(
            control,
            alt,
            shift,
            modifierVirtualKey,
            string.Empty,
            IsModifierOnly: true);
    }

    private static bool TryParseKey(string token, out int virtualKey, out string keyName)
    {
        virtualKey = 0;
        keyName = string.Empty;

        switch (token.ToLowerInvariant())
        {
            case "space":
                virtualKey = 0x20;
                keyName = "Space";
                return true;
            case "esc":
            case "escape":
                virtualKey = 0x1B;
                keyName = "Escape";
                return true;
        }

        if (token.Length == 1)
        {
            var key = char.ToUpperInvariant(token[0]);
            if (key is >= 'A' and <= 'Z' or >= '0' and <= '9')
            {
                virtualKey = key;
                keyName = key.ToString();
                return true;
            }
        }

        if (token.Length is >= 2 and <= 3
            && token[0] is 'f' or 'F'
            && int.TryParse(token[1..], out var functionKey)
            && functionKey is >= 1 and <= 24)
        {
            virtualKey = 0x70 + functionKey - 1;
            keyName = $"F{functionKey}";
            return true;
        }

        return false;
    }

    private static bool TryGetKeyName(int virtualKey, out string keyName)
    {
        keyName = string.Empty;

        if (virtualKey == 0x20)
        {
            keyName = "Space";
            return true;
        }

        if (virtualKey == 0x1B)
        {
            keyName = "Escape";
            return true;
        }

        if (virtualKey is >= 'A' and <= 'Z' or >= '0' and <= '9')
        {
            keyName = ((char)virtualKey).ToString();
            return true;
        }

        if (virtualKey is >= 0x70 and <= 0x87)
        {
            keyName = $"F{virtualKey - 0x70 + 1}";
            return true;
        }

        return false;
    }
}
