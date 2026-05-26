using System.ComponentModel;
using System.Runtime.InteropServices;
using VoiceInk.Windows.Core.PowerMode;
using VoiceInk.Windows.Core.Services;

namespace VoiceInk.Windows.Native.Text;

public sealed class PowerModeAutoSendService : IPowerModeAutoSendService
{
    public async Task SendAsync(PowerModeAutoSendKey key, CancellationToken cancellationToken)
    {
        if (key == PowerModeAutoSendKey.None)
        {
            return;
        }

        await Task.Delay(TimeSpan.FromMilliseconds(500), cancellationToken);
        INPUT[] inputs = key switch
        {
            PowerModeAutoSendKey.Enter =>
            [
                KeyboardInput(VirtualKeyReturn, keyUp: false),
                KeyboardInput(VirtualKeyReturn, keyUp: true)
            ],
            PowerModeAutoSendKey.ShiftEnter =>
            [
                KeyboardInput(VirtualKeyShift, keyUp: false),
                KeyboardInput(VirtualKeyReturn, keyUp: false),
                KeyboardInput(VirtualKeyReturn, keyUp: true),
                KeyboardInput(VirtualKeyShift, keyUp: true)
            ],
            PowerModeAutoSendKey.CommandEnter =>
            [
                KeyboardInput(VirtualKeyControl, keyUp: false),
                KeyboardInput(VirtualKeyReturn, keyUp: false),
                KeyboardInput(VirtualKeyReturn, keyUp: true),
                KeyboardInput(VirtualKeyControl, keyUp: true)
            ],
            _ => []
        };

        if (inputs.Length == 0)
        {
            return;
        }

        var sent = SendInput((uint)inputs.Length, inputs, Marshal.SizeOf<INPUT>());
        if (sent != inputs.Length)
        {
            throw new InvalidOperationException(
                "Windows did not accept the Power Mode auto-send keyboard input.",
                new Win32Exception(Marshal.GetLastPInvokeError()));
        }
    }

    private static INPUT KeyboardInput(ushort virtualKey, bool keyUp) =>
        new()
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

    private const int InputKeyboard = 1;
    private const ushort VirtualKeyShift = 0x10;
    private const ushort VirtualKeyControl = 0x11;
    private const ushort VirtualKeyReturn = 0x0D;
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
        public nuint dwExtraInfo;
    }
}
