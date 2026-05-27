using System.Runtime.InteropServices;
using VoiceInk.Windows.Core.PowerMode;

namespace VoiceInk.Windows.Native.PowerMode;

public sealed class StartMenuInstalledApplicationProvider : IInstalledApplicationProvider
{
    public Task<IReadOnlyList<PowerModeInstalledApplicationChoice>> GetInstalledApplicationsAsync(
        CancellationToken cancellationToken) =>
        Task.Run(() => ReadInstalledApplications(cancellationToken), cancellationToken);

    private static IReadOnlyList<PowerModeInstalledApplicationChoice> ReadInstalledApplications(
        CancellationToken cancellationToken)
    {
        var choices = new List<PowerModeInstalledApplicationChoice>();
        foreach (var directory in StartMenuProgramDirectories())
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (!Directory.Exists(directory))
            {
                continue;
            }

            foreach (var shortcutPath in Directory.EnumerateFiles(directory, "*.lnk", SearchOption.AllDirectories))
            {
                cancellationToken.ThrowIfCancellationRequested();
                var targetPath = TryReadShortcutTarget(shortcutPath);
                var processName = ProcessNameFromTarget(targetPath);
                if (string.IsNullOrWhiteSpace(processName))
                {
                    continue;
                }

                choices.Add(new PowerModeInstalledApplicationChoice(
                    Path.GetFileNameWithoutExtension(shortcutPath),
                    processName,
                    "Start Menu"));
            }
        }

        return PowerModeInstalledApplicationPresenter.Present(choices);
    }

    private static IEnumerable<string> StartMenuProgramDirectories()
    {
        var common = Environment.GetFolderPath(Environment.SpecialFolder.CommonStartMenu);
        if (!string.IsNullOrWhiteSpace(common))
        {
            yield return Path.Combine(common, "Programs");
        }

        var user = Environment.GetFolderPath(Environment.SpecialFolder.StartMenu);
        if (!string.IsNullOrWhiteSpace(user))
        {
            yield return Path.Combine(user, "Programs");
        }
    }

    private static string TryReadShortcutTarget(string shortcutPath)
    {
        object? shell = null;
        object? shortcut = null;
        try
        {
            var shellType = Type.GetTypeFromProgID("WScript.Shell");
            if (shellType is null)
            {
                return string.Empty;
            }

            shell = Activator.CreateInstance(shellType);
            if (shell is null)
            {
                return string.Empty;
            }

            shortcut = shellType.InvokeMember(
                "CreateShortcut",
                System.Reflection.BindingFlags.InvokeMethod,
                binder: null,
                target: shell,
                args: [shortcutPath]);
            var targetPath = shortcut?.GetType().InvokeMember(
                "TargetPath",
                System.Reflection.BindingFlags.GetProperty,
                binder: null,
                target: shortcut,
                args: null) as string;
            return targetPath ?? string.Empty;
        }
        catch
        {
            return string.Empty;
        }
        finally
        {
            ReleaseComObject(shortcut);
            ReleaseComObject(shell);
        }
    }

    private static string ProcessNameFromTarget(string targetPath)
    {
        if (string.IsNullOrWhiteSpace(targetPath)
            || !targetPath.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
        {
            return string.Empty;
        }

        return Path.GetFileNameWithoutExtension(targetPath);
    }

    private static void ReleaseComObject(object? value)
    {
        if (value is not null && Marshal.IsComObject(value))
        {
            Marshal.FinalReleaseComObject(value);
        }
    }
}
