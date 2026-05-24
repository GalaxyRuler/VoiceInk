using VoiceInk.Windows.Core.Services;

namespace VoiceInk.Windows.Core.History;

public sealed class LastTranscriptionActionService(
    IHistoryStore historyStore,
    ITextInjectionService textInjection)
{
    public async Task<LastTranscriptionActionResult> PasteLastAsync(
        LastTranscriptionTextKind textKind,
        CancellationToken cancellationToken,
        Func<CancellationToken, Task>? prepareTargetAsync = null)
    {
        var item = await historyStore.GetLatestCompletedAsync(cancellationToken);

        if (item is null)
        {
            return new LastTranscriptionActionResult(false, "No transcription available");
        }

        var selection = TextFor(item, textKind);
        if (string.IsNullOrWhiteSpace(selection.Text))
        {
            return new LastTranscriptionActionResult(false, "No transcription available");
        }

        if (prepareTargetAsync is not null)
        {
            await prepareTargetAsync(cancellationToken);
        }

        await textInjection.InsertAsync(selection.Text, cancellationToken);
        return new LastTranscriptionActionResult(true, MessageFor(selection.UsedEnhancedText));
    }

    private static TextSelection TextFor(TranscriptionHistoryItem item, LastTranscriptionTextKind textKind)
    {
        if (textKind == LastTranscriptionTextKind.EnhancedPreferred
            && !string.IsNullOrWhiteSpace(item.EnhancedText))
        {
            return new TextSelection(item.EnhancedText, UsedEnhancedText: true);
        }

        return new TextSelection(item.Text, UsedEnhancedText: false);
    }

    private static string MessageFor(bool usedEnhancedText) =>
        usedEnhancedText ? "Last enhanced transcription pasted" : "Last transcription pasted";

    private sealed record TextSelection(string Text, bool UsedEnhancedText);
}
