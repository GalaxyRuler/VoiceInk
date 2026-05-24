using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.ApplicationModel.DataTransfer;
using VoiceInk.Windows.Core.Audio;
using VoiceInk.Windows.Core.AudioFiles;
using VoiceInk.Windows.Core.Dictionary;
using VoiceInk.Windows.Core.Dictation;
using VoiceInk.Windows.Core.Enhancement;
using VoiceInk.Windows.Core.History;
using VoiceInk.Windows.Core.Models;
using VoiceInk.Windows.Core.Onboarding;
using VoiceInk.Windows.Core.PowerMode;
using VoiceInk.Windows.Core.Recorder;
using VoiceInk.Windows.Core.Settings;
using VoiceInk.Windows.Core.Shell;
using VoiceInk.Windows.Core.Shortcuts;
using VoiceInk.Windows.Core.Text;
using VoiceInk.Windows.Core.Transcription;
using VoiceInk.Windows.Infrastructure.Dictionary;
using VoiceInk.Windows.Infrastructure.Enhancement;
using VoiceInk.Windows.Infrastructure.History;
using VoiceInk.Windows.Infrastructure.Settings;
using VoiceInk.Windows.Infrastructure.Transcription;
using VoiceInk.Windows.Native.Audio;
using VoiceInk.Windows.Native.Hotkeys;
using VoiceInk.Windows.Native.PowerMode;
using VoiceInk.Windows.Native.Security;
using VoiceInk.Windows.Native.Text;
using VoiceInk.Windows.Native.Tray;
using VoiceInk.Windows.Native.Transcription;
using Windows.Media.Core;
using Windows.Storage;
using Windows.Storage.Pickers;
using WinRT.Interop;

namespace VoiceInk.Windows.App;

public sealed partial class MainWindow : Window
{
    private const string DashboardSectionTag = "Dashboard";
    private const string TranscribeAudioSectionTag = "Transcribe Audio";
    private const string ModelsSectionTag = "AI Models";
    private const string AudioInputSectionTag = "Audio Input";
    private const string DictionarySectionTag = "Dictionary";
    private const string HistorySectionTag = "History";
    private const string SettingsSectionTag = "Settings";
    private const string AboutSectionTag = "About";
    private const string EnhancementSectionTag = "Enhancement";
    private const string PowerModeSectionTag = "Power Mode";

    private readonly string appDataDirectory;
    private readonly string recordingsDirectory;
    private readonly string dictionaryPath;
    private readonly string historyPath;
    private readonly string settingsPath;
    private readonly Dictionary<string, NavigationViewItem> navigationItemsByTag = [];
    private readonly JsonDictionaryStore dictionaryStore;
    private readonly SqliteHistoryStore historyStore;
    private readonly JsonSettingsStore settingsStore;
    private readonly ClipboardTextInjectionService textInjectionService;
    private readonly LastTranscriptionActionService lastTranscriptionActionService;
    private readonly HistoryRetryService historyRetryService;
    private readonly DictionaryQuickAddService dictionaryQuickAddService;
    private readonly AudioFileQueueService audioFileQueueService = new();
    private readonly WindowsCredentialSecretStore secretStore;
    private readonly OpenAICompatibleTextEnhancementService textEnhancementService;
    private readonly TextEnhancementPipeline textEnhancementPipeline;
    private readonly OpenAICompatibleCloudTranscriptionService cloudTranscriptionService;
    private readonly TranscriptionServiceRouter transcriptionService;
    private readonly NAudioInputDeviceProvider audioInputDeviceProvider;
    private readonly ActiveWindowPowerModeTargetProvider powerModeTargetProvider = new();
    private readonly CancellationTokenSource windowLifetime = new();
    private readonly DispatcherQueueTimer floatingRecorderRefreshTimer;
    private GlobalHotkeyService? hotkeyService;
    private TrayIconService? trayIconService;
    private FloatingRecorderWindow? floatingRecorderWindow;
    private AudioFileTranscriptionService audioFileTranscriptionService;
    private CancellationTokenSource? audioFileQueueCancellation;
    private NAudioCaptureService audioCapture;
    private DictationController controller;
    private IReadOnlyList<AudioInputDeviceChoice> audioInputChoices = [];
    private IReadOnlyList<VocabularyWord> vocabularyItems = [];
    private IReadOnlyList<WordReplacement> replacementItems = [];
    private IReadOnlyList<TranscriptionHistoryItem> historyItems = [];
    private IReadOnlyList<AudioFileQueueItem> audioFileQueueItems = [];
    private IReadOnlyList<LocalWhisperModel> localWhisperModels = [];
    private IReadOnlyList<LocalWhisperModel> modelChoices = [];
    private IReadOnlyList<EnhancementPrompt> enhancementPrompts = EnhancementPromptCatalog.CreateDefaultPrompts();
    private IReadOnlyList<PowerModeRule> powerModeRules = [];
    private AudioInputDeviceChoice? activeAudioInputDeviceChoice;
    private bool isStarting;
    private bool isStopping;
    private bool isCanceling;
    private bool isPastingLast;
    private bool isRetryingHistory;
    private bool isQuickAdding;
    private bool isImportingModel;
    private bool isTranscribingAudioFiles;
    private bool isSavingEnhancementKey;
    private bool isSavingCloudTranscriptionKey;
    private bool isOnboardingOpen;
    private bool settingsLoaded;
    private bool modelPathEdited;
    private bool suppressModelPathChanged;
    private bool suppressCloudTranscriptionPresetChanged;
    private bool suppressCloudTranscriptionModelChanged;
    private bool exitRequested;
    private DateTimeOffset? recordingStartedAt;
    private string activeSectionTag = DashboardSectionTag;
    private string? hotkeyRegistrationError;

    public MainWindow()
    {
        InitializeComponent();
        CloudTranscriptionPresetComboBox.ItemsSource = TranscriptionProviderPresetCatalog.All;
        floatingRecorderRefreshTimer = DispatcherQueue.CreateTimer();
        floatingRecorderRefreshTimer.Interval = TimeSpan.FromSeconds(1);
        floatingRecorderRefreshTimer.Tick += (_, _) => RefreshFloatingRecorderFromTimer();

        InitializeNavigationItems();
        ShowShellSection(DashboardSectionTag);
        RefreshAboutSection();

        appDataDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "VoiceInk.Windows");
        recordingsDirectory = Path.Combine(appDataDirectory, "Recordings");
        dictionaryPath = Path.Combine(appDataDirectory, "dictionary.json");
        historyPath = Path.Combine(appDataDirectory, "history.db");
        settingsPath = Path.Combine(appDataDirectory, "settings.json");

        dictionaryStore = new JsonDictionaryStore(dictionaryPath);
        historyStore = new SqliteHistoryStore(historyPath);
        settingsStore = new JsonSettingsStore(settingsPath);
        textInjectionService = new ClipboardTextInjectionService(restoreClipboard: true);
        lastTranscriptionActionService = new LastTranscriptionActionService(historyStore, textInjectionService);
        secretStore = new WindowsCredentialSecretStore();
        textEnhancementService = new OpenAICompatibleTextEnhancementService(new HttpClient(), secretStore);
        textEnhancementPipeline = new TextEnhancementPipeline(textEnhancementService, enhancementPrompts);
        cloudTranscriptionService = new OpenAICompatibleCloudTranscriptionService(new HttpClient(), secretStore);
        transcriptionService = new TranscriptionServiceRouter(
            new WhisperNetTranscriptionService(),
            cloudTranscriptionService);
        historyRetryService = new HistoryRetryService(
            transcriptionService,
            historyStore,
            settingsStore,
            dictionaryStore);
        audioFileTranscriptionService = new AudioFileTranscriptionService(
            new MediaFoundationAudioFileImportService(),
            transcriptionService,
            historyStore,
            settingsStore,
            dictionaryStore,
            textEnhancementPipeline);
        dictionaryQuickAddService = new DictionaryQuickAddService(dictionaryStore);
        audioInputDeviceProvider = new NAudioInputDeviceProvider();
        audioCapture = new NAudioCaptureService(recordingsDirectory);
        controller = CreateController(audioCapture);

        CreateTrayIconService();
        AppWindow.Closing += MainWindow_AppWindowClosing;
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

    private async void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        await CancelCurrentRecordingAsync();
    }

    private void RootNavigationView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
    {
        if (args.SelectedItem is NavigationViewItem { Tag: string tag })
        {
            ShowShellSection(tag);
        }
    }

    private void TrayIconService_ShowRequested(object? sender, EventArgs e)
    {
        RestoreAndActivateWindow();
        RefreshUiFromControllerState();
    }

    private void TrayIconService_HideRequested(object? sender, EventArgs e)
    {
        HideWindowToTray();
    }

    private async void TrayIconService_ToggleRecordingRequested(object? sender, EventArgs e)
    {
        try
        {
            await ToggleCurrentRecordingAsync();
        }
        catch (Exception ex)
        {
            RefreshUiFromControllerState($"Tray recording command failed: {ex.Message}");
        }
    }

    private async void TrayIconService_QuickAddDictionaryRequested(object? sender, EventArgs e)
    {
        await ShowQuickAddDictionaryAsync();
    }

    private async void TrayIconService_OpenHistoryRequested(object? sender, EventArgs e)
    {
        await OpenHistoryWindowAsync();
    }

    private void TrayIconService_ExitRequested(object? sender, EventArgs e)
    {
        exitRequested = true;
        Close();
    }

    private async void RefreshAudioInputsButton_Click(object sender, RoutedEventArgs e)
    {
        await RefreshAudioInputDevicesWithStatusAsync();
    }

    private async void ApplyAudioInputButton_Click(object sender, RoutedEventArgs e)
    {
        await ApplyAudioInputAsync();
    }

    private async void ChooseAudioFilesButton_Click(object sender, RoutedEventArgs e)
    {
        await ChooseAudioFilesAsync();
    }

    private async void StartAudioFileQueueButton_Click(object sender, RoutedEventArgs e)
    {
        await StartAudioFileQueueAsync();
    }

    private void CancelAudioFileQueueButton_Click(object sender, RoutedEventArgs e)
    {
        audioFileQueueCancellation?.Cancel();
    }

    private void ClearAudioFileQueueButton_Click(object sender, RoutedEventArgs e)
    {
        if (isTranscribingAudioFiles)
        {
            return;
        }

        audioFileQueueItems = audioFileQueueService.Clear(audioFileQueueItems).Items;
        RefreshAudioFileQueueListView();
        RefreshUiFromControllerState("Audio file queue cleared");
    }

    private void RemoveAudioFileQueueItemButton_Click(object sender, RoutedEventArgs e)
    {
        if (isTranscribingAudioFiles || SelectedAudioFileQueueItem() is not { } item)
        {
            return;
        }

        audioFileQueueItems = audioFileQueueService.RemovePending(audioFileQueueItems, item.Id).Items;
        RefreshAudioFileQueueListView();
        RefreshUiFromControllerState("Audio file removed");
    }

    private void RetryAudioFileQueueItemButton_Click(object sender, RoutedEventArgs e)
    {
        if (isTranscribingAudioFiles || SelectedAudioFileQueueItem() is not { } item)
        {
            return;
        }

        audioFileQueueItems = audioFileQueueService.RetryFailed(audioFileQueueItems, item.Id).Items;
        RefreshAudioFileQueueListView(item.Id);
        RefreshUiFromControllerState("Audio file queued for retry");
    }

    private async Task StartCurrentRecordingAsync()
    {
        if (IsOperationActive() || !settingsLoaded)
        {
            return;
        }

        var statusOverride = "Starting recording";
        isStarting = true;
        RefreshUiFromControllerState(statusOverride);

        try
        {
            var currentSettings = await CurrentSettingsAsync(windowLifetime.Token, includeShortcutFields: false);
            var configurationError = TranscriptionConfiguration.ValidateRequiredSettings(currentSettings);
            if (configurationError is not null)
            {
                statusOverride = configurationError;
                return;
            }

            if (currentSettings.TranscriptionProvider == TranscriptionProviderKind.OpenAICompatible
                && !await HasCloudTranscriptionApiKeyAsync(
                    currentSettings.CloudTranscriptionProviderId,
                    windowLifetime.Token))
            {
                statusOverride = "Cloud transcription API key is required.";
                return;
            }

            if (controller.State == DictationState.Error
                || !AudioInputDeviceChoicesMatch(activeAudioInputDeviceChoice, SelectedAudioInputDeviceChoice()))
            {
                RecreateController();
            }

            await SaveSettingsAsync(windowLifetime.Token);
            await controller.StartAsync(windowLifetime.Token);
            if (controller.State == DictationState.Recording)
            {
                recordingStartedAt = DateTimeOffset.Now;
            }

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
        if (isStarting
            || isStopping
            || isCanceling
            || isPastingLast
            || isRetryingHistory
            || controller.State != DictationState.Recording)
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
            recordingStartedAt = null;
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

    private async Task CancelCurrentRecordingAsync()
    {
        if (isStarting || isStopping || isCanceling || controller.State != DictationState.Recording)
        {
            return;
        }

        var statusOverride = "Canceling recording";
        isCanceling = true;
        RefreshUiFromControllerState(statusOverride);

        try
        {
            await controller.CancelAsync(windowLifetime.Token);
            await RefreshHistoryAsync(windowLifetime.Token);
            recordingStartedAt = null;
            statusOverride = controller.LastWarning ?? "Recording canceled";
        }
        catch (OperationCanceledException) when (windowLifetime.IsCancellationRequested)
        {
            statusOverride = "Closing";
        }
        catch (Exception ex)
        {
            statusOverride = $"Cancel failed: {ex.Message}";
        }
        finally
        {
            isCanceling = false;
            RefreshUiFromControllerState(statusOverride);
        }
    }

    private async void HotkeyService_HotkeyPressed(object? sender, GlobalHotkeyPressedEventArgs e)
    {
        try
        {
            if (isOnboardingOpen)
            {
                return;
            }

            switch (e.Action)
            {
                case GlobalShortcutAction.PasteLastTranscription:
                    await PasteLastAsync(LastTranscriptionTextKind.Final);
                    break;
                case GlobalShortcutAction.PasteLastEnhancedTranscription:
                    await PasteLastAsync(LastTranscriptionTextKind.EnhancedPreferred);
                    break;
                case GlobalShortcutAction.RetryLastTranscription:
                    await RetryLastHistoryAsync();
                    break;
                case GlobalShortcutAction.CancelRecording:
                    await CancelCurrentRecordingAsync();
                    break;
                case GlobalShortcutAction.OpenHistoryWindow:
                    await OpenHistoryWindowAsync();
                    break;
                case GlobalShortcutAction.QuickAddToDictionary:
                    await ShowQuickAddDictionaryAsync();
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
            RefreshModelChoices(ModelPathTextBox.Text);
        }
    }

    private void ModelComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        RefreshUiFromControllerState();
    }

    private void TranscriptionProviderComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (settingsLoaded)
        {
            _ = RefreshCloudTranscriptionKeyStatusAsync(windowLifetime.Token);
        }

        RefreshUiFromControllerState();
    }

    private void CloudTranscriptionPresetComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (!suppressCloudTranscriptionPresetChanged)
        {
            ApplySelectedCloudTranscriptionPreset(fillConfiguration: true);
            if (settingsLoaded)
            {
                _ = RefreshCloudTranscriptionKeyStatusAsync(windowLifetime.Token);
            }
        }

        RefreshUiFromControllerState();
    }

    private void CloudTranscriptionModelComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (!suppressCloudTranscriptionModelChanged
            && CloudTranscriptionModelComboBox.SelectedItem is string model)
        {
            CloudTranscriptionModelTextBox.Text = model;
        }

        RefreshUiFromControllerState();
    }

    private async void ImportModelButton_Click(object sender, RoutedEventArgs e)
    {
        await ImportLocalModelAsync();
    }

    private async void UseSelectedModelButton_Click(object sender, RoutedEventArgs e)
    {
        await UseSelectedLocalModelAsync();
    }

    private void OpenModelDownloadsButton_Click(object sender, RoutedEventArgs e)
    {
        OpenModelDownloads();
    }

    private async void ApplyTranscriptionProviderSettingsButton_Click(object sender, RoutedEventArgs e)
    {
        await ApplyTranscriptionProviderSettingsAsync();
    }

    private async void SaveCloudTranscriptionKeyButton_Click(object sender, RoutedEventArgs e)
    {
        await SaveCloudTranscriptionKeyAsync();
    }

    private async void ClearCloudTranscriptionKeyButton_Click(object sender, RoutedEventArgs e)
    {
        await ClearCloudTranscriptionKeyAsync();
    }

    private async void ApplyEnhancementSettingsButton_Click(object sender, RoutedEventArgs e)
    {
        await ApplyEnhancementSettingsAsync();
    }

    private async void SaveEnhancementKeyButton_Click(object sender, RoutedEventArgs e)
    {
        await SaveEnhancementKeyAsync();
    }

    private async void ClearEnhancementKeyButton_Click(object sender, RoutedEventArgs e)
    {
        await ClearEnhancementKeyAsync();
    }

    private async void RefreshPowerModeTargetButton_Click(object sender, RoutedEventArgs e)
    {
        await RefreshPowerModeActiveTargetAsync(fillRuleFields: false);
    }

    private async void UsePowerModeTargetButton_Click(object sender, RoutedEventArgs e)
    {
        await RefreshPowerModeActiveTargetAsync(fillRuleFields: true);
    }

    private void PowerModeRulesListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        FillPowerModeFormFromSelection();
        RefreshUiFromControllerState();
    }

    private async void AddPowerModeRuleButton_Click(object sender, RoutedEventArgs e)
    {
        await AddPowerModeRuleAsync();
    }

    private async void UpdatePowerModeRuleButton_Click(object sender, RoutedEventArgs e)
    {
        await UpdateSelectedPowerModeRuleAsync();
    }

    private async void RemovePowerModeRuleButton_Click(object sender, RoutedEventArgs e)
    {
        await RemoveSelectedPowerModeRuleAsync();
    }

    private async void MovePowerModeRuleUpButton_Click(object sender, RoutedEventArgs e)
    {
        await MoveSelectedPowerModeRuleAsync(-1);
    }

    private async void MovePowerModeRuleDownButton_Click(object sender, RoutedEventArgs e)
    {
        await MoveSelectedPowerModeRuleAsync(1);
    }

    private void OpenDiagnosticsFolderButton_Click(object sender, RoutedEventArgs e)
    {
        OpenDiagnosticsFolder();
    }

    private async void CopyDiagnosticsSummaryButton_Click(object sender, RoutedEventArgs e)
    {
        await CopyDiagnosticsSummaryAsync();
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

    private async void EditReplacementButton_Click(object sender, RoutedEventArgs e)
    {
        await EditSelectedReplacementAsync();
    }

    private async void ToggleReplacementButton_Click(object sender, RoutedEventArgs e)
    {
        await ToggleSelectedReplacementAsync();
    }

    private async void ExportDictionaryButton_Click(object sender, RoutedEventArgs e)
    {
        await ExportDictionaryAsync();
    }

    private async void ImportDictionaryButton_Click(object sender, RoutedEventArgs e)
    {
        await ImportDictionaryAsync();
    }

    private async void QuickAddDictionaryButton_Click(object sender, RoutedEventArgs e)
    {
        await ShowQuickAddDictionaryAsync();
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

    private async void RetryLastButton_Click(object sender, RoutedEventArgs e)
    {
        await RetryLastHistoryAsync();
    }

    private async void RetryHistoryButton_Click(object sender, RoutedEventArgs e)
    {
        await RetrySelectedHistoryAsync();
    }

    private void OpenHistoryAudioButton_Click(object sender, RoutedEventArgs e)
    {
        OpenSelectedHistoryAudio();
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

    private void AudioFileQueueListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        RefreshSelectedAudioFileQueueDetails();
        RefreshUiFromControllerState();
    }

    private void ReplacementListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        RefreshUiFromControllerState();
    }

    private async void DictionarySortComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (!settingsLoaded)
        {
            return;
        }

        try
        {
            await RefreshDictionaryAsync(windowLifetime.Token);
            RefreshUiFromControllerState("Dictionary sorted");
        }
        catch (OperationCanceledException) when (windowLifetime.IsCancellationRequested)
        {
            RefreshUiFromControllerState("Closing");
        }
        catch (Exception ex)
        {
            RefreshUiFromControllerState($"Dictionary sort failed: {ex.Message}");
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

            localWhisperModels = settings.ImportedWhisperModels;
            RefreshModelChoices(settings.ModelPath);
            TranscriptionProviderComboBox.SelectedIndex = TranscriptionProviderToSelectedIndex(settings.TranscriptionProvider);
            suppressCloudTranscriptionPresetChanged = true;
            SelectCloudTranscriptionPreset(settings.CloudTranscriptionProviderId);
            suppressCloudTranscriptionPresetChanged = false;
            CloudTranscriptionEndpointTextBox.Text = settings.CloudTranscriptionEndpoint;
            CloudTranscriptionModelTextBox.Text = settings.CloudTranscriptionModel;
            RefreshCloudTranscriptionModelChoices(settings.CloudTranscriptionModel);
            RecordingHotkeyTextBox.Text = settings.Hotkey;
            SecondaryRecordingHotkeyTextBox.Text = settings.SecondaryRecordingHotkey;
            PasteLastHotkeyTextBox.Text = settings.PasteLastTranscriptionHotkey;
            PasteLastEnhancedHotkeyTextBox.Text = settings.PasteLastEnhancementHotkey;
            RetryLastHotkeyTextBox.Text = settings.RetryLastTranscriptionHotkey;
            CancelHotkeyTextBox.Text = settings.CancelRecordingHotkey;
            OpenHistoryHotkeyTextBox.Text = settings.OpenHistoryHotkey;
            QuickAddHotkeyTextBox.Text = settings.QuickAddDictionaryHotkey;
            EnhancementEnabledCheckBox.IsChecked = settings.IsEnhancementEnabled;
            EnhancementEndpointTextBox.Text = settings.EnhancementEndpoint;
            EnhancementModelTextBox.Text = settings.EnhancementModel;
            EnhancementTimeoutTextBox.Text = EnhancementTimeoutSeconds(settings).ToString(CultureInfo.InvariantCulture);
            ShortEnhancementThresholdTextBox.Text = ShortEnhancementThreshold(settings).ToString(CultureInfo.InvariantCulture);
            SkipShortEnhancementCheckBox.IsChecked = settings.SkipShortEnhancement;
            EnhancementRetryOnTimeoutCheckBox.IsChecked = settings.EnhancementRetryOnTimeout;
            RefreshEnhancementPromptChoices(settings.SelectedEnhancementPromptId);
            powerModeRules = settings.PowerModeRules;
            RefreshPowerModePromptChoices(selectedPromptId: null);
            RefreshPowerModeRulesListView();
            RemoveFillerWordsCheckBox.IsChecked = settings.RemoveFillerWords;
            LowercaseTranscriptionCheckBox.IsChecked = settings.LowercaseTranscription;
            AppendTrailingSpaceCheckBox.IsChecked = settings.AppendTrailingSpace;
            PunctuationCleanupComboBox.SelectedIndex = PunctuationCleanupModeToSelectedIndex(settings.PunctuationCleanupMode);
            await RefreshCloudTranscriptionKeyStatusAsync(windowLifetime.Token);
            await RefreshEnhancementKeyStatusAsync(windowLifetime.Token);
            var audioInputWarning = await RefreshAudioInputDevicesAsync(settings, windowLifetime.Token);
            await RefreshDictionaryAsync(windowLifetime.Token);
            await RefreshHistoryAsync(windowLifetime.Token);
            TryReplaceGlobalHotkeys(settings, rollbackSettings: null);

            settingsLoaded = true;
            RefreshUiFromControllerState(audioInputWarning);
            await ShowOnboardingIfNeededAsync(settings);
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
            suppressCloudTranscriptionPresetChanged = false;
            suppressCloudTranscriptionModelChanged = false;
        }
    }

    private async Task ShowOnboardingIfNeededAsync(AppSettings settings)
    {
        if (settings.HasCompletedOnboarding || isOnboardingOpen || windowLifetime.IsCancellationRequested)
        {
            return;
        }

        isOnboardingOpen = true;
        RefreshUiFromControllerState("Opening first-run setup");

        try
        {
            await ShowFirstRunOnboardingDialogAsync();
        }
        finally
        {
            isOnboardingOpen = false;
            RefreshUiFromControllerState();
        }
    }

    private async Task ShowFirstRunOnboardingDialogAsync()
    {
        var modelPathTextBox = new TextBox
        {
            Header = "Local whisper model path",
            PlaceholderText = "C:\\Models\\ggml-base.en.bin",
            Text = ModelPathTextBox.Text
        };
        var browseModelButton = new Button
        {
            Content = "Browse .bin"
        };
        browseModelButton.Click += async (_, _) => await BrowseOnboardingModelAsync(modelPathTextBox);

        var audioInputComboBox = new ComboBox
        {
            Header = "Microphone",
            ItemsSource = audioInputChoices,
            SelectedIndex = audioInputChoices.Count == 0
                ? -1
                : Math.Clamp(AudioInputComboBox.SelectedIndex, 0, audioInputChoices.Count - 1)
        };
        var microphoneSettingsButton = new Button
        {
            Content = "Open Windows Microphone Settings"
        };
        microphoneSettingsButton.Click += (_, _) => OpenWindowsMicrophoneSettings();

        var shortcutTextBox = new TextBox
        {
            Header = "Primary shortcut",
            PlaceholderText = "Ctrl+Alt+Space",
            Text = RecordingHotkeyTextBox.Text
        };
        var statusTextBlock = new TextBlock
        {
            TextWrapping = TextWrapping.Wrap
        };
        RefreshOnboardingStatus(statusTextBlock, modelPathTextBox.Text, shortcutTextBox.Text);

        modelPathTextBox.TextChanged += (_, _) => RefreshOnboardingStatus(statusTextBlock, modelPathTextBox.Text, shortcutTextBox.Text);
        shortcutTextBox.TextChanged += (_, _) => RefreshOnboardingStatus(statusTextBlock, modelPathTextBox.Text, shortcutTextBox.Text);

        var content = new StackPanel
        {
            Spacing = 12
        };
        content.Children.Add(new TextBlock
        {
            Text = "Set up the essentials once, then use the tray icon or shortcut from anywhere.",
            TextWrapping = TextWrapping.Wrap
        });
        content.Children.Add(modelPathTextBox);
        content.Children.Add(browseModelButton);
        content.Children.Add(audioInputComboBox);
        content.Children.Add(microphoneSettingsButton);
        content.Children.Add(shortcutTextBox);
        content.Children.Add(new TextBlock
        {
            Text = "Try it after setup: click a text field, press your shortcut, speak, then press the shortcut again.",
            TextWrapping = TextWrapping.Wrap
        });
        content.Children.Add(statusTextBlock);

        var dialog = new ContentDialog
        {
            Title = "Welcome to VoiceInk",
            PrimaryButtonText = "Save Setup",
            SecondaryButtonText = "Skip For Now",
            CloseButtonText = string.Empty,
            DefaultButton = ContentDialogButton.Primary,
            Content = content,
            XamlRoot = Content.XamlRoot
        };

        dialog.PrimaryButtonClick += async (sender, args) =>
        {
            args.Cancel = true;
            var deferral = args.GetDeferral();
            try
            {
                var saved = await TrySaveOnboardingSetupAsync(
                    modelPathTextBox.Text,
                    shortcutTextBox.Text,
                    audioInputComboBox.SelectedIndex,
                    statusTextBlock);
                if (saved)
                {
                    ((ContentDialog)sender).Hide();
                }
            }
            catch (OperationCanceledException) when (windowLifetime.IsCancellationRequested)
            {
                statusTextBlock.Text = "Closing";
            }
            catch (Exception ex)
            {
                statusTextBlock.Text = $"Setup save failed: {ex.Message}";
                RefreshUiFromControllerState(statusTextBlock.Text);
            }
            finally
            {
                deferral.Complete();
            }
        };

        dialog.SecondaryButtonClick += async (sender, args) =>
        {
            args.Cancel = true;
            var deferral = args.GetDeferral();
            try
            {
                await MarkOnboardingCompleteAsync();
                RefreshUiFromControllerState("First-run setup skipped");
                ((ContentDialog)sender).Hide();
            }
            catch (OperationCanceledException) when (windowLifetime.IsCancellationRequested)
            {
                statusTextBlock.Text = "Closing";
            }
            catch (Exception ex)
            {
                statusTextBlock.Text = $"Setup skip failed: {ex.Message}";
                RefreshUiFromControllerState(statusTextBlock.Text);
            }
            finally
            {
                deferral.Complete();
            }
        };

        await dialog.ShowAsync();
    }

    private async Task BrowseOnboardingModelAsync(TextBox modelPathTextBox)
    {
        try
        {
            var picker = new FileOpenPicker();
            picker.FileTypeFilter.Add(".bin");
            InitializeWithWindow.Initialize(picker, WindowNative.GetWindowHandle(this));

            var file = await picker.PickSingleFileAsync();
            if (file is null)
            {
                return;
            }

            modelPathTextBox.Text = file.Path;
            ModelPathTextBox.Text = file.Path;
        }
        catch (Exception ex)
        {
            RefreshUiFromControllerState($"Model picker failed: {ex.Message}");
        }
    }

    private async Task<bool> TrySaveOnboardingSetupAsync(
        string modelPath,
        string primaryShortcut,
        int selectedAudioInputIndex,
        TextBlock statusTextBlock)
    {
        var trimmedModelPath = modelPath.Trim();
        var trimmedShortcut = primaryShortcut.Trim();
        var setupStatus = OnboardingSetupStatusService.Build(
            new AppSettings
            {
                ModelPath = trimmedModelPath,
                Hotkey = trimmedShortcut
            },
            HasPhysicalAudioInputChoices());

        if (!setupStatus.HasModelPath)
        {
            statusTextBlock.Text = "Choose a local whisper.cpp .bin model file, or skip for now.";
            return false;
        }

        if (!File.Exists(trimmedModelPath))
        {
            statusTextBlock.Text = "Choose an existing whisper.cpp .bin model file, or skip for now.";
            return false;
        }

        if (!setupStatus.HasPrimaryShortcut)
        {
            statusTextBlock.Text = "Set a primary shortcut, or skip for now.";
            return false;
        }

        var previousSettings = await settingsStore.LoadAsync(windowLifetime.Token);
        var previousModelPath = ModelPathTextBox.Text;
        var previousRecordingHotkey = RecordingHotkeyTextBox.Text;
        var previousAudioInputIndex = AudioInputComboBox.SelectedIndex;

        ModelPathTextBox.Text = trimmedModelPath;
        RecordingHotkeyTextBox.Text = trimmedShortcut;
        if (selectedAudioInputIndex >= 0 && selectedAudioInputIndex < audioInputChoices.Count)
        {
            AudioInputComboBox.SelectedIndex = selectedAudioInputIndex;
        }

        var settings = await CurrentSettingsAsync(windowLifetime.Token, includeShortcutFields: true);
        settings = settings with
        {
            ModelPath = trimmedModelPath,
            Hotkey = trimmedShortcut,
            HasCompletedOnboarding = true
        };

        if (!TryReplaceGlobalHotkeys(settings, previousSettings))
        {
            statusTextBlock.Text = hotkeyRegistrationError ?? "Global shortcut unavailable.";
            return false;
        }

        try
        {
            await settingsStore.SaveAsync(settings, windowLifetime.Token);
        }
        catch
        {
            TryReplaceGlobalHotkeys(previousSettings, rollbackSettings: null);
            ModelPathTextBox.Text = previousModelPath;
            RecordingHotkeyTextBox.Text = previousRecordingHotkey;
            AudioInputComboBox.SelectedIndex = previousAudioInputIndex;
            throw;
        }

        if (!AudioInputDeviceChoicesMatch(activeAudioInputDeviceChoice, SelectedAudioInputDeviceChoice()))
        {
            RecreateController();
        }

        RefreshUiFromControllerState("First-run setup saved");
        return true;
    }

    private async Task MarkOnboardingCompleteAsync()
    {
        var settings = await settingsStore.LoadAsync(windowLifetime.Token);
        await settingsStore.SaveAsync(settings with { HasCompletedOnboarding = true }, windowLifetime.Token);
    }

    private void RefreshOnboardingStatus(TextBlock statusTextBlock, string modelPath, string primaryShortcut)
    {
        var status = OnboardingSetupStatusService.Build(
            new AppSettings
            {
                ModelPath = modelPath,
                Hotkey = primaryShortcut
            },
            HasPhysicalAudioInputChoices());

        statusTextBlock.Text = status.CanCompleteSetup
            ? "Ready to save setup."
            : "Model path and primary shortcut are required to save setup.";
    }

    private bool HasPhysicalAudioInputChoices() =>
        audioInputChoices.Any(choice => choice.DeviceNumber is not null);

    private void OpenWindowsMicrophoneSettings()
    {
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "ms-settings:privacy-microphone",
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            RefreshUiFromControllerState($"Microphone settings failed: {ex.Message}");
        }
    }

    private async Task ImportLocalModelAsync()
    {
        if (!CanEditModelLibrary())
        {
            return;
        }

        var statusOverride = "Opening model picker";
        isImportingModel = true;
        RefreshUiFromControllerState(statusOverride);

        try
        {
            var picker = new FileOpenPicker
            {
                ViewMode = PickerViewMode.List,
                SuggestedStartLocation = PickerLocationId.DocumentsLibrary
            };
            picker.FileTypeFilter.Add(".bin");
            InitializeWithWindow.Initialize(picker, WindowNative.GetWindowHandle(this));

            var file = await picker.PickSingleFileAsync();
            if (file is null)
            {
                statusOverride = "Model import canceled";
                return;
            }

            if (!CanEditModelLibrary(includeCurrentModelImport: false))
            {
                statusOverride = "Model import canceled because VoiceInk is busy";
                return;
            }

            var importedModels = LocalWhisperModelService.Import(
                file.Path,
                localWhisperModels,
                DateTimeOffset.Now,
                out var error);
            if (error is not null)
            {
                statusOverride = error;
                return;
            }

            localWhisperModels = importedModels;
            ModelPathTextBox.Text = file.Path;
            await SaveSettingsAsync(windowLifetime.Token);
            RefreshModelChoices(file.Path);
            statusOverride = $"Model imported: {Path.GetFileName(file.Path)}";
        }
        catch (OperationCanceledException) when (windowLifetime.IsCancellationRequested)
        {
            statusOverride = "Closing";
        }
        catch (Exception ex)
        {
            statusOverride = $"Model import failed: {ex.Message}";
        }
        finally
        {
            isImportingModel = false;
            RefreshUiFromControllerState(statusOverride);
        }
    }

    private async Task UseSelectedLocalModelAsync()
    {
        var selectedModel = SelectedLocalWhisperModelChoice();
        if (!CanEditModelLibrary())
        {
            return;
        }

        if (selectedModel is null)
        {
            RefreshUiFromControllerState("Select an imported model");
            return;
        }

        try
        {
            ModelPathTextBox.Text = selectedModel.Path;
            await SaveSettingsAsync(windowLifetime.Token);
            RefreshModelChoices(selectedModel.Path);
            RefreshUiFromControllerState($"Default model: {selectedModel.DisplayName}");
        }
        catch (OperationCanceledException) when (windowLifetime.IsCancellationRequested)
        {
            RefreshUiFromControllerState("Closing");
        }
        catch (Exception ex)
        {
            RefreshUiFromControllerState($"Model selection failed: {ex.Message}");
        }
    }

    private void OpenModelDownloads()
    {
        if (!CanEditModelLibrary())
        {
            return;
        }

        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "https://huggingface.co/ggerganov/whisper.cpp/tree/main",
                UseShellExecute = true
            });
            RefreshUiFromControllerState("Model downloads opened");
        }
        catch (Exception ex)
        {
            RefreshUiFromControllerState($"Model downloads failed: {ex.Message}");
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
        var selectedVocabularyId = VocabularyListView.SelectedIndex >= 0 && VocabularyListView.SelectedIndex < vocabularyItems.Count
            ? vocabularyItems[VocabularyListView.SelectedIndex].Id
            : (Guid?)null;
        var selectedReplacementId = ReplacementListView.SelectedIndex >= 0 && ReplacementListView.SelectedIndex < replacementItems.Count
            ? replacementItems[ReplacementListView.SelectedIndex].Id
            : (Guid?)null;

        vocabularyItems = DictionarySortService.SortVocabulary(
            await dictionaryStore.ListVocabularyAsync(cancellationToken),
            SelectedVocabularySortMode());
        replacementItems = DictionarySortService.SortReplacements(
            await dictionaryStore.ListReplacementsAsync(cancellationToken),
            SelectedReplacementSortMode());

        VocabularyListView.ItemsSource = vocabularyItems
            .Select(item => item.Word)
            .ToArray();
        ReplacementListView.ItemsSource = replacementItems
            .Select(ReplacementListItem)
            .ToArray();
        VocabularyListView.SelectedIndex = selectedVocabularyId is null
            ? -1
            : vocabularyItems.ToList().FindIndex(item => item.Id == selectedVocabularyId.Value);
        ReplacementListView.SelectedIndex = selectedReplacementId is null
            ? -1
            : replacementItems.ToList().FindIndex(item => item.Id == selectedReplacementId.Value);
    }

    private async Task ExportDictionaryAsync()
    {
        try
        {
            var fileName = $"VoiceInk-dictionary-{DateTimeOffset.Now:yyyyMMdd-HHmmss}.json";
            var picker = new FileSavePicker
            {
                SuggestedStartLocation = PickerLocationId.DocumentsLibrary,
                SuggestedFileName = Path.GetFileNameWithoutExtension(fileName)
            };
            picker.FileTypeChoices.Add("JSON file", [".json"]);
            InitializeWithWindow.Initialize(picker, WindowNative.GetWindowHandle(this));

            var file = await picker.PickSaveFileAsync();
            if (file is null)
            {
                RefreshUiFromControllerState("Dictionary export canceled");
                return;
            }

            var json = await dictionaryStore.ExportBackupAsync(windowLifetime.Token);
            await FileIO.WriteTextAsync(file, json);
            RefreshUiFromControllerState($"Dictionary exported: {file.Name}");
        }
        catch (OperationCanceledException) when (windowLifetime.IsCancellationRequested)
        {
            RefreshUiFromControllerState("Closing");
        }
        catch (Exception ex)
        {
            RefreshUiFromControllerState($"Dictionary export failed: {ex.Message}");
        }
    }

    private async Task ImportDictionaryAsync()
    {
        try
        {
            var picker = new FileOpenPicker
            {
                ViewMode = PickerViewMode.List,
                SuggestedStartLocation = PickerLocationId.DocumentsLibrary
            };
            picker.FileTypeFilter.Add(".json");
            InitializeWithWindow.Initialize(picker, WindowNative.GetWindowHandle(this));

            var file = await picker.PickSingleFileAsync();
            if (file is null)
            {
                RefreshUiFromControllerState("Dictionary import canceled");
                return;
            }

            var json = await FileIO.ReadTextAsync(file);
            var result = await dictionaryStore.ImportBackupAsync(json, windowLifetime.Token);
            await RefreshDictionaryAsync(windowLifetime.Token);
            RefreshUiFromControllerState(
                $"Dictionary imported: {result.ImportedVocabularyCount} vocabulary, {result.ImportedReplacementCount} replacements, {result.SkippedDuplicateCount} skipped");
        }
        catch (OperationCanceledException) when (windowLifetime.IsCancellationRequested)
        {
            RefreshUiFromControllerState("Closing");
        }
        catch (Exception ex)
        {
            RefreshUiFromControllerState($"Dictionary import failed: {ex.Message}");
        }
    }

    private async Task ShowQuickAddDictionaryAsync()
    {
        if (!settingsLoaded
            || isStarting
            || isStopping
            || isCanceling
            || isPastingLast
            || isRetryingHistory
            || isQuickAdding
            || controller.State != DictationState.Idle)
        {
            return;
        }

        isQuickAdding = true;
        RefreshUiFromControllerState("Opening quick add");

        DictionaryQuickAddResult? lastResult = null;
        string? finalStatus = null;
        try
        {
            RestoreAndActivateWindow();

            var modeComboBox = new ComboBox
            {
                Header = "Mode",
                ItemsSource = new[] { "Vocabulary", "Word Replacement" },
                SelectedIndex = 0
            };
            var vocabularyTextBox = new TextBox
            {
                Header = "Vocabulary",
                PlaceholderText = "e.g. Prakash, VoiceInk"
            };
            var replacementOriginalTextBox = new TextBox
            {
                Header = "Replace",
                PlaceholderText = "e.g. my email, my mail"
            };
            var replacementTextBox = new TextBox
            {
                Header = "With",
                PlaceholderText = "e.g. support@example.com"
            };
            var replacementPanel = new StackPanel
            {
                Spacing = 8,
                Visibility = Visibility.Collapsed
            };
            replacementPanel.Children.Add(replacementOriginalTextBox);
            replacementPanel.Children.Add(replacementTextBox);

            var statusTextBlock = new TextBlock
            {
                TextWrapping = TextWrapping.Wrap
            };
            var content = new StackPanel
            {
                Spacing = 12
            };
            content.Children.Add(modeComboBox);
            content.Children.Add(vocabularyTextBox);
            content.Children.Add(replacementPanel);
            content.Children.Add(statusTextBlock);

            modeComboBox.SelectionChanged += (_, _) =>
            {
                var replacementMode = modeComboBox.SelectedIndex == 1;
                vocabularyTextBox.Visibility = replacementMode ? Visibility.Collapsed : Visibility.Visible;
                replacementPanel.Visibility = replacementMode ? Visibility.Visible : Visibility.Collapsed;

                if (replacementMode)
                {
                    replacementOriginalTextBox.Focus(FocusState.Programmatic);
                }
                else
                {
                    vocabularyTextBox.Focus(FocusState.Programmatic);
                }
            };
            vocabularyTextBox.Loaded += (_, _) => vocabularyTextBox.Focus(FocusState.Programmatic);

            var dialog = new ContentDialog
            {
                XamlRoot = Content.XamlRoot,
                Title = "Quick Add to Dictionary",
                Content = content,
                PrimaryButtonText = "Add",
                SecondaryButtonText = "Cancel",
                DefaultButton = ContentDialogButton.Primary
            };
            dialog.PrimaryButtonClick += async (_, args) =>
            {
                var deferral = args.GetDeferral();
                try
                {
                    var mode = modeComboBox.SelectedIndex == 1
                        ? DictionaryQuickAddMode.WordReplacement
                        : DictionaryQuickAddMode.Vocabulary;
                    var result = await dictionaryQuickAddService.SubmitAsync(
                        mode,
                        vocabularyTextBox.Text,
                        replacementOriginalTextBox.Text,
                        replacementTextBox.Text,
                        windowLifetime.Token);
                    lastResult = result;
                    if (!result.Succeeded)
                    {
                        args.Cancel = true;
                        statusTextBlock.Text = result.Message;
                        return;
                    }

                    await RefreshDictionaryAsync(windowLifetime.Token);
                }
                catch (OperationCanceledException) when (windowLifetime.IsCancellationRequested)
                {
                    args.Cancel = true;
                    statusTextBlock.Text = "Closing";
                }
                catch (Exception ex)
                {
                    args.Cancel = true;
                    statusTextBlock.Text = $"Quick add failed: {ex.Message}";
                }
                finally
                {
                    deferral.Complete();
                }
            };

            var dialogResult = await dialog.ShowAsync();
            if (lastResult?.Succeeded == true)
            {
                finalStatus = lastResult.Message;
            }
            else if (dialogResult == ContentDialogResult.Secondary)
            {
                finalStatus = "Quick add canceled";
            }
        }
        catch (OperationCanceledException) when (windowLifetime.IsCancellationRequested)
        {
            finalStatus = "Closing";
        }
        catch (Exception ex)
        {
            finalStatus = $"Quick add failed: {ex.Message}";
        }
        finally
        {
            isQuickAdding = false;
            RefreshUiFromControllerState(finalStatus);
        }
    }

    private async Task EditSelectedReplacementAsync()
    {
        var selectedIndex = ReplacementListView.SelectedIndex;
        if (!settingsLoaded || selectedIndex < 0 || selectedIndex >= replacementItems.Count)
        {
            RefreshUiFromControllerState("Select a word replacement to edit");
            return;
        }

        var selected = replacementItems[selectedIndex];
        var statusTextBlock = new TextBlock
        {
            TextWrapping = TextWrapping.Wrap
        };
        var originalTextBox = new TextBox
        {
            Header = "Original text",
            Text = selected.OriginalText
        };
        var replacementTextBox = new TextBox
        {
            Header = "Replacement text",
            Text = selected.ReplacementText,
            AcceptsReturn = true,
            TextWrapping = TextWrapping.Wrap,
            Height = 92
        };
        var enabledCheckBox = new CheckBox
        {
            Content = "Enabled",
            IsChecked = selected.IsEnabled
        };
        var content = new StackPanel
        {
            Spacing = 12
        };
        content.Children.Add(originalTextBox);
        content.Children.Add(replacementTextBox);
        content.Children.Add(enabledCheckBox);
        content.Children.Add(statusTextBlock);

        var dialog = new ContentDialog
        {
            XamlRoot = Content.XamlRoot,
            Title = "Edit Word Replacement",
            Content = content,
            PrimaryButtonText = "Save",
            SecondaryButtonText = "Cancel",
            DefaultButton = ContentDialogButton.Primary
        };
        dialog.PrimaryButtonClick += async (_, args) =>
        {
            var deferral = args.GetDeferral();
            try
            {
                var error = await dictionaryStore.UpdateWordReplacementAsync(
                    selected.Id,
                    originalTextBox.Text,
                    replacementTextBox.Text,
                    enabledCheckBox.IsChecked == true,
                    windowLifetime.Token);
                if (error is not null)
                {
                    args.Cancel = true;
                    statusTextBlock.Text = error;
                    return;
                }

                await RefreshDictionaryAsync(windowLifetime.Token);
                SelectReplacementItem(selected.Id);
            }
            catch (OperationCanceledException) when (windowLifetime.IsCancellationRequested)
            {
                args.Cancel = true;
                statusTextBlock.Text = "Closing";
            }
            catch (Exception ex)
            {
                args.Cancel = true;
                statusTextBlock.Text = $"Word replacement update failed: {ex.Message}";
            }
            finally
            {
                deferral.Complete();
            }
        };

        var result = await dialog.ShowAsync();
        RefreshUiFromControllerState(result == ContentDialogResult.Primary
            ? "Word replacements updated"
            : "Word replacement edit canceled");
    }

    private async Task ToggleSelectedReplacementAsync()
    {
        var selectedIndex = ReplacementListView.SelectedIndex;
        if (!settingsLoaded || selectedIndex < 0 || selectedIndex >= replacementItems.Count)
        {
            RefreshUiFromControllerState("Select a word replacement to update");
            return;
        }

        var selected = replacementItems[selectedIndex];
        try
        {
            var error = await dictionaryStore.UpdateWordReplacementAsync(
                selected.Id,
                selected.OriginalText,
                selected.ReplacementText,
                !selected.IsEnabled,
                windowLifetime.Token);
            if (error is not null)
            {
                RefreshUiFromControllerState(error);
                return;
            }

            await RefreshDictionaryAsync(windowLifetime.Token);
            SelectReplacementItem(selected.Id);
            RefreshUiFromControllerState(selected.IsEnabled ? "Word replacement disabled" : "Word replacement enabled");
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

    private async Task ChooseAudioFilesAsync()
    {
        if (!CanEditAudioFileQueue())
        {
            return;
        }

        try
        {
            var picker = new FileOpenPicker
            {
                SuggestedStartLocation = PickerLocationId.MusicLibrary
            };
            foreach (var extension in AudioFileQueueService.SupportedExtensions)
            {
                picker.FileTypeFilter.Add(extension);
            }

            InitializeWithWindow.Initialize(picker, WindowNative.GetWindowHandle(this));
            var files = await picker.PickMultipleFilesAsync();
            if (files.Count == 0)
            {
                RefreshUiFromControllerState("Audio file selection canceled");
                return;
            }

            var update = audioFileQueueService.AddFiles(audioFileQueueItems, files.Select(file => file.Path));
            audioFileQueueItems = update.Items;
            RefreshAudioFileQueueListView();

            var status = update.AddedCount == 0
                ? "No supported audio files added"
                : $"Added {update.AddedCount} audio file{Plural(update.AddedCount)}";
            if (update.SkippedCount > 0)
            {
                status += $" ({update.SkippedCount} skipped)";
            }

            RefreshUiFromControllerState(status);
        }
        catch (OperationCanceledException) when (windowLifetime.IsCancellationRequested)
        {
            RefreshUiFromControllerState("Closing");
        }
        catch (Exception ex)
        {
            RefreshUiFromControllerState($"Audio file selection failed: {ex.Message}");
        }
    }

    private async Task StartAudioFileQueueAsync()
    {
        if (!CanEditAudioFileQueue() || !audioFileQueueItems.Any(item => item.Status == AudioFileQueueStatus.Pending))
        {
            return;
        }

        var statusOverride = "Transcribing audio files";
        isTranscribingAudioFiles = true;
        audioFileQueueCancellation = CancellationTokenSource.CreateLinkedTokenSource(windowLifetime.Token);
        RefreshUiFromControllerState(statusOverride);

        try
        {
            await SaveSettingsAsync(audioFileQueueCancellation.Token);

            while (audioFileQueueItems.FirstOrDefault(item => item.Status == AudioFileQueueStatus.Pending) is { } pending)
            {
                audioFileQueueCancellation.Token.ThrowIfCancellationRequested();
                await ProcessAudioFileQueueItemAsync(pending, audioFileQueueCancellation.Token);
            }

            await RefreshHistoryAsync(windowLifetime.Token);
            statusOverride = "Audio file queue processed";
        }
        catch (OperationCanceledException) when (windowLifetime.IsCancellationRequested)
        {
            statusOverride = "Closing";
        }
        catch (OperationCanceledException)
        {
            ResetProcessingAudioFileItems();
            statusOverride = "Audio transcription canceled";
        }
        catch (Exception ex)
        {
            statusOverride = $"Audio transcription failed: {ex.Message}";
        }
        finally
        {
            isTranscribingAudioFiles = false;
            audioFileQueueCancellation?.Dispose();
            audioFileQueueCancellation = null;
            RefreshAudioFileQueueListView();
            RefreshUiFromControllerState(statusOverride);
        }
    }

    private async Task ProcessAudioFileQueueItemAsync(
        AudioFileQueueItem item,
        CancellationToken cancellationToken)
    {
        UpdateAudioFileQueueItem(item.Id, current => current.MarkProcessing("Transcribing"));
        RefreshAudioFileQueueListView(item.Id);

        var result = await audioFileTranscriptionService.TranscribeAsync(
            item.FilePath,
            recordingsDirectory,
            cancellationToken);
        UpdateAudioFileQueueItem(
            item.Id,
            current => result.Success && result.Item is not null
                ? current.MarkCompleted(result.Item)
                : current.MarkFailed(result.Message));
        RefreshAudioFileQueueListView(item.Id);
    }

    private void UpdateAudioFileQueueItem(
        Guid id,
        Func<AudioFileQueueItem, AudioFileQueueItem> update)
    {
        audioFileQueueItems = audioFileQueueItems
            .Select(item => item.Id == id ? update(item) : item)
            .ToArray();
    }

    private void ResetProcessingAudioFileItems()
    {
        audioFileQueueItems = audioFileQueueItems
            .Select(item => item.Status == AudioFileQueueStatus.Processing ? item.MarkPending() : item)
            .ToArray();
    }

    private void RefreshAudioFileQueueListView(Guid? selectedId = null)
    {
        selectedId ??= SelectedAudioFileQueueItem()?.Id;
        AudioFileQueueListView.ItemsSource = audioFileQueueItems
            .Select(AudioFileQueueListItem)
            .ToArray();

        var selectedIndex = selectedId is null
            ? -1
            : audioFileQueueItems.ToList().FindIndex(item => item.Id == selectedId.Value);
        AudioFileQueueListView.SelectedIndex = selectedIndex;
        RefreshSelectedAudioFileQueueDetails();
    }

    private void RefreshSelectedAudioFileQueueDetails()
    {
        var item = SelectedAudioFileQueueItem();
        if (item is null)
        {
            AudioFileQueueMetadataTextBlock.Text = audioFileQueueItems.Count == 0
                ? "No files queued"
                : "Select a queued file";
            AudioFileQueueTranscriptTextBox.Text = string.Empty;
            return;
        }

        AudioFileQueueMetadataTextBlock.Text = string.Join(
            Environment.NewLine,
            $"File: {item.FileName}",
            $"Status: {item.Status}",
            $"Detail: {item.ErrorMessage ?? item.StatusDetail}",
            $"Path: {item.FilePath}");
        AudioFileQueueTranscriptTextBox.Text = item.HistoryItem?.Text ?? string.Empty;
    }

    private AudioFileQueueItem? SelectedAudioFileQueueItem() =>
        AudioFileQueueListView.SelectedIndex >= 0 && AudioFileQueueListView.SelectedIndex < audioFileQueueItems.Count
            ? audioFileQueueItems[AudioFileQueueListView.SelectedIndex]
            : null;

    private static string AudioFileQueueListItem(AudioFileQueueItem item)
    {
        var detail = item.ErrorMessage ?? item.StatusDetail;
        return $"{item.Status}: {item.FileName} - {detail}";
    }

    private bool CanEditAudioFileQueue() =>
        settingsLoaded
        && !IsOperationActive()
        && !IsControllerBusy()
        && controller.State != DictationState.Recording;

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
        var item = SelectedHistoryItem();
        if (item is null)
        {
            RefreshUiFromControllerState("Select a transcription to delete");
            return;
        }

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
            ClearHistoryAudioPlayer();
            var deleted = await historyStore.DeleteAsync(item.Id, windowLifetime.Token);
            TryDeleteHistoryAudioFile(item);
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

    private async Task RetrySelectedHistoryAsync()
    {
        if (isRetryingHistory)
        {
            return;
        }

        var item = SelectedHistoryItem();
        if (item is null)
        {
            RefreshUiFromControllerState("Select a transcription to retry");
            return;
        }

        if (SelectedHistoryAudioPath() is null)
        {
            RefreshUiFromControllerState("Audio file not found");
            return;
        }

        var statusOverride = "Retrying transcription";
        isRetryingHistory = true;
        RefreshUiFromControllerState(statusOverride);

        try
        {
            await SaveSettingsAsync(windowLifetime.Token);
            var result = await historyRetryService.RetryAsync(item, windowLifetime.Token);
            await RefreshHistoryAsync(windowLifetime.Token);
            if (result.Item is not null)
            {
                SelectHistoryItem(result.Item.Id);
            }

            statusOverride = result.Message;
        }
        catch (OperationCanceledException) when (windowLifetime.IsCancellationRequested)
        {
            statusOverride = "Closing";
        }
        catch (Exception ex)
        {
            statusOverride = $"Retry failed: {ex.Message}";
        }
        finally
        {
            isRetryingHistory = false;
            RefreshUiFromControllerState(statusOverride);
        }
    }

    private async Task OpenHistoryWindowAsync()
    {
        if (!settingsLoaded
            || isStarting
            || isStopping
            || isCanceling
            || isPastingLast
            || isRetryingHistory
            || isQuickAdding)
        {
            return;
        }

        try
        {
            RestoreAndActivateWindow();
            ShowShellSection(HistorySectionTag);
            await RefreshHistoryAsync(windowLifetime.Token);
            HistorySearchTextBox.Focus(FocusState.Programmatic);
            RefreshUiFromControllerState("History opened");
        }
        catch (OperationCanceledException) when (windowLifetime.IsCancellationRequested)
        {
            RefreshUiFromControllerState("Closing");
        }
        catch (Exception ex)
        {
            RefreshUiFromControllerState($"Open history failed: {ex.Message}");
        }
    }

    private async Task RetryLastHistoryAsync()
    {
        if (!settingsLoaded
            || isStarting
            || isStopping
            || isCanceling
            || isPastingLast
            || isRetryingHistory
            || controller.State == DictationState.Recording)
        {
            return;
        }

        var statusOverride = "Retrying last transcription";
        isRetryingHistory = true;
        RefreshUiFromControllerState(statusOverride);

        try
        {
            await SaveSettingsAsync(windowLifetime.Token);
            var result = await historyRetryService.RetryLatestAsync(windowLifetime.Token);
            if (result.Item is not null)
            {
                HistorySearchTextBox.Text = string.Empty;
                await RefreshHistoryAsync(windowLifetime.Token);
                SelectHistoryItem(result.Item.Id);
                await textInjectionService.CopyAsync(result.Item.Text, windowLifetime.Token);
                statusOverride = "Retry transcription copied";
            }
            else
            {
                statusOverride = result.Message;
            }
        }
        catch (OperationCanceledException) when (windowLifetime.IsCancellationRequested)
        {
            statusOverride = "Closing";
        }
        catch (Exception ex)
        {
            statusOverride = $"Retry last failed: {ex.Message}";
        }
        finally
        {
            isRetryingHistory = false;
            RefreshUiFromControllerState(statusOverride);
        }
    }

    private void OpenSelectedHistoryAudio()
    {
        var audioPath = SelectedHistoryAudioPath();
        if (audioPath is null)
        {
            RefreshUiFromControllerState("Audio file not found");
            return;
        }

        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "explorer.exe",
                Arguments = $"/select,\"{audioPath}\"",
                UseShellExecute = true
            });
            RefreshUiFromControllerState("Audio file opened");
        }
        catch (Exception ex)
        {
            RefreshUiFromControllerState($"Open audio failed: {ex.Message}");
        }
    }

    private async Task PasteLastAsync(LastTranscriptionTextKind textKind)
    {
        if (isStopping || isCanceling || isPastingLast || isRetryingHistory)
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
        var item = SelectedHistoryItem();
        if (item is null)
        {
            HistoryMetadataTextBlock.Text = historyItems.Count == 0
                ? "No transcriptions"
                : "Select a transcription";
            HistoryOriginalTextBox.Text = string.Empty;
            HistoryFinalTextBox.Text = string.Empty;
            HistoryEnhancedTextBox.Text = string.Empty;
            ClearHistoryAudioPlayer();
            RefreshUiFromControllerState();
            return;
        }

        var audioPath = SelectedHistoryAudioPath();
        var audioStatus = string.IsNullOrWhiteSpace(item.AudioFilePath)
            ? "Not recorded"
            : audioPath is null
                ? $"Missing: {item.AudioFilePath}"
                : audioPath;
        HistoryMetadataTextBlock.Text = string.Join(
            Environment.NewLine,
            $"Status: {item.Status}",
            $"Provider: {item.ProviderName}",
            $"Language: {item.Language}",
            $"Model: {item.ModelPath ?? "Not recorded"}",
            $"Prompt: {item.PromptName ?? "None"}",
            $"Power Mode: {PowerModeDisplay(item.PowerModeName, item.PowerModeEmoji)}",
            $"Recorded: {item.CreatedAt.LocalDateTime:g}",
            $"Audio: {Seconds(item.AudioDuration)}s",
            $"Audio file: {audioStatus}",
            $"Transcription: {Seconds(item.TranscriptionDuration)}s",
            $"Enhancement: {(item.EnhancementDuration is null ? "None" : $"{Seconds(item.EnhancementDuration.Value)}s")}",
            $"Error: {item.ErrorMessage ?? "None"}");
        HistoryOriginalTextBox.Text = item.OriginalText;
        HistoryFinalTextBox.Text = item.Text;
        HistoryEnhancedTextBox.Text = item.EnhancedText ?? string.Empty;
        RefreshHistoryAudioPlayer(audioPath);
        RefreshUiFromControllerState();
    }

    private TranscriptionHistoryItem? SelectedHistoryItem() =>
        HistoryListView.SelectedIndex >= 0 && HistoryListView.SelectedIndex < historyItems.Count
            ? historyItems[HistoryListView.SelectedIndex]
            : null;

    private string? SelectedHistoryAudioPath()
    {
        var item = SelectedHistoryItem();
        if (string.IsNullOrWhiteSpace(item?.AudioFilePath))
        {
            return null;
        }

        try
        {
            var fullPath = Path.GetFullPath(item.AudioFilePath);
            return File.Exists(fullPath) ? fullPath : null;
        }
        catch
        {
            return null;
        }
    }

    private void SelectHistoryItem(Guid id)
    {
        var selectedIndex = historyItems.ToList().FindIndex(item => item.Id == id);
        if (selectedIndex >= 0)
        {
            HistoryListView.SelectedIndex = selectedIndex;
            RefreshSelectedHistoryDetails();
        }
    }

    private void RefreshHistoryAudioPlayer(string? audioPath)
    {
        if (audioPath is null)
        {
            ClearHistoryAudioPlayer();
            return;
        }

        HistoryAudioPlayer.Source = MediaSource.CreateFromUri(new Uri(audioPath, UriKind.Absolute));
        HistoryAudioPlayer.Visibility = Visibility.Visible;
    }

    private void ClearHistoryAudioPlayer()
    {
        HistoryAudioPlayer.Source = null;
        HistoryAudioPlayer.Visibility = Visibility.Collapsed;
    }

    private void TryDeleteHistoryAudioFile(TranscriptionHistoryItem item)
    {
        if (string.IsNullOrWhiteSpace(item.AudioFilePath))
        {
            return;
        }

        try
        {
            var audioPath = Path.GetFullPath(item.AudioFilePath);
            if (!File.Exists(audioPath) || !IsUnderDirectory(audioPath, recordingsDirectory))
            {
                return;
            }

            File.Delete(audioPath);
        }
        catch
        {
            // Deleting the history row should not fail just because a stale audio file is locked.
        }
    }

    private static bool IsUnderDirectory(string candidatePath, string directoryPath)
    {
        var fullDirectory = Path.GetFullPath(directoryPath);
        if (!fullDirectory.EndsWith(Path.DirectorySeparatorChar))
        {
            fullDirectory += Path.DirectorySeparatorChar;
        }

        return candidatePath.StartsWith(fullDirectory, StringComparison.OrdinalIgnoreCase);
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

    private static string ReplacementListItem(WordReplacement replacement)
    {
        var prefix = replacement.IsEnabled ? string.Empty : "[Disabled] ";
        return $"{prefix}{replacement.OriginalText} -> {replacement.ReplacementText}";
    }

    private void SelectReplacementItem(Guid id)
    {
        var selectedIndex = replacementItems.ToList().FindIndex(item => item.Id == id);
        if (selectedIndex >= 0)
        {
            ReplacementListView.SelectedIndex = selectedIndex;
        }
    }

    private static string Seconds(TimeSpan duration) =>
        duration.TotalSeconds.ToString("0.000", CultureInfo.InvariantCulture);

    private static string Plural(int count) =>
        count == 1 ? string.Empty : "s";

    private Task MinimizeForExternalPasteAsync(CancellationToken cancellationToken)
    {
        var windowHandle = WindowNative.GetWindowHandle(this);
        if (windowHandle != IntPtr.Zero)
        {
            ShowWindow(windowHandle, ShowWindowMinimize);
        }

        return Task.Delay(TimeSpan.FromMilliseconds(150), cancellationToken);
    }

    private void HideWindowToTray()
    {
        var windowHandle = WindowNative.GetWindowHandle(this);
        if (windowHandle != IntPtr.Zero)
        {
            ShowWindow(windowHandle, ShowWindowHide);
        }

        RefreshUiFromControllerState();
    }

    private void RestoreAndActivateWindow()
    {
        var windowHandle = WindowNative.GetWindowHandle(this);
        if (windowHandle != IntPtr.Zero)
        {
            if (IsIconic(windowHandle))
            {
                ShowWindow(windowHandle, ShowWindowRestore);
            }
            else
            {
                ShowWindow(windowHandle, ShowWindowShow);
            }

            _ = SetForegroundWindow(windowHandle);
        }

        Activate();
    }

    private async Task ApplyTranscriptionProviderSettingsAsync()
    {
        if (!settingsLoaded || IsOperationActive())
        {
            return;
        }

        try
        {
            var settings = await CurrentSettingsAsync(windowLifetime.Token, includeShortcutFields: false);
            var configurationError = TranscriptionConfiguration.ValidateRequiredSettings(settings);
            if (configurationError is not null)
            {
                RefreshUiFromControllerState(configurationError);
                return;
            }

            await settingsStore.SaveAsync(settings, windowLifetime.Token);
            RefreshUiFromControllerState("Transcription provider settings updated");
        }
        catch (OperationCanceledException) when (windowLifetime.IsCancellationRequested)
        {
            RefreshUiFromControllerState("Closing");
        }
        catch (Exception ex)
        {
            RefreshUiFromControllerState($"Transcription provider settings update failed: {ex.Message}");
        }
    }

    private async Task SaveCloudTranscriptionKeyAsync()
    {
        if (!settingsLoaded || IsOperationActive(includeCurrentCloudTranscriptionKeySave: false))
        {
            return;
        }

        var secret = CloudTranscriptionApiKeyPasswordBox.Password.Trim();
        if (secret.Length == 0)
        {
            RefreshUiFromControllerState("Enter a cloud transcription API key to save");
            return;
        }

        isSavingCloudTranscriptionKey = true;
        var statusOverride = "Saving cloud transcription API key";
        RefreshUiFromControllerState(statusOverride);
        try
        {
            var providerId = SelectedCloudTranscriptionProviderId();
            await secretStore.SaveSecretAsync(
                TranscriptionConfiguration.SecretNameForCloudProvider(providerId),
                secret,
                windowLifetime.Token);
            CloudTranscriptionApiKeyPasswordBox.Password = string.Empty;
            await RefreshCloudTranscriptionKeyStatusAsync(windowLifetime.Token);
            statusOverride = $"{TranscriptionProviderPresetCatalog.Resolve(providerId).DisplayName} API key saved";
        }
        catch (OperationCanceledException) when (windowLifetime.IsCancellationRequested)
        {
            statusOverride = "Closing";
        }
        catch (Exception ex)
        {
            statusOverride = $"Cloud transcription API key save failed: {ex.Message}";
        }
        finally
        {
            isSavingCloudTranscriptionKey = false;
            RefreshUiFromControllerState(statusOverride);
        }
    }

    private async Task ClearCloudTranscriptionKeyAsync()
    {
        if (!settingsLoaded || IsOperationActive(includeCurrentCloudTranscriptionKeySave: false))
        {
            return;
        }

        isSavingCloudTranscriptionKey = true;
        var statusOverride = "Clearing cloud transcription API key";
        RefreshUiFromControllerState(statusOverride);
        try
        {
            var providerId = SelectedCloudTranscriptionProviderId();
            foreach (var secretName in TranscriptionConfiguration.SecretNamesForCloudProvider(providerId))
            {
                await secretStore.DeleteSecretAsync(secretName, windowLifetime.Token);
            }

            CloudTranscriptionApiKeyPasswordBox.Password = string.Empty;
            await RefreshCloudTranscriptionKeyStatusAsync(windowLifetime.Token);
            statusOverride = $"{TranscriptionProviderPresetCatalog.Resolve(providerId).DisplayName} API key cleared";
        }
        catch (OperationCanceledException) when (windowLifetime.IsCancellationRequested)
        {
            statusOverride = "Closing";
        }
        catch (Exception ex)
        {
            statusOverride = $"Cloud transcription API key clear failed: {ex.Message}";
        }
        finally
        {
            isSavingCloudTranscriptionKey = false;
            RefreshUiFromControllerState(statusOverride);
        }
    }

    private async Task ApplyEnhancementSettingsAsync()
    {
        if (!settingsLoaded || IsOperationActive())
        {
            return;
        }

        try
        {
            if (!TryParsePositiveInt(EnhancementTimeoutTextBox.Text, out var timeoutSeconds))
            {
                RefreshUiFromControllerState("Enhancement timeout must be a positive number.");
                return;
            }

            if (!TryParsePositiveInt(ShortEnhancementThresholdTextBox.Text, out var shortThreshold))
            {
                RefreshUiFromControllerState("Short enhancement threshold must be a positive number.");
                return;
            }

            EnhancementTimeoutTextBox.Text = timeoutSeconds.ToString(CultureInfo.InvariantCulture);
            ShortEnhancementThresholdTextBox.Text = shortThreshold.ToString(CultureInfo.InvariantCulture);
            await SaveSettingsAsync(windowLifetime.Token);
            RefreshUiFromControllerState("Enhancement settings updated");
        }
        catch (OperationCanceledException) when (windowLifetime.IsCancellationRequested)
        {
            RefreshUiFromControllerState("Closing");
        }
        catch (Exception ex)
        {
            RefreshUiFromControllerState($"Enhancement settings update failed: {ex.Message}");
        }
    }

    private async Task SaveEnhancementKeyAsync()
    {
        if (!settingsLoaded || IsOperationActive(includeCurrentEnhancementKeySave: false))
        {
            return;
        }

        var secret = EnhancementApiKeyPasswordBox.Password.Trim();
        if (secret.Length == 0)
        {
            RefreshUiFromControllerState("Enter an API key to save");
            return;
        }

        isSavingEnhancementKey = true;
        var statusOverride = "Saving enhancement API key";
        RefreshUiFromControllerState(statusOverride);
        try
        {
            await secretStore.SaveSecretAsync(
                OpenAICompatibleTextEnhancementService.SecretName,
                secret,
                windowLifetime.Token);
            EnhancementApiKeyPasswordBox.Password = string.Empty;
            await RefreshEnhancementKeyStatusAsync(windowLifetime.Token);
            statusOverride = "Enhancement API key saved";
        }
        catch (OperationCanceledException) when (windowLifetime.IsCancellationRequested)
        {
            statusOverride = "Closing";
        }
        catch (Exception ex)
        {
            statusOverride = $"Enhancement API key save failed: {ex.Message}";
        }
        finally
        {
            isSavingEnhancementKey = false;
            RefreshUiFromControllerState(statusOverride);
        }
    }

    private async Task ClearEnhancementKeyAsync()
    {
        if (!settingsLoaded || IsOperationActive(includeCurrentEnhancementKeySave: false))
        {
            return;
        }

        isSavingEnhancementKey = true;
        var statusOverride = "Clearing enhancement API key";
        RefreshUiFromControllerState(statusOverride);
        try
        {
            await secretStore.DeleteSecretAsync(
                OpenAICompatibleTextEnhancementService.SecretName,
                windowLifetime.Token);
            EnhancementApiKeyPasswordBox.Password = string.Empty;
            await RefreshEnhancementKeyStatusAsync(windowLifetime.Token);
            statusOverride = "Enhancement API key cleared";
        }
        catch (OperationCanceledException) when (windowLifetime.IsCancellationRequested)
        {
            statusOverride = "Closing";
        }
        catch (Exception ex)
        {
            statusOverride = $"Enhancement API key clear failed: {ex.Message}";
        }
        finally
        {
            isSavingEnhancementKey = false;
            RefreshUiFromControllerState(statusOverride);
        }
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
            ImportedWhisperModels = localWhisperModels.ToArray(),
            TranscriptionProvider = SelectedTranscriptionProvider(),
            CloudTranscriptionProviderId = SelectedCloudTranscriptionProviderId(),
            CloudTranscriptionEndpoint = CloudTranscriptionEndpointTextBox.Text.Trim(),
            CloudTranscriptionModel = CloudTranscriptionModelTextBox.Text.Trim(),
            Hotkey = includeShortcutFields ? RecordingHotkeyTextBox.Text.Trim() : settings.Hotkey,
            SecondaryRecordingHotkey = includeShortcutFields
                ? SecondaryRecordingHotkeyTextBox.Text.Trim()
                : settings.SecondaryRecordingHotkey,
            PasteLastTranscriptionHotkey = includeShortcutFields
                ? PasteLastHotkeyTextBox.Text.Trim()
                : settings.PasteLastTranscriptionHotkey,
            PasteLastEnhancementHotkey = includeShortcutFields
                ? PasteLastEnhancedHotkeyTextBox.Text.Trim()
                : settings.PasteLastEnhancementHotkey,
            RetryLastTranscriptionHotkey = includeShortcutFields
                ? RetryLastHotkeyTextBox.Text.Trim()
                : settings.RetryLastTranscriptionHotkey,
            CancelRecordingHotkey = includeShortcutFields
                ? CancelHotkeyTextBox.Text.Trim()
                : settings.CancelRecordingHotkey,
            OpenHistoryHotkey = includeShortcutFields
                ? OpenHistoryHotkeyTextBox.Text.Trim()
                : settings.OpenHistoryHotkey,
            QuickAddDictionaryHotkey = includeShortcutFields
                ? QuickAddHotkeyTextBox.Text.Trim()
                : settings.QuickAddDictionaryHotkey,
            AudioInputDeviceNumber = SelectedAudioInputDeviceNumber(),
            AudioInputDeviceName = SelectedAudioInputDeviceName(),
            IsEnhancementEnabled = EnhancementEnabledCheckBox.IsChecked == true,
            EnhancementEndpoint = EnhancementEndpointTextBox.Text.Trim(),
            EnhancementModel = EnhancementModelTextBox.Text.Trim(),
            SelectedEnhancementPromptId = SelectedEnhancementPromptId(),
            EnhancementTimeoutSeconds = ParsedPositiveOrDefault(EnhancementTimeoutTextBox.Text, 7),
            EnhancementRetryOnTimeout = EnhancementRetryOnTimeoutCheckBox.IsChecked == true,
            SkipShortEnhancement = SkipShortEnhancementCheckBox.IsChecked == true,
            ShortEnhancementWordThreshold = ParsedPositiveOrDefault(ShortEnhancementThresholdTextBox.Text, 3),
            RemoveFillerWords = RemoveFillerWordsCheckBox.IsChecked == true,
            LowercaseTranscription = LowercaseTranscriptionCheckBox.IsChecked == true,
            AppendTrailingSpace = AppendTrailingSpaceCheckBox.IsChecked == true,
            PunctuationCleanupMode = SelectedPunctuationCleanupMode(),
            PowerModeRules = powerModeRules.ToArray()
        };
    }

    private DictationController CreateController(NAudioCaptureService captureService) =>
        new(
            captureService,
            transcriptionService,
            textInjectionService,
            historyStore,
            settingsStore,
            dictionaryStore,
            textEnhancementPipeline,
            powerModeTargetProvider);

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

    private LocalWhisperModel? SelectedLocalWhisperModelChoice()
    {
        var selectedIndex = ModelComboBox.SelectedIndex;
        return selectedIndex >= 0 && selectedIndex < modelChoices.Count
            ? modelChoices[selectedIndex]
            : null;
    }

    private TranscriptionProviderKind SelectedTranscriptionProvider() =>
        TranscriptionProviderComboBox.SelectedIndex == 1
            ? TranscriptionProviderKind.OpenAICompatible
            : TranscriptionProviderKind.LocalWhisper;

    private string SelectedCloudTranscriptionProviderId() =>
        SelectedCloudTranscriptionPreset()?.Id ?? TranscriptionProviderPresetCatalog.Custom.Id;

    private TranscriptionProviderPreset? SelectedCloudTranscriptionPreset() =>
        CloudTranscriptionPresetComboBox.SelectedItem as TranscriptionProviderPreset;

    private Guid? SelectedEnhancementPromptId()
    {
        var selectedIndex = EnhancementPromptComboBox.SelectedIndex;
        return selectedIndex >= 0 && selectedIndex < enhancementPrompts.Count
            ? enhancementPrompts[selectedIndex].Id
            : EnhancementPromptCatalog.DefaultPromptId;
    }

    private void RefreshModelChoices(string? selectedPath = null)
    {
        var modelPath = selectedPath ?? ModelPathTextBox.Text;
        modelChoices = LocalWhisperModelService.BuildChoices(
            new AppSettings
            {
                ModelPath = modelPath,
                ImportedWhisperModels = localWhisperModels.ToArray()
            });
        ModelComboBox.ItemsSource = modelChoices;

        var trimmedPath = modelPath.Trim();
        ModelComboBox.SelectedIndex = string.IsNullOrWhiteSpace(trimmedPath)
            ? -1
            : modelChoices.ToList().FindIndex(model =>
                string.Equals(model.Path, trimmedPath, StringComparison.OrdinalIgnoreCase));
    }

    private void RefreshEnhancementPromptChoices(Guid? selectedPromptId)
    {
        EnhancementPromptComboBox.ItemsSource = enhancementPrompts
            .Select(prompt => prompt.Title)
            .ToArray();

        var promptId = selectedPromptId ?? EnhancementPromptCatalog.DefaultPromptId;
        var selectedIndex = enhancementPrompts.ToList().FindIndex(prompt => prompt.Id == promptId);
        EnhancementPromptComboBox.SelectedIndex = selectedIndex >= 0 ? selectedIndex : 0;
    }

    private void RefreshPowerModePromptChoices(Guid? selectedPromptId)
    {
        PowerModePromptOverrideComboBox.ItemsSource = new[] { "Keep base" }
            .Concat(enhancementPrompts.Select(prompt => prompt.Title))
            .ToArray();
        if (selectedPromptId is null)
        {
            PowerModePromptOverrideComboBox.SelectedIndex = 0;
            return;
        }

        var selectedIndex = enhancementPrompts.ToList().FindIndex(prompt => prompt.Id == selectedPromptId.Value);
        PowerModePromptOverrideComboBox.SelectedIndex = selectedIndex >= 0 ? selectedIndex + 1 : 0;
    }

    private async Task RefreshPowerModeActiveTargetAsync(bool fillRuleFields)
    {
        try
        {
            var target = await powerModeTargetProvider.GetCurrentTargetAsync(windowLifetime.Token);
            if (target is null)
            {
                PowerModeActiveWindowTextBlock.Text = "Active window unavailable";
                RefreshUiFromControllerState("Active window unavailable");
                return;
            }

            PowerModeActiveWindowTextBlock.Text = $"Process: {target.ProcessName}; Title: {target.WindowTitle}";
            if (fillRuleFields)
            {
                PowerModeProcessTextBox.Text = target.ProcessName;
                PowerModeWindowTitleTextBox.Text = target.WindowTitle;
            }

            RefreshUiFromControllerState("Active window refreshed");
        }
        catch (OperationCanceledException) when (windowLifetime.IsCancellationRequested)
        {
            RefreshUiFromControllerState("Closing");
        }
        catch (Exception ex)
        {
            PowerModeActiveWindowTextBlock.Text = $"Active window failed: {ex.Message}";
            RefreshUiFromControllerState($"Active window failed: {ex.Message}");
        }
    }

    private async Task AddPowerModeRuleAsync()
    {
        if (!CanEditPowerModeRules())
        {
            return;
        }

        var rule = PowerModeRuleFromForm(existing: null);
        powerModeRules = NormalizeDefaultRule(powerModeRules.Concat([rule]).ToArray(), rule);
        await SaveSettingsAsync(windowLifetime.Token);
        RefreshPowerModeRulesListView(rule.Id);
        RefreshUiFromControllerState("Power Mode rule added");
    }

    private async Task UpdateSelectedPowerModeRuleAsync()
    {
        if (!CanEditPowerModeRules() || SelectedPowerModeRule() is not { } selectedRule)
        {
            return;
        }

        var updatedRule = PowerModeRuleFromForm(selectedRule);
        powerModeRules = NormalizeDefaultRule(
            powerModeRules.Select(rule => rule.Id == selectedRule.Id ? updatedRule : rule).ToArray(),
            updatedRule);
        await SaveSettingsAsync(windowLifetime.Token);
        RefreshPowerModeRulesListView(updatedRule.Id);
        RefreshUiFromControllerState("Power Mode rule updated");
    }

    private async Task RemoveSelectedPowerModeRuleAsync()
    {
        if (!CanEditPowerModeRules() || SelectedPowerModeRule() is not { } selectedRule)
        {
            return;
        }

        powerModeRules = powerModeRules.Where(rule => rule.Id != selectedRule.Id).ToArray();
        await SaveSettingsAsync(windowLifetime.Token);
        RefreshPowerModeRulesListView();
        RefreshUiFromControllerState("Power Mode rule removed");
    }

    private async Task MoveSelectedPowerModeRuleAsync(int direction)
    {
        if (!CanEditPowerModeRules() || SelectedPowerModeRule() is not { } selectedRule)
        {
            return;
        }

        var currentIndex = PowerModeRulesListView.SelectedIndex;
        var targetIndex = currentIndex + direction;
        if (targetIndex < 0 || targetIndex >= powerModeRules.Count)
        {
            return;
        }

        var rules = powerModeRules.ToList();
        rules.RemoveAt(currentIndex);
        rules.Insert(targetIndex, selectedRule);
        powerModeRules = rules;
        await SaveSettingsAsync(windowLifetime.Token);
        RefreshPowerModeRulesListView(selectedRule.Id);
        RefreshUiFromControllerState("Power Mode rules reordered");
    }

    private void RefreshPowerModeRulesListView(Guid? selectedId = null)
    {
        selectedId ??= SelectedPowerModeRule()?.Id;
        PowerModeRulesListView.ItemsSource = powerModeRules
            .Select(PowerModeRuleListItem)
            .ToArray();

        PowerModeRulesListView.SelectedIndex = selectedId is null
            ? -1
            : powerModeRules.ToList().FindIndex(rule => rule.Id == selectedId.Value);
        FillPowerModeFormFromSelection();
    }

    private void FillPowerModeFormFromSelection()
    {
        var rule = SelectedPowerModeRule();
        PowerModeNameTextBox.Text = rule?.Name ?? string.Empty;
        PowerModeEmojiTextBox.Text = rule?.Emoji ?? string.Empty;
        PowerModeProcessTextBox.Text = rule?.ProcessNamePattern ?? string.Empty;
        PowerModeWindowTitleTextBox.Text = rule?.WindowTitlePattern ?? string.Empty;
        PowerModeEnabledCheckBox.IsChecked = rule?.IsEnabled ?? true;
        PowerModeDefaultCheckBox.IsChecked = rule?.IsDefault ?? false;
        PowerModeModelPathTextBox.Text = rule?.ModelPathOverride ?? string.Empty;
        PowerModeLanguageTextBox.Text = rule?.LanguageOverride ?? string.Empty;
        PowerModeEnhancementOverrideComboBox.SelectedIndex = rule?.IsEnhancementEnabledOverride switch
        {
            true => 1,
            false => 2,
            _ => 0
        };
        RefreshPowerModePromptChoices(rule?.SelectedEnhancementPromptIdOverride);
        PowerModeAppendTrailingSpaceCheckBox.IsChecked = rule?.AppendTrailingSpaceOverride;
        PowerModeRemoveFillerWordsCheckBox.IsChecked = rule?.RemoveFillerWordsOverride;
        PowerModeLowercaseCheckBox.IsChecked = rule?.LowercaseTranscriptionOverride;
        PowerModePunctuationCleanupComboBox.SelectedIndex = rule?.PunctuationCleanupModeOverride switch
        {
            PunctuationCleanupMode.Keep => 1,
            PunctuationCleanupMode.RemoveAll => 2,
            PunctuationCleanupMode.RemoveTrailingPeriod => 3,
            _ => 0
        };
    }

    private PowerModeRule PowerModeRuleFromForm(PowerModeRule? existing)
    {
        var name = PowerModeNameTextBox.Text.Trim();
        var emoji = PowerModeEmojiTextBox.Text.Trim();
        return new PowerModeRule
        {
            Id = existing?.Id ?? Guid.NewGuid(),
            Name = string.IsNullOrWhiteSpace(name) ? "New Power Mode" : name,
            Emoji = string.IsNullOrWhiteSpace(emoji) ? "*" : emoji,
            IsEnabled = PowerModeEnabledCheckBox.IsChecked == true,
            IsDefault = PowerModeDefaultCheckBox.IsChecked == true,
            ProcessNamePattern = PowerModeProcessTextBox.Text.Trim(),
            WindowTitlePattern = PowerModeWindowTitleTextBox.Text.Trim(),
            ModelPathOverride = TrimToNull(PowerModeModelPathTextBox.Text),
            LanguageOverride = TrimToNull(PowerModeLanguageTextBox.Text),
            IsEnhancementEnabledOverride = SelectedEnhancementOverride(),
            SelectedEnhancementPromptIdOverride = SelectedPowerModePromptOverrideId(),
            AppendTrailingSpaceOverride = PowerModeAppendTrailingSpaceCheckBox.IsChecked,
            RemoveFillerWordsOverride = PowerModeRemoveFillerWordsCheckBox.IsChecked,
            LowercaseTranscriptionOverride = PowerModeLowercaseCheckBox.IsChecked,
            PunctuationCleanupModeOverride = SelectedPowerModePunctuationCleanupMode()
        };
    }

    private static IReadOnlyList<PowerModeRule> NormalizeDefaultRule(
        IReadOnlyList<PowerModeRule> rules,
        PowerModeRule changedRule) =>
        changedRule.IsDefault
            ? rules.Select(rule => rule.Id == changedRule.Id ? rule : rule with { IsDefault = false }).ToArray()
            : rules;

    private PowerModeRule? SelectedPowerModeRule() =>
        PowerModeRulesListView.SelectedIndex >= 0 && PowerModeRulesListView.SelectedIndex < powerModeRules.Count
            ? powerModeRules[PowerModeRulesListView.SelectedIndex]
            : null;

    private bool CanEditPowerModeRules() =>
        settingsLoaded
        && !IsOperationActive()
        && !IsControllerBusy()
        && controller.State != DictationState.Recording;

    private Guid? SelectedPowerModePromptOverrideId()
    {
        var selectedIndex = PowerModePromptOverrideComboBox.SelectedIndex;
        return selectedIndex > 0 && selectedIndex - 1 < enhancementPrompts.Count
            ? enhancementPrompts[selectedIndex - 1].Id
            : null;
    }

    private bool? SelectedEnhancementOverride() =>
        PowerModeEnhancementOverrideComboBox.SelectedIndex switch
        {
            1 => true,
            2 => false,
            _ => null
        };

    private PunctuationCleanupMode? SelectedPowerModePunctuationCleanupMode() =>
        PowerModePunctuationCleanupComboBox.SelectedIndex switch
        {
            1 => PunctuationCleanupMode.Keep,
            2 => PunctuationCleanupMode.RemoveAll,
            3 => PunctuationCleanupMode.RemoveTrailingPeriod,
            _ => null
        };

    private static string? TrimToNull(string value)
    {
        var trimmed = value.Trim();
        return string.IsNullOrWhiteSpace(trimmed) ? null : trimmed;
    }

    private static string PowerModeRuleListItem(PowerModeRule rule)
    {
        var enabled = rule.IsEnabled ? string.Empty : "Off - ";
        var target = rule.IsDefault
            ? "Default"
            : string.Join(
                ", ",
                new[]
                {
                    string.IsNullOrWhiteSpace(rule.ProcessNamePattern) ? null : $"Process: {rule.ProcessNamePattern}",
                    string.IsNullOrWhiteSpace(rule.WindowTitlePattern) ? null : $"Title: {rule.WindowTitlePattern}"
                }.Where(value => value is not null));
        if (string.IsNullOrWhiteSpace(target))
        {
            target = "No target";
        }

        return $"{enabled}{rule.Emoji} {rule.Name} - {target}";
    }

    private static string PowerModeDisplay(string? name, string? emoji)
    {
        var trimmedName = name?.Trim();
        var trimmedEmoji = emoji?.Trim();
        return (trimmedEmoji, trimmedName) switch
        {
            ({ Length: > 0 }, { Length: > 0 }) => $"{trimmedEmoji} {trimmedName}",
            ({ Length: > 0 }, _) => trimmedEmoji,
            (_, { Length: > 0 }) => trimmedName,
            _ => "None"
        };
    }

    private async Task RefreshEnhancementKeyStatusAsync(CancellationToken cancellationToken)
    {
        var hasKey = await secretStore.HasSecretAsync(
            OpenAICompatibleTextEnhancementService.SecretName,
            cancellationToken);
        EnhancementKeyStatusTextBlock.Text = hasKey
            ? "API key stored in Windows Credential Manager"
            : "No API key stored";
    }

    private async Task RefreshCloudTranscriptionKeyStatusAsync(CancellationToken cancellationToken)
    {
        var preset = TranscriptionProviderPresetCatalog.Resolve(SelectedCloudTranscriptionProviderId());
        var hasKey = await HasCloudTranscriptionApiKeyAsync(preset.Id, cancellationToken);
        CloudTranscriptionKeyStatusTextBlock.Text = hasKey
            ? $"{preset.DisplayName} API key stored in Windows Credential Manager"
            : $"No {preset.DisplayName} API key stored";
    }

    private async Task<bool> HasCloudTranscriptionApiKeyAsync(
        string providerId,
        CancellationToken cancellationToken)
    {
        foreach (var secretName in TranscriptionConfiguration.SecretNamesForCloudProvider(providerId))
        {
            if (await secretStore.HasSecretAsync(secretName, cancellationToken))
            {
                return true;
            }
        }

        return false;
    }

    private static int EnhancementTimeoutSeconds(AppSettings settings) =>
        settings.EnhancementTimeoutSeconds > 0 ? settings.EnhancementTimeoutSeconds : 7;

    private static int ShortEnhancementThreshold(AppSettings settings) =>
        settings.ShortEnhancementWordThreshold > 0 ? settings.ShortEnhancementWordThreshold : 3;

    private static int ParsedPositiveOrDefault(string text, int fallback) =>
        TryParsePositiveInt(text, out var value) ? value : fallback;

    private static bool TryParsePositiveInt(string text, out int value) =>
        int.TryParse(text.Trim(), NumberStyles.None, CultureInfo.InvariantCulture, out value)
        && value > 0;

    private static int TranscriptionProviderToSelectedIndex(TranscriptionProviderKind provider) =>
        provider == TranscriptionProviderKind.OpenAICompatible ? 1 : 0;

    private void SelectCloudTranscriptionPreset(string? providerId)
    {
        var preset = TranscriptionProviderPresetCatalog.Resolve(providerId);
        var index = TranscriptionProviderPresetCatalog.All.ToList().FindIndex(item => item.Id == preset.Id);
        CloudTranscriptionPresetComboBox.SelectedIndex = Math.Max(0, index);
    }

    private void ApplySelectedCloudTranscriptionPreset(bool fillConfiguration)
    {
        var preset = TranscriptionProviderPresetCatalog.Resolve(SelectedCloudTranscriptionProviderId());
        if (fillConfiguration && preset.Id != TranscriptionProviderPresetCatalog.Custom.Id)
        {
            CloudTranscriptionEndpointTextBox.Text = preset.Endpoint;
            if (string.IsNullOrWhiteSpace(CloudTranscriptionModelTextBox.Text)
                || !preset.ModelIds.Any(model => string.Equals(
                    model,
                    CloudTranscriptionModelTextBox.Text.Trim(),
                    StringComparison.OrdinalIgnoreCase)))
            {
                CloudTranscriptionModelTextBox.Text = preset.DefaultModel;
            }
        }

        RefreshCloudTranscriptionModelChoices(CloudTranscriptionModelTextBox.Text);
    }

    private void RefreshCloudTranscriptionModelChoices(string selectedModel)
    {
        var preset = TranscriptionProviderPresetCatalog.Resolve(SelectedCloudTranscriptionProviderId());
        suppressCloudTranscriptionModelChanged = true;
        CloudTranscriptionModelComboBox.ItemsSource = preset.ModelIds.ToArray();
        if (preset.ModelIds.Count == 0)
        {
            CloudTranscriptionModelComboBox.SelectedIndex = -1;
        }
        else
        {
            var index = preset.ModelIds.ToList().FindIndex(model => string.Equals(
                model,
                selectedModel.Trim(),
                StringComparison.OrdinalIgnoreCase));
            CloudTranscriptionModelComboBox.SelectedIndex = index >= 0 ? index : 0;
        }

        suppressCloudTranscriptionModelChanged = false;
    }

    private bool IsOperationActive(
        bool includeCurrentModelImport = true,
        bool includeCurrentEnhancementKeySave = true,
        bool includeCurrentCloudTranscriptionKeySave = true) =>
        isStarting
        || isStopping
        || isCanceling
        || isPastingLast
        || isRetryingHistory
        || isQuickAdding
        || isOnboardingOpen
        || isTranscribingAudioFiles
        || (includeCurrentEnhancementKeySave && isSavingEnhancementKey)
        || (includeCurrentCloudTranscriptionKeySave && isSavingCloudTranscriptionKey)
        || (includeCurrentModelImport && isImportingModel);

    private bool IsControllerBusy() =>
        controller.State is DictationState.Transcribing or DictationState.Inserting;

    private bool CanEditModelLibrary(bool includeCurrentModelImport = true) =>
        settingsLoaded
        && !IsOperationActive(includeCurrentModelImport)
        && !IsControllerBusy()
        && controller.State != DictationState.Recording;

    private void InitializeNavigationItems()
    {
        RootNavigationView.MenuItems.Clear();
        navigationItemsByTag.Clear();

        foreach (var item in ShellNavigationPresenter.BuildItems())
        {
            var navigationItem = CreateNavigationItem(item);
            RootNavigationView.MenuItems.Add(navigationItem);
            navigationItemsByTag[item.Tag] = navigationItem;
        }

        RootNavigationView.SelectedItem = navigationItemsByTag[DashboardSectionTag];
    }

    private static NavigationViewItem CreateNavigationItem(ShellNavigationItem item)
    {
        return new NavigationViewItem
        {
            Content = item.Label,
            Icon = new SymbolIcon(ParseNavigationSymbol(item.Icon)),
            IsEnabled = item.IsEnabled,
            Tag = item.Tag
        };
    }

    private void ShowShellSection(string tag)
    {
        activeSectionTag = tag;
        DashboardSectionPanel.Visibility = tag == DashboardSectionTag ? Visibility.Visible : Visibility.Collapsed;
        TranscribeAudioSectionPanel.Visibility = tag == TranscribeAudioSectionTag ? Visibility.Visible : Visibility.Collapsed;
        ModelsSectionPanel.Visibility = tag == ModelsSectionTag ? Visibility.Visible : Visibility.Collapsed;
        EnhancementSectionPanel.Visibility = tag == EnhancementSectionTag ? Visibility.Visible : Visibility.Collapsed;
        PowerModeSectionPanel.Visibility = tag == PowerModeSectionTag ? Visibility.Visible : Visibility.Collapsed;
        AudioInputSectionPanel.Visibility = tag == AudioInputSectionTag ? Visibility.Visible : Visibility.Collapsed;
        DictionarySectionPanel.Visibility = tag == DictionarySectionTag ? Visibility.Visible : Visibility.Collapsed;
        HistorySectionPanel.Visibility = tag == HistorySectionTag ? Visibility.Visible : Visibility.Collapsed;
        SettingsSectionPanel.Visibility = tag == SettingsSectionTag ? Visibility.Visible : Visibility.Collapsed;
        AboutSectionPanel.Visibility = tag == AboutSectionTag ? Visibility.Visible : Visibility.Collapsed;

        if (navigationItemsByTag.TryGetValue(tag, out var navigationItem) &&
            !ReferenceEquals(RootNavigationView.SelectedItem, navigationItem))
        {
            RootNavigationView.SelectedItem = navigationItem;
        }
    }

    private static Symbol ParseNavigationSymbol(string symbolName) =>
        Enum.TryParse<Symbol>(symbolName, ignoreCase: false, out var symbol)
            ? symbol
            : Symbol.Help;

    private void RefreshAboutSection()
    {
        var version = typeof(MainWindow).Assembly.GetName().Version?.ToString() ?? "source build";
        AboutVersionTextBlock.Text = $"VoiceInk for Windows {version}";
        RuntimePathTextBox.Text = AppContext.BaseDirectory;
    }

    private void OpenDiagnosticsFolder()
    {
        try
        {
            Directory.CreateDirectory(appDataDirectory);
            Process.Start(new ProcessStartInfo
            {
                FileName = appDataDirectory,
                UseShellExecute = true
            });
            RefreshUiFromControllerState("Diagnostics folder opened");
        }
        catch (Exception ex)
        {
            RefreshUiFromControllerState($"Diagnostics folder failed: {ex.Message}");
        }
    }

    private Task CopyDiagnosticsSummaryAsync()
    {
        try
        {
            var summary = string.Join(
                Environment.NewLine,
                "VoiceInk for Windows diagnostics",
                $"Version: {typeof(MainWindow).Assembly.GetName().Version?.ToString() ?? "source build"}",
                $"App data: {appDataDirectory}",
                $"Recordings: {recordingsDirectory}",
                $"Settings: {settingsPath}",
                $"History: {historyPath}",
                $"Dictionary: {dictionaryPath}",
                $"Active section: {activeSectionTag}",
                $"Dictation state: {controller.State}",
                $"Model path: {ModelPathTextBox.Text}");
            var package = new DataPackage();
            package.SetText(summary);
            Clipboard.SetContent(package);
            RefreshUiFromControllerState("Diagnostics summary copied");
        }
        catch (Exception ex)
        {
            RefreshUiFromControllerState($"Diagnostics copy failed: {ex.Message}");
        }

        return Task.CompletedTask;
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
        powerModeTargetProvider.ExcludeWindowHandle(windowHandle);
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
        var operationActive = IsOperationActive();
        var controllerBusy = IsControllerBusy();
        var modelControlsEnabled = CanEditModelLibrary();
        var cloudTranscriptionControlsEnabled = modelControlsEnabled
            && SelectedTranscriptionProvider() == TranscriptionProviderKind.OpenAICompatible;
        var cloudPresetHasModelChoices = SelectedCloudTranscriptionPreset()?.ModelIds.Count > 0;
        var enhancementControlsEnabled = settingsLoaded
            && !operationActive
            && !controllerBusy
            && controller.State != DictationState.Recording;
        var powerModeControlsEnabled = CanEditPowerModeRules();
        var historyAudioAvailable = SelectedHistoryAudioPath() is not null;
        var audioFileQueueEditable = CanEditAudioFileQueue();
        var selectedAudioFileQueueItem = SelectedAudioFileQueueItem();
        var hasPendingAudioFiles = audioFileQueueItems.Any(item => item.Status == AudioFileQueueStatus.Pending);
        var selectedReplacement = ReplacementListView.SelectedIndex >= 0 && ReplacementListView.SelectedIndex < replacementItems.Count
            ? replacementItems[ReplacementListView.SelectedIndex]
            : null;

        StartButton.IsEnabled = settingsLoaded
            && !operationActive
            && !controllerBusy
            && controller.State != DictationState.Recording;
        StopButton.IsEnabled = settingsLoaded
            && !operationActive
            && controller.State == DictationState.Recording;
        CancelButton.IsEnabled = settingsLoaded
            && !operationActive
            && controller.State == DictationState.Recording;
        PasteLastButton.IsEnabled = settingsLoaded
            && !operationActive
            && controller.State != DictationState.Recording;
        PasteLastEnhancedButton.IsEnabled = settingsLoaded
            && !operationActive
            && controller.State != DictationState.Recording;
        RetryLastButton.IsEnabled = settingsLoaded
            && !operationActive
            && !controllerBusy
            && controller.State != DictationState.Recording;
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
        ModelPathTextBox.IsEnabled = modelControlsEnabled;
        ModelComboBox.IsEnabled = modelControlsEnabled;
        ImportModelButton.IsEnabled = modelControlsEnabled;
        UseSelectedModelButton.IsEnabled = modelControlsEnabled && SelectedLocalWhisperModelChoice() is not null;
        OpenModelDownloadsButton.IsEnabled = modelControlsEnabled;
        TranscriptionProviderComboBox.IsEnabled = modelControlsEnabled;
        CloudTranscriptionPresetComboBox.IsEnabled = cloudTranscriptionControlsEnabled;
        CloudTranscriptionEndpointTextBox.IsEnabled = cloudTranscriptionControlsEnabled;
        CloudTranscriptionModelTextBox.IsEnabled = cloudTranscriptionControlsEnabled;
        CloudTranscriptionModelComboBox.IsEnabled = cloudTranscriptionControlsEnabled && cloudPresetHasModelChoices;
        CloudTranscriptionApiKeyPasswordBox.IsEnabled = cloudTranscriptionControlsEnabled;
        SaveCloudTranscriptionKeyButton.IsEnabled = cloudTranscriptionControlsEnabled;
        ClearCloudTranscriptionKeyButton.IsEnabled = cloudTranscriptionControlsEnabled;
        ApplyTranscriptionProviderSettingsButton.IsEnabled = modelControlsEnabled;
        EnhancementEnabledCheckBox.IsEnabled = enhancementControlsEnabled;
        EnhancementEndpointTextBox.IsEnabled = enhancementControlsEnabled;
        EnhancementModelTextBox.IsEnabled = enhancementControlsEnabled;
        EnhancementApiKeyPasswordBox.IsEnabled = enhancementControlsEnabled;
        SaveEnhancementKeyButton.IsEnabled = enhancementControlsEnabled;
        ClearEnhancementKeyButton.IsEnabled = enhancementControlsEnabled;
        EnhancementPromptComboBox.IsEnabled = enhancementControlsEnabled;
        EnhancementTimeoutTextBox.IsEnabled = enhancementControlsEnabled;
        ShortEnhancementThresholdTextBox.IsEnabled = enhancementControlsEnabled;
        SkipShortEnhancementCheckBox.IsEnabled = enhancementControlsEnabled;
        EnhancementRetryOnTimeoutCheckBox.IsEnabled = enhancementControlsEnabled;
        ApplyEnhancementSettingsButton.IsEnabled = enhancementControlsEnabled;
        RefreshPowerModeTargetButton.IsEnabled = powerModeControlsEnabled;
        UsePowerModeTargetButton.IsEnabled = powerModeControlsEnabled;
        PowerModeRulesListView.IsEnabled = powerModeControlsEnabled;
        PowerModeNameTextBox.IsEnabled = powerModeControlsEnabled;
        PowerModeEmojiTextBox.IsEnabled = powerModeControlsEnabled;
        PowerModeProcessTextBox.IsEnabled = powerModeControlsEnabled;
        PowerModeWindowTitleTextBox.IsEnabled = powerModeControlsEnabled;
        PowerModeEnabledCheckBox.IsEnabled = powerModeControlsEnabled;
        PowerModeDefaultCheckBox.IsEnabled = powerModeControlsEnabled;
        PowerModeModelPathTextBox.IsEnabled = powerModeControlsEnabled;
        PowerModeLanguageTextBox.IsEnabled = powerModeControlsEnabled;
        PowerModeEnhancementOverrideComboBox.IsEnabled = powerModeControlsEnabled;
        PowerModePromptOverrideComboBox.IsEnabled = powerModeControlsEnabled;
        PowerModeAppendTrailingSpaceCheckBox.IsEnabled = powerModeControlsEnabled;
        PowerModeRemoveFillerWordsCheckBox.IsEnabled = powerModeControlsEnabled;
        PowerModeLowercaseCheckBox.IsEnabled = powerModeControlsEnabled;
        PowerModePunctuationCleanupComboBox.IsEnabled = powerModeControlsEnabled;
        AddPowerModeRuleButton.IsEnabled = powerModeControlsEnabled;
        UpdatePowerModeRuleButton.IsEnabled = powerModeControlsEnabled && SelectedPowerModeRule() is not null;
        RemovePowerModeRuleButton.IsEnabled = powerModeControlsEnabled && SelectedPowerModeRule() is not null;
        MovePowerModeRuleUpButton.IsEnabled = powerModeControlsEnabled && PowerModeRulesListView.SelectedIndex > 0;
        MovePowerModeRuleDownButton.IsEnabled = powerModeControlsEnabled
            && PowerModeRulesListView.SelectedIndex >= 0
            && PowerModeRulesListView.SelectedIndex < powerModeRules.Count - 1;
        ChooseAudioFilesButton.IsEnabled = audioFileQueueEditable;
        StartAudioFileQueueButton.IsEnabled = audioFileQueueEditable && hasPendingAudioFiles;
        CancelAudioFileQueueButton.IsEnabled = isTranscribingAudioFiles;
        ClearAudioFileQueueButton.IsEnabled = settingsLoaded && !isTranscribingAudioFiles && audioFileQueueItems.Count > 0;
        RemoveAudioFileQueueItemButton.IsEnabled = audioFileQueueEditable
            && selectedAudioFileQueueItem?.Status == AudioFileQueueStatus.Pending;
        RetryAudioFileQueueItemButton.IsEnabled = audioFileQueueEditable
            && selectedAudioFileQueueItem?.Status == AudioFileQueueStatus.Failed;
        ExportDictionaryButton.IsEnabled = settingsLoaded && !operationActive;
        ImportDictionaryButton.IsEnabled = settingsLoaded
            && !operationActive
            && !controllerBusy
            && controller.State != DictationState.Recording;
        QuickAddDictionaryButton.IsEnabled = settingsLoaded
            && !operationActive
            && !controllerBusy
            && controller.State == DictationState.Idle;
        EditReplacementButton.IsEnabled = settingsLoaded
            && !operationActive
            && !controllerBusy
            && controller.State == DictationState.Idle
            && selectedReplacement is not null;
        ToggleReplacementButton.IsEnabled = settingsLoaded
            && !operationActive
            && !controllerBusy
            && controller.State == DictationState.Idle
            && selectedReplacement is not null;
        ToggleReplacementButton.Content = selectedReplacement?.IsEnabled == false ? "Enable" : "Disable";
        SearchHistoryButton.IsEnabled = settingsLoaded && !operationActive;
        ClearHistorySearchButton.IsEnabled = settingsLoaded && !operationActive;
        DeleteHistoryButton.IsEnabled = settingsLoaded && !operationActive;
        RetryHistoryButton.IsEnabled = settingsLoaded
            && !operationActive
            && !controllerBusy
            && controller.State != DictationState.Recording
            && historyAudioAvailable;
        OpenHistoryAudioButton.IsEnabled = settingsLoaded && !operationActive && historyAudioAvailable;

        var stateStatus = StateToStatusText(controller.State);
        var idleHotkeyWarning = controller.State == DictationState.Idle && !operationActive
            ? hotkeyRegistrationError
            : null;

        var displayStatus = statusOverride
            ?? controller.LastError
            ?? controller.LastWarning
            ?? idleHotkeyWarning
            ?? stateStatus;
        StatusTextBlock.Text = displayStatus;
        UpdateFloatingRecorder(displayStatus, operationActive);
        UpdateTrayFromControllerState(displayStatus, operationActive);
    }

    private void UpdateFloatingRecorder(string displayStatus, bool operationActive)
    {
        var recorderActivityActive = FloatingRecorderActivityPolicy.ShouldShowForActivity(
            controller.State,
            isStarting,
            isStopping,
            isCanceling);
        if (!recorderActivityActive)
        {
            recordingStartedAt = null;
        }

        var elapsed = recordingStartedAt is null
            ? TimeSpan.Zero
            : DateTimeOffset.Now - recordingStartedAt.Value;
        var state = FloatingRecorderPresenter.FromState(
            controller.State,
            elapsed,
            displayStatus,
            recorderActivityActive && operationActive);

        if (!state.IsVisible && floatingRecorderWindow is null)
        {
            UpdateFloatingRecorderRefreshTimer(state);
            return;
        }

        EnsureFloatingRecorderWindow().Apply(state);
        UpdateFloatingRecorderRefreshTimer(state);
    }

    private void RefreshFloatingRecorderFromTimer()
    {
        if (windowLifetime.IsCancellationRequested)
        {
            floatingRecorderRefreshTimer.Stop();
            return;
        }

        RefreshUiFromControllerState();
    }

    private void UpdateFloatingRecorderRefreshTimer(FloatingRecorderViewState state)
    {
        if (FloatingRecorderRefreshPolicy.ShouldRefresh(state))
        {
            if (!floatingRecorderRefreshTimer.IsRunning)
            {
                floatingRecorderRefreshTimer.Start();
            }

            return;
        }

        if (floatingRecorderRefreshTimer.IsRunning)
        {
            floatingRecorderRefreshTimer.Stop();
        }
    }

    private FloatingRecorderWindow EnsureFloatingRecorderWindow()
    {
        if (floatingRecorderWindow is not null)
        {
            return floatingRecorderWindow;
        }

        floatingRecorderWindow = new FloatingRecorderWindow();
        floatingRecorderWindow.Closed += (_, _) => floatingRecorderWindow = null;
        return floatingRecorderWindow;
    }

    private void UpdateTrayFromControllerState(string displayStatus, bool operationActive)
    {
        var trayState = TrayShellPresenter.FromState(
            settingsLoaded,
            controller.State,
            operationActive,
            displayStatus);
        trayIconService?.UpdateState(trayState);
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

    private string SelectedVocabularySortMode() =>
        VocabularySortComboBox.SelectedIndex == 1
            ? DictionarySortModes.VocabularyWordDescending
            : DictionarySortModes.VocabularyWordAscending;

    private string SelectedReplacementSortMode() =>
        ReplacementSortComboBox.SelectedIndex switch
        {
            1 => DictionarySortModes.ReplacementOriginalDescending,
            2 => DictionarySortModes.ReplacementTextAscending,
            3 => DictionarySortModes.ReplacementTextDescending,
            _ => DictionarySortModes.ReplacementOriginalAscending
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
        AppWindow.Closing -= MainWindow_AppWindowClosing;
        windowLifetime.Cancel();
        audioFileQueueCancellation?.Cancel();
        audioFileQueueCancellation?.Dispose();
        DisposeGlobalHotkeyService();
        DisposeTrayIconService();
        ClearHistoryAudioPlayer();
        floatingRecorderRefreshTimer.Stop();
        floatingRecorderWindow?.Close();

        audioCapture.Dispose();
        windowLifetime.Dispose();
    }

    private void MainWindow_AppWindowClosing(AppWindow sender, AppWindowClosingEventArgs args)
    {
        if (exitRequested || windowLifetime.IsCancellationRequested)
        {
            return;
        }

        args.Cancel = true;
        HideWindowToTray();
    }

    private void CreateTrayIconService()
    {
        trayIconService = new TrayIconService();
        trayIconService.ShowRequested += TrayIconService_ShowRequested;
        trayIconService.HideRequested += TrayIconService_HideRequested;
        trayIconService.ToggleRecordingRequested += TrayIconService_ToggleRecordingRequested;
        trayIconService.QuickAddDictionaryRequested += TrayIconService_QuickAddDictionaryRequested;
        trayIconService.OpenHistoryRequested += TrayIconService_OpenHistoryRequested;
        trayIconService.ExitRequested += TrayIconService_ExitRequested;
    }

    private void DisposeTrayIconService()
    {
        if (trayIconService is null)
        {
            return;
        }

        trayIconService.ShowRequested -= TrayIconService_ShowRequested;
        trayIconService.HideRequested -= TrayIconService_HideRequested;
        trayIconService.ToggleRecordingRequested -= TrayIconService_ToggleRecordingRequested;
        trayIconService.QuickAddDictionaryRequested -= TrayIconService_QuickAddDictionaryRequested;
        trayIconService.OpenHistoryRequested -= TrayIconService_OpenHistoryRequested;
        trayIconService.ExitRequested -= TrayIconService_ExitRequested;
        trayIconService.Dispose();
        trayIconService = null;
    }

    private const int ShowWindowHide = 0;
    private const int ShowWindowShow = 5;
    private const int ShowWindowRestore = 9;
    private const int ShowWindowMinimize = 6;

    [DllImport("user32.dll")]
    private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    [DllImport("user32.dll")]
    private static extern bool SetForegroundWindow(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern bool IsIconic(IntPtr hWnd);
}
