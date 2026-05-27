using Xunit;

namespace VoiceInk.Windows.Core.Tests.Shell;

public sealed class AppLifecycleStaticTests
{
    [Fact]
    public void App_RegistersStableSingleInstanceAndRedirectsDuplicateActivations()
    {
        var code = File.ReadAllText(SourcePath("VoiceInk.Windows", "src", "VoiceInk.Windows.App", "App.xaml.cs"));

        Assert.Contains("Microsoft.Windows.AppLifecycle", code);
        Assert.Contains("FindOrRegisterForKey", code);
        Assert.Contains("VoiceInk.Windows.App", code);
        Assert.Contains("IsCurrent", code);
        Assert.Contains("RedirectActivationToAsync", code);
        Assert.Contains("GetActivatedEventArgs", code);
        Assert.Contains("Environment.Exit(0)", code);
    }

    [Fact]
    public void App_RestoresExistingWindowWhenActivationIsRedirected()
    {
        var appCode = File.ReadAllText(SourcePath("VoiceInk.Windows", "src", "VoiceInk.Windows.App", "App.xaml.cs"));
        var mainWindowCode = File.ReadAllText(SourcePath("VoiceInk.Windows", "src", "VoiceInk.Windows.App", "MainWindow.xaml.cs"));

        Assert.Contains("Activated +=", appCode);
        Assert.Contains("RestoreFromExternalActivation", appCode);
        Assert.Contains("internal void RestoreFromExternalActivation()", mainWindowCode);
        Assert.Contains("RestoreAndActivateWindow();", mainWindowCode);
    }

    private static string SourcePath(params string[] parts)
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            var candidate = Path.Combine(new[] { directory.FullName }.Concat(parts).ToArray());
            if (File.Exists(candidate))
            {
                return candidate;
            }

            directory = directory.Parent;
        }

        return Path.Combine(parts);
    }
}
