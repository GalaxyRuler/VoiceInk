using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace VoiceInk.Windows.Native.Hotkeys;

public sealed class GlobalHotkeyService : IDisposable
{
    private const int HotkeyId = 0x5649;
    private const int WmHotkey = 0x0312;
    private const uint ModAlt = 0x0001;
    private const uint ModControl = 0x0002;
    private const uint VkSpace = 0x20;

    private readonly HotkeyWindow hotkeyWindow;
    private bool registered;
    private bool disposed;

    public GlobalHotkeyService(IntPtr windowHandle)
    {
        if (windowHandle == IntPtr.Zero)
        {
            throw new ArgumentException("A valid window handle is required.", nameof(windowHandle));
        }

        hotkeyWindow = new HotkeyWindow(this);
        hotkeyWindow.AssignHandle(windowHandle);
    }

    public event EventHandler? HotkeyPressed;

    public void RegisterCtrlAltSpace()
    {
        ObjectDisposedException.ThrowIf(disposed, this);

        if (registered)
        {
            return;
        }

        if (!RegisterHotKey(hotkeyWindow.Handle, HotkeyId, ModControl | ModAlt, VkSpace))
        {
            var errorCode = Marshal.GetLastWin32Error();
            var error = new Win32Exception(errorCode);
            throw new InvalidOperationException(
                $"RegisterHotKey failed for Ctrl+Alt+Space ({errorCode}: {error.Message}).",
                error);
        }

        registered = true;
    }

    public void Dispose()
    {
        if (disposed)
        {
            return;
        }

        if (registered)
        {
            UnregisterHotKey(hotkeyWindow.Handle, HotkeyId);
            registered = false;
        }

        hotkeyWindow.ReleaseHandle();
        disposed = true;
    }

    private void OnHotkeyPressed()
    {
        HotkeyPressed?.Invoke(this, EventArgs.Empty);
    }

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

    private sealed class HotkeyWindow(GlobalHotkeyService owner) : NativeWindow
    {
        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WmHotkey && m.WParam.ToInt32() == HotkeyId)
            {
                owner.OnHotkeyPressed();
                return;
            }

            base.WndProc(ref m);
        }
    }
}
