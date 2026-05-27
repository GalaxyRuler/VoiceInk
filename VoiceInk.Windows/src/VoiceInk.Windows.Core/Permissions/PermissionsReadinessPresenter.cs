using VoiceInk.Windows.Core.Settings;
using VoiceInk.Windows.Core.Text;

namespace VoiceInk.Windows.Core.Permissions;

public static class PermissionsReadinessPresenter
{
    public static PermissionsReadinessStatus Build(AppSettings settings, bool hasAudioInputChoices)
    {
        ArgumentNullException.ThrowIfNull(settings);

        var hasShortcut = !string.IsNullOrWhiteSpace(settings.Hotkey);
        var pasteMethod = PasteMethodSettings.Normalize(settings.PasteMethod);
        var usesDirectText = pasteMethod == PasteMethodSettings.DirectText;
        var hasValidOcrRegion = settings.OcrCaptureRegionWidth > 0 && settings.OcrCaptureRegionHeight > 0;
        var screenContextReady = !settings.UseOcrContext
            || !settings.UseOcrCaptureRegion
            || hasValidOcrRegion;

        var items = new[]
        {
            new PermissionReadinessItem(
                "Keyboard Shortcut",
                "Set up a global recording shortcut to use VoiceInk from any app.",
                hasShortcut ? "Configured" : "Shortcut required",
                hasShortcut,
                "Configure Shortcut",
                "Settings"),
            new PermissionReadinessItem(
                "Microphone Access",
                "Allow Windows desktop apps to use your microphone for recording.",
                hasAudioInputChoices ? "Audio input available" : "Check Windows microphone privacy",
                hasAudioInputChoices,
                "Open Windows Microphone Settings",
                "ms-settings:privacy-microphone",
                "Manual path: Settings > Privacy & security > Microphone."),
            new PermissionReadinessItem(
                "Text Insertion",
                "Choose how VoiceInk inserts transcribed text at the cursor.",
                usesDirectText ? "Direct text" : "Clipboard paste",
                true,
                "Review Insertion Settings",
                "Settings"),
            new PermissionReadinessItem(
                "Screen Context",
                "Use OCR context for better enhancement prompts when enabled.",
                ScreenContextStatus(settings, screenContextReady),
                screenContextReady,
                "Review Context Settings",
                "Enhancement")
        };

        var readyCount = items.Count(item => item.IsReady);
        var summarySeverity = readyCount == items.Length ? "Ready" : "Needs attention";
        var summaryTitle = readyCount == items.Length
            ? "VoiceInk is ready for dictation"
            : "Some setup items need attention";
        var summaryMessage = $"{readyCount} of {items.Length} permission and readiness checks are ready.";

        return new PermissionsReadinessStatus(summarySeverity, summaryTitle, summaryMessage, items);
    }

    private static string ScreenContextStatus(AppSettings settings, bool isReady)
    {
        if (!settings.UseOcrContext)
        {
            return "Off";
        }

        if (!settings.UseOcrCaptureRegion)
        {
            return "Full screen OCR context";
        }

        return isReady ? "OCR region configured" : "OCR region required";
    }
}
