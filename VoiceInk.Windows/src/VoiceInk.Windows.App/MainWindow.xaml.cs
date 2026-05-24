using System;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using VoiceInk.Windows.Core.Audio;
using VoiceInk.Windows.Core.Dictionary;
using VoiceInk.Windows.Core.Dictation;
using VoiceInk.Windows.Core.History;
using VoiceInk.Windows.Core.Settings;
using VoiceInk.Windows.Core.Shortcuts;
using VoiceInk.Windows.Core.Text;
using VoiceInk.Windows.Infrastructure.Dictionary;
using VoiceInk.Windows.Infrastructure.History;
using VoiceInk.Windows.Infrastructure.Settings;
using VoiceInk.Windows.Native.Audio;
using VoiceInk.Windows.Native.Hotkeys;
using VoiceInk.Windows.Native.Text;
using VoiceInk.Windows.Native.Transcription;
using Windows.Storage;
using Windows.Storage.Pickers;
using WinRT.Interop;

namespace VoiceInk.Windows.App;

public sealed partial class MainWindow : Window
{
    private readonly string recordingsDirectory;
    private readonly JsonDictionaryStore dictionaryStore;
    private readonly SqliteHistoryStore historyStore;
    private readonly JsonSettingsStore settingsStore;
    private readonly ClipboardTextInjectionService textInjectionService;
    private readonly LastTranscriptionActionService lastTranscriptionActionService;
    private readonly NAudioInputDeviceProvider audioInputDeviceProvider;
    private readonly CancellationTokenSource windowLifetime = new();
    private GlobalHotkeyService? hotkeyService;
    private NAudioCaptureService audioCapture;
    private DictationController controller;
    private IReadOnlyList<AudioInputDeviceChoice> audioInputChoices = [];
    private IReadOnlyList<VocabularyWord> vocabularyItems = [];
    private IReadOnlyList<WordReplacement> replacementItems = [];
    private IReadOnlyList<TranscriptionHistoryItem> historyItems = [];
    private AudioInputDeviceChoice? activeAudioInputDeviceChoice;
    private bool isStarting;
    private bool isStopping;
    private bool isPastingLast;
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
        var historyPath = Path.Combine(appData, "history.db");

        dictionaryStore = new JsonDictionaryStore(Path.Combine(appData, "dictionary.json"));
        historyStore = new SqliteHistoryStore(historyPath);
        settingsStore = new JsonSettingsStore(Path.Combine(appData, "settings.json"));
        textInjectionService = new ClipboardTextInjectionService(restoreClipboard: true);
        lastTranscriptionActionService = new LastTranscriptionActionService(historyStore, textInjectionService);
        audioInputDeviceProvider = new NAudioInputDeviceProvider();
        audioCapture = new NAudioCaptureService(recordingsDirectory);
        controller = CreateController(audioCapture);

        Closed += MainWindow_Closed;
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

    private async void RefreshAudioInputsButton_Click(object sender, RoutedEventArgs e)
    {
        await RefreshAudioInputDevicesWithStatusAsync();
    }

    private async void ApplyAudioInputButton_Click(object sender, RoutedEventArgs e)
    {
        await ApplyAudioInputAsync();
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

            if (controller.State == DictationState.Error
                || !AudioInputDeviceChoicesMatch(activeAudioInputDeviceChoice, SelectedAudioInputDeviceChoice()))
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

    private async void HotkeyService_HotkeyPressed(object? sender, GlobalHotkeyPressedEventArgs e)
    {
        try
        {
            switch (e.Action)
            {
                case GlobalShortcutAction.PasteLastTranscription:
                    await PasteLastAsync(LastTranscriptionTextKind.Final);
                    break;
                case GlobalShortcutAction.PasteLastEnhancedTranscription:
                    await PasteLastAsync(LastTranscriptionTextKind.EnhancedPreferred);
                    break;
                default:
                    await ToggleCurrentRecordingAsync();
                    break;
            }
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

    private async void SearchHistoryButton_Click(object sender, RoutedEventArgs e)
    {
        await RefreshHistoryWithStatusAsync("History search updated");
    }

    private async void ClearHistorySearchButton_Click(object sender, RoutedEventArgs e)
    {
        HistorySearchTextBox.Text = string.Empty;
        await RefreshHistoryWithStatusAsync("History search cleared");
    }

    private async void ExportHistoryButton_Click(object sender, RoutedEventArgs e)
    {
        await ExportHistoryAsync();
    }

    private async void PasteLastButton_Click(object sender, RoutedEventArgs e)
    {
        await PasteLastAsync(LastTranscriptionTextKind.Final);
    }

    private async void PasteLastEnhancedButton_Click(object sender, RoutedEventArgs e)
    {
        await PasteLastAsync(LastTranscriptionTextKind.EnhancedPreferred);
    }

    private async void DeleteHistoryButton_Click(object sender, RoutedEventArgs e)
    {
        await DeleteSelectedHistoryAsync();
    }

    private async void ApplyShortcutsButton_Click(object sender, RoutedEventArgs e)
    {
        await ApplyShortcutsAsync();
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

            RecordingHotkeyTextBox.Text = settings.Hotkey;
            PasteLastHotkeyTextBox.Text = settings.PasteLastTranscriptionHotkey;
            PasteLastEnhancedHotkeyTextBox.Text = settings.PasteLastEnhancementHotkey;
            RemoveFillerWordsCheckBox.IsChecked = settings.RemoveFillerWords;
            LowercaseTranscriptionCheckBox.IsChecked = settings.LowercaseTranscription;
            AppendTrailingSpaceCheckBox.IsChecked = settings.AppendTrailingSpace;
            PunctuationCleanupComboBox.SelectedIndex = PunctuationCleanupModeToSelectedIndex(settings.PunctuationCleanupMode);
            var audioInputWarning = await RefreshAudioInputDevicesAsync(settings, windowLifetime.Token);
            await RefreshDictionaryAsync(windowLifetime.Token);
            await RefreshHistoryAsync(windowLifetime.Token);
            TryReplaceGlobalHotkeys(settings, rollbackSettings: null);

            settingsLoaded = true;
            RefreshUiFromControllerState(audioInputWarning);
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

        var searchText = HistorySearchTextBox.Text.Trim();
        historyItems = string.IsNullOrWhiteSpace(searchText)
            ? await historyStore.ListRecentAsync(50, cancellationToken)
            : await historyStore.SearchAsync(searchText, 50, cancellationToken);
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

            var fileName = $"VoiceInk-history-{DateTimeOffset.Now:yyyyMMdd-HHmmss}.csv";
            var picker = new FileSavePicker
            {
                SuggestedStartLocation = PickerLocationId.DocumentsLibrary,
                SuggestedFileName = Path.GetFileNameWithoutExtension(fileName)
            };
            picker.FileTypeChoices.Add("CSV file", [".csv"]);
            InitializeWithWindow.Initialize(picker, WindowNative.GetWindowHandle(this));

            var file = await picker.PickSaveFileAsync();
            if (file is null)
            {
                RefreshUiFromControllerState("History export canceled");
                return;
            }

            await FileIO.WriteTextAsync(file, HistoryCsvExporter.Export(historyItems));
            RefreshUiFromControllerState($"History exported: {file.Name}");
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

    private async Task DeleteSelectedHistoryAsync()
    {
        if (HistoryListView.SelectedIndex < 0 || HistoryListView.SelectedIndex >= historyItems.Count)
        {
            RefreshUiFromControllerState("Select a transcription to delete");
            return;
        }

        var item = historyItems[HistoryListView.SelectedIndex];
        var dialog = new ContentDialog
        {
            XamlRoot = Content.XamlRoot,
            Title = "Delete transcription?",
            Content = "This action cannot be undone.",
            PrimaryButtonText = "Delete",
            SecondaryButtonText = "Cancel",
            DefaultButton = ContentDialogButton.Secondary
        };

        var result = await dialog.ShowAsync();
        if (result != ContentDialogResult.Primary)
        {
            RefreshUiFromControllerState("Delete canceled");
            return;
        }

        try
        {
            var deleted = await historyStore.DeleteAsync(item.Id, windowLifetime.Token);
            await RefreshHistoryAsync(windowLifetime.Token);
            RefreshUiFromControllerState(deleted ? "Transcription deleted" : "Transcription was already deleted");
        }
        catch (OperationCanceledException) when (windowLifetime.IsCancellationRequested)
        {
            RefreshUiFromControllerState("Closing");
        }
        catch (Exception ex)
        {
            RefreshUiFromControllerState($"History delete failed: {ex.Message}");
        }
    }

    private async Task PasteLastAsync(LastTranscriptionTextKind textKind)
    {
        if (isStopping || isPastingLast)
        {
            return;
        }

        var statusOverride = "Preparing paste target";
        isPastingLast = true;
        RefreshUiFromControllerState(statusOverride);
        try
        {
            var result = await lastTranscriptionActionService.PasteLastAsync(
                textKind,
                windowLifetime.Token,
                MinimizeForExternalPasteAsync);
            statusOverride = result.Message;
        }
        catch (OperationCanceledException) when (windowLifetime.IsCancellationRequested)
        {
            statusOverride = "Closing";
        }
        catch (Exception ex)
        {
            statusOverride = $"Paste last failed: {ex.Message}";
        }
        finally
        {
            isPastingLast = false;
            RefreshUiFromControllerState(statusOverride);
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

    private Task MinimizeForExternalPasteAsync(CancellationToken cancellationToken)
    {
        var windowHandle = WindowNative.GetWindowHandle(this);
        if (windowHandle != IntPtr.Zero)
        {
            ShowWindow(windowHandle, ShowWindowMinimize);
        }

        return Task.Delay(TimeSpan.FromMilliseconds(150), cancellationToken);
    }

    private async Task ApplyShortcutsAsync()
    {
        if (!settingsLoaded)
        {
            return;
        }

        try
        {
            var persistedSettings = await settingsStore.LoadAsync(windowLifetime.Token);
            var settings = await CurrentSettingsAsync(windowLifetime.Token, includeShortcutFields: true);
            var shortcutRegistration = GlobalShortcutSettings.BuildRegistrations(settings);
            if (shortcutRegistration.Errors.Count > 0)
            {
                hotkeyRegistrationError = string.Join(" ", shortcutRegistration.Errors);
                RefreshUiFromControllerState(hotkeyRegistrationError);
                return;
            }

            if (!TryReplaceGlobalHotkeys(settings, persistedSettings))
            {
                RefreshUiFromControllerState(hotkeyRegistrationError);
                return;
            }

            try
            {
                await settingsStore.SaveAsync(settings, windowLifetime.Token);
            }
            catch (Exception saveEx)
            {
                TryReplaceGlobalHotkeys(persistedSettings, rollbackSettings: null);
                if (hotkeyRegistrationError is not null)
                {
                    throw new InvalidOperationException(
                        $"Shortcut settings save failed: {saveEx.Message} {hotkeyRegistrationError}",
                        saveEx);
                }

                throw;
            }

            if (hotkeyRegistrationError is not null)
            {
                RefreshUiFromControllerState(hotkeyRegistrationError);
                return;
            }

            RefreshUiFromControllerState("Shortcuts updated");
        }
        catch (OperationCanceledException) when (windowLifetime.IsCancellationRequested)
        {
            RefreshUiFromControllerState("Closing");
        }
        catch (Exception ex)
        {
            RefreshUiFromControllerState($"Shortcut update failed: {ex.Message}");
        }
    }

    private async Task RefreshAudioInputDevicesWithStatusAsync()
    {
        if (!settingsLoaded)
        {
            return;
        }

        try
        {
            var settings = await CurrentSettingsAsync(windowLifetime.Token, includeShortcutFields: false);
            var warning = await RefreshAudioInputDevicesAsync(settings, windowLifetime.Token);
            RefreshUiFromControllerState(warning ?? "Audio inputs refreshed");
        }
        catch (OperationCanceledException) when (windowLifetime.IsCancellationRequested)
        {
            RefreshUiFromControllerState("Closing");
        }
        catch (Exception ex)
        {
            RefreshUiFromControllerState($"Audio input refresh failed: {ex.Message}");
        }
    }

    private async Task ApplyAudioInputAsync()
    {
        if (!settingsLoaded || controller.State == DictationState.Recording)
        {
            return;
        }

        try
        {
            await SaveSettingsAsync(windowLifetime.Token);
            if (!AudioInputDeviceChoicesMatch(activeAudioInputDeviceChoice, SelectedAudioInputDeviceChoice()))
            {
                RecreateController();
            }

            RefreshUiFromControllerState("Audio input updated");
        }
        catch (OperationCanceledException) when (windowLifetime.IsCancellationRequested)
        {
            RefreshUiFromControllerState("Closing");
        }
        catch (Exception ex)
        {
            RefreshUiFromControllerState($"Audio input update failed: {ex.Message}");
        }
    }

    private async Task<string?> RefreshAudioInputDevicesAsync(
        AppSettings settings,
        CancellationToken cancellationToken)
    {
        var devices = await audioInputDeviceProvider.ListInputDevicesAsync(cancellationToken);
        var result = AudioInputDeviceSelection.BuildChoices(devices, settings);

        audioInputChoices = result.Choices;
        AudioInputComboBox.ItemsSource = audioInputChoices;
        AudioInputComboBox.SelectedIndex = Math.Clamp(
            result.SelectedIndex,
            0,
            Math.Max(0, audioInputChoices.Count - 1));

        return result.Warning;
    }

    private async Task SaveSettingsAsync(CancellationToken cancellationToken)
    {
        var settings = await CurrentSettingsAsync(cancellationToken, includeShortcutFields: false);
        await settingsStore.SaveAsync(settings, cancellationToken);
    }

    private async Task<AppSettings> CurrentSettingsAsync(
        CancellationToken cancellationToken,
        bool includeShortcutFields)
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

        return settings with
        {
            ModelPath = ModelPathTextBox.Text,
            Hotkey = includeShortcutFields ? RecordingHotkeyTextBox.Text.Trim() : settings.Hotkey,
            PasteLastTranscriptionHotkey = includeShortcutFields
                ? PasteLastHotkeyTextBox.Text.Trim()
                : settings.PasteLastTranscriptionHotkey,
            PasteLastEnhancementHotkey = includeShortcutFields
                ? PasteLastEnhancedHotkeyTextBox.Text.Trim()
                : settings.PasteLastEnhancementHotkey,
            AudioInputDeviceNumber = SelectedAudioInputDeviceNumber(),
            AudioInputDeviceName = SelectedAudioInputDeviceName(),
            RemoveFillerWords = RemoveFillerWordsCheckBox.IsChecked == true,
            LowercaseTranscription = LowercaseTranscriptionCheckBox.IsChecked == true,
            AppendTrailingSpace = AppendTrailingSpaceCheckBox.IsChecked == true,
            PunctuationCleanupMode = SelectedPunctuationCleanupMode()
        };
    }

    private DictationController CreateController(NAudioCaptureService captureService) =>
        new(
            captureService,
            new WhisperNetTranscriptionService(),
            textInjectionService,
            historyStore,
            settingsStore,
            dictionaryStore);

    private int? SelectedAudioInputDeviceNumber() =>
        SelectedAudioInputDeviceChoice()?.DeviceNumber;

    private string SelectedAudioInputDeviceName()
    {
        var choice = SelectedAudioInputDeviceChoice();
        return choice?.DeviceNumber is null ? string.Empty : choice.Name;
    }

    private AudioInputDeviceChoice? SelectedAudioInputDeviceChoice()
    {
        var selectedIndex = AudioInputComboBox.SelectedIndex;
        return selectedIndex >= 0 && selectedIndex < audioInputChoices.Count
            ? audioInputChoices[selectedIndex]
            : null;
    }

    private bool TryReplaceGlobalHotkeys(AppSettings settings, AppSettings? rollbackSettings)
    {
        try
        {
            var shortcutRegistration = GlobalShortcutSettings.BuildRegistrations(settings);
            if (shortcutRegistration.Errors.Count > 0)
            {
                hotkeyRegistrationError = string.Join(" ", shortcutRegistration.Errors);
                return false;
            }

            ReplaceGlobalHotkeyService(shortcutRegistration.Registrations);
            hotkeyRegistrationError = null;
            return true;
        }
        catch (Exception ex)
        {
            var originalError = $"Global shortcut unavailable: {ex.Message}";
            hotkeyRegistrationError = originalError;

            if (rollbackSettings is not null)
            {
                TryRestoreGlobalHotkeys(rollbackSettings, originalError);
            }

            return false;
        }
    }

    private void TryRestoreGlobalHotkeys(AppSettings settings, string originalError)
    {
        try
        {
            var shortcutRegistration = GlobalShortcutSettings.BuildRegistrations(settings);
            if (shortcutRegistration.Errors.Count == 0)
            {
                ReplaceGlobalHotkeyService(shortcutRegistration.Registrations);
                hotkeyRegistrationError = originalError;
                return;
            }

            DisposeGlobalHotkeyService();
            hotkeyRegistrationError = $"{originalError} Previous shortcuts could not be restored: {string.Join(" ", shortcutRegistration.Errors)}";
        }
        catch (Exception restoreEx)
        {
            DisposeGlobalHotkeyService();
            hotkeyRegistrationError = $"{originalError} Previous shortcuts could not be restored: {restoreEx.Message}";
        }
    }

    private void ReplaceGlobalHotkeyService(IReadOnlyList<GlobalShortcutRegistration> registrations)
    {
        DisposeGlobalHotkeyService();
        var windowHandle = WindowNative.GetWindowHandle(this);
        var newHotkeyService = new GlobalHotkeyService(windowHandle);
        try
        {
            newHotkeyService.HotkeyPressed += HotkeyService_HotkeyPressed;
            newHotkeyService.RegisterHotkeys(registrations);
            hotkeyService = newHotkeyService;
        }
        catch
        {
            newHotkeyService.HotkeyPressed -= HotkeyService_HotkeyPressed;
            newHotkeyService.Dispose();
            throw;
        }
    }

    private void DisposeGlobalHotkeyService()
    {
        if (hotkeyService is null)
        {
            return;
        }

        hotkeyService.HotkeyPressed -= HotkeyService_HotkeyPressed;
        hotkeyService.Dispose();
        hotkeyService = null;
    }

    private void RecreateController()
    {
        var selectedAudioInputDeviceChoice = SelectedAudioInputDeviceChoice();
        audioCapture.Dispose();
        audioCapture = new NAudioCaptureService(recordingsDirectory, selectedAudioInputDeviceChoice?.DeviceNumber);
        activeAudioInputDeviceChoice = selectedAudioInputDeviceChoice;
        controller = CreateController(audioCapture);
    }

    private static bool AudioInputDeviceChoicesMatch(
        AudioInputDeviceChoice? first,
        AudioInputDeviceChoice? second) =>
        first?.DeviceNumber == second?.DeviceNumber
        && string.Equals(first?.Name, second?.Name, StringComparison.Ordinal);

    private void RefreshUiFromControllerState(string? statusOverride = null)
    {
        var operationActive = isStarting || isStopping || isPastingLast;
        var controllerBusy = controller.State is DictationState.Transcribing or DictationState.Inserting;

        StartButton.IsEnabled = settingsLoaded
            && !operationActive
            && !controllerBusy
            && controller.State != DictationState.Recording;
        StopButton.IsEnabled = settingsLoaded
            && !operationActive
            && controller.State == DictationState.Recording;
        PasteLastButton.IsEnabled = settingsLoaded && !operationActive;
        PasteLastEnhancedButton.IsEnabled = settingsLoaded && !operationActive;
        RefreshHistoryButton.IsEnabled = settingsLoaded && !operationActive;
        ExportHistoryButton.IsEnabled = settingsLoaded && !operationActive;
        ApplyShortcutsButton.IsEnabled = settingsLoaded && !operationActive;
        RefreshAudioInputsButton.IsEnabled = settingsLoaded
            && !operationActive
            && !controllerBusy
            && controller.State != DictationState.Recording;
        ApplyAudioInputButton.IsEnabled = settingsLoaded
            && !operationActive
            && !controllerBusy
            && controller.State != DictationState.Recording;
        SearchHistoryButton.IsEnabled = settingsLoaded && !operationActive;
        ClearHistorySearchButton.IsEnabled = settingsLoaded && !operationActive;
        DeleteHistoryButton.IsEnabled = settingsLoaded && !operationActive;

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
        DisposeGlobalHotkeyService();

        audioCapture.Dispose();
        windowLifetime.Dispose();
    }

    private const int ShowWindowMinimize = 6;

    [DllImport("user32.dll")]
    private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
}
