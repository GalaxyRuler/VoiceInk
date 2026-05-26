using VoiceInk.Windows.Core.Recording;
using VoiceInk.Windows.Core.Text;

namespace VoiceInk.Windows.Core.Settings;

public sealed record SettingsSectionPresentation(
    string HeroTitle,
    string HeroDescription,
    string OverviewSummary,
    string DataSafetyGuidance,
    IReadOnlyList<SettingsActionSummary> ActionSummaries,
    IReadOnlyList<SettingsPreferenceSummary> PreferenceSummaries,
    IReadOnlyList<SettingsSectionCopy> Sections);

public sealed record SettingsActionSummary(
    string Title,
    string Description,
    string StatusBadge);

public sealed record SettingsPreferenceSummary(
    string Title,
    string Value,
    string Detail,
    string StatusBadge);

public sealed record SettingsSectionCopy(
    string Key,
    string Title,
    string Description);

public static class SettingsSectionPresenter
{
    public static SettingsSectionPresentation Present() =>
        Present(new AppSettings());

    public static SettingsSectionPresentation Present(AppSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);

        return
        new(
            "Settings",
            "Tune the everyday behavior of VoiceInk.",
            "Settings, backups, cleanup, and diagnostics stay local to this Windows profile.",
            "Backups exclude API keys. Diagnostic exports use sanitized local logs and are never sent automatically.",
            [
                new(
                    "Shortcuts",
                    "Record, paste, retry, cancel, history, dictionary, enhancement, and Power Mode actions.",
                    "Configure"),
                new(
                    "Data Safety",
                    "Local cleanup and diagnostics stay on this Windows profile unless you export a file.",
                    "Local"),
                new(
                    "Backup",
                    "Export settings, prompts, Power Mode, models, and dictionary data without API keys.",
                    "No secrets"),
                new(
                    "Diagnostics",
                    "Copy or export sanitized troubleshooting context without telemetry.",
                    "Sanitized")
            ],
            PreferenceSummaries(settings),
            [
                new("shortcuts", "Shortcuts", "Configure recording, paste, retry, cancel, history, dictionary, enhancement, and Power Mode shortcuts."),
                new("recordingFeedback", "Recording Feedback", "Control sound feedback, audio muting, media pause, and resume timing while recording."),
                new("interface", "Interface", "Choose how the floating recorder appears while you dictate."),
                new("clipboard", "Clipboard", "Control paste behavior and whether VoiceInk restores your previous clipboard content."),
                new("cleanup", "Cleanup", "Adjust transcript cleanup rules before text is inserted."),
                new("privacy", "Privacy", "Control local transcript and audio retention. Cleanup runs only on this device."),
                new("general", "General", "Manage launch behavior and reset first-run setup when you want to re-run onboarding."),
                new("backup", "Backup", "Export settings locally, or choose specific categories when importing a backup. API keys are never included."),
                new("diagnostics", "Diagnostics", "Export local logs for troubleshooting without sending telemetry.")
            ]);
    }

    private static IReadOnlyList<SettingsPreferenceSummary> PreferenceSummaries(AppSettings settings) =>
    [
        PasteSummary(settings),
        ClipboardSummary(settings),
        RecordingFeedbackSummary(settings),
        PrivacyCleanupSummary(settings)
    ];

    private static SettingsPreferenceSummary PasteSummary(AppSettings settings) =>
        PasteMethodSettings.Normalize(settings.PasteMethod) == PasteMethodSettings.DirectText
            ? new(
                "Paste Method",
                "Direct Text",
                "Types text directly when possible instead of relying on clipboard paste.",
                "Direct")
            : new(
                "Paste Method",
                "Default",
                "Uses the standard clipboard paste path.",
                "Default");

    private static SettingsPreferenceSummary ClipboardSummary(AppSettings settings) =>
        settings.RestoreClipboard
            ? new(
                "Clipboard Restore",
                $"{FormatDelay(settings.ClipboardRestoreDelaySeconds)} delay",
                "Restores previous clipboard content after insertion.",
                "Protected")
            : new(
                "Clipboard Restore",
                "Not restored",
                "VoiceInk may replace clipboard content during insertion.",
                "Off");

    private static SettingsPreferenceSummary RecordingFeedbackSummary(AppSettings settings)
    {
        if (!settings.IsSoundFeedbackEnabled)
        {
            return new(
                "Recording Feedback",
                "Sounds off",
                "Start/stop sounds are muted.",
                "Silent");
        }

        var start = RecordingSoundModeSettings.Normalize(settings.StartSoundMode);
        var stop = RecordingSoundModeSettings.Normalize(settings.StopSoundMode);
        var detail = start == RecordingSoundModeSettings.Custom
            || stop == RecordingSoundModeSettings.Custom
                ? "Start/stop sounds include custom local cues."
                : "Start/stop sounds use System Default cues.";

        return new(
            "Recording Feedback",
            "Sounds on",
            detail,
            "Audible");
    }

    private static SettingsPreferenceSummary PrivacyCleanupSummary(AppSettings settings)
    {
        if (!settings.IsTranscriptionCleanupEnabled && !settings.IsAudioCleanupEnabled)
        {
            return new(
                "Privacy Cleanup",
                "Manual cleanup",
                "Transcript and audio retention are kept until you enable local cleanup.",
                "Manual");
        }

        return new(
            "Privacy Cleanup",
            $"Transcript {FormatTranscriptRetention(settings)}; Audio {FormatAudioRetention(settings)}",
            "Local transcript and audio cleanup run on this device.",
            "Auto");
    }

    private static string FormatDelay(double seconds) =>
        seconds < 1
            ? $"{Math.Round(seconds * 1000):0}ms"
            : $"{seconds:0.#}s";

    private static string FormatTranscriptRetention(AppSettings settings) =>
        settings.IsTranscriptionCleanupEnabled
            ? FormatMinutes(settings.TranscriptionRetentionMinutes)
            : "manual";

    private static string FormatAudioRetention(AppSettings settings) =>
        settings.IsAudioCleanupEnabled
            ? $"{Math.Max(0, settings.AudioRetentionPeriod)}d"
            : "manual";

    private static string FormatMinutes(int minutes) =>
        minutes < 60
            ? $"{Math.Max(0, minutes)}m"
            : $"{minutes / 60}h";
}
