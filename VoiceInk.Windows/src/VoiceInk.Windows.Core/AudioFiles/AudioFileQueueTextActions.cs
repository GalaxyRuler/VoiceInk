using System.Globalization;
using System.Text.RegularExpressions;

namespace VoiceInk.Windows.Core.AudioFiles;

public static partial class AudioFileQueueTextActions
{
    public static bool TryGetActionText(AudioFileQueueItem? item, out string text, out string message)
    {
        text = string.Empty;
        if (item is null)
        {
            message = "Select a completed transcription to copy or save";
            return false;
        }

        if (item.Status != AudioFileQueueStatus.Completed || item.HistoryItem is null)
        {
            message = "Transcribe the selected file before copying or saving";
            return false;
        }

        text = string.IsNullOrWhiteSpace(item.HistoryItem.EnhancedText)
            ? item.HistoryItem.Text.Trim()
            : item.HistoryItem.EnhancedText.Trim();
        if (string.IsNullOrWhiteSpace(text))
        {
            text = string.Empty;
            message = "The selected transcription has no text to copy or save";
            return false;
        }

        message = "Ready";
        return true;
    }

    public static string SuggestFileName(string text)
    {
        var words = WhitespaceRegex()
            .Split(text.Trim())
            .Where(word => !string.IsNullOrWhiteSpace(word))
            .Take(8)
            .Select(SanitizeFileNameWord)
            .Where(word => word.Length > 0)
            .ToArray();
        if (words.Length == 0)
        {
            return "transcription";
        }

        var fileName = RepeatedDashRegex().Replace(string.Join("-", words).ToLowerInvariant(), "-").Trim('-');
        if (fileName.Length == 0)
        {
            return "transcription";
        }

        return fileName.Length <= 50 ? fileName : fileName[..50].Trim('-');
    }

    public static string FormatMarkdown(string text, DateTimeOffset createdAt)
    {
        var timestamp = createdAt.ToLocalTime().ToString("f", CultureInfo.CurrentCulture);
        return string.Join(
            Environment.NewLine + Environment.NewLine,
            "# Transcription",
            $"**Date:** {timestamp}",
            text.Trim());
    }

    private static string SanitizedReplacement(Match match) => match.Value == "-" ? "-" : string.Empty;

    private static string SanitizeFileNameWord(string word) =>
        InvalidFileNameWordRegex().Replace(word, SanitizedReplacement).Trim('-');

    [GeneratedRegex(@"\s+")]
    private static partial Regex WhitespaceRegex();

    [GeneratedRegex(@"[^A-Za-z0-9-]+")]
    private static partial Regex InvalidFileNameWordRegex();

    [GeneratedRegex("-+")]
    private static partial Regex RepeatedDashRegex();
}
