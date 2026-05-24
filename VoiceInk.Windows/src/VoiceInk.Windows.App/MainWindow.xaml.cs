using System;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using VoiceInk.Windows.Core.Dictionary;
using VoiceInk.Windows.Core.Dictation;
using VoiceInk.Windows.Core.History;
using VoiceInk.Windows.Core.Settings;
using VoiceInk.Windows.Core.Text;
using VoiceInk.Windows.Infrastructure.Dictionary;
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
    private readonly string exportDirectory;
    private readonly JsonDictionaryStore dictionaryStore;
    private readonly SqliteHistoryStore historyStore;
    private readonly JsonSettingsStore settingsStore;
    private readonly CancellationTokenSource windowLifetime = new();
    private GlobalHotkeyService? hotkeyService;
    private NAudioCaptureService audioCapture;
    private DictationController controller;
    private IReadOnlyList<VocabularyWord> vocabularyItems = [];
    private IReadOnlyList<WordReplacement> replacementItems = [];
    private IReadOnlyList<TranscriptionHistoryItem> historyItems = [];
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
        exportDirectory = Path.Combine(appData, "Exports");

        dictionaryStore = new JsonDictionaryStore(Path.Combine(appData, "dictionary.json"));
        historyStore = new SqliteHistoryStore(historyPath);
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
            await RefreshHistoryAsync(windowLifetime.Token);
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

    private async void AddVocabularyButton_Click(object sender, RoutedEventArgs e)
    {
        await AddVocabularyAsync();
    }

    private async void RemoveVocabularyButton_Click(object sender, RoutedEventArgs e)
    {
        await RemoveSelectedVocabularyAsync();
    }

    private async void AddReplacementButton_Click(object sender, RoutedEventArgs e)
    {
        await AddReplacementAsync();
    }

    private async void RemoveReplacementButton_Click(object sender, RoutedEventArgs e)
    {
        await RemoveSelectedReplacementAsync();
    }

    private async void RefreshHistoryButton_Click(object sender, RoutedEventArgs e)
    {
        await RefreshHistoryWithStatusAsync("History refreshed");
    }

    private async void ExportHistoryButton_Click(object sender, RoutedEventArgs e)
    {
        await ExportHistoryAsync();
    }

    private void HistoryListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        RefreshSelectedHistoryDetails();
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
            await RefreshDictionaryAsync(windowLifetime.Token);
            await RefreshHistoryAsync(windowLifetime.Token);

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

    private async Task AddVocabularyAsync()
    {
        if (!settingsLoaded)
        {
            return;
        }

        var input = VocabularyInputTextBox.Text.Trim();
        if (input.Length == 0)
        {
            return;
        }

        try
        {
            var error = await dictionaryStore.AddVocabularyWordsAsync(input, windowLifetime.Token);
            if (error is not null)
            {
                RefreshUiFromControllerState(error);
                return;
            }

            VocabularyInputTextBox.Text = string.Empty;
            await RefreshDictionaryAsync(windowLifetime.Token);
            RefreshUiFromControllerState("Vocabulary updated");
        }
        catch (OperationCanceledException) when (windowLifetime.IsCancellationRequested)
        {
            RefreshUiFromControllerState("Closing");
        }
        catch (Exception ex)
        {
            RefreshUiFromControllerState($"Vocabulary update failed: {ex.Message}");
        }
    }

    private async Task RemoveSelectedVocabularyAsync()
    {
        if (!settingsLoaded || VocabularyListView.SelectedIndex < 0)
        {
            return;
        }

        var selectedIndex = VocabularyListView.SelectedIndex;
        if (selectedIndex >= vocabularyItems.Count)
        {
            return;
        }

        try
        {
            await dictionaryStore.DeleteVocabularyWordAsync(vocabularyItems[selectedIndex].Id, windowLifetime.Token);
            await RefreshDictionaryAsync(windowLifetime.Token);
            RefreshUiFromControllerState("Vocabulary updated");
        }
        catch (OperationCanceledException) when (windowLifetime.IsCancellationRequested)
        {
            RefreshUiFromControllerState("Closing");
        }
        catch (Exception ex)
        {
            RefreshUiFromControllerState($"Vocabulary update failed: {ex.Message}");
        }
    }

    private async Task AddReplacementAsync()
    {
        if (!settingsLoaded)
        {
            return;
        }

        var original = ReplacementOriginalTextBox.Text.Trim();
        var replacement = ReplacementTextBox.Text.Trim();
        if (original.Length == 0 || replacement.Length == 0)
        {
            return;
        }

        try
        {
            var error = await dictionaryStore.AddWordReplacementAsync(original, replacement, windowLifetime.Token);
            if (error is not null)
            {
                RefreshUiFromControllerState(error);
                return;
            }

            ReplacementOriginalTextBox.Text = string.Empty;
            ReplacementTextBox.Text = string.Empty;
            await RefreshDictionaryAsync(windowLifetime.Token);
            RefreshUiFromControllerState("Word replacements updated");
        }
        catch (OperationCanceledException) when (windowLifetime.IsCancellationRequested)
        {
            RefreshUiFromControllerState("Closing");
        }
        catch (Exception ex)
        {
            RefreshUiFromControllerState($"Word replacement update failed: {ex.Message}");
        }
    }

    private async Task RemoveSelectedReplacementAsync()
    {
        if (!settingsLoaded || ReplacementListView.SelectedIndex < 0)
        {
            return;
        }

        var selectedIndex = ReplacementListView.SelectedIndex;
        if (selectedIndex >= replacementItems.Count)
        {
            return;
        }

        try
        {
            await dictionaryStore.DeleteReplacementAsync(replacementItems[selectedIndex].Id, windowLifetime.Token);
            await RefreshDictionaryAsync(windowLifetime.Token);
            RefreshUiFromControllerState("Word replacements updated");
        }
        catch (OperationCanceledException) when (windowLifetime.IsCancellationRequested)
        {
            RefreshUiFromControllerState("Closing");
        }
        catch (Exception ex)
        {
            RefreshUiFromControllerState($"Word replacement update failed: {ex.Message}");
        }
    }

    private async Task RefreshDictionaryAsync(CancellationToken cancellationToken)
    {
        vocabularyItems = await dictionaryStore.ListVocabularyAsync(cancellationToken);
        replacementItems = await dictionaryStore.ListReplacementsAsync(cancellationToken);

        VocabularyListView.ItemsSource = vocabularyItems
            .Select(item => item.Word)
            .ToArray();
        ReplacementListView.ItemsSource = replacementItems
            .Select(item => $"{item.OriginalText} -> {item.ReplacementText}")
            .ToArray();
    }

    private async Task RefreshHistoryWithStatusAsync(string status)
    {
        try
        {
            await RefreshHistoryAsync(windowLifetime.Token);
            RefreshUiFromControllerState(status);
        }
        catch (OperationCanceledException) when (windowLifetime.IsCancellationRequested)
        {
            RefreshUiFromControllerState("Closing");
        }
        catch (Exception ex)
        {
            RefreshUiFromControllerState($"History refresh failed: {ex.Message}");
        }
    }

    private async Task RefreshHistoryAsync(CancellationToken cancellationToken)
    {
        var selectedId = HistoryListView.SelectedIndex >= 0 && HistoryListView.SelectedIndex < historyItems.Count
            ? historyItems[HistoryListView.SelectedIndex].Id
            : (Guid?)null;

        historyItems = await historyStore.ListRecentAsync(50, cancellationToken);
        HistoryListView.ItemsSource = historyItems
            .Select(HistoryListItem)
            .ToArray();

        var selectedIndex = selectedId is null
            ? -1
            : historyItems.ToList().FindIndex(item => item.Id == selectedId.Value);
        HistoryListView.SelectedIndex = selectedIndex;
        RefreshSelectedHistoryDetails();
    }

    private async Task ExportHistoryAsync()
    {
        try
        {
            if (historyItems.Count == 0)
            {
                await RefreshHistoryAsync(windowLifetime.Token);
            }

            Directory.CreateDirectory(exportDirectory);
            var fileName = $"VoiceInk-history-{DateTimeOffset.Now:yyyyMMdd-HHmmss}.csv";
            var exportPath = Path.Combine(exportDirectory, fileName);
            await File.WriteAllTextAsync(
                exportPath,
                HistoryCsvExporter.Export(historyItems),
                windowLifetime.Token);
            RefreshUiFromControllerState($"History exported: {exportPath}");
        }
        catch (OperationCanceledException) when (windowLifetime.IsCancellationRequested)
        {
            RefreshUiFromControllerState("Closing");
        }
        catch (Exception ex)
        {
            RefreshUiFromControllerState($"History export failed: {ex.Message}");
        }
    }

    private void RefreshSelectedHistoryDetails()
    {
        if (HistoryListView.SelectedIndex < 0 || HistoryListView.SelectedIndex >= historyItems.Count)
        {
            HistoryMetadataTextBlock.Text = historyItems.Count == 0
                ? "No transcriptions"
                : "Select a transcription";
            HistoryOriginalTextBox.Text = string.Empty;
            HistoryFinalTextBox.Text = string.Empty;
            HistoryEnhancedTextBox.Text = string.Empty;
            return;
        }

        var item = historyItems[HistoryListView.SelectedIndex];
        HistoryMetadataTextBlock.Text = string.Join(
            Environment.NewLine,
            $"Status: {item.Status}",
            $"Provider: {item.ProviderName}",
            $"Language: {item.Language}",
            $"Model: {item.ModelPath ?? "Not recorded"}",
            $"Prompt: {item.PromptName ?? "None"}",
            $"Recorded: {item.CreatedAt.LocalDateTime:g}",
            $"Audio: {Seconds(item.AudioDuration)}s",
            $"Transcription: {Seconds(item.TranscriptionDuration)}s",
            $"Enhancement: {(item.EnhancementDuration is null ? "None" : $"{Seconds(item.EnhancementDuration.Value)}s")}",
            $"Error: {item.ErrorMessage ?? "None"}");
        HistoryOriginalTextBox.Text = item.OriginalText;
        HistoryFinalTextBox.Text = item.Text;
        HistoryEnhancedTextBox.Text = item.EnhancedText ?? string.Empty;
    }

    private static string HistoryListItem(TranscriptionHistoryItem item)
    {
        var text = item.EnhancedText ?? item.Text;
        var preview = text.ReplaceLineEndings(" ").Trim();
        if (preview.Length > 80)
        {
            preview = $"{preview[..80]}...";
        }

        return $"{item.CreatedAt.LocalDateTime:g}  [{item.Status}]  {preview}";
    }

    private static string Seconds(TimeSpan duration) =>
        duration.TotalSeconds.ToString("0.000", CultureInfo.InvariantCulture);

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
            historyStore,
            settingsStore,
            dictionaryStore);

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
}
