using System.Globalization;
using System.Text.RegularExpressions;

namespace VoiceInk.Windows.Core.History;

public sealed record HistoryAnalysisRow(
    string Title,
    string Value,
    string Detail);

public static partial class HistoryAnalysisPresenter
{
    public static IReadOnlyList<HistoryAnalysisRow> Present(TranscriptionHistoryItem item)
    {
        var displayText = DisplayText(item);
        var wordCount = WordCount(displayText);

        return
        [
            new(
                "Words",
                wordCount.ToString(CultureInfo.InvariantCulture),
                string.IsNullOrWhiteSpace(item.EnhancedText) ? "Original transcript" : "Final transcript"),
            new(
                "Audio",
                FormatDuration(item.AudioDuration),
                SpeechRateDetail(wordCount, item.AudioDuration)),
            new(
                "Enhancement",
                EnhancementValue(item),
                EnhancementDetail(item)),
            new(
                "Provider",
                string.IsNullOrWhiteSpace(item.ProviderName) ? "Unknown" : item.ProviderName,
                ProviderDetail(item)),
            new(
                "Audio Storage",
                string.IsNullOrWhiteSpace(item.AudioFilePath) ? "Text only" : "Audio saved",
                string.IsNullOrWhiteSpace(item.AudioFilePath)
                    ? "No audio file is attached to this history item."
                    : "Audio can be opened or replayed while the file remains on disk."),
            RetryAndReenhanceRow(item),
            new(
                "Export Scope",
                "User initiated",
                "History text, metadata, and audio paths stay local unless you copy, paste, or export them.")
        ];
    }

    private static string DisplayText(TranscriptionHistoryItem item) =>
        !string.IsNullOrWhiteSpace(item.EnhancedText)
            ? item.EnhancedText
            : !string.IsNullOrWhiteSpace(item.Text)
                ? item.Text
                : item.OriginalText;

    private static int WordCount(string text) =>
        string.IsNullOrWhiteSpace(text)
            ? 0
            : WordPattern().Matches(text).Count;

    private static string SpeechRateDetail(int wordCount, TimeSpan audioDuration)
    {
        if (audioDuration <= TimeSpan.Zero)
        {
            return "Speech rate unavailable";
        }

        var wordsPerMinute = (int)Math.Round(wordCount / audioDuration.TotalMinutes, MidpointRounding.AwayFromZero);
        return $"{wordsPerMinute} wpm";
    }

    private static string EnhancementValue(TranscriptionHistoryItem item) =>
        item.Status switch
        {
            TranscriptionHistoryStatus.Canceled => "Canceled",
            TranscriptionHistoryStatus.Failed => "Failed",
            _ when !string.IsNullOrWhiteSpace(item.EnhancedText) => "Enhanced",
            _ => "Original only"
        };

    private static string EnhancementDetail(TranscriptionHistoryItem item)
    {
        if (item.Status == TranscriptionHistoryStatus.Failed)
        {
            return string.IsNullOrWhiteSpace(item.ErrorMessage) ? "Transcription failed" : item.ErrorMessage;
        }

        if (item.Status == TranscriptionHistoryStatus.Canceled)
        {
            return "Recording was canceled before completion";
        }

        if (!string.IsNullOrWhiteSpace(item.EnhancedText))
        {
            return item.EnhancementDuration is { } duration && duration > TimeSpan.Zero
                ? $"AI cleanup completed in {FormatDuration(duration)}"
                : "AI cleanup completed";
        }

        return "No enhanced text saved";
    }

    private static string ProviderDetail(TranscriptionHistoryItem item) =>
        item.TranscriptionDuration > TimeSpan.Zero
            ? $"Transcription completed in {FormatDuration(item.TranscriptionDuration)}."
            : "Transcription duration unavailable.";

    private static HistoryAnalysisRow RetryAndReenhanceRow(TranscriptionHistoryItem item)
    {
        if (item.Status != TranscriptionHistoryStatus.Completed)
        {
            return new(
                "Retry and Re-enhance",
                "Unavailable",
                "Retry and re-enhance are available only for completed history items.");
        }

        var hasAudio = !string.IsNullOrWhiteSpace(item.AudioFilePath);
        var hasOriginal = !string.IsNullOrWhiteSpace(item.OriginalText);

        return (hasAudio, hasOriginal) switch
        {
            (true, true) => new(
                "Retry and Re-enhance",
                "Audio and original text",
                "Retry uses the saved audio file; re-enhance uses the original transcript text for a fresh AI pass."),
            (true, false) => new(
                "Retry and Re-enhance",
                "Audio and final text",
                "Retry uses the saved audio file; re-enhance uses the final transcript because no separate original text was saved."),
            (false, true) => new(
                "Retry and Re-enhance",
                "Original text",
                "Retry needs saved audio; re-enhance uses the original transcript text for a fresh AI pass."),
            _ => new(
                "Retry and Re-enhance",
                "Final text",
                "Retry needs saved audio; re-enhance uses the final transcript because no separate original text was saved.")
        };
    }

    private static string FormatDuration(TimeSpan duration) =>
        duration.TotalMinutes >= 1
            ? $"{duration.TotalMinutes:0.#}m"
            : $"{Math.Max(0, duration.TotalSeconds):0.#}s";

    [GeneratedRegex(@"\b[\p{L}\p{N}']+\b")]
    private static partial Regex WordPattern();
}
