using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using VoiceInk.Windows.Core.Shortcuts;

namespace VoiceInk.Windows.Native.Hotkeys;

public sealed class GlobalHotkeyService : IDisposable
{
    private const int HotkeyIdBase = 0x5649;
    private const int WmHotkey = 0x0312;
    private const uint ModAlt = 0x0001;
    private const uint ModControl = 0x0002;
    private const uint ModShift = 0x0004;
    private const uint ModNoRepeat = 0x4000;

    private readonly HotkeyWindow hotkeyWindow;
    private readonly Dictionary<int, GlobalShortcutAction> registeredActions = [];
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

    public event EventHandler<GlobalHotkeyPressedEventArgs>? HotkeyPressed;

    public void RegisterHotkeys(IEnumerable<GlobalShortcutRegistration> registrations)
    {
        ObjectDisposedException.ThrowIf(disposed, this);

        UnregisterHotkeys();

        var nextId = HotkeyIdBase;
        try
        {
            foreach (var registration in registrations)
            {
                var id = nextId++;
                if (!RegisterHotKey(
                        hotkeyWindow.Handle,
                        id,
                        ModifierMask(registration.Shortcut),
                        (uint)registration.Shortcut.VirtualKey))
                {
                    var errorCode = Marshal.GetLastWin32Error();
                    var error = new Win32Exception(errorCode);
                    throw new InvalidOperationException(
                        $"RegisterHotKey failed for {registration.Action} ({registration.Shortcut.DisplayText}) ({errorCode}: {error.Message}).",
                        error);
                }

                registeredActions.Add(id, registration.Action);
            }
        }
        catch
        {
            UnregisterHotkeys();
            throw;
        }
    }

    public void Dispose()
    {
        if (disposed)
        {
            return;
        }

        UnregisterHotkeys();

        hotkeyWindow.ReleaseHandle();
        disposed = true;
    }

    private void OnHotkeyPressed(GlobalShortcutAction action)
    {
        HotkeyPressed?.Invoke(this, new GlobalHotkeyPressedEventArgs(action));
    }

    private void UnregisterHotkeys()
    {
        foreach (var id in registeredActions.Keys)
        {
            UnregisterHotKey(hotkeyWindow.Handle, id);
        }

        registeredActions.Clear();
    }

    private static uint ModifierMask(GlobalShortcut shortcut)
    {
        var mask = ModNoRepeat;
        if (shortcut.Control)
        {
            mask |= ModControl;
        }

        if (shortcut.Alt)
        {
            mask |= ModAlt;
        }

        if (shortcut.Shift)
        {
            mask |= ModShift;
        }

        return mask;
    }

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

    private sealed class HotkeyWindow(GlobalHotkeyService owner) : NativeWindow
    {
        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WmHotkey
                && owner.registeredActions.TryGetValue(m.WParam.ToInt32(), out var action))
            {
                owner.OnHotkeyPressed(action);
                return;
            }

            base.WndProc(ref m);
        }
    }
}

public sealed class GlobalHotkeyPressedEventArgs(GlobalShortcutAction action) : EventArgs
{
    public GlobalShortcutAction Action { get; } = action;
}
