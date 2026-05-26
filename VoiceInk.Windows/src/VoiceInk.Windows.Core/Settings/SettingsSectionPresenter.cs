namespace VoiceInk.Windows.Core.Settings;

public sealed record SettingsSectionPresentation(
    string HeroTitle,
    string HeroDescription,
    string OverviewSummary,
    string DataSafetyGuidance,
    IReadOnlyList<SettingsActionSummary> ActionSummaries,
    IReadOnlyList<SettingsSectionCopy> Sections);

public sealed record SettingsActionSummary(
    string Title,
    string Description,
    string StatusBadge);

public sealed record SettingsSectionCopy(
    string Key,
    string Title,
    string Description);

public static class SettingsSectionPresenter
{
    public static SettingsSectionPresentation Present() =>
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
