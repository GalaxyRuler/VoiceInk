using VoiceInk.Windows.Core.Settings;
using VoiceInk.Windows.Core.Recording;
using VoiceInk.Windows.Core.Text;
using Xunit;

namespace VoiceInk.Windows.Core.Tests.Settings;

public sealed class SettingsSectionPresenterTests
{
    [Fact]
    public void Present_ReturnsMacStyleSettingsSectionCopy()
    {
        var presentation = SettingsSectionPresenter.Present();

        Assert.Equal("Settings", presentation.HeroTitle);
        Assert.Equal("Tune the everyday behavior of VoiceInk.", presentation.HeroDescription);
        Assert.Equal(
            "Settings, backups, cleanup, and diagnostics stay local to this Windows profile.",
            presentation.OverviewSummary);
        Assert.Equal(
            "Backups exclude API keys. Diagnostic exports use sanitized local logs and are never sent automatically.",
            presentation.DataSafetyGuidance);
        Assert.Collection(
            presentation.ActionSummaries,
            row =>
            {
                Assert.Equal("Shortcuts", row.Title);
                Assert.Equal("Record, paste, retry, cancel, history, dictionary, enhancement, and Power Mode actions.", row.Description);
                Assert.Equal("Configure", row.StatusBadge);
                Assert.Equal(
                    "Shortcuts, Configure, Record, paste, retry, cancel, history, dictionary, enhancement, and Power Mode actions.",
                    row.AccessibleName);
            },
            row =>
            {
                Assert.Equal("Find Settings", row.Title);
                Assert.Equal("Use grouped sections for shortcuts, feedback, interface, clipboard, cleanup, privacy, backup, and diagnostics.", row.Description);
                Assert.Equal("Grouped", row.StatusBadge);
            },
            row =>
            {
                Assert.Equal("Data Safety", row.Title);
                Assert.Equal("Local cleanup and diagnostics stay on this Windows profile unless you export a file.", row.Description);
                Assert.Equal("Local", row.StatusBadge);
            },
            row =>
            {
                Assert.Equal("Backup", row.Title);
                Assert.Equal("Export settings, prompts, Power Mode, models, and dictionary data without API keys.", row.Description);
                Assert.Equal("No secrets", row.StatusBadge);
            },
            row =>
            {
                Assert.Equal("Diagnostics", row.Title);
                Assert.Equal("Copy or export sanitized troubleshooting context without telemetry.", row.Description);
                Assert.Equal("Sanitized", row.StatusBadge);
            });
        Assert.Collection(
            presentation.PreferenceSummaries,
            row =>
            {
                Assert.Equal("Paste Method", row.Title);
                Assert.Equal("Default", row.Value);
                Assert.Equal("Uses the standard clipboard paste path.", row.Detail);
                Assert.Equal("Default", row.StatusBadge);
                Assert.Equal(
                    "Paste Method, Default, Default, Uses the standard clipboard paste path.",
                    row.AccessibleName);
            },
            row =>
            {
                Assert.Equal("Clipboard Restore", row.Title);
                Assert.Equal("2s delay", row.Value);
                Assert.Equal("Restores previous clipboard content after insertion.", row.Detail);
                Assert.Equal("Protected", row.StatusBadge);
            },
            row =>
            {
                Assert.Equal("Recording Feedback", row.Title);
                Assert.Equal("Sounds on", row.Value);
                Assert.Equal("Start/stop sounds use System Default cues.", row.Detail);
                Assert.Equal("Audible", row.StatusBadge);
            },
            row =>
            {
                Assert.Equal("Privacy Cleanup", row.Title);
                Assert.Equal("Manual cleanup", row.Value);
                Assert.Equal("Transcript and audio retention are kept until you enable local cleanup.", row.Detail);
                Assert.Equal("Manual", row.StatusBadge);
            });
        Assert.Collection(
            presentation.BackupGuidanceRows,
            row =>
            {
                Assert.Equal("Settings and Prompts", row.Title);
                Assert.Equal("Included", row.Value);
                Assert.Equal("General settings, custom prompts, and Power Mode rules travel in the local backup.", row.Detail);
                Assert.Equal("Portable", row.StatusBadge);
                Assert.Equal(
                    "Settings and Prompts, Included, Portable, General settings, custom prompts, and Power Mode rules travel in the local backup.",
                    row.AccessibleName);
            },
            row =>
            {
                Assert.Equal("Provider API Keys", row.Title);
                Assert.Equal("Excluded", row.Value);
                Assert.Equal("Keys stay in Windows Credential Manager and must be re-entered after import.", row.Detail);
                Assert.Equal("Local only", row.StatusBadge);
            },
            row =>
            {
                Assert.Equal("Model References", row.Title);
                Assert.Equal("References only", row.Value);
                Assert.Equal("Imported model paths are restored, but large model files are not copied into the backup.", row.Detail);
                Assert.Equal("Paths", row.StatusBadge);
            },
            row =>
            {
                Assert.Equal("Dictionary", row.Title);
                Assert.Equal("Included", row.Value);
                Assert.Equal("Vocabulary words and replacements can be restored through settings backup or dictionary import/export.", row.Detail);
                Assert.Equal("Portable", row.StatusBadge);
            },
            row =>
            {
                Assert.Equal("Windows Profile", row.Title);
                Assert.Equal("User-chosen file", row.Value);
                Assert.Equal("Backups are written only when you choose an export location; VoiceInk does not roam settings automatically.", row.Detail);
                Assert.Equal("Manual", row.StatusBadge);
            });
        Assert.Collection(
            presentation.UpdateGuidanceRows,
            row =>
            {
                Assert.Equal("App Installer", row.Title);
                Assert.Equal("Optional", row.Value);
                Assert.Equal("Signed .appinstaller releases can check for updates on launch when a maintainer publishes them.", row.Detail);
                Assert.Equal("MSIX", row.StatusBadge);
                Assert.Equal(
                    "App Installer, Optional, MSIX, Signed .appinstaller releases can check for updates on launch when a maintainer publishes them.",
                    row.AccessibleName);
            },
            row =>
            {
                Assert.Equal("WinGet", row.Title);
                Assert.Equal("Manual command", row.Value);
                Assert.Equal("Run winget upgrade --id VoiceInk.VoiceInkWindows from a terminal you control after a signed manifest is published.", row.Detail);
                Assert.Equal("Package manager", row.StatusBadge);
            },
            row =>
            {
                Assert.Equal("Source Builds", row.Title);
                Assert.Equal("Repository", row.Value);
                Assert.Equal("Source-built ZIP users rebuild or download release artifacts manually; VoiceInk has no private updater or commercial channel.", row.Detail);
                Assert.Equal("Open source", row.StatusBadge);
            });
        Assert.Collection(
            presentation.DiagnosticsGuidanceRows,
            row =>
            {
                Assert.Equal("Diagnostic Logs", row.Title);
                Assert.Equal("Local export", row.Value);
                Assert.Equal("Logs are opened or exported from this Windows profile and are not sent automatically.", row.Detail);
                Assert.Equal("Local", row.StatusBadge);
                Assert.Equal(
                    "Diagnostic Logs, Local export, Local, Logs are opened or exported from this Windows profile and are not sent automatically.",
                    row.AccessibleName);
            },
            row =>
            {
                Assert.Equal("Summary Copy", row.Title);
                Assert.Equal("Sanitized", row.Value);
                Assert.Equal("Copied diagnostics exclude API keys and credential values.", row.Detail);
                Assert.Equal("No secrets", row.StatusBadge);
            },
            row =>
            {
                Assert.Equal("Recent Events", row.Title);
                Assert.Equal("Bounded", row.Value);
                Assert.Equal("Diagnostics summary includes recent sanitized VoiceInk status events, not an unlimited activity log.", row.Detail);
                Assert.Equal("Recent only", row.StatusBadge);
            },
            row =>
            {
                Assert.Equal("Windows App Diagnostics", row.Title);
                Assert.Equal("OS controlled", row.Value);
                Assert.Equal("Windows privacy settings control app-diagnostics access outside VoiceInk's local logs.", row.Detail);
                Assert.Equal("Windows", row.StatusBadge);
            },
            row =>
            {
                Assert.Equal("Optional Diagnostic Data", row.Title);
                Assert.Equal("Not required", row.Value);
                Assert.Equal("VoiceInk diagnostics export does not enable Windows optional diagnostic data or upload logs.", row.Detail);
                Assert.Equal("Manual export", row.StatusBadge);
            });
        Assert.Equal(
            [
                "Shortcuts",
                "Recording Feedback",
                "Interface",
                "Clipboard",
                "Cleanup",
                "Privacy",
                "General",
                "Backup",
                "Diagnostics"
            ],
            presentation.Sections.Select(section => section.Title).ToArray());
        Assert.Contains(
            presentation.Sections,
            section => section.Title == "Backup"
                && section.Description == "Export settings locally, or choose specific categories when importing a backup. API keys are never included.");
        Assert.Contains(
            presentation.Sections,
            section => section.Title == "Diagnostics"
                && section.Description == "Export local logs for troubleshooting without sending telemetry.");
    }

    [Fact]
    public void Present_ShowsBoundedDiagnosticsSummaryGuidance()
    {
        var presentation = SettingsSectionPresenter.Present();

        Assert.Contains(
            presentation.DiagnosticsGuidanceRows,
            row => row.Title == "Recent Events"
                && row.Value == "Bounded"
                && row.Detail == "Diagnostics summary includes recent sanitized VoiceInk status events, not an unlimited activity log."
                && row.StatusBadge == "Recent only");
    }

    [Fact]
    public void Present_WithCustomSettings_ReturnsCurrentStateSummary()
    {
        var presentation = SettingsSectionPresenter.Present(
            new AppSettings
            {
                PasteMethod = PasteMethodSettings.DirectText,
                RestoreClipboard = false,
                ClipboardRestoreDelaySeconds = 0.5,
                IsSoundFeedbackEnabled = false,
                StartSoundMode = RecordingSoundModeSettings.Custom,
                StopSoundMode = RecordingSoundModeSettings.Custom,
                CustomStartSoundPath = @"C:\VoiceInk\Sounds\start.wav",
                CustomStopSoundPath = @"C:\VoiceInk\Sounds\stop.wav",
                IsTranscriptionCleanupEnabled = true,
                TranscriptionRetentionMinutes = 30,
                IsAudioCleanupEnabled = true,
                AudioRetentionPeriod = 2
            });

        Assert.Collection(
            presentation.PreferenceSummaries,
            row =>
            {
                Assert.Equal("Paste Method", row.Title);
                Assert.Equal("Direct Text", row.Value);
                Assert.Equal("Types text directly when possible instead of relying on clipboard paste.", row.Detail);
                Assert.Equal("Direct", row.StatusBadge);
            },
            row =>
            {
                Assert.Equal("Clipboard Restore", row.Title);
                Assert.Equal("Not restored", row.Value);
                Assert.Equal("VoiceInk may replace clipboard content during insertion.", row.Detail);
                Assert.Equal("Off", row.StatusBadge);
            },
            row =>
            {
                Assert.Equal("Recording Feedback", row.Title);
                Assert.Equal("Sounds off", row.Value);
                Assert.Equal("Start/stop sounds are muted.", row.Detail);
                Assert.Equal("Silent", row.StatusBadge);
            },
            row =>
            {
                Assert.Equal("Privacy Cleanup", row.Title);
                Assert.Equal("Transcript 30m; Audio 2d", row.Value);
                Assert.Equal("Local transcript and audio cleanup run on this device.", row.Detail);
                Assert.Equal("Auto", row.StatusBadge);
            });
    }

    [Fact]
    public void Present_WithBuiltInRecordingSounds_DescribesWindowsSoundSchemeCues()
    {
        var presentation = SettingsSectionPresenter.Present(
            new AppSettings
            {
                IsSoundFeedbackEnabled = true,
                StartSoundMode = RecordingSoundModeSettings.Asterisk,
                StopSoundMode = RecordingSoundModeSettings.Beep
            });

        var row = Assert.Single(
            presentation.PreferenceSummaries,
            row => row.Title == "Recording Feedback");
        Assert.Equal("Sounds on", row.Value);
        Assert.Equal("Start/stop sounds use selected Windows sound scheme cues.", row.Detail);
        Assert.Equal("Audible", row.StatusBadge);
    }
}
