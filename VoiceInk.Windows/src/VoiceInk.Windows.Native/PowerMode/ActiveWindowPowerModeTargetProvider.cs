using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using VoiceInk.Windows.Core.Enhancement;
using VoiceInk.Windows.Core.PowerMode;
using VoiceInk.Windows.Core.Services;
using VoiceInk.Windows.Native.Text;

namespace VoiceInk.Windows.Native.PowerMode;

public sealed class ActiveWindowPowerModeTargetProvider : IPowerModeTargetProvider
{
    private const int TitleBufferLength = 512;
    private readonly int currentProcessId = Environment.ProcessId;
    private readonly HashSet<nint> excludedWindowHandles = [];
    private readonly IBrowserUrlReader browserUrlReader;
    private PowerModeTarget? lastExternalTarget;

    public ActiveWindowPowerModeTargetProvider(IBrowserUrlReader? browserUrlReader = null)
    {
        this.browserUrlReader = browserUrlReader ?? new BrowserUrlEnhancementContextProvider();
    }

    public void ExcludeWindowHandle(IntPtr windowHandle)
    {
        if (windowHandle != IntPtr.Zero)
        {
            excludedWindowHandles.Add(windowHandle);
        }
    }

    public async Task<PowerModeTarget?> GetCurrentTargetAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var windowHandle = GetForegroundWindow();
        if (windowHandle == IntPtr.Zero)
        {
            return null;
        }

        _ = GetWindowThreadProcessId(windowHandle, out var processId);
        var title = ReadWindowTitle(windowHandle);
        var processName = ReadProcessName(processId);

        if (IsExcludedWindow(windowHandle, processId))
        {
            return lastExternalTarget;
        }

        if (string.IsNullOrWhiteSpace(processName) && string.IsNullOrWhiteSpace(title))
        {
            return null;
        }

        var browserUrl = await ReadBrowserUrlAsync(cancellationToken);
        lastExternalTarget = new PowerModeTarget(
            processName,
            title,
            processId == 0 ? null : (int)processId,
            browserUrl);
        return lastExternalTarget;
    }

    private bool IsExcludedWindow(IntPtr windowHandle, uint processId)
    {
        return excludedWindowHandles.Contains(windowHandle)
            || processId == currentProcessId;
    }

    private static string ReadWindowTitle(IntPtr windowHandle)
    {
        var buffer = new StringBuilder(TitleBufferLength);
        var length = GetWindowTextW(windowHandle, buffer, buffer.Capacity);
        return length <= 0 ? string.Empty : buffer.ToString(0, length);
    }

    private static string ReadProcessName(uint processId)
    {
        if (processId == 0)
        {
            return string.Empty;
        }

        try
        {
            using var process = Process.GetProcessById((int)processId);
            return process.ProcessName;
        }
        catch (ArgumentException)
        {
            return string.Empty;
        }
        catch (InvalidOperationException)
        {
            return string.Empty;
        }
        catch (System.ComponentModel.Win32Exception)
        {
            return string.Empty;
        }
    }

    private async Task<string> ReadBrowserUrlAsync(CancellationToken cancellationToken)
    {
        try
        {
            var browserUrl = await browserUrlReader.GetBrowserUrlAsync(cancellationToken);
            return BrowserUrlContextSanitizer.Sanitize(browserUrl);
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

    [DllImport("user32.dll")]
    private static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll")]
    private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

    [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern int GetWindowTextW(IntPtr hWnd, StringBuilder lpString, int nMaxCount);
}
