namespace VoiceInk.Windows.Core.History;

public static class HistoryCopyTextSelector
{
    public static HistoryCopyTextResult Select(
        TranscriptionHistoryItem item,
        HistoryCopyTextKind kind) =>
        kind switch
        {
            HistoryCopyTextKind.Original => TextResult(
                item.OriginalText,
                "Original transcription copied",
                "No original transcription available"),
            HistoryCopyTextKind.Final => TextResult(
                item.Text,
                "Final transcription copied",
                "No final transcription available"),
            HistoryCopyTextKind.Enhanced => TextResult(
                item.EnhancedText ?? string.Empty,
                "Enhanced transcription copied",
                "No enhanced transcription available"),
            HistoryCopyTextKind.AiRequest => AiRequestResult(item),
            _ => new HistoryCopyTextResult(false, "Unsupported copy action", string.Empty)
        };

    public static bool HasText(
        TranscriptionHistoryItem? item,
        HistoryCopyTextKind kind) =>
        item is not null && Select(item, kind).Success;

    private static HistoryCopyTextResult AiRequestResult(TranscriptionHistoryItem item)
    {
        var parts = new List<string>();
        if (!string.IsNullOrWhiteSpace(item.AiRequestSystemMessage))
        {
            parts.Add($"System Prompt:{Environment.NewLine}{item.AiRequestSystemMessage.Trim()}");
        }

        if (!string.IsNullOrWhiteSpace(item.AiRequestUserMessage))
        {
            parts.Add($"User Message:{Environment.NewLine}{item.AiRequestUserMessage.Trim()}");
        }

        return TextResult(
            string.Join($"{Environment.NewLine}{Environment.NewLine}", parts),
            "AI request copied",
            "No AI request available");
    }

    private static HistoryCopyTextResult TextResult(
        string text,
        string successMessage,
        string failureMessage)
    {
        var trimmed = text.Trim();
        return trimmed.Length == 0
            ? new HistoryCopyTextResult(false, failureMessage, string.Empty)
            : new HistoryCopyTextResult(true, successMessage, trimmed);
    }
}
