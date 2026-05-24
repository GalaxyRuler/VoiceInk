using System.Runtime.InteropServices;
using System.Windows.Forms;
using VoiceInk.Windows.Core.Services;

namespace VoiceInk.Windows.Native.Text;

public sealed class ClipboardTextInjectionService(bool restoreClipboard) : ITextInjectionService
{
    public async Task InsertAsync(string text, CancellationToken cancellationToken)
    {
        var previousText = Clipboard.ContainsText() ? Clipboard.GetText() : null;

        Clipboard.SetText(text);
        await Task.Delay(TimeSpan.FromMilliseconds(80), cancellationToken);
        SendCtrlV();

        if (restoreClipboard && previousText is not null)
        {
            await Task.Delay(TimeSpan.FromMilliseconds(400), cancellationToken);
            Clipboard.SetText(previousText);
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
            throw new InvalidOperationException("Windows did not accept the paste keyboard input.");
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
        public KEYBDINPUT ki;
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
}
