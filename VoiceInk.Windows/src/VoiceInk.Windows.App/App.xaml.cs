using Microsoft.UI.Xaml;
using VoiceInk.Windows.Core.Startup;

namespace VoiceInk.Windows.App;

public partial class App : Application
{
    private Window? window;

    public App()
    {
        InitializeComponent();
    }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        var isLoginStartup = StartupLaunchMode.IsLoginStartup(Environment.GetCommandLineArgs().Skip(1));
        window = new MainWindow(isLoginStartup);
        if (!isLoginStartup)
        {
            window.Activate();
        }
    }
}
