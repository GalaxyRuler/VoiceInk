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
        AppDomain.CurrentDomain.UnhandledException += (_, args) => LogStartupFailure(args.ExceptionObject as Exception);
        UnhandledException += (_, args) => LogStartupFailure(args.Exception);
        TaskScheduler.UnobservedTaskException += (_, args) => LogStartupFailure(args.Exception);

        try
        {
            InitializeComponent();
        }
        catch (Exception ex)
        {
            LogStartupFailure(ex);
            throw;
        }
    }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        try
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
        catch (Exception ex)
        {
            LogStartupFailure(ex);
            throw;
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

    private static void LogStartupFailure(Exception? exception)
    {
        if (exception is null)
        {
            return;
        }

        try
        {
            var logDirectory = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "VoiceInk");
            Directory.CreateDirectory(logDirectory);
            var logPath = Path.Combine(logDirectory, "startup-crash.log");
            File.AppendAllText(
                logPath,
                $"{DateTimeOffset.UtcNow:O}{Environment.NewLine}{exception}{Environment.NewLine}{Environment.NewLine}");
        }
        catch
        {
            // Startup crash logging must never mask the original failure.
        }
    }
}
