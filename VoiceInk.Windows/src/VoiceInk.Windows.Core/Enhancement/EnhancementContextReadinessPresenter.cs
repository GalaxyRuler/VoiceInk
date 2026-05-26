using VoiceInk.Windows.Core.Settings;

namespace VoiceInk.Windows.Core.Enhancement;

public sealed record EnhancementContextReadinessPresentation(
    string Title,
    string Description,
    IReadOnlyList<EnhancementContextReadinessRow> Rows,
    IReadOnlyList<EnhancementContextPrivacyRow> PrivacyRows,
    IReadOnlyList<EnhancementContextActionRow> ActionRows);

public sealed record EnhancementContextReadinessRow(
    string Title,
    string Value,
    string Detail,
    string StatusBadge);

public sealed record EnhancementContextActionRow(
    string Title,
    string Value,
    string Detail,
    string StatusBadge);

public sealed record EnhancementContextPrivacyRow(
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
            ],
            PrivacyRows(settings),
            ActionRows(settings));
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

    private static IReadOnlyList<EnhancementContextActionRow> ActionRows(AppSettings settings)
    {
        var rows = new List<EnhancementContextActionRow>
        {
            EnhancementActionRow(settings),
            ClipboardActionRow(settings)
        };

        var ocrActionRow = OcrActionRow(settings);
        if (ocrActionRow is not null)
        {
            rows.Add(ocrActionRow);
        }

        rows.Add(SourceOrderRow());
        return rows;
    }

    private static EnhancementContextActionRow EnhancementActionRow(AppSettings settings) =>
        settings.IsEnhancementEnabled
            ? new(
                "Enhancement Pipeline",
                "Enabled",
                "Context sources are appended when enhancement runs.",
                "Ready")
            : new(
                "Enhancement Pipeline",
                "Off",
                "Enable Enhancement before context is appended to prompts.",
                "Enable");

    private static EnhancementContextActionRow ClipboardActionRow(AppSettings settings) =>
        settings.UseClipboardContext
            ? new(
                "Clipboard Context",
                "Enabled",
                "Clipboard text is included only during enhancement prompt construction.",
                "On")
            : new(
                "Clipboard Context",
                "Off",
                "Turn on Clipboard Context when clipboard text should guide enhancement.",
                "Optional");

    private static EnhancementContextActionRow? OcrActionRow(AppSettings settings)
    {
        if (!settings.UseOcrContext)
        {
            return null;
        }

        if (!settings.UseOcrCaptureRegion)
        {
            return new(
                "Screen OCR",
                "Full screen",
                "OCR uses visible screen text without a region constraint.",
                "Ready");
        }

        if (settings.OcrCaptureRegionWidth <= 0 || settings.OcrCaptureRegionHeight <= 0)
        {
            return new(
                "Screen OCR",
                "Select Region",
                "Choose an OCR region or switch back to full-screen OCR before relying on screen text.",
                "Setup");
        }

        return new(
            "Screen OCR",
            "Region ready",
            "OCR uses the configured screen region.",
            "Ready");
    }

    private static EnhancementContextActionRow SourceOrderRow() =>
        new(
            "Context Source Order",
            "App, OCR, selection, clipboard",
            "Prompt rendering keeps local app/site context before OCR, selected text, and clipboard text.",
            "Local");

    private static IReadOnlyList<EnhancementContextPrivacyRow> PrivacyRows(AppSettings settings)
    {
        var rows = new List<EnhancementContextPrivacyRow>
        {
            new(
                "Capture Timing",
                "During enhancement",
                "Context is requested only while building an enhancement prompt, not while idle.",
                "Local first"),
            new(
                "Selection and Clipboard",
                "Transient",
                "Selected text and clipboard context are read best-effort and are not stored as separate context records.",
                "Ephemeral"),
            new(
                "Screen OCR Boundary",
                "Local capture",
                "OCR runs locally before prompt rendering; OCR text can be included if the selected enhancement provider is cloud-based.",
                "Prompt scope"),
            new(
                "Context Toggles",
                "User controlled",
                "Disabled context sources are not requested from the Windows integration layer.",
                "Opt in")
        };

        if (settings.IsEnhancementEnabled)
        {
            rows.Add(ProviderBoundaryRow(settings));
        }

        return rows;
    }

    private static EnhancementContextPrivacyRow ProviderBoundaryRow(AppSettings settings)
    {
        var provider = EnhancementProviderPresetCatalog.Resolve(settings.EnhancementProviderId);
        if (IsLocalProvider(provider))
        {
            return new(
                "Enhancement Provider Boundary",
                "Local provider",
                $"Enabled context is sent only to {provider.DisplayName} on this PC.",
                "Local");
        }

        return new(
            "Enhancement Provider Boundary",
            "Cloud provider",
            $"Enabled context can be included in prompts sent to {provider.DisplayName}. Use a local provider when context must stay on this PC.",
            "Cloud");
    }

    private static bool IsLocalProvider(EnhancementProviderPreset provider) =>
        string.Equals(provider.Id, EnhancementProviderPresetCatalog.Ollama.Id, StringComparison.OrdinalIgnoreCase)
        || string.Equals(provider.Id, EnhancementProviderPresetCatalog.LocalCli.Id, StringComparison.OrdinalIgnoreCase);
}
