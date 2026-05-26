using VoiceInk.Windows.Core.Settings;

namespace VoiceInk.Windows.Core.Enhancement;

public sealed record EnhancementContextReadinessPresentation(
    string Title,
    string Description,
    IReadOnlyList<EnhancementContextReadinessRow> Rows);

public sealed record EnhancementContextReadinessRow(
    string Title,
    string Value,
    string Detail,
    string StatusBadge);

public static class EnhancementContextReadinessPresenter
{
    public static EnhancementContextReadinessPresentation Present(AppSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);

        return new(
            "Context Awareness",
            "VoiceInk can add local app, selection, clipboard, and screen text context to enhancement prompts.",
            [
                ClipboardRow(settings),
                SelectedTextRow(),
                ActiveAppRow(),
                OcrRow(settings)
            ]);
    }

    private static EnhancementContextReadinessRow ClipboardRow(AppSettings settings) =>
        settings.UseClipboardContext
            ? new(
                "Clipboard Context",
                "Enabled",
                "Adds clipboard text to enhancement prompts when available.",
                "On")
            : new(
                "Clipboard Context",
                "Off",
                "Clipboard text is skipped unless you enable it.",
                "Off");

    private static EnhancementContextReadinessRow SelectedTextRow() =>
        new(
            "Selected Text",
            "Automatic",
            "Reads selected text with clipboard fallback when Windows allows it.",
            "Best effort");

    private static EnhancementContextReadinessRow ActiveAppRow() =>
        new(
            "Active App & Site",
            "Automatic",
            "Adds active window details and sanitized browser URLs when available.",
            "Best effort");

    private static EnhancementContextReadinessRow OcrRow(AppSettings settings)
    {
        if (!settings.UseOcrContext)
        {
            return new(
                "Screen OCR",
                "Off",
                "Screen text is skipped until OCR context is enabled.",
                "Off");
        }

        if (!settings.UseOcrCaptureRegion)
        {
            return new(
                "Screen OCR",
                "Full screen",
                "Captures visible screen text locally when enhancement runs.",
                "On");
        }

        if (settings.OcrCaptureRegionWidth <= 0 || settings.OcrCaptureRegionHeight <= 0)
        {
            return new(
                "Screen OCR",
                "Region required",
                "Select a screen region before using constrained OCR context.",
                "Needs setup");
        }

        return new(
            "Screen OCR",
            $"Region {settings.OcrCaptureRegionWidth} x {settings.OcrCaptureRegionHeight}",
            "Captures only the selected screen region.",
            "Region");
    }
}
