namespace VoiceInk.Windows.Core.Diagnostics;

public static class DiagnosticEventSanitizer
{
    public static string? Normalize(string? status)
    {
        var trimmed = status?.Trim();
        if (string.IsNullOrWhiteSpace(trimmed))
        {
            return null;
        }

        if (trimmed is "Idle"
            or "Recording"
            or "Transcribing"
            or "Inserting"
            or "Error"
            or "Loading settings"
            or "Closing")
        {
            return trimmed;
        }

        if (trimmed.EndsWith(" prompt saved", StringComparison.OrdinalIgnoreCase))
        {
            return "Prompt saved";
        }

        if (trimmed.EndsWith(" prompt deleted", StringComparison.OrdinalIgnoreCase))
        {
            return "Prompt deleted";
        }

        if (trimmed.StartsWith("Default model:", StringComparison.OrdinalIgnoreCase))
        {
            return "Default model changed";
        }

        if (trimmed.StartsWith("Duplicate vocabulary word:", StringComparison.OrdinalIgnoreCase))
        {
            return "Duplicate vocabulary word";
        }

        if (trimmed.StartsWith("Duplicate word replacement:", StringComparison.OrdinalIgnoreCase))
        {
            return "Duplicate word replacement";
        }

        foreach (var prefix in SafeStatusPrefixes)
        {
            if (trimmed.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                return prefix;
            }
        }

        return null;
    }

    private static readonly string[] SafeStatusPrefixes =
    [
        "Active window failed",
        "Active window refreshed",
        "Active window unavailable",
        "Audio file not found",
        "Audio file opened",
        "Audio file queue cleared",
        "Audio file queued for retry",
        "Audio file removed",
        "Audio file selection canceled",
        "Audio file selection failed",
        "Audio input refresh failed",
        "Audio input refreshed",
        "Audio input update failed",
        "Audio input updated",
        "Cancel Recording failed",
        "Cleanup settings save failed",
        "Cleanup settings saved",
        "Cloud transcription key cleared",
        "Cloud transcription key save failed",
        "Cloud transcription key saved",
        "Diagnostic log export canceled",
        "Diagnostic log export failed",
        "Diagnostic logs exported",
        "Diagnostics copy failed",
        "Diagnostics folder failed",
        "Diagnostics folder opened",
        "Diagnostics summary copied",
        "Dictionary export canceled",
        "Dictionary export failed",
        "Dictionary exported",
        "Dictionary import canceled",
        "Dictionary import failed",
        "Dictionary imported",
        "Dictionary sort failed",
        "Dictionary sorted",
        "Enhancement key cleared",
        "Enhancement key save failed",
        "Enhancement key saved",
        "Enhancement settings update failed",
        "Enhancement settings updated",
        "First-run setup saved",
        "First-run setup skipped",
        "History delete failed",
        "History export canceled",
        "History export failed",
        "History exported",
        "History opened",
        "History refresh failed",
        "History refreshed",
        "Hotkey failed",
        "Launch at Login update failed",
        "Launch at Login updated",
        "Metrics export canceled",
        "Metrics export failed",
        "Metrics exported",
        "Metrics refresh failed",
        "Metrics refreshed",
        "Model downloads failed",
        "Model downloads opened",
        "Model import canceled",
        "Model import failed",
        "Model imported",
        "Model picker failed",
        "Onboarding reset canceled",
        "Onboarding reset failed",
        "Onboarding will show on next launch",
        "Opening first-run setup",
        "Opening quick add",
        "Open audio failed",
        "Open history failed",
        "Power Mode rule added",
        "Power Mode rule removed",
        "Power Mode rule reordered",
        "Power Mode rule updated",
        "Preparing diagnostic logs",
        "Preparing settings backup",
        "Prompt delete failed",
        "Prompt save failed",
        "Quick Add failed",
        "Recording feedback settings save failed",
        "Recording feedback settings saved",
        "Running privacy cleanup",
        "Settings export canceled",
        "Settings export failed",
        "Settings exported",
        "Settings import canceled",
        "Settings import failed",
        "Settings imported",
        "Settings load failed",
        "Shortcut update failed",
        "Shortcuts updated",
        "Transcription deleted",
        "Transcription provider settings update failed",
        "Transcription provider settings updated",
        "Transcript cleanup canceled",
        "Vocabulary update failed",
        "Vocabulary updated",
        "Word replacement disabled",
        "Word replacement enabled",
        "Word replacement update failed",
        "Word replacements updated"
    ];
}
