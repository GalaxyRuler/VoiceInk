using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using VoiceInk.Windows.Core.Shortcuts;

namespace VoiceInk.Windows.Native.Hotkeys;

public sealed class GlobalHotkeyService : IDisposable
{
    private const int HotkeyIdBase = 0x5649;
    private const int WhKeyboardLl = 13;
    private const int WmHotkey = 0x0312;
    private const int WmKeydown = 0x0100;
    private const int WmKeyup = 0x0101;
    private const int WmSyskeydown = 0x0104;
    private const int WmSyskeyup = 0x0105;
    private const int VkShift = 0x10;
    private const int VkControl = 0x11;
    private const int VkMenu = 0x12;
    private const uint ModAlt = 0x0001;
    private const uint ModControl = 0x0002;
    private const uint ModShift = 0x0004;
    private const uint ModNoRepeat = 0x4000;

    private readonly HotkeyWindow hotkeyWindow;
    private readonly Dictionary<int, GlobalShortcutRegistration> registeredActions = [];
    private readonly Dictionary<string, GlobalShortcutRegistration> pressedRecordingShortcuts = new(StringComparer.OrdinalIgnoreCase);
    private readonly LowLevelKeyboardProc keyboardProc;
    private IReadOnlyList<GlobalShortcutRegistration> recordingRegistrations = [];
    private IntPtr keyboardHook;
    private bool disposed;

    public GlobalHotkeyService(IntPtr windowHandle)
    {
        if (windowHandle == IntPtr.Zero)
        {
            throw new ArgumentException("A valid window handle is required.", nameof(windowHandle));
        }

        hotkeyWindow = new HotkeyWindow(this);
        hotkeyWindow.AssignHandle(windowHandle);
        keyboardProc = KeyboardHookCallback;
    }

    public event EventHandler<GlobalHotkeyPressedEventArgs>? HotkeyPressed;

    public void RegisterHotkeys(IEnumerable<GlobalShortcutRegistration> registrations)
    {
        ObjectDisposedException.ThrowIf(disposed, this);

        UnregisterHotkeys();

        var nextId = HotkeyIdBase;
        try
        {
            recordingRegistrations = registrations
                .Where(registration => registration.Action == GlobalShortcutAction.ToggleRecording)
                .ToArray();

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

                registeredActions.Add(id, registration);
            }

            if (recordingRegistrations.Count > 0)
            {
                InstallKeyboardHook();
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

    private void OnHotkeyPressed(
        GlobalShortcutRegistration registration,
        GlobalHotkeyTransition transition = GlobalHotkeyTransition.Pressed)
    {
        HotkeyPressed?.Invoke(
            this,
            new GlobalHotkeyPressedEventArgs(
                registration.Action,
                registration.PowerModeRuleId,
                transition,
                registration.RecordingShortcutMode));
    }

    private void UnregisterHotkeys()
    {
        UninstallKeyboardHook();
        recordingRegistrations = [];
        pressedRecordingShortcuts.Clear();

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

    private void InstallKeyboardHook()
    {
        if (keyboardHook != IntPtr.Zero)
        {
            return;
        }

        keyboardHook = SetWindowsHookEx(WhKeyboardLl, keyboardProc, GetModuleHandle(null), 0);
        if (keyboardHook == IntPtr.Zero)
        {
            var errorCode = Marshal.GetLastWin32Error();
            var error = new Win32Exception(errorCode);
            throw new InvalidOperationException(
                $"SetWindowsHookEx failed for recording shortcut key-up handling ({errorCode}: {error.Message}).",
                error);
        }
    }

    private void UninstallKeyboardHook()
    {
        if (keyboardHook == IntPtr.Zero)
        {
            return;
        }

        UnhookWindowsHookEx(keyboardHook);
        keyboardHook = IntPtr.Zero;
    }

    private IntPtr KeyboardHookCallback(int nCode, IntPtr wParam, IntPtr lParam)
    {
        if (nCode >= 0)
        {
            var message = wParam.ToInt32();
            var hookInfo = Marshal.PtrToStructure<Kbdllhookstruct>(lParam);

            if (message is WmKeydown or WmSyskeydown)
            {
                HandleRecordingKeyDown((int)hookInfo.VirtualKeyCode);
            }
            else if (message is WmKeyup or WmSyskeyup)
            {
                HandleRecordingKeyUp((int)hookInfo.VirtualKeyCode);
            }
        }

        return CallNextHookEx(keyboardHook, nCode, wParam, lParam);
    }

    private void HandleRecordingKeyDown(int virtualKey)
    {
        var registration = recordingRegistrations.FirstOrDefault(registration =>
            registration.Shortcut.VirtualKey == virtualKey
            && AreModifiersPressed(registration.Shortcut));
        if (registration is null)
        {
            return;
        }

        var key = registration.Shortcut.DisplayText;
        if (pressedRecordingShortcuts.ContainsKey(key))
        {
            return;
        }

        pressedRecordingShortcuts[key] = registration;
        OnHotkeyPressed(registration);
    }

    private void HandleRecordingKeyUp(int virtualKey)
    {
        var released = pressedRecordingShortcuts
            .Where(pair => pair.Value.Shortcut.VirtualKey == virtualKey)
            .Select(pair => pair.Key)
            .ToArray();

        foreach (var key in released)
        {
            var registration = pressedRecordingShortcuts[key];
            pressedRecordingShortcuts.Remove(key);
            OnHotkeyPressed(registration, GlobalHotkeyTransition.Released);
        }
    }

    private static bool AreModifiersPressed(GlobalShortcut shortcut) =>
        IsKeyPressed(VkControl) == shortcut.Control
        && IsKeyPressed(VkMenu) == shortcut.Alt
        && IsKeyPressed(VkShift) == shortcut.Shift;

    private static bool IsKeyPressed(int virtualKey) =>
        (GetAsyncKeyState(virtualKey) & 0x8000) != 0;

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool UnhookWindowsHookEx(IntPtr hhk);

    [DllImport("user32.dll")]
    private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

    [DllImport("user32.dll")]
    private static extern short GetAsyncKeyState(int vKey);

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern IntPtr GetModuleHandle(string? lpModuleName);

    private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

    [StructLayout(LayoutKind.Sequential)]
    private readonly struct Kbdllhookstruct
    {
        public readonly uint VirtualKeyCode;
        public readonly uint ScanCode;
        public readonly uint Flags;
        public readonly uint Time;
        public readonly IntPtr ExtraInfo;
    }

    private sealed class HotkeyWindow(GlobalHotkeyService owner) : NativeWindow
    {
        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WmHotkey
                && owner.registeredActions.TryGetValue(m.WParam.ToInt32(), out var registration))
            {
                if (registration.Action != GlobalShortcutAction.ToggleRecording)
                {
                    owner.OnHotkeyPressed(registration);
                }

                return;
            }

            base.WndProc(ref m);
        }
    }
}

public sealed class GlobalHotkeyPressedEventArgs(
    GlobalShortcutAction action,
    Guid? powerModeRuleId = null,
    GlobalHotkeyTransition transition = GlobalHotkeyTransition.Pressed,
    string? recordingShortcutMode = null) : EventArgs
{
    public GlobalShortcutAction Action { get; } = action;
    public Guid? PowerModeRuleId { get; } = powerModeRuleId;
    public GlobalHotkeyTransition Transition { get; } = transition;
    public string? RecordingShortcutMode { get; } = recordingShortcutMode;
}

public enum GlobalHotkeyTransition
{
    Pressed,
    Released
}
