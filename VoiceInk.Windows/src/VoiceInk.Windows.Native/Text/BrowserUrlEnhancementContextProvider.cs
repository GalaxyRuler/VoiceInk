using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Automation;
using VoiceInk.Windows.Core.Enhancement;

namespace VoiceInk.Windows.Native.Text;

public sealed class BrowserUrlEnhancementContextProvider : IBrowserUrlReader
{
    private static readonly TimeSpan DefaultTimeout = TimeSpan.FromMilliseconds(500);
    private static readonly HashSet<string> BrowserProcessNames = new(StringComparer.OrdinalIgnoreCase)
    {
        "arc",
        "brave",
        "chrome",
        "firefox",
        "msedge",
        "opera",
        "vivaldi"
    };

    private readonly TimeSpan timeout;

    public BrowserUrlEnhancementContextProvider(TimeSpan? timeout = null)
    {
        this.timeout = timeout is { } value && value > TimeSpan.Zero
            ? value
            : DefaultTimeout;
    }

    public async Task<string> GetBrowserUrlAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var readTask = Task.Run(() => ReadBrowserUrl(cancellationToken), cancellationToken);
        var timeoutTask = Task.Delay(timeout, cancellationToken);
        var completedTask = await Task.WhenAny(readTask, timeoutTask);
        if (completedTask == readTask)
        {
            return await readTask;
        }

        cancellationToken.ThrowIfCancellationRequested();
        return string.Empty;
    }

    private static string ReadBrowserUrl(CancellationToken cancellationToken)
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            var windowHandle = GetForegroundWindow();
            if (windowHandle == IntPtr.Zero)
            {
                return string.Empty;
            }

            _ = GetWindowThreadProcessId(windowHandle, out var processId);
            if (!IsSupportedBrowserProcess(processId))
            {
                return string.Empty;
            }

            var root = AutomationElement.FromHandle(windowHandle);
            if (root is null)
            {
                return string.Empty;
            }

            var edits = root.FindAll(
                TreeScope.Descendants,
                new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Edit));
            foreach (AutomationElement edit in edits)
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (!edit.TryGetCurrentPattern(ValuePattern.Pattern, out var patternObject)
                    || patternObject is not ValuePattern valuePattern)
                {
                    continue;
                }

                var sanitized = BrowserUrlContextSanitizer.Sanitize(valuePattern.Current.Value);
                if (!string.IsNullOrEmpty(sanitized))
                {
                    return sanitized;
                }
            }

            return string.Empty;
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

    private static bool IsSupportedBrowserProcess(uint processId)
    {
        if (processId == 0)
        {
            return false;
        }

        try
        {
            using var process = Process.GetProcessById((int)processId);
            return BrowserProcessNames.Contains(process.ProcessName);
        }
        catch (ArgumentException)
        {
            return false;
        }
        catch (InvalidOperationException)
        {
            return false;
        }
        catch (System.ComponentModel.Win32Exception)
        {
            return false;
        }
    }

    [DllImport("user32.dll")]
    private static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll")]
    private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);
}
