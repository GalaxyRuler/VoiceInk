using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using VoiceInk.Windows.Core.Dictionary;
using VoiceInk.Windows.Core.Dictation;
using VoiceInk.Windows.Core.Settings;
using VoiceInk.Windows.Core.Services;
using VoiceInk.Windows.Core.Text;
using VoiceInk.Windows.Infrastructure.History;
using VoiceInk.Windows.Infrastructure.Settings;
using VoiceInk.Windows.Native.Audio;
using VoiceInk.Windows.Native.Hotkeys;
using VoiceInk.Windows.Native.Text;
using VoiceInk.Windows.Native.Transcription;
using WinRT.Interop;

namespace VoiceInk.Windows.App;

public sealed partial class MainWindow : Window
{
    private readonly string recordingsDirectory;
    private readonly string historyPath;
    private readonly JsonSettingsStore settingsStore;
    private readonly CancellationTokenSource windowLifetime = new();
    private GlobalHotkeyService? hotkeyService;
    private NAudioCaptureService audioCapture;
    private DictationController controller;
    private bool isStarting;
    private bool isStopping;
    private bool settingsLoaded;
    private bool modelPathEdited;
    private bool suppressModelPathChanged;
    private string? hotkeyRegistrationError;

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
        RegisterGlobalHotkey();
        RefreshUiFromControllerState("Loading settings");
        _ = InitializeAsync();
    }

    private async void StartButton_Click(object sender, RoutedEventArgs e)
    {
        await StartCurrentRecordingAsync();
    }

    private async void StopButton_Click(object sender, RoutedEventArgs e)
    {
        await StopCurrentRecordingAsync();
    }

    private async Task StartCurrentRecordingAsync()
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

    private async Task StopCurrentRecordingAsync()
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

    private async Task ToggleCurrentRecordingAsync()
    {
        if (controller.State == DictationState.Recording)
        {
            await StopCurrentRecordingAsync();
            return;
        }

        await StartCurrentRecordingAsync();
    }

    private async void HotkeyService_HotkeyPressed(object? sender, EventArgs e)
    {
        try
        {
            await ToggleCurrentRecordingAsync();
        }
        catch (Exception ex)
        {
            RefreshUiFromControllerState($"Hotkey failed: {ex.Message}");
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

            RemoveFillerWordsCheckBox.IsChecked = settings.RemoveFillerWords;
            LowercaseTranscriptionCheckBox.IsChecked = settings.LowercaseTranscription;
            AppendTrailingSpaceCheckBox.IsChecked = settings.AppendTrailingSpace;
            PunctuationCleanupComboBox.SelectedIndex = PunctuationCleanupModeToSelectedIndex(settings.PunctuationCleanupMode);

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
            ModelPath = ModelPathTextBox.Text,
            RemoveFillerWords = RemoveFillerWordsCheckBox.IsChecked == true,
            LowercaseTranscription = LowercaseTranscriptionCheckBox.IsChecked == true,
            AppendTrailingSpace = AppendTrailingSpaceCheckBox.IsChecked == true,
            PunctuationCleanupMode = SelectedPunctuationCleanupMode()
        }, cancellationToken);
    }

    private DictationController CreateController(NAudioCaptureService captureService) =>
        new(
            captureService,
            new WhisperNetTranscriptionService(),
            new ClipboardTextInjectionService(restoreClipboard: true),
            new SqliteHistoryStore(historyPath),
            settingsStore,
            new EmptyDictionaryStore());

    private void RegisterGlobalHotkey()
    {
        try
        {
            var windowHandle = WindowNative.GetWindowHandle(this);
            hotkeyService = new GlobalHotkeyService(windowHandle);
            hotkeyService.HotkeyPressed += HotkeyService_HotkeyPressed;
            hotkeyService.RegisterCtrlAltSpace();
        }
        catch (Exception ex)
        {
            hotkeyService?.Dispose();
            hotkeyService = null;
            hotkeyRegistrationError = $"Ctrl+Alt+Space hotkey unavailable: {ex.Message}";
        }
    }

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

        var stateStatus = StateToStatusText(controller.State);
        var idleHotkeyWarning = controller.State == DictationState.Idle && !operationActive
            ? hotkeyRegistrationError
            : null;

        StatusTextBlock.Text = statusOverride
            ?? controller.LastError
            ?? controller.LastWarning
            ?? idleHotkeyWarning
            ?? stateStatus;
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

    private PunctuationCleanupMode SelectedPunctuationCleanupMode() =>
        PunctuationCleanupComboBox.SelectedIndex switch
        {
            1 => PunctuationCleanupMode.RemoveAll,
            2 => PunctuationCleanupMode.RemoveTrailingPeriod,
            _ => PunctuationCleanupMode.Keep
        };

    private static int PunctuationCleanupModeToSelectedIndex(PunctuationCleanupMode mode) =>
        mode switch
        {
            PunctuationCleanupMode.RemoveAll => 1,
            PunctuationCleanupMode.RemoveTrailingPeriod => 2,
            _ => 0
        };

    private void MainWindow_Closed(object sender, WindowEventArgs args)
    {
        windowLifetime.Cancel();
        if (hotkeyService is not null)
        {
            hotkeyService.HotkeyPressed -= HotkeyService_HotkeyPressed;
            hotkeyService.Dispose();
            hotkeyService = null;
        }

        audioCapture.Dispose();
        windowLifetime.Dispose();
    }

    private sealed class EmptyDictionaryStore : IDictionaryStore
    {
        public Task<IReadOnlyList<VocabularyWord>> ListVocabularyAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult<IReadOnlyList<VocabularyWord>>([]);
        }

        public Task<IReadOnlyList<WordReplacement>> ListReplacementsAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult<IReadOnlyList<WordReplacement>>([]);
        }
    }
}
