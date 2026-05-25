using System.Windows.Forms;

namespace VoiceInk.Windows.Native.Text;

public sealed class SelectedTextClipboardFallbackReader : ISelectedTextClipboardFallbackReader
{
    private static readonly TimeSpan DefaultCopyDelay = TimeSpan.FromMilliseconds(150);

    private readonly int maxCharacters;
    private readonly TimeSpan copyDelay;

    public SelectedTextClipboardFallbackReader(int maxCharacters = 4_000, TimeSpan? copyDelay = null)
    {
        this.maxCharacters = Math.Max(1, maxCharacters);
        this.copyDelay = copyDelay is { } value && value > TimeSpan.Zero
            ? value
            : DefaultCopyDelay;
    }

    public async Task<string> GetSelectedTextAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        ClipboardSnapshot? snapshot = null;
        try
        {
            snapshot = await ClipboardSta.RunAsync(ClipboardSnapshot.Capture, cancellationToken);
            await ClipboardSta.RunAsync(
                () =>
                {
                    SendKeys.SendWait("^c");
                    return true;
                },
                cancellationToken);

            await Task.Delay(copyDelay, cancellationToken);
            var text = await ClipboardSta.RunAsync(ReadClipboardText, cancellationToken);
            return text.Length <= maxCharacters ? text : text[..maxCharacters];
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch
        {
            return string.Empty;
        }
        finally
        {
            if (snapshot is not null)
            {
                try
                {
                    await ClipboardSta.RunAsync(
                        () =>
                        {
                            snapshot.Restore();
                            return true;
                        },
                        CancellationToken.None);
                }
                catch
                {
                    // Best-effort clipboard restore; context capture must not break dictation.
                }
            }
        }
    }

    private static string ReadClipboardText()
    {
        if (!Clipboard.ContainsText())
        {
            return string.Empty;
        }

        return Clipboard.GetText().Trim();
    }

    private sealed record ClipboardSnapshot(IDataObject? Data, bool HadData)
    {
        public static ClipboardSnapshot Capture()
        {
            var data = Clipboard.GetDataObject();
            return new ClipboardSnapshot(data, data is not null);
        }

        public void Restore()
        {
            if (HadData && Data is not null)
            {
                Clipboard.SetDataObject(Data, copy: true);
                return;
            }

            Clipboard.Clear();
        }
    }

    private static class ClipboardSta
    {
        public static Task<T> RunAsync<T>(Func<T> action, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var completion = new TaskCompletionSource<T>(TaskCreationOptions.RunContinuationsAsynchronously);
            var thread = new Thread(() =>
            {
                try
                {
                    completion.TrySetResult(action());
                }
                catch (Exception ex)
                {
                    completion.TrySetException(ex);
                }
            })
            {
                IsBackground = true,
                Name = "VoiceInk Clipboard Context STA"
            };
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();

            return cancellationToken.CanBeCanceled
                ? completion.Task.WaitAsync(cancellationToken)
                : completion.Task;
        }
    }
}
