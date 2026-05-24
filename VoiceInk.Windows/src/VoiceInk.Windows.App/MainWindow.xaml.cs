using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
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
    private readonly string recordingsDirectory;
    private readonly string historyPath;
    private readonly JsonSettingsStore settingsStore;
    private readonly CancellationTokenSource windowLifetime = new();
    private NAudioCaptureService audioCapture;
    private DictationController controller;
    private bool isStarting;
    private bool isStopping;
    private bool settingsLoaded;
    private bool modelPathEdited;
    private bool suppressModelPathChanged;

    public MainWindow()
    {
        InitializeComponent();

        var appData = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "VoiceInk.Windows");
        recordingsDirectory = Path.Combine(appData, "Recordings");
        historyPath = Path.Combine(appData, "history.db");

        settingsStore = new JsonSettingsStore(Path.Combine(appData, "settings.json"));
        audioCapture = new NAudioCaptureService(recordingsDirectory);
        controller = CreateController(audioCapture);

        Closed += MainWindow_Closed;
        RefreshUiFromControllerState("Loading settings");
        _ = InitializeAsync();
    }

    private async void StartButton_Click(object sender, RoutedEventArgs e)
    {
        if (isStarting || isStopping || !settingsLoaded)
        {
            return;
        }

        var statusOverride = "Starting recording";
        isStarting = true;
        RefreshUiFromControllerState(statusOverride);

        try
        {
            if (string.IsNullOrWhiteSpace(ModelPathTextBox.Text))
            {
                statusOverride = "Local whisper model path is required.";
                return;
            }

            if (controller.State == DictationState.Error)
            {
                RecreateController();
            }

            await SaveSettingsAsync(windowLifetime.Token);
            await controller.StartAsync(windowLifetime.Token);
            statusOverride = null;
        }
        catch (OperationCanceledException) when (windowLifetime.IsCancellationRequested)
        {
            statusOverride = "Closing";
        }
        catch (Exception ex)
        {
            statusOverride = $"Start failed: {ex.Message}";
        }
        finally
        {
            isStarting = false;
            RefreshUiFromControllerState(statusOverride);
        }
    }

    private async void StopButton_Click(object sender, RoutedEventArgs e)
    {
        if (isStarting || isStopping || controller.State != DictationState.Recording)
        {
            return;
        }

        var statusOverride = "Stopping and inserting";
        isStopping = true;
        RefreshUiFromControllerState(statusOverride);

        try
        {
            await controller.StopAsync(windowLifetime.Token);
            statusOverride = null;
        }
        catch (OperationCanceledException) when (windowLifetime.IsCancellationRequested)
        {
            statusOverride = "Closing";
        }
        catch (Exception ex)
        {
            statusOverride = $"Stop failed: {ex.Message}";
        }
        finally
        {
            isStopping = false;
            RefreshUiFromControllerState(statusOverride);
        }
    }

    private void ModelPathTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (!suppressModelPathChanged)
        {
            modelPathEdited = true;
        }
    }

    private async Task InitializeAsync()
    {
        try
        {
            var settings = await settingsStore.LoadAsync(windowLifetime.Token);
            if (!modelPathEdited)
            {
                suppressModelPathChanged = true;
                ModelPathTextBox.Text = settings.ModelPath;
                suppressModelPathChanged = false;
            }

            settingsLoaded = true;
            RefreshUiFromControllerState();
        }
        catch (OperationCanceledException) when (windowLifetime.IsCancellationRequested)
        {
        }
        catch (Exception ex)
        {
            settingsLoaded = true;
            RefreshUiFromControllerState($"Settings load failed: {ex.Message}");
        }
        finally
        {
            suppressModelPathChanged = false;
        }
    }

    private async Task SaveSettingsAsync(CancellationToken cancellationToken)
    {
        AppSettings settings;
        try
        {
            settings = await settingsStore.LoadAsync(cancellationToken);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch
        {
            settings = new AppSettings();
        }

        await settingsStore.SaveAsync(settings with
        {
            ModelPath = ModelPathTextBox.Text
        }, cancellationToken);
    }

    private DictationController CreateController(NAudioCaptureService captureService) =>
        new(
            captureService,
            new WhisperNetTranscriptionService(),
            new ClipboardTextInjectionService(restoreClipboard: true),
            new SqliteHistoryStore(historyPath),
            settingsStore);

    private void RecreateController()
    {
        audioCapture.Dispose();
        audioCapture = new NAudioCaptureService(recordingsDirectory);
        controller = CreateController(audioCapture);
    }

    private void RefreshUiFromControllerState(string? statusOverride = null)
    {
        var operationActive = isStarting || isStopping;
        var controllerBusy = controller.State is DictationState.Transcribing or DictationState.Inserting;

        StartButton.IsEnabled = settingsLoaded
            && !operationActive
            && !controllerBusy
            && controller.State != DictationState.Recording;
        StopButton.IsEnabled = settingsLoaded
            && !operationActive
            && controller.State == DictationState.Recording;

        StatusTextBlock.Text = statusOverride
            ?? controller.LastError
            ?? controller.LastWarning
            ?? StateToStatusText(controller.State);
    }

    private static string StateToStatusText(DictationState state) =>
        state switch
        {
            DictationState.Idle => "Idle",
            DictationState.Recording => "Recording",
            DictationState.Transcribing => "Transcribing",
            DictationState.Inserting => "Inserting",
            DictationState.Error => "Error",
            _ => state.ToString()
        };

    private void MainWindow_Closed(object sender, WindowEventArgs args)
    {
        windowLifetime.Cancel();
        audioCapture.Dispose();
        windowLifetime.Dispose();
    }
}
