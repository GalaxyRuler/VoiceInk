using VoiceInk.Windows.Core.History;
using VoiceInk.Windows.Core.Services;

namespace VoiceInk.Windows.Core.Metrics;

public static class SessionMetricRecorder
{
    public const string DefaultSource = "recorder";

    public static async Task<SessionMetricRecorderResult> RecordAsync(
        TranscriptionHistoryItem item,
        ISessionMetricStore store,
        string source,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(item);
        ArgumentNullException.ThrowIfNull(store);

        if (item.Status != TranscriptionHistoryStatus.Completed)
        {
            return new SessionMetricRecorderResult(false);
        }

        try
        {
            if (await store.HasTranscriptionAsync(item.Id, cancellationToken))
            {
                return new SessionMetricRecorderResult(false);
            }

            await store.SaveAsync(CreateMetric(item, source), cancellationToken);
            return new SessionMetricRecorderResult(true);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            return new SessionMetricRecorderResult(false, $"Metrics save failed: {ex.Message}");
        }
    }

    public static SessionMetric CreateMetric(
        TranscriptionHistoryItem item,
        string source)
    {
        ArgumentNullException.ThrowIfNull(item);

        var audioDuration = item.AudioDuration > TimeSpan.Zero
            ? item.AudioDuration
            : TimeSpan.Zero;
        var transcriptionDuration = item.TranscriptionDuration > TimeSpan.Zero
            ? item.TranscriptionDuration
            : (TimeSpan?)null;
        var enhancementDuration = item.EnhancementDuration is not null
            && item.EnhancementDuration.Value > TimeSpan.Zero
            ? item.EnhancementDuration
            : null;
        var speedFactor = transcriptionDuration is not null && audioDuration > TimeSpan.Zero
            ? audioDuration.TotalSeconds / transcriptionDuration.Value.TotalSeconds
            : (double?)null;

        return new SessionMetric(
            Guid.NewGuid(),
            item.Id,
            item.CreatedAt,
            string.IsNullOrWhiteSpace(source) ? DefaultSource : source.Trim(),
            CountWords(TextForCounting(item)),
            audioDuration,
            EmptyToNull(item.ModelPath),
            transcriptionDuration,
            speedFactor,
            EmptyToNull(item.PowerModeName),
            EmptyToNull(item.EnhancementModelName),
            enhancementDuration);
    }

    private static string TextForCounting(TranscriptionHistoryItem item) =>
        item.EnhancementDuration is not null && !string.IsNullOrWhiteSpace(item.EnhancedText)
            ? item.EnhancedText
            : item.Text;

    private static int CountWords(string text) =>
        text.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries).Length;

    private static string? EmptyToNull(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
