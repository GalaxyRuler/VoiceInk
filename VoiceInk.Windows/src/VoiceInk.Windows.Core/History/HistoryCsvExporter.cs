using System.Globalization;
using System.Text;

namespace VoiceInk.Windows.Core.History;

public static class HistoryCsvExporter
{
    private const string Header =
        "Original Transcript,Final Transcript,Enhanced Transcript,Prompt Name,Transcription Model,Power Mode,Provider,Enhancement Provider,Enhancement Model,Status,Language,Transcription Time,Enhancement Time,Timestamp,Duration,Error Message,Audio File Path";

    public static string Export(IEnumerable<TranscriptionHistoryItem> items)
    {
        var builder = new StringBuilder();
        builder.AppendLine(Header);

        foreach (var item in items)
        {
            builder.AppendLine(string.Join(
                ",",
                Escape(item.OriginalText),
                Escape(item.Text),
                Escape(item.EnhancedText ?? string.Empty),
                Escape(item.PromptName ?? string.Empty),
                Escape(item.ModelPath ?? string.Empty),
                Escape(PowerModeDisplay(item.PowerModeName, item.PowerModeEmoji)),
                Escape(item.ProviderName),
                Escape(item.EnhancementProviderName ?? string.Empty),
                Escape(item.EnhancementModelName ?? string.Empty),
                Escape(item.Status.ToString()),
                Escape(item.Language),
                Escape(Seconds(item.TranscriptionDuration)),
                Escape(item.EnhancementDuration is null ? string.Empty : Seconds(item.EnhancementDuration.Value)),
                Escape(item.CreatedAt.ToString("O", CultureInfo.InvariantCulture)),
                Escape(Seconds(item.AudioDuration)),
                Escape(item.ErrorMessage ?? string.Empty),
                Escape(item.AudioFilePath ?? string.Empty)));
        }

        return builder.ToString();
    }

    private static string Seconds(TimeSpan duration) =>
        duration.TotalSeconds.ToString("0.000", CultureInfo.InvariantCulture);

    private static string Escape(string value)
    {
        if (value.Length == 0)
        {
            return string.Empty;
        }

        var escaped = value.Replace("\"", "\"\"", StringComparison.Ordinal);
        return escaped.IndexOfAny([',', '"', '\r', '\n']) >= 0
            ? $"\"{escaped}\""
            : escaped;
    }

    private static string PowerModeDisplay(string? name, string? emoji)
    {
        var trimmedName = name?.Trim();
        var trimmedEmoji = emoji?.Trim();
        return (trimmedEmoji, trimmedName) switch
        {
            ({ Length: > 0 }, { Length: > 0 }) => $"{trimmedEmoji} {trimmedName}",
            ({ Length: > 0 }, _) => trimmedEmoji,
            (_, { Length: > 0 }) => trimmedName,
            _ => string.Empty
        };
    }
}
