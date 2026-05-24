using System.Collections.Concurrent;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using VoiceInk.Windows.Core.Services;

namespace VoiceInk.Windows.Native.Text;

public sealed class ClipboardTextInjectionService(bool restoreClipboard) : ITextInjectionService
{
    public async Task InsertAsync(string text, CancellationToken cancellationToken)
    {
        var previousClipboard = restoreClipboard ? ClipboardSnapshot.Capture() : null;

        ClipboardStaDispatcher.Invoke(() => Clipboard.SetText(text));
        try
        {
            await Task.Delay(TimeSpan.FromMilliseconds(80), cancellationToken);
            SendCtrlV();
        }
        finally
        {
            if (restoreClipboard && previousClipboard is not null)
            {
                await Task.Delay(TimeSpan.FromMilliseconds(400), CancellationToken.None);

                try
                {
                    await previousClipboard.RestoreAsync(CancellationToken.None);
                }
                catch
                {
                }
            }
        }
    }

    private static void SendCtrlV()
    {
        var inputs = new[]
        {
            KeyboardInput(VirtualKeyControl, keyUp: false),
            KeyboardInput(VirtualKeyV, keyUp: false),
            KeyboardInput(VirtualKeyV, keyUp: true),
            KeyboardInput(VirtualKeyControl, keyUp: true)
        };

        var sent = SendInput((uint)inputs.Length, inputs, Marshal.SizeOf<INPUT>());
        if (sent != inputs.Length)
        {
            throw new InvalidOperationException(
                "Windows did not accept the paste keyboard input.",
                new Win32Exception(Marshal.GetLastPInvokeError()));
        }
    }

    private static INPUT KeyboardInput(ushort virtualKey, bool keyUp)
    {
        return new INPUT
        {
            type = InputKeyboard,
            U = new InputUnion
            {
                ki = new KEYBDINPUT
                {
                    wVk = virtualKey,
                    dwFlags = keyUp ? KeyEventKeyUp : 0
                }
            }
        };
    }

    private const int InputKeyboard = 1;
    private const ushort VirtualKeyControl = 0x11;
    private const ushort VirtualKeyV = 0x56;
    private const uint KeyEventKeyUp = 0x0002;

    [DllImport("user32.dll", SetLastError = true)]
    private static extern uint SendInput(uint nInputs, INPUT[] pInputs, int cbSize);

    private sealed record ClipboardSnapshot(IDataObject? Data, string? Text, bool HadData)
    {
        public static ClipboardSnapshot Capture()
        {
            return ClipboardStaDispatcher.Invoke(() =>
            {
                var data = Clipboard.GetDataObject();
                var text = Clipboard.ContainsText() ? Clipboard.GetText() : null;
                return new ClipboardSnapshot(CloneDataObject(data) ?? data, text, data is not null);
            });
        }

        public Task RestoreAsync(CancellationToken cancellationToken)
        {
            return ClipboardStaDispatcher.InvokeAsync(() =>
            {
                if (Data is not null)
                {
                    Clipboard.SetDataObject(Data, copy: true);
                    return;
                }

                if (Text is not null)
                {
                    Clipboard.SetText(Text);
                    return;
                }

                if (!HadData)
                {
                    Clipboard.Clear();
                }
            }, cancellationToken);
        }

        private static DataObject? CloneDataObject(IDataObject? source)
        {
            if (source is null)
            {
                return null;
            }

            var clone = new DataObject();
            var copiedAnyFormat = false;

            foreach (var format in source.GetFormats(autoConvert: false))
            {
                try
                {
                    var data = source.GetData(format, autoConvert: false);
                    if (data is not null)
                    {
                        clone.SetData(format, autoConvert: false, data);
                        copiedAnyFormat = true;
                    }
                }
                catch (ExternalException)
                {
                }
            }

            return copiedAnyFormat ? clone : null;
        }
    }

    private static class ClipboardStaDispatcher
    {
        private static readonly BlockingCollection<WorkItem> Queue = [];

        static ClipboardStaDispatcher()
        {
            var thread = new Thread(Run)
            {
                IsBackground = true,
                Name = "VoiceInk Clipboard STA"
            };
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
        }

        public static void Invoke(Action action)
        {
            Invoke<object?>(() =>
            {
                action();
                return null;
            });
        }

        public static T Invoke<T>(Func<T> action)
        {
            return InvokeAsync(action, CancellationToken.None).GetAwaiter().GetResult();
        }

        public static Task InvokeAsync(Action action, CancellationToken cancellationToken)
        {
            return InvokeAsync<object?>(() =>
            {
                action();
                return null;
            }, cancellationToken);
        }

        private static Task<T> InvokeAsync<T>(Func<T> action, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return Task.FromCanceled<T>(cancellationToken);
            }

            var completion = new TaskCompletionSource<T>(TaskCreationOptions.RunContinuationsAsynchronously);
            Queue.Add(new WorkItem(
                () =>
                {
                    try
                    {
                        completion.SetResult(action());
                    }
                    catch (Exception exception)
                    {
                        completion.SetException(exception);
                    }
                },
                cancellationToken,
                () => completion.TrySetCanceled(cancellationToken)));

            return completion.Task;
        }

        private static void Run()
        {
            foreach (var workItem in Queue.GetConsumingEnumerable())
            {
                if (workItem.CancellationToken.IsCancellationRequested)
                {
                    workItem.Cancel();
                    continue;
                }

                workItem.Execute();
            }
        }

        private sealed record WorkItem(Action Execute, CancellationToken CancellationToken, Action Cancel);
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct INPUT
    {
        public int type;
        public InputUnion U;
    }

    [StructLayout(LayoutKind.Explicit)]
    private struct InputUnion
    {
        [FieldOffset(0)]
        public MOUSEINPUT mi;

        [FieldOffset(0)]
        public KEYBDINPUT ki;

        [FieldOffset(0)]
        public HARDWAREINPUT hi;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct MOUSEINPUT
    {
        public int dx;
        public int dy;
        public uint mouseData;
        public uint dwFlags;
        public uint time;
        public IntPtr dwExtraInfo;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct KEYBDINPUT
    {
        public ushort wVk;
        public ushort wScan;
        public uint dwFlags;
        public uint time;
        public IntPtr dwExtraInfo;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct HARDWAREINPUT
    {
        public uint uMsg;
        public ushort wParamL;
        public ushort wParamH;
    }
}
