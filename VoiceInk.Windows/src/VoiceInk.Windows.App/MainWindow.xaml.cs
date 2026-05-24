using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using VoiceInk.Windows.Core.Dictation;
using VoiceInk.Windows.Core.Settings;
using VoiceInk.Windows.Infrastructure.History;
using VoiceInk.Windows.Infrastructure.Settings;
using VoiceInk.Windows.Native.Audio;
using VoiceInk.Windows.Native.Text;
using VoiceInk.Windows.Native.Transcription;

namespace VoiceInk.Windows.App;

public sealed partial class MainWindow : Window
{
    private readonly JsonSettingsStore settingsStore;
    private readonly DictationController controller;

    public MainWindow()
    {
        InitializeComponent();

        var appData = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "VoiceInk.Windows");
        var recordings = Path.Combine(appData, "Recordings");

        settingsStore = new JsonSettingsStore(Path.Combine(appData, "settings.json"));
        controller = new DictationController(
            new NAudioCaptureService(recordings),
            new WhisperNetTranscriptionService(),
            new ClipboardTextInjectionService(restoreClipboard: true),
            new SqliteHistoryStore(Path.Combine(appData, "history.db")),
            settingsStore);

        _ = LoadSettingsAsync();
    }

    private async void StartButton_Click(object sender, RoutedEventArgs e)
    {
        await SaveSettingsAsync();
        await controller.StartAsync(CancellationToken.None);
        StatusTextBlock.Text = "Recording";
        StartButton.IsEnabled = false;
        StopButton.IsEnabled = true;
    }

    private async void StopButton_Click(object sender, RoutedEventArgs e)
    {
        await controller.StopAsync(CancellationToken.None);
        StatusTextBlock.Text = controller.LastError ?? controller.State.ToString();
        StartButton.IsEnabled = true;
        StopButton.IsEnabled = false;
    }

    private async Task LoadSettingsAsync()
    {
        var settings = await settingsStore.LoadAsync(CancellationToken.None);
        ModelPathTextBox.Text = settings.ModelPath;
    }

    private async Task SaveSettingsAsync()
    {
        var settings = await settingsStore.LoadAsync(CancellationToken.None);
        await settingsStore.SaveAsync(settings with
        {
            ModelPath = ModelPathTextBox.Text
        }, CancellationToken.None);
    }
}
