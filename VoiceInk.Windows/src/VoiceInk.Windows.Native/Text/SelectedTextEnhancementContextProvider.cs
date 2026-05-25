using System.Windows.Automation;

namespace VoiceInk.Windows.Native.Text;

public sealed class SelectedTextEnhancementContextProvider : ISelectedTextReader
{
    private static readonly TimeSpan DefaultTimeout = TimeSpan.FromMilliseconds(500);

    private readonly int maxCharacters;
    private readonly TimeSpan timeout;

    public SelectedTextEnhancementContextProvider(int maxCharacters = 4_000, TimeSpan? timeout = null)
    {
        this.maxCharacters = Math.Max(1, maxCharacters);
        this.timeout = timeout is { } value && value > TimeSpan.Zero
            ? value
            : DefaultTimeout;
    }

    public async Task<string> GetSelectedTextAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var readTask = Task.Run(() => ReadSelectedText(cancellationToken), cancellationToken);
        var timeoutTask = Task.Delay(timeout, cancellationToken);
        var completedTask = await Task.WhenAny(readTask, timeoutTask);
        if (completedTask == readTask)
        {
            return await readTask;
        }

        cancellationToken.ThrowIfCancellationRequested();
        return string.Empty;
    }

    private string ReadSelectedText(CancellationToken cancellationToken)
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            var focusedElement = AutomationElement.FocusedElement;
            if (focusedElement is null ||
                !focusedElement.TryGetCurrentPattern(TextPattern.Pattern, out var patternObject) ||
                patternObject is not TextPattern textPattern)
            {
                return string.Empty;
            }

            var selections = textPattern.GetSelection();
            if (selections.Length == 0)
            {
                return string.Empty;
            }

            var parts = new List<string>();
            var remaining = maxCharacters;
            foreach (var range in selections)
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (remaining <= 0)
                {
                    break;
                }

                var text = range.GetText(remaining + 1).Trim();
                if (text.Length == 0)
                {
                    continue;
                }

                parts.Add(text.Length <= remaining ? text : text[..remaining]);
                remaining -= parts[^1].Length;
            }

            return string.Join(Environment.NewLine, parts).Trim();
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch
        {
            return string.Empty;
        }
    }
}
