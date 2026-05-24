using VoiceInk.Windows.Core.Services;

namespace VoiceInk.Windows.Core.History;

public sealed class LastTranscriptionActionService(
    IHistoryStore historyStore,
    ITextInjectionService textInjection)
{
    public async Task<LastTranscriptionActionResult> PasteLastAsync(
        LastTranscriptionTextKind textKind,
        CancellationToken cancellationToken)
    {
        var item = (await historyStore.ListRecentAsync(10, cancellationToken))
            .FirstOrDefault(entry => entry.Status == TranscriptionHistoryStatus.Completed);

        if (item is null)
        {
            return new LastTranscriptionActionResult(false, "No transcription available");
        }

        var text = TextFor(item, textKind);
        if (string.IsNullOrWhiteSpace(text))
        {
            return new LastTranscriptionActionResult(false, "No transcription available");
        }

        await textInjection.InsertAsync(text, cancellationToken);
        return new LastTranscriptionActionResult(true, MessageFor(textKind));
    }

    private static string TextFor(TranscriptionHistoryItem item, LastTranscriptionTextKind textKind) =>
        textKind switch
        {
            LastTranscriptionTextKind.EnhancedPreferred when !string.IsNullOrWhiteSpace(item.EnhancedText) =>
                item.EnhancedText,
            _ => item.Text
        };

    private static string MessageFor(LastTranscriptionTextKind textKind) =>
        textKind switch
        {
            LastTranscriptionTextKind.EnhancedPreferred => "Last enhanced transcription pasted",
            _ => "Last transcription pasted"
        };
}
