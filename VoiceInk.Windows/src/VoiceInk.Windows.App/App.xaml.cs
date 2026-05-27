using Microsoft.Windows.AppLifecycle;
using Microsoft.UI.Xaml;
using VoiceInk.Windows.Core.Startup;

namespace VoiceInk.Windows.App;

public partial class App : Application
{
    private const string SingleInstanceKey = "VoiceInk.Windows.App";

    private AppInstance? mainInstance;
    private Window? window;

    public App()
    {
        InitializeComponent();
    }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        if (RedirectDuplicateActivation())
        {
            return;
        }

        var isLoginStartup = StartupLaunchMode.IsLoginStartup(Environment.GetCommandLineArgs().Skip(1));
        window = new MainWindow(isLoginStartup);
        if (!isLoginStartup)
        {
            window.Activate();
        }
    }

    private bool RedirectDuplicateActivation()
    {
        mainInstance = AppInstance.FindOrRegisterForKey(SingleInstanceKey);
        if (!mainInstance.IsCurrent)
        {
            var activationArgs = AppInstance.GetCurrent().GetActivatedEventArgs();
            mainInstance.RedirectActivationToAsync(activationArgs).AsTask().GetAwaiter().GetResult();
            Environment.Exit(0);
            return true;
        }

        mainInstance.Activated += MainInstance_Activated;
        return false;
    }

    private void MainInstance_Activated(object? sender, AppActivationArguments args)
    {
        if (window is MainWindow mainWindow)
        {
            mainWindow.DispatcherQueue.TryEnqueue(mainWindow.RestoreFromExternalActivation);
        }
    }
}
