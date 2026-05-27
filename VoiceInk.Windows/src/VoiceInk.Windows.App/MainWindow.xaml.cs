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
using Microsoft.UI.Xaml.Input;
using Microsoft.Win32;
using Windows.ApplicationModel.DataTransfer;
using VoiceInk.Windows.Core.Audio;
using VoiceInk.Windows.Core.AudioFiles;
using VoiceInk.Windows.Core.Backup;
using VoiceInk.Windows.Core.Dictionary;
using VoiceInk.Windows.Core.Dictation;
using VoiceInk.Windows.Core.Diagnostics;
using VoiceInk.Windows.Core.Enhancement;
using VoiceInk.Windows.Core.History;
using VoiceInk.Windows.Core.Metrics;
using VoiceInk.Windows.Core.Models;
using VoiceInk.Windows.Core.Notifications;
using VoiceInk.Windows.Core.Onboarding;
using VoiceInk.Windows.Core.Permissions;
using VoiceInk.Windows.Core.PowerMode;
using VoiceInk.Windows.Core.Privacy;
using VoiceInk.Windows.Core.Recording;
using VoiceInk.Windows.Core.Recorder;
using VoiceInk.Windows.Core.Services;
using VoiceInk.Windows.Core.Settings;
using VoiceInk.Windows.Core.Shell;
using VoiceInk.Windows.Core.Shortcuts;
using VoiceInk.Windows.Core.Startup;
using VoiceInk.Windows.Core.Text;
using VoiceInk.Windows.Core.Transcription;
using VoiceInk.Windows.Infrastructure.Dictionary;
using VoiceInk.Windows.Infrastructure.AudioFiles;
using VoiceInk.Windows.Infrastructure.Enhancement;
using VoiceInk.Windows.Infrastructure.History;
using VoiceInk.Windows.Infrastructure.Metrics;
using VoiceInk.Windows.Infrastructure.Models;
using VoiceInk.Windows.Infrastructure.Settings;
using VoiceInk.Windows.Infrastructure.Transcription;
using VoiceInk.Windows.Native.Audio;
using VoiceInk.Windows.Native.Hotkeys;
using VoiceInk.Windows.Native.PowerMode;
using VoiceInk.Windows.Native.Recording;
using VoiceInk.Windows.Native.Security;
using VoiceInk.Windows.Native.Startup;
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
    private const string PermissionsSectionTag = "Permissions";
    private const string DictionarySectionTag = "Dictionary";
    private const string HistorySectionTag = "History";
    private const string MetricsSectionTag = "Metrics";
    private const string SettingsSectionTag = "Settings";
    private const string AboutSectionTag = "About";
    private const string EnhancementSectionTag = "Enhancement";
    private const string PowerModeSectionTag = "Power Mode";
    private const int MaxDiagnosticEvents = 200;
    private const int HistoryWaveformPeakCount = 64;
    private const double HybridShortcutPressThresholdSeconds = 0.5;
    private static readonly int[] TranscriptionRetentionMinuteChoices = [0, 60, 24 * 60, 3 * 24 * 60, 7 * 24 * 60];
    private static readonly int[] AudioRetentionDayChoices = [1, 3, 7, 14, 30];
    private static readonly double[] ClipboardRestoreDelayChoices = [0.25, 0.5, 1.0, 2.0, 3.0, 4.0, 5.0];
    private static readonly double[] AudioResumptionDelayChoices = [0, 1, 2, 3, 4, 5];

    private readonly string appDataDirectory;
    private readonly string modelsDirectory;
    private readonly string recordingsDirectory;
    private readonly string soundsDirectory;
    private readonly string dictionaryPath;
    private readonly string historyPath;
    private readonly string metricsPath;
    private readonly string settingsPath;
    private readonly string audioFileQueueSnapshotPath;
    private readonly Dictionary<string, NavigationViewItem> navigationItemsByTag = [];
    private readonly JsonDictionaryStore dictionaryStore;
    private readonly SqliteHistoryStore historyStore;
    private readonly ISessionMetricStore sessionMetricStore;
    private readonly JsonSettingsStore settingsStore;
    private readonly IWhisperModelDownloader modelDownloader;
    private readonly WhisperModelWarmupCoordinator modelWarmupCoordinator;
    private readonly HttpClient modelDownloadHttpClient = new();
    private readonly HttpClient ollamaModelHttpClient = new();
    private readonly HttpClient openRouterModelHttpClient = new();
    private readonly string? metricsInitializationWarning;
    private readonly ClipboardTextInjectionService textInjectionService;
    private readonly LastTranscriptionActionService lastTranscriptionActionService;
    private readonly HistoryRetryService historyRetryService;
    private readonly HistoryReenhancementService historyReenhancementService;
    private readonly PrivacyCleanupService privacyCleanupService;
    private readonly IStartupRegistrationService startupRegistrationService;
    private readonly RecordingFeedbackCoordinator recordingFeedback;
    private readonly WindowsRecordingSoundFeedback recordingSoundFeedback;
    private readonly CustomRecordingSoundImporter customRecordingSoundImporter;
    private readonly WindowsSystemAudioFeedback systemAudioFeedback;
    private readonly NAudioInputDeviceChangeWatcher audioInputDeviceChangeWatcher;
    private readonly DictionaryQuickAddService dictionaryQuickAddService;
    private readonly AudioFileQueueService audioFileQueueService = new();
    private readonly IAudioFileQueueSnapshotStore audioFileQueueSnapshotStore;
    private readonly List<string> diagnosticEvents = [];
    private string? lastDiagnosticStatus;
    private readonly WindowsCredentialSecretStore secretStore;
    private readonly PowerModeAutoSendService powerModeAutoSendService = new();
    private readonly OpenAICompatibleTextEnhancementService textEnhancementService;
    private readonly OllamaModelCatalogClient ollamaModelCatalogClient;
    private readonly OpenRouterModelCatalogClient openRouterModelCatalogClient;
    private readonly TextEnhancementPipeline textEnhancementPipeline;
    private readonly OpenAICompatibleCloudTranscriptionService cloudTranscriptionService;
    private readonly DeepgramCloudTranscriptionService deepgramTranscriptionService;
    private readonly AssemblyAICloudTranscriptionService assemblyAITranscriptionService;
    private readonly ElevenLabsCloudTranscriptionService elevenLabsTranscriptionService;
    private readonly SonioxCloudTranscriptionService sonioxTranscriptionService;
    private readonly SpeechmaticsCloudTranscriptionService speechmaticsTranscriptionService;
    private readonly GeminiCloudTranscriptionService geminiTranscriptionService;
    private readonly XaiCloudTranscriptionService xaiTranscriptionService;
    private readonly CartesiaCloudTranscriptionService cartesiaTranscriptionService;
    private readonly CloudTranscriptionProviderProbeService cloudTranscriptionProviderProbeService;
    private readonly ILiveTranscriptionPreviewService liveTranscriptionPreviewService;
    private readonly TranscriptionServiceRouter transcriptionService;
    private readonly NAudioInputDeviceProvider audioInputDeviceProvider;
    private readonly ActiveWindowPowerModeTargetProvider powerModeTargetProvider = new();
    private readonly FloatingRecorderControlUpdateCoordinator floatingRecorderControlUpdates = new();
    private readonly CancellationTokenSource windowLifetime = new();
    private readonly DispatcherQueueTimer floatingRecorderRefreshTimer;
    private readonly DispatcherQueueTimer privacyCleanupTimer;
    private readonly DispatcherQueueTimer appNotificationTimer;
    private GlobalHotkeyService? hotkeyService;
    private TrayIconService? trayIconService;
    private FloatingRecorderWindow? floatingRecorderWindow;
    private HistoryWindow? historyWindow;
    private AudioFileTranscriptionService audioFileTranscriptionService;
    private CancellationTokenSource? audioFileQueueCancellation;
    private CancellationTokenSource? modelDownloadCancellation;
    private Task? modelDownloadTask;
    private CancellationTokenSource? stopOperationCancellation;
    private NAudioCaptureService audioCapture;
    private DictationController controller;
    private IReadOnlyList<AudioInputDevice> audioInputDevices = [];
    private IReadOnlyList<AudioInputDeviceChoice> audioInputChoices = [];
    private IReadOnlyList<AudioInputDeviceHealthRow> audioInputHealthRows = [];
    private IReadOnlyList<PrioritizedAudioInputDevice> prioritizedAudioInputDevices = [];
    private AudioInputDeviceSelectionNotice? audioInputSelectionNotice;
    private IReadOnlyList<VocabularyWord> vocabularyItems = [];
    private IReadOnlyList<WordReplacement> replacementItems = [];
    private IReadOnlyList<TranscriptionHistoryItem> historyItems = [];
    private IReadOnlyList<AudioFileQueueItem> audioFileQueueItems = [];
    private IReadOnlyList<LocalWhisperModel> localWhisperModels = [];
    private IReadOnlyList<LocalWhisperModel> modelChoices = [];
    private IReadOnlyList<WhisperModelCatalogItem> modelCatalogItems = [];
    private IReadOnlyList<TranscriptionLanguageChoice> languageChoices = [];
    private IReadOnlyList<EnhancementPrompt> enhancementPrompts = EnhancementPromptCatalog.CreateDefaultPrompts();
    private IReadOnlyList<string> ollamaEnhancementModelChoices = [];
    private IReadOnlyList<string> openRouterEnhancementModelChoices = [];
    private IReadOnlyList<PowerModeRule> powerModeRules = [];
    private Guid? selectedPowerModeRuleId;
    private AudioInputDeviceChoice? activeAudioInputDeviceChoice;
    private IReadOnlyList<ScreenCaptureDisplay> ocrDisplays = [];
    private LocalWhisperModelRepairAction currentModelRepairAction = LocalWhisperModelRepairAction.ImportReplacement;
    private string customStartSoundPath = string.Empty;
    private string customStopSoundPath = string.Empty;
    private bool isStarting;
    private bool isStopping;
    private bool isCanceling;
    private bool isPastingLast;
    private bool isRetryingHistory;
    private bool isReenhancingHistory;
    private bool isCopyingHistory;
    private bool isCopyingAudioFileQueueText;
    private bool isEnhancingAudioFileQueueItem;
    private bool isQuickAdding;
    private bool isImportingModel;
    private bool isDownloadingModel;
    private bool isTranscribingAudioFiles;
    private bool isSavingEnhancementKey;
    private bool isSavingCloudTranscriptionKey;
    private bool isTestingCloudTranscriptionProvider;
    private bool isRefreshingEnhancementModels;
    private bool isSavingAudioFileQueueText;
    private bool isExportingSettingsBackup;
    private bool isImportingSettingsBackup;
    private bool isRunningPrivacyCleanup;
    private bool isExportingDiagnosticLogs;
    private bool isOnboardingOpen;
    private bool recordingFeedbackSessionActive;
    private bool isRefreshingAudioInputsFromDeviceChange;
    private bool pendingAudioInputDeviceChangeRefresh;
    private bool audioInputDeviceChangeRefreshQueued;
    private bool suppressLaunchAtLoginChanged;
    private readonly bool startHiddenToTray;
    private bool settingsLoaded;
    private bool modelPathEdited;
    private bool suppressModelPathChanged;
    private bool suppressLanguageChanged;
    private bool suppressPrewarmChanged;
    private bool suppressLiveTranscriptPreviewChanged;
    private bool suppressVadChanged;
    private bool suppressRecorderStyleChanged;
    private bool suppressCloudTranscriptionPresetChanged;
    private bool suppressCloudTranscriptionModelChanged;
    private bool suppressEnhancementPresetChanged;
    private bool suppressEnhancementModelChanged;
    private bool suppressEnhancementPromptChanged;
    private Guid? promptEditorPromptId;
    private bool exitRequested;
    private bool powerModeEventsSubscribed;
    private double latestRecordingInputLevel;
    private int meterRefreshQueued;
    private DateTimeOffset? recordingStartedAt;
    private DateTimeOffset? recordingShortcutPressedAt;
    private bool recordingShortcutHandsFree;
    private string activeSectionTag = DashboardSectionTag;
    private string? hotkeyRegistrationError;
    private string? lastAppNotificationMessage;

    public MainWindow(bool startHiddenToTray = false)
    {
        this.startHiddenToTray = startHiddenToTray;
        InitializeComponent();
        CloudTranscriptionPresetComboBox.ItemsSource = TranscriptionProviderPresetCatalog.All;
        EnhancementProviderPresetComboBox.ItemsSource = EnhancementProviderPresetCatalog.All;
        PowerModeAutoSendComboBox.ItemsSource = PowerModeAutoSendKeyPresenter.Choices;
        PowerModeAutoSendComboBox.DisplayMemberPath = nameof(PowerModeAutoSendKeyChoice.DisplayName);
        MetricsTimeFilterComboBox.ItemsSource = SessionMetricsTimeFilter.AllChoices;
        MetricsTimeFilterComboBox.SelectedIndex = 0;
        HistoryPlaybackRateComboBox.ItemsSource = HistoryPlaybackRatePresenter.Choices;
        HistoryPlaybackRateComboBox.SelectedIndex = HistoryPlaybackRatePresenter.SelectedIndexFor(
            HistoryPlaybackRatePresenter.DefaultChoice.Value);
        floatingRecorderRefreshTimer = DispatcherQueue.CreateTimer();
        floatingRecorderRefreshTimer.Interval = TimeSpan.FromSeconds(1);
        floatingRecorderRefreshTimer.Tick += (_, _) => RefreshFloatingRecorderFromTimer();
        privacyCleanupTimer = DispatcherQueue.CreateTimer();
        privacyCleanupTimer.Interval = TimeSpan.FromDays(1);
        privacyCleanupTimer.Tick += async (_, _) => await RunConfiguredPrivacyCleanupFromTimerAsync();
        appNotificationTimer = DispatcherQueue.CreateTimer();
        appNotificationTimer.Tick += (_, _) => HideAppNotification();

        InitializeNavigationItems();
        ShowShellSection(DashboardSectionTag);
        RefreshAboutSection();

        appDataDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "VoiceInk.Windows");
        modelsDirectory = Path.Combine(appDataDirectory, "Models");
        recordingsDirectory = Path.Combine(appDataDirectory, "Recordings");
        soundsDirectory = Path.Combine(appDataDirectory, "Sounds");
        dictionaryPath = Path.Combine(appDataDirectory, "dictionary.json");
        historyPath = Path.Combine(appDataDirectory, "history.db");
        metricsPath = Path.Combine(appDataDirectory, "metrics.db");
        settingsPath = Path.Combine(appDataDirectory, "settings.json");
        audioFileQueueSnapshotPath = Path.Combine(appDataDirectory, "audio-file-queue.json");

        dictionaryStore = new JsonDictionaryStore(dictionaryPath);
        historyStore = new SqliteHistoryStore(historyPath);
        var metricsStore = CreateSessionMetricStore(metricsPath);
        sessionMetricStore = metricsStore.Store;
        metricsInitializationWarning = metricsStore.Warning;
        settingsStore = new JsonSettingsStore(settingsPath);
        audioFileQueueSnapshotStore = new JsonAudioFileQueueSnapshotStore(audioFileQueueSnapshotPath);
        modelDownloader = new HttpWhisperModelDownloader(modelDownloadHttpClient);
        modelWarmupCoordinator = new WhisperModelWarmupCoordinator(new WhisperNetModelWarmupService());
        modelWarmupCoordinator.StateChanged += ModelWarmupCoordinator_StateChanged;
        textInjectionService = new ClipboardTextInjectionService(settingsStore);
        lastTranscriptionActionService = new LastTranscriptionActionService(historyStore, textInjectionService);
        secretStore = new WindowsCredentialSecretStore();
        ollamaModelCatalogClient = new OllamaModelCatalogClient(ollamaModelHttpClient);
        openRouterModelCatalogClient = new OpenRouterModelCatalogClient(openRouterModelHttpClient);
        textEnhancementService = new OpenAICompatibleTextEnhancementService(new HttpClient(), secretStore);
        textEnhancementPipeline = new TextEnhancementPipeline(
            textEnhancementService,
            () => enhancementPrompts,
            new WindowsEnhancementContextProvider(new SettingsBackedOcrTextReader(settingsStore)));
        cloudTranscriptionService = new OpenAICompatibleCloudTranscriptionService(new HttpClient(), secretStore);
        deepgramTranscriptionService = new DeepgramCloudTranscriptionService(new HttpClient(), secretStore);
        assemblyAITranscriptionService = new AssemblyAICloudTranscriptionService(new HttpClient(), secretStore);
        elevenLabsTranscriptionService = new ElevenLabsCloudTranscriptionService(new HttpClient(), secretStore);
        sonioxTranscriptionService = new SonioxCloudTranscriptionService(new HttpClient(), secretStore);
        speechmaticsTranscriptionService = new SpeechmaticsCloudTranscriptionService(new HttpClient(), secretStore);
        geminiTranscriptionService = new GeminiCloudTranscriptionService(new HttpClient(), secretStore);
        xaiTranscriptionService = new XaiCloudTranscriptionService(new HttpClient(), secretStore);
        cartesiaTranscriptionService = new CartesiaCloudTranscriptionService(new HttpClient(), secretStore);
        cloudTranscriptionProviderProbeService = new CloudTranscriptionProviderProbeService(new HttpClient(), secretStore);
        liveTranscriptionPreviewService = new CompositeLiveTranscriptionPreviewService(
            [
                new DeepgramLiveTranscriptionPreviewService(
                    secretStore,
                    () => new ClientStreamingWebSocket()),
                new AssemblyAILiveTranscriptionPreviewService(
                    secretStore,
                    () => new ClientStreamingWebSocket()),
                new SonioxLiveTranscriptionPreviewService(
                    secretStore,
                    () => new ClientStreamingWebSocket()),
                new SpeechmaticsLiveTranscriptionPreviewService(
                    secretStore,
                    () => new ClientStreamingWebSocket()),
                new CartesiaLiveTranscriptionPreviewService(
                    secretStore,
                    () => new ClientStreamingWebSocket())
            ]);
        transcriptionService = new TranscriptionServiceRouter(
            new WhisperNetTranscriptionService(),
            cloudTranscriptionService,
            deepgramTranscriptionService,
            assemblyAITranscriptionService,
            elevenLabsTranscriptionService,
            sonioxTranscriptionService,
            speechmaticsTranscriptionService,
            geminiTranscriptionService,
            xaiTranscriptionService,
            cartesiaTranscriptionService);
        historyRetryService = new HistoryRetryService(
            transcriptionService,
            historyStore,
            settingsStore,
            dictionaryStore,
            sessionMetricStore);
        historyReenhancementService = new HistoryReenhancementService(
            historyStore,
            settingsStore,
            textEnhancementPipeline,
            dictionaryStore);
        privacyCleanupService = new PrivacyCleanupService(
            settingsStore,
            historyStore,
            TimeProvider.System,
            recordingsDirectory);
        startupRegistrationService = new RegistryStartupRegistrationService();
        recordingSoundFeedback = new WindowsRecordingSoundFeedback();
        customRecordingSoundImporter = new CustomRecordingSoundImporter(
            new LocalRecordingSoundFileSystem(),
            new WindowsRecordingSoundFileProbe(),
            soundsDirectory);
        systemAudioFeedback = new WindowsSystemAudioFeedback();
        recordingFeedback = new RecordingFeedbackCoordinator(
            recordingSoundFeedback,
            systemAudioFeedback,
            new WindowsMediaPlaybackFeedback());
        audioFileTranscriptionService = new AudioFileTranscriptionService(
            new MediaFoundationAudioFileImportService(),
            transcriptionService,
            historyStore,
            settingsStore,
            dictionaryStore,
            textEnhancementPipeline,
            sessionMetricStore);
        dictionaryQuickAddService = new DictionaryQuickAddService(dictionaryStore);
        audioInputDeviceProvider = new NAudioInputDeviceProvider();
        audioInputDeviceChangeWatcher = new NAudioInputDeviceChangeWatcher();
        audioInputDeviceChangeWatcher.DevicesChanged += AudioInputDeviceChangeWatcher_DevicesChanged;
        audioCapture = new NAudioCaptureService(recordingsDirectory);
        audioCapture.LevelAvailable += AudioCapture_LevelAvailable;
        controller = CreateController(audioCapture);

        CreateTrayIconService();
        TrySubscribePowerModeEvents();
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
            if (tag == MetricsSectionTag)
            {
                _ = RefreshMetricsWithStatusAsync("Metrics refreshed");
            }
            else if (tag == PermissionsSectionTag)
            {
                _ = RefreshPermissionsSectionAsync();
            }
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

    private async void TrayIconService_SelectTranscriptionModelRequested(object? sender, TrayMenuOptionEventArgs e)
    {
        var selectedModel = modelChoices.FirstOrDefault(model =>
            string.Equals(model.Path, e.Id, StringComparison.OrdinalIgnoreCase));
        if (selectedModel is null)
        {
            RefreshUiFromControllerState("Tray model selection unavailable");
            return;
        }

        await UseLocalModelPathFromTrayAsync(selectedModel.Path, selectedModel.DisplayName);
    }

    private async void TrayIconService_SelectTranscriptionProviderRequested(object? sender, TrayMenuOptionEventArgs e)
    {
        if (string.Equals(e.Id, "local", StringComparison.OrdinalIgnoreCase))
        {
            TranscriptionProviderComboBox.SelectedIndex = 0;
        }
        else
        {
            TranscriptionProviderComboBox.SelectedIndex = 1;
            SelectCloudTranscriptionPreset(e.Id);
            ApplySelectedCloudTranscriptionPreset(fillConfiguration: true);
        }

        await ApplyTranscriptionProviderSettingsAsync();
    }

    private async void TrayIconService_ToggleEnhancementRequested(object? sender, EventArgs e)
    {
        EnhancementEnabledCheckBox.IsChecked = EnhancementEnabledCheckBox.IsChecked != true;
        await SaveSettingsFromTrayAsync(EnhancementEnabledCheckBox.IsChecked == true
            ? "AI Enhancement enabled"
            : "AI Enhancement disabled");
    }

    private async void TrayIconService_SelectEnhancementPromptRequested(object? sender, TrayMenuOptionEventArgs e)
    {
        if (!Guid.TryParse(e.Id, out var promptId))
        {
            RefreshUiFromControllerState("Tray prompt selection unavailable");
            return;
        }

        SelectEnhancementPrompt(promptId);
        await SaveSettingsFromTrayAsync($"Prompt: {SelectedEnhancementPrompt()?.Title ?? "Default"}");
    }

    private async void TrayIconService_SelectEnhancementProviderRequested(object? sender, TrayMenuOptionEventArgs e)
    {
        SelectEnhancementPreset(e.Id);
        ApplySelectedEnhancementPreset(fillConfiguration: true);
        await ApplyEnhancementSettingsAsync();
    }

    private async void TrayIconService_SelectEnhancementModelRequested(object? sender, TrayMenuOptionEventArgs e)
    {
        EnhancementModelTextBox.Text = e.Id;
        RefreshEnhancementModelChoices(e.Id);
        await ApplyEnhancementSettingsAsync();
    }

    private async void TrayIconService_SelectLanguageRequested(object? sender, TrayMenuOptionEventArgs e)
    {
        var selectedIndex = languageChoices
            .ToList()
            .FindIndex(choice => string.Equals(choice.Code, e.Id, StringComparison.OrdinalIgnoreCase));
        if (selectedIndex < 0)
        {
            RefreshUiFromControllerState("Tray language selection unavailable");
            return;
        }

        LanguageComboBox.SelectedIndex = selectedIndex;
        await SaveSettingsFromTrayAsync($"Language: {languageChoices[selectedIndex].DisplayName}");
    }

    private async void TrayIconService_SelectAudioInputRequested(object? sender, TrayMenuOptionEventArgs e)
    {
        if (string.Equals(e.Id, AudioInputModeSettings.SystemDefault, StringComparison.OrdinalIgnoreCase))
        {
            AudioInputModeComboBox.SelectedIndex = AudioInputModeToSelectedIndex(AudioInputModeSettings.SystemDefault);
            AudioInputComboBox.SelectedIndex = audioInputChoices.ToList().FindIndex(choice => choice.DeviceNumber is null);
            await ApplyAudioInputAsync();
            return;
        }

        if (!int.TryParse(e.Id, NumberStyles.Integer, CultureInfo.InvariantCulture, out var deviceNumber))
        {
            RefreshUiFromControllerState("Tray audio input selection unavailable");
            return;
        }

        var selectedIndex = audioInputChoices
            .ToList()
            .FindIndex(choice => choice.DeviceNumber == deviceNumber);
        if (selectedIndex < 0)
        {
            RefreshUiFromControllerState("Tray audio input is no longer available");
            return;
        }

        AudioInputModeComboBox.SelectedIndex = AudioInputModeToSelectedIndex(AudioInputModeSettings.Custom);
        AudioInputComboBox.SelectedIndex = selectedIndex;
        await ApplyAudioInputAsync();
    }

    private async void TrayIconService_SelectPowerModeRequested(object? sender, TrayMenuOptionEventArgs e)
    {
        selectedPowerModeRuleId = string.IsNullOrWhiteSpace(e.Id)
            ? null
            : Guid.TryParse(e.Id, out var ruleId)
                ? ruleId
                : selectedPowerModeRuleId;
        RefreshPowerModeRulesListView(selectedPowerModeRuleId);
        var selectedRule = selectedPowerModeRuleId is { } selectedId
            ? powerModeRules.FirstOrDefault(rule => rule.Id == selectedId)
            : null;
        await SaveSettingsFromTrayAsync(selectedRule is null
            ? "Power Mode: Auto"
            : $"Power Mode: {PowerModeDisplay(selectedRule.Name, selectedRule.Emoji)}");
    }

    private async void TrayIconService_ToggleClipboardContextRequested(object? sender, EventArgs e)
    {
        UseClipboardContextCheckBox.IsChecked = UseClipboardContextCheckBox.IsChecked != true;
        await SaveSettingsFromTrayAsync(UseClipboardContextCheckBox.IsChecked == true
            ? "Clipboard Context enabled"
            : "Clipboard Context disabled");
    }

    private async void TrayIconService_ToggleOcrContextRequested(object? sender, EventArgs e)
    {
        UseOcrContextCheckBox.IsChecked = UseOcrContextCheckBox.IsChecked != true;
        await SaveSettingsFromTrayAsync(UseOcrContextCheckBox.IsChecked == true
            ? "Context Awareness enabled"
            : "Context Awareness disabled");
    }

    private void TrayIconService_OpenModelsRequested(object? sender, EventArgs e) =>
        RestoreAndShowSection(ModelsSectionTag);

    private void TrayIconService_OpenEnhancementRequested(object? sender, EventArgs e) =>
        RestoreAndShowSection(EnhancementSectionTag);

    private void TrayIconService_OpenAudioInputRequested(object? sender, EventArgs e) =>
        RestoreAndShowSection(AudioInputSectionTag);

    private void TrayIconService_OpenSettingsRequested(object? sender, EventArgs e) =>
        RestoreAndShowSection(SettingsSectionTag);

    private void TrayIconService_ExitRequested(object? sender, EventArgs e)
    {
        exitRequested = true;
        Close();
    }

    private void AudioCapture_LevelAvailable(object? sender, AudioInputLevel level)
    {
        Interlocked.Exchange(ref latestRecordingInputLevel, level.Peak);
        QueueMeterRefresh();
    }

    private async void RefreshAudioInputsButton_Click(object sender, RoutedEventArgs e)
    {
        await RefreshAudioInputDevicesWithStatusAsync();
    }

    private async void ApplyAudioInputButton_Click(object sender, RoutedEventArgs e)
    {
        await ApplyAudioInputAsync();
    }

    private void AudioInputModeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        RefreshAudioInputPriorityControls();
        RefreshAudioInputHealthRows();
        if (controller is null)
        {
            return;
        }

        RefreshUiFromControllerState();
    }

    private void AudioInputComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        RefreshAudioInputHealthRows();
        if (controller is null)
        {
            return;
        }

        RefreshUiFromControllerState();
    }

    private void AudioInputPriorityListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (controller is null)
        {
            return;
        }

        RefreshUiFromControllerState();
    }

    private void AddAudioInputPriorityButton_Click(object sender, RoutedEventArgs e)
    {
        var selectedChoice = SelectedAudioInputDeviceChoice();
        if (selectedChoice?.DeviceNumber is null)
        {
            RefreshUiFromControllerState("Choose a microphone to add to priority");
            return;
        }

        prioritizedAudioInputDevices = AudioInputPriorityList.Add(
            prioritizedAudioInputDevices,
            selectedChoice.Name,
            selectedChoice.EndpointId);
        RefreshAudioInputPriorityControls(selectedChoice.Name);
        RefreshAudioInputHealthRows();
        RefreshUiFromControllerState("Audio input priority updated");
    }

    private void RemoveAudioInputPriorityButton_Click(object sender, RoutedEventArgs e)
    {
        var selectedPriorityItem = SelectedAudioInputPriorityItem();
        if (selectedPriorityItem is null)
        {
            RefreshUiFromControllerState("Select a prioritized microphone");
            return;
        }

        prioritizedAudioInputDevices = AudioInputPriorityList.Remove(
            prioritizedAudioInputDevices,
            selectedPriorityItem.Name);
        RefreshAudioInputPriorityControls();
        RefreshAudioInputHealthRows();
        RefreshUiFromControllerState("Audio input priority updated");
    }

    private void MoveAudioInputPriorityUpButton_Click(object sender, RoutedEventArgs e)
    {
        var selectedPriorityItem = SelectedAudioInputPriorityItem();
        if (selectedPriorityItem is null)
        {
            RefreshUiFromControllerState("Select a prioritized microphone");
            return;
        }

        prioritizedAudioInputDevices = AudioInputPriorityList.MoveUp(
            prioritizedAudioInputDevices,
            selectedPriorityItem.Name);
        RefreshAudioInputPriorityControls(selectedPriorityItem.Name);
        RefreshAudioInputHealthRows();
        RefreshUiFromControllerState("Audio input priority updated");
    }

    private void MoveAudioInputPriorityDownButton_Click(object sender, RoutedEventArgs e)
    {
        var selectedPriorityItem = SelectedAudioInputPriorityItem();
        if (selectedPriorityItem is null)
        {
            RefreshUiFromControllerState("Select a prioritized microphone");
            return;
        }

        prioritizedAudioInputDevices = AudioInputPriorityList.MoveDown(
            prioritizedAudioInputDevices,
            selectedPriorityItem.Name);
        RefreshAudioInputPriorityControls(selectedPriorityItem.Name);
        RefreshAudioInputHealthRows();
        RefreshUiFromControllerState("Audio input priority updated");
    }

    private void OcrContextControl_Click(object sender, RoutedEventArgs e)
    {
        RefreshUiFromControllerState();
    }

    private async void PickOcrRegionButton_Click(object sender, RoutedEventArgs e)
    {
        await PickOcrRegionAsync();
    }

    private void AudioInputDeviceChangeWatcher_DevicesChanged(object? sender, EventArgs e)
    {
        if (windowLifetime.IsCancellationRequested)
        {
            return;
        }

        DispatcherQueue.TryEnqueue(QueueAudioInputDeviceChangeRefresh);
    }

    private async void ChooseAudioFilesButton_Click(object sender, RoutedEventArgs e)
    {
        await ChooseAudioFilesAsync();
    }

    private void TranscribeAudioSectionPanel_DragOver(object sender, DragEventArgs e)
    {
        e.AcceptedOperation = CanEditAudioFileQueue()
            && e.DataView.Contains(StandardDataFormats.StorageItems)
                ? DataPackageOperation.Copy
                : DataPackageOperation.None;
    }

    private async void TranscribeAudioSectionPanel_Drop(object sender, DragEventArgs e)
    {
        if (!CanEditAudioFileQueue() || !e.DataView.Contains(StandardDataFormats.StorageItems))
        {
            return;
        }

        try
        {
            var items = await e.DataView.GetStorageItemsAsync();
            await AddAudioFilePathsAsync(
                items
                    .OfType<StorageFile>()
                    .Select(file => file.Path)
                    .Where(path => !string.IsNullOrWhiteSpace(path)),
                "Dropped");
        }
        catch (Exception ex)
        {
            RefreshUiFromControllerState($"Audio file drop failed: {ex.Message}");
        }
    }

    private async void StartAudioFileQueueButton_Click(object sender, RoutedEventArgs e)
    {
        await StartAudioFileQueueAsync();
    }

    private void CancelAudioFileQueueButton_Click(object sender, RoutedEventArgs e)
    {
        audioFileQueueCancellation?.Cancel();
    }

    private async void ClearAudioFileQueueButton_Click(object sender, RoutedEventArgs e)
    {
        if (isTranscribingAudioFiles)
        {
            return;
        }

        audioFileQueueItems = audioFileQueueService.Clear(audioFileQueueItems).Items;
        RefreshAudioFileQueueListView();
        var persistWarning = await PersistAudioFileQueueSnapshotAsync();
        RefreshUiFromControllerState(persistWarning ?? "Audio file queue cleared");
    }

    private async void RemoveAudioFileQueueItemButton_Click(object sender, RoutedEventArgs e)
    {
        if (isTranscribingAudioFiles || SelectedAudioFileQueueItem() is not { } item)
        {
            return;
        }

        audioFileQueueItems = audioFileQueueService.RemovePending(audioFileQueueItems, item.Id).Items;
        RefreshAudioFileQueueListView();
        var persistWarning = await PersistAudioFileQueueSnapshotAsync();
        RefreshUiFromControllerState(persistWarning ?? "Audio file removed");
    }

    private async void RetryAudioFileQueueItemButton_Click(object sender, RoutedEventArgs e)
    {
        if (isTranscribingAudioFiles || SelectedAudioFileQueueItem() is not { } item)
        {
            return;
        }

        audioFileQueueItems = audioFileQueueService.RetryFailed(audioFileQueueItems, item.Id).Items;
        RefreshAudioFileQueueListView(item.Id);
        var persistWarning = await PersistAudioFileQueueSnapshotAsync();
        RefreshUiFromControllerState(persistWarning ?? "Audio file queued for retry");
    }

    private async void CopyAudioFileQueueTextButton_Click(object sender, RoutedEventArgs e)
    {
        await CopySelectedAudioFileQueueTextAsync();
    }

    private async void SaveAudioFileQueueTextButton_Click(object sender, RoutedEventArgs e)
    {
        await SaveSelectedAudioFileQueueTextAsync();
    }

    private async void EnhanceAudioFileQueueTextButton_Click(object sender, RoutedEventArgs e)
    {
        await EnhanceSelectedAudioFileQueueItemAsync();
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
            await recordingFeedback.BeginAsync(currentSettings, windowLifetime.Token);
            recordingFeedbackSessionActive = true;
            Interlocked.Exchange(ref latestRecordingInputLevel, 0);
            await controller.StartAsync(windowLifetime.Token);
            if (controller.State == DictationState.Recording)
            {
                recordingStartedAt = DateTimeOffset.Now;
            }
            else
            {
                await CancelRecordingFeedbackSessionAsync(immediate: true);
            }

            statusOverride = null;
        }
        catch (OperationCanceledException) when (windowLifetime.IsCancellationRequested)
        {
            await CancelRecordingFeedbackSessionAsync(immediate: true);
            statusOverride = "Closing";
        }
        catch (Exception ex)
        {
            await CancelRecordingFeedbackSessionAsync(immediate: true);
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
            || isReenhancingHistory
            || isCopyingHistory
            || controller.State != DictationState.Recording)
        {
            return;
        }

        var statusOverride = "Stopping and inserting";
        var shouldCompleteFeedback = false;
        isStopping = true;
        stopOperationCancellation?.Dispose();
        stopOperationCancellation = CancellationTokenSource.CreateLinkedTokenSource(windowLifetime.Token);
        var stopToken = stopOperationCancellation.Token;
        RefreshUiFromControllerState(statusOverride);

        try
        {
            await floatingRecorderControlUpdates.WaitForPendingUpdateAsync(stopToken);
            await controller.StopAsync(stopToken);
            shouldCompleteFeedback = controller.LastStopInsertedText;
            await CompleteRecordingFeedbackSessionAsync(playStopSound: shouldCompleteFeedback);
            await RefreshHistoryAsync(stopToken);
            var metricsWarning = await RefreshMetricsBestEffortAsync(stopToken);
            var cleanupStatus = await RunConfiguredPrivacyCleanupIfNeededAsync(stopToken);
            if (cleanupStatus is not null)
            {
                await RefreshHistoryAsync(stopToken);
            }

            recordingStartedAt = null;
            Interlocked.Exchange(ref latestRecordingInputLevel, 0);
            statusOverride = controller.LastWarning is null ? cleanupStatus ?? metricsWarning : null;
        }
        catch (OperationCanceledException) when (windowLifetime.IsCancellationRequested)
        {
            statusOverride = "Closing";
        }
        catch (OperationCanceledException) when (stopOperationCancellation?.IsCancellationRequested == true)
        {
            await RefreshHistoryAsync(CancellationToken.None);
            recordingStartedAt = null;
            Interlocked.Exchange(ref latestRecordingInputLevel, 0);
            statusOverride = controller.LastWarning ?? "Processing canceled";
        }
        catch (Exception ex)
        {
            statusOverride = $"Stop failed: {ex.Message}";
        }
        finally
        {
            if (recordingFeedbackSessionActive)
            {
                if (shouldCompleteFeedback)
                {
                    await CompleteRecordingFeedbackSessionAsync(playStopSound: true);
                }
                else
                {
                    await CancelRecordingFeedbackSessionAsync(immediate: true);
                }
            }

            isStopping = false;
            stopOperationCancellation?.Dispose();
            stopOperationCancellation = null;
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
        if (isStopping && IsControllerBusy())
        {
            if (stopOperationCancellation?.IsCancellationRequested != true)
            {
                stopOperationCancellation?.Cancel();
            }

            RefreshUiFromControllerState("Canceling processing");
            return;
        }

        if (isStarting || isStopping || isCanceling || controller.State != DictationState.Recording)
        {
            return;
        }

        var statusOverride = "Canceling recording";
        isCanceling = true;
        RefreshUiFromControllerState(statusOverride);

        try
        {
            await floatingRecorderControlUpdates.WaitForPendingUpdateAsync(windowLifetime.Token);
            await controller.CancelAsync(windowLifetime.Token);
            await CancelRecordingFeedbackSessionAsync();
            await RefreshHistoryAsync(windowLifetime.Token);
            var metricsWarning = await RefreshMetricsBestEffortAsync(windowLifetime.Token);
            recordingStartedAt = null;
            Interlocked.Exchange(ref latestRecordingInputLevel, 0);
            statusOverride = controller.LastWarning ?? metricsWarning ?? "Recording canceled";
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
            if (recordingFeedbackSessionActive)
            {
                await CancelRecordingFeedbackSessionAsync();
            }

            isCanceling = false;
            RefreshUiFromControllerState(statusOverride);
        }
    }

    private async Task CompleteRecordingFeedbackSessionAsync(bool playStopSound)
    {
        if (!recordingFeedbackSessionActive)
        {
            return;
        }

        recordingFeedbackSessionActive = false;
        await recordingFeedback.CompleteAsync(playStopSound, CancellationToken.None);
    }

    private async Task CancelRecordingFeedbackSessionAsync(bool immediate = false)
    {
        if (!recordingFeedbackSessionActive)
        {
            return;
        }

        recordingFeedbackSessionActive = false;
        if (immediate)
        {
            await recordingFeedback.CancelImmediatelyAsync(CancellationToken.None);
            return;
        }

        await recordingFeedback.CancelAsync(CancellationToken.None);
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
                case GlobalShortcutAction.ToggleEnhancement:
                    await ToggleEnhancementAsync();
                    break;
                case GlobalShortcutAction.CyclePowerMode:
                    await CyclePowerModeAsync();
                    break;
                case GlobalShortcutAction.SelectPowerModeRule:
                    await SelectPowerModeRuleShortcutAsync(e.PowerModeRuleId);
                    break;
                case GlobalShortcutAction.ToggleRecording:
                    await HandleRecordingShortcutAsync(e);
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

    private async Task HandleRecordingShortcutAsync(GlobalHotkeyPressedEventArgs e)
    {
        var mode = RecordingShortcutModeSettings.Normalize(e.RecordingShortcutMode);
        if (e.Transition == GlobalHotkeyTransition.Released)
        {
            await HandleRecordingShortcutReleasedAsync(mode);
            return;
        }

        await HandleRecordingShortcutPressedAsync(mode);
    }

    private async Task HandleRecordingShortcutPressedAsync(string mode)
    {
        recordingShortcutPressedAt = DateTimeOffset.Now;

        switch (mode)
        {
            case RecordingShortcutModeSettings.PushToTalk:
                if (controller.State != DictationState.Recording)
                {
                    await StartCurrentRecordingAsync();
                }

                break;
            case RecordingShortcutModeSettings.Hybrid:
                if (recordingShortcutHandsFree && controller.State == DictationState.Recording)
                {
                    recordingShortcutHandsFree = false;
                    await StopCurrentRecordingAsync();
                }
                else if (controller.State != DictationState.Recording)
                {
                    await StartCurrentRecordingAsync();
                }

                break;
            default:
                await ToggleCurrentRecordingAsync();
                break;
        }
    }

    private async Task HandleRecordingShortcutReleasedAsync(string mode)
    {
        switch (mode)
        {
            case RecordingShortcutModeSettings.PushToTalk:
                if (controller.State == DictationState.Recording)
                {
                    await StopCurrentRecordingAsync();
                }

                break;
            case RecordingShortcutModeSettings.Hybrid:
                var pressDuration = recordingShortcutPressedAt is null
                    ? TimeSpan.Zero
                    : DateTimeOffset.Now - recordingShortcutPressedAt.Value;
                if (pressDuration.TotalSeconds >= HybridShortcutPressThresholdSeconds
                    && controller.State == DictationState.Recording)
                {
                    recordingShortcutHandsFree = false;
                    await StopCurrentRecordingAsync();
                }
                else
                {
                    recordingShortcutHandsFree = controller.State == DictationState.Recording;
                }

                break;
        }

        recordingShortcutPressedAt = null;
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

    private void LocalModelCatalogListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        RefreshUiFromControllerState();
    }

    private async void LanguageComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (suppressLanguageChanged)
        {
            return;
        }

        if (settingsLoaded && !IsOperationActive())
        {
            try
            {
                await SaveSettingsAsync(windowLifetime.Token);
            }
            catch (OperationCanceledException) when (windowLifetime.IsCancellationRequested)
            {
            }
            catch (Exception ex)
            {
                RefreshUiFromControllerState($"Language save failed: {ex.Message}");
                return;
            }
        }

        RefreshUiFromControllerState();
    }

    private void TranscriptionProviderComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (settingsLoaded)
        {
            _ = RefreshCloudTranscriptionKeyStatusAsync(windowLifetime.Token);
        }

        RefreshLanguageChoices(ModelPathTextBox.Text, selectedLanguage: SelectedLanguageCode());
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

    private void EnhancementProviderPresetComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (!suppressEnhancementPresetChanged)
        {
            ApplySelectedEnhancementPreset(fillConfiguration: true);
            if (settingsLoaded)
            {
                _ = RefreshEnhancementKeyStatusAsync(windowLifetime.Token);
            }
        }

        RefreshUiFromControllerState();
    }

    private void EnhancementModelComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (!suppressEnhancementModelChanged
            && EnhancementModelComboBox.SelectedItem is string model)
        {
            EnhancementModelTextBox.Text = model;
        }

        RefreshUiFromControllerState();
    }

    private void EnhancementPromptComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (!suppressEnhancementPromptChanged)
        {
            RefreshPromptEditorFields();
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

    private async void RemoveSelectedModelButton_Click(object sender, RoutedEventArgs e)
    {
        var selectedModel = SelectedLocalWhisperModelChoice();
        if (selectedModel is null)
        {
            RefreshUiFromControllerState("Select an imported model to remove");
            return;
        }

        await RemoveLocalModelAsync(selectedModel.Path, selectedModel.DisplayName);
    }

    private async void RemoveUnavailableModelsButton_Click(object sender, RoutedEventArgs e)
    {
        await RemoveUnavailableImportedModelsAsync();
    }

    private void OpenModelDownloadsButton_Click(object sender, RoutedEventArgs e)
    {
        OpenModelDownloads();
    }

    private async void DownloadCatalogModelButton_Click(object sender, RoutedEventArgs e)
    {
        await DownloadSelectedCatalogModelAsync();
    }

    private async void UseCatalogModelButton_Click(object sender, RoutedEventArgs e)
    {
        await UseSelectedCatalogModelAsync();
    }

    private void ShowCatalogModelButton_Click(object sender, RoutedEventArgs e)
    {
        ShowSelectedCatalogModel();
    }

    private async void DeleteCatalogModelButton_Click(object sender, RoutedEventArgs e)
    {
        var selectedModel = SelectedCatalogModelItem();
        if (selectedModel?.LocalPath is not { Length: > 0 } localPath)
        {
            RefreshUiFromControllerState("Download a catalog model before deleting it");
            return;
        }

        await RemoveLocalModelAsync(localPath, selectedModel.DisplayName);
    }

    private async void CancelModelDownloadButton_Click(object sender, RoutedEventArgs e)
    {
        await CancelModelDownloadAsync();
    }

    private async void WarmupSelectedModelButton_Click(object sender, RoutedEventArgs e)
    {
        await ScheduleModelWarmupFromCurrentSettingsAsync("manual", updateMainStatus: true);
    }

    private async void RepairModelPathButton_Click(object sender, RoutedEventArgs e)
    {
        if (currentModelRepairAction == LocalWhisperModelRepairAction.Warmup)
        {
            await ScheduleModelWarmupFromCurrentSettingsAsync("model repair", updateMainStatus: true);
            return;
        }

        await ImportLocalModelAsync();
    }

    private async void PrewarmModelOnWakeCheckBox_Changed(object sender, RoutedEventArgs e)
    {
        if (suppressPrewarmChanged || !settingsLoaded || IsOperationActive())
        {
            return;
        }

        try
        {
            await SaveSettingsAsync(windowLifetime.Token);
            RefreshUiFromControllerState(
                PrewarmModelOnWakeCheckBox.IsChecked == true
                    ? "Local model prewarm enabled"
                    : "Local model prewarm disabled");
        }
        catch (OperationCanceledException) when (windowLifetime.IsCancellationRequested)
        {
            RefreshUiFromControllerState("Closing");
        }
        catch (Exception ex)
        {
            RefreshUiFromControllerState($"Local model prewarm update failed: {ex.Message}");
        }
    }

    private async void ShowLiveTranscriptPreviewCheckBox_Changed(object sender, RoutedEventArgs e)
    {
        if (suppressLiveTranscriptPreviewChanged || !settingsLoaded || IsOperationActive())
        {
            return;
        }

        try
        {
            await SaveSettingsAsync(windowLifetime.Token);
            RefreshUiFromControllerState(
                ShowLiveTranscriptPreviewCheckBox.IsChecked == true
                    ? "Live transcript preview enabled"
                    : "Live transcript preview disabled");
        }
        catch (OperationCanceledException) when (windowLifetime.IsCancellationRequested)
        {
            RefreshUiFromControllerState("Closing");
        }
        catch (Exception ex)
        {
            RefreshUiFromControllerState($"Live transcript preview update failed: {ex.Message}");
        }
    }

    private async void VoiceActivityDetectionCheckBox_Changed(object sender, RoutedEventArgs e)
    {
        if (suppressVadChanged || !settingsLoaded || IsOperationActive())
        {
            return;
        }

        try
        {
            await SaveSettingsAsync(windowLifetime.Token);
            RefreshUiFromControllerState(
                VoiceActivityDetectionCheckBox.IsChecked == true
                    ? "Voice Activity Detection enabled"
                    : "Voice Activity Detection disabled");
        }
        catch (OperationCanceledException) when (windowLifetime.IsCancellationRequested)
        {
            RefreshUiFromControllerState("Closing");
        }
        catch (Exception ex)
        {
            RefreshUiFromControllerState($"Voice Activity Detection update failed: {ex.Message}");
        }
    }

    private async void RecorderStyleComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (suppressRecorderStyleChanged
            || !settingsLoaded
            || IsOperationActive()
            || IsControllerBusy()
            || controller.State == DictationState.Recording)
        {
            return;
        }

        try
        {
            await SaveSettingsAsync(windowLifetime.Token);
            RefreshUiFromControllerState(
                SelectedRecorderStyle() == RecorderStyleSettings.Notch
                    ? "Recorder style: Notch"
                    : "Recorder style: Mini");
        }
        catch (OperationCanceledException) when (windowLifetime.IsCancellationRequested)
        {
            RefreshUiFromControllerState("Closing");
        }
        catch (Exception ex)
        {
            RefreshUiFromControllerState($"Recorder style update failed: {ex.Message}");
        }
    }

    private async void ApplyTranscriptionProviderSettingsButton_Click(object sender, RoutedEventArgs e)
    {
        await ApplyTranscriptionProviderSettingsAsync();
    }

    private async void TestCloudTranscriptionProviderButton_Click(object sender, RoutedEventArgs e)
    {
        await TestCloudTranscriptionProviderAsync();
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

    private void NewPromptButton_Click(object sender, RoutedEventArgs e)
    {
        SetPromptEditorForNewPrompt();
        RefreshUiFromControllerState();
    }

    private async void SavePromptButton_Click(object sender, RoutedEventArgs e)
    {
        await SavePromptAsync();
    }

    private async void DeletePromptButton_Click(object sender, RoutedEventArgs e)
    {
        await DeletePromptAsync();
    }

    private async void SaveEnhancementKeyButton_Click(object sender, RoutedEventArgs e)
    {
        await SaveEnhancementKeyAsync();
    }

    private async void ClearEnhancementKeyButton_Click(object sender, RoutedEventArgs e)
    {
        await ClearEnhancementKeyAsync();
    }

    private async void RefreshEnhancementModelsButton_Click(object sender, RoutedEventArgs e)
    {
        await RefreshEnhancementModelsAsync();
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

    private async void ExportDiagnosticLogsButton_Click(object sender, RoutedEventArgs e)
    {
        await ExportDiagnosticLogsAsync();
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

    private async void RefreshMetricsButton_Click(object sender, RoutedEventArgs e)
    {
        await RefreshMetricsWithStatusAsync("Metrics refreshed");
    }

    private async void ExportMetricsButton_Click(object sender, RoutedEventArgs e)
    {
        await ExportMetricsAsync();
    }

    private async void ResetMetricsButton_Click(object sender, RoutedEventArgs e)
    {
        await ResetMetricsAsync();
    }

    private async void MetricsTimeFilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (settingsLoaded)
        {
            await RefreshMetricsWithStatusAsync("Metrics filter updated");
        }
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

    private async void ReEnhanceHistoryButton_Click(object sender, RoutedEventArgs e)
    {
        await ReenhanceSelectedHistoryAsync();
    }

    private async void CopyHistoryOriginalButton_Click(object sender, RoutedEventArgs e)
    {
        await CopySelectedHistoryTextAsync(HistoryCopyTextKind.Original);
    }

    private async void CopyHistoryFinalButton_Click(object sender, RoutedEventArgs e)
    {
        await CopySelectedHistoryTextAsync(HistoryCopyTextKind.Final);
    }

    private async void CopyHistoryEnhancedButton_Click(object sender, RoutedEventArgs e)
    {
        await CopySelectedHistoryTextAsync(HistoryCopyTextKind.Enhanced);
    }

    private async void CopyHistoryAiRequestButton_Click(object sender, RoutedEventArgs e)
    {
        await CopySelectedHistoryTextAsync(HistoryCopyTextKind.AiRequest);
    }

    private void OpenHistoryAudioButton_Click(object sender, RoutedEventArgs e)
    {
        OpenSelectedHistoryAudio();
    }

    private async void DeleteHistoryButton_Click(object sender, RoutedEventArgs e)
    {
        await DeleteSelectedHistoryAsync();
    }

    private void HistoryPlaybackRateComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        ApplyHistoryPlaybackRate();
    }

    private async void ApplyShortcutsButton_Click(object sender, RoutedEventArgs e)
    {
        await ApplyShortcutsAsync();
    }

    private void ShortcutRecorderButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button { Tag: string targetName }
            || ShortcutTextBoxByName(targetName) is not { } textBox)
        {
            RefreshUiFromControllerState("Shortcut recorder unavailable");
            return;
        }

        textBox.Focus(FocusState.Programmatic);
        textBox.Select(0, textBox.Text.Length);
        RefreshUiFromControllerState("Press shortcut keys, or Escape to clear");
    }

    private void ShortcutTextBox_KeyDown(object sender, KeyRoutedEventArgs e)
    {
        if (sender is not TextBox textBox)
        {
            return;
        }

        var virtualKey = (int)e.Key;
        var controlDown = IsKeyDown(VirtualKeyControl) || virtualKey == VirtualKeyControl;
        var altDown = IsKeyDown(VirtualKeyMenu) || virtualKey == VirtualKeyMenu;
        var shiftDown = IsKeyDown(VirtualKeyShift) || virtualKey == VirtualKeyShift;
        if (virtualKey == VirtualKeyEscape
            && !controlDown
            && !altDown
            && !shiftDown)
        {
            textBox.Text = string.Empty;
            e.Handled = true;
            RefreshUiFromControllerState("Shortcut cleared");
            return;
        }

        string? modifierOnlyError = null;
        if (IsShortcutModifierKey(virtualKey)
            && IsRecordingShortcutTextBox(textBox)
            && GlobalShortcut.TryCreateModifierOnlyFromKeyCapture(
                controlDown,
                altDown,
                shiftDown,
                virtualKey,
                out var modifierOnlyShortcut,
                out modifierOnlyError))
        {
            textBox.Text = modifierOnlyShortcut!.DisplayText;
            textBox.Select(textBox.Text.Length, 0);
            e.Handled = true;
            RefreshUiFromControllerState("Modifier-only shortcut captured");
        }
        else if (GlobalShortcut.TryCreateFromKeyCapture(
                controlDown,
                altDown,
                shiftDown,
                virtualKey,
                out var shortcut,
                out var error))
        {
            textBox.Text = shortcut!.DisplayText;
            textBox.Select(textBox.Text.Length, 0);
            e.Handled = true;
            RefreshUiFromControllerState("Shortcut captured");
        }
        else if (IsShortcutModifierKey(virtualKey))
        {
            e.Handled = true;
            RefreshUiFromControllerState(modifierOnlyError ?? error ?? "Press a non-modifier key with your shortcut");
        }
    }

    private TextBox? ShortcutTextBoxByName(string name) =>
        name switch
        {
            nameof(RecordingHotkeyTextBox) => RecordingHotkeyTextBox,
            nameof(SecondaryRecordingHotkeyTextBox) => SecondaryRecordingHotkeyTextBox,
            nameof(PasteLastHotkeyTextBox) => PasteLastHotkeyTextBox,
            nameof(PasteLastEnhancedHotkeyTextBox) => PasteLastEnhancedHotkeyTextBox,
            nameof(RetryLastHotkeyTextBox) => RetryLastHotkeyTextBox,
            nameof(CancelHotkeyTextBox) => CancelHotkeyTextBox,
            nameof(OpenHistoryHotkeyTextBox) => OpenHistoryHotkeyTextBox,
            nameof(QuickAddHotkeyTextBox) => QuickAddHotkeyTextBox,
            nameof(ToggleEnhancementHotkeyTextBox) => ToggleEnhancementHotkeyTextBox,
            nameof(CyclePowerModeHotkeyTextBox) => CyclePowerModeHotkeyTextBox,
            nameof(PowerModeShortcutTextBox) => PowerModeShortcutTextBox,
            _ => null
        };

    private async void ApplyClipboardSettingsButton_Click(object sender, RoutedEventArgs e)
    {
        await ApplyClipboardSettingsAsync();
    }

    private async void ApplyRecordingFeedbackSettingsButton_Click(object sender, RoutedEventArgs e)
    {
        await ApplyRecordingFeedbackSettingsAsync();
    }

    private void TestStartSoundButton_Click(object sender, RoutedEventArgs e)
    {
        TestRecordingSound(RecordingSoundKind.Start);
    }

    private void TestStopSoundButton_Click(object sender, RoutedEventArgs e)
    {
        TestRecordingSound(RecordingSoundKind.Stop);
    }

    private async void ChooseStartSoundButton_Click(object sender, RoutedEventArgs e)
    {
        await ChooseRecordingSoundAsync(RecordingSoundKind.Start);
    }

    private async void ChooseStopSoundButton_Click(object sender, RoutedEventArgs e)
    {
        await ChooseRecordingSoundAsync(RecordingSoundKind.Stop);
    }

    private async void ResetStartSoundButton_Click(object sender, RoutedEventArgs e)
    {
        await ResetRecordingSoundAsync(RecordingSoundKind.Start);
    }

    private async void ResetStopSoundButton_Click(object sender, RoutedEventArgs e)
    {
        await ResetRecordingSoundAsync(RecordingSoundKind.Stop);
    }

    private void RecordingFeedbackSetting_Changed(object sender, RoutedEventArgs e)
    {
        if (!settingsLoaded)
        {
            return;
        }

        UpdateRecordingFeedbackSettingControlState();
        RefreshRecordingSoundControls();
        RefreshUiFromControllerState();
    }

    private void RecordingFeedbackSettingComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (!settingsLoaded)
        {
            return;
        }

        UpdateRecordingFeedbackSettingControlState();
        RefreshRecordingSoundControls();
        RefreshUiFromControllerState();
    }

    private void ClipboardSetting_Changed(object sender, RoutedEventArgs e)
    {
        if (!settingsLoaded)
        {
            return;
        }

        UpdateClipboardSettingControlState();
        RefreshUiFromControllerState();
    }

    private void ClipboardSettingComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (!settingsLoaded)
        {
            return;
        }

        UpdateClipboardSettingControlState();
        RefreshUiFromControllerState();
    }

    private async void ApplyCleanupSettingsButton_Click(object sender, RoutedEventArgs e)
    {
        await ApplyCleanupSettingsAsync();
    }

    private async void RunTranscriptCleanupButton_Click(object sender, RoutedEventArgs e)
    {
        await RunTranscriptCleanupAsync();
    }

    private async void RunAudioCleanupButton_Click(object sender, RoutedEventArgs e)
    {
        await RunAudioCleanupAsync();
    }

    private async void ResetOnboardingButton_Click(object sender, RoutedEventArgs e)
    {
        await ResetOnboardingAsync();
    }

    private async void LaunchAtLoginCheckBox_Changed(object sender, RoutedEventArgs e)
    {
        if (!settingsLoaded || suppressLaunchAtLoginChanged)
        {
            return;
        }

        await ApplyLaunchAtLoginAsync(repairRegistration: false);
    }

    private async void RepairLaunchAtLoginButton_Click(object sender, RoutedEventArgs e)
    {
        await ApplyLaunchAtLoginAsync(repairRegistration: true);
    }

    private void CleanupSettingCheckBox_Changed(object sender, RoutedEventArgs e)
    {
        UpdateCleanupSettingControlState();
        RefreshUiFromControllerState();
    }

    private async void ExportSettingsBackupButton_Click(object sender, RoutedEventArgs e)
    {
        await ExportSettingsBackupAsync();
    }

    private async void ImportSettingsBackupButton_Click(object sender, RoutedEventArgs e)
    {
        await ImportSettingsBackupAsync();
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
            var refreshWarning = await ApplySettingsToUiAsync(
                settings,
                forceModelPath: false,
                cancellationToken: windowLifetime.Token);
            RecreateControllerIfAudioInputChanged();
            TryReplaceGlobalHotkeys(settings, rollbackSettings: null);

            settingsLoaded = true;
            var queueRestoreStatus = await RestoreAudioFileQueueSnapshotAsync(windowLifetime.Token);
            var cleanupStatus = await RunConfiguredPrivacyCleanupIfNeededAsync(windowLifetime.Token);
            if (cleanupStatus is not null)
            {
                await RefreshHistoryAsync(windowLifetime.Token);
            }

            RefreshUiFromControllerState(refreshWarning ?? cleanupStatus ?? queueRestoreStatus);
            ScheduleModelWarmup(settings, "startup", updateMainStatus: false);
            if (startHiddenToTray)
            {
                HideWindowToTray();
            }

            if (!startHiddenToTray)
            {
                await ShowOnboardingIfNeededAsync(settings);
            }
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
            suppressPrewarmChanged = false;
            suppressLiveTranscriptPreviewChanged = false;
            suppressVadChanged = false;
            suppressRecorderStyleChanged = false;
            suppressCloudTranscriptionPresetChanged = false;
            suppressCloudTranscriptionModelChanged = false;
            suppressEnhancementPresetChanged = false;
            suppressEnhancementModelChanged = false;
            suppressEnhancementPromptChanged = false;
            suppressLaunchAtLoginChanged = false;
        }
    }

    private async Task<string?> ApplySettingsToUiAsync(
        AppSettings settings,
        bool forceModelPath,
        CancellationToken cancellationToken)
    {
        ApplySettingsSectionPresentation(SettingsSectionPresenter.Present(settings));

        if (forceModelPath || !modelPathEdited)
        {
            suppressModelPathChanged = true;
            ModelPathTextBox.Text = settings.ModelPath;
            suppressModelPathChanged = false;
        }

        localWhisperModels = settings.ImportedWhisperModels;
        TranscriptionProviderComboBox.SelectedIndex = TranscriptionProviderToSelectedIndex(settings.TranscriptionProvider);
        RefreshModelChoices(settings.ModelPath);
        RefreshLanguageChoices(settings.ModelPath, settings.Language);
        suppressCloudTranscriptionPresetChanged = true;
        SelectCloudTranscriptionPreset(settings.CloudTranscriptionProviderId);
        suppressCloudTranscriptionPresetChanged = false;
        CloudTranscriptionEndpointTextBox.Text = settings.CloudTranscriptionEndpoint;
        CloudTranscriptionModelTextBox.Text = settings.CloudTranscriptionModel;
        RefreshCloudTranscriptionModelChoices(settings.CloudTranscriptionModel);
        RecordingHotkeyTextBox.Text = settings.Hotkey;
        PrimaryRecordingShortcutModeComboBox.SelectedIndex = RecordingShortcutModeToSelectedIndex(
            settings.PrimaryRecordingShortcutMode);
        SecondaryRecordingHotkeyTextBox.Text = settings.SecondaryRecordingHotkey;
        SecondaryRecordingShortcutModeComboBox.SelectedIndex = RecordingShortcutModeToSelectedIndex(
            settings.SecondaryRecordingShortcutMode);
        MiddleClickRecordingCheckBox.IsChecked = settings.IsMiddleClickRecordingEnabled;
        MiddleClickDelayNumberBox.Value = settings.MiddleClickActivationDelayMilliseconds;
        PasteLastHotkeyTextBox.Text = settings.PasteLastTranscriptionHotkey;
        PasteLastEnhancedHotkeyTextBox.Text = settings.PasteLastEnhancementHotkey;
        RetryLastHotkeyTextBox.Text = settings.RetryLastTranscriptionHotkey;
        CancelHotkeyTextBox.Text = settings.CancelRecordingHotkey;
        OpenHistoryHotkeyTextBox.Text = settings.OpenHistoryHotkey;
        QuickAddHotkeyTextBox.Text = settings.QuickAddDictionaryHotkey;
        ToggleEnhancementHotkeyTextBox.Text = settings.ToggleEnhancementHotkey;
        CyclePowerModeHotkeyTextBox.Text = settings.CyclePowerModeHotkey;
        EnhancementEnabledCheckBox.IsChecked = settings.IsEnhancementEnabled;
        UseClipboardContextCheckBox.IsChecked = settings.UseClipboardContext;
        UseOcrContextCheckBox.IsChecked = settings.UseOcrContext;
        UseOcrCaptureRegionCheckBox.IsChecked = settings.UseOcrCaptureRegion;
        OcrRegionLeftNumberBox.Value = settings.OcrCaptureRegionLeft;
        OcrRegionTopNumberBox.Value = settings.OcrCaptureRegionTop;
        OcrRegionWidthNumberBox.Value = settings.OcrCaptureRegionWidth;
        OcrRegionHeightNumberBox.Value = settings.OcrCaptureRegionHeight;
        ApplyEnhancementContextReadinessPresentation(EnhancementContextReadinessPresenter.Present(settings));
        RefreshOcrDisplayChoices();
        suppressEnhancementPresetChanged = true;
        SelectEnhancementPreset(settings.EnhancementProviderId);
        suppressEnhancementPresetChanged = false;
        EnhancementEndpointTextBox.Text = settings.EnhancementEndpoint;
        EnhancementModelTextBox.Text = settings.EnhancementModel;
        RefreshEnhancementModelChoices(settings.EnhancementModel);
        enhancementPrompts = EnhancementPromptLibrary.BuildPrompts(settings.CustomEnhancementPrompts);
        EnhancementTimeoutTextBox.Text = EnhancementTimeoutSeconds(settings).ToString(CultureInfo.InvariantCulture);
        ShortEnhancementThresholdTextBox.Text = ShortEnhancementThreshold(settings).ToString(CultureInfo.InvariantCulture);
        SkipShortEnhancementCheckBox.IsChecked = settings.SkipShortEnhancement;
        EnhancementRetryOnTimeoutCheckBox.IsChecked = settings.EnhancementRetryOnTimeout;
        RefreshEnhancementPromptChoices(settings.SelectedEnhancementPromptId);
        RefreshPromptEditorFields();
        powerModeRules = settings.PowerModeRules;
        selectedPowerModeRuleId = settings.SelectedPowerModeRuleId;
        RefreshPowerModePromptChoices(selectedPromptId: null);
        RefreshPowerModeRulesListView();
        RestoreClipboardCheckBox.IsChecked = settings.RestoreClipboard;
        ClipboardRestoreDelayComboBox.SelectedIndex = ClipboardRestoreDelayToSelectedIndex(
            settings.ClipboardRestoreDelaySeconds);
        PasteMethodComboBox.SelectedIndex = PasteMethodToSelectedIndex(settings.PasteMethod);
        UpdateClipboardSettingControlState();
        suppressPrewarmChanged = true;
        PrewarmModelOnWakeCheckBox.IsChecked = settings.PrewarmModelOnWake;
        suppressPrewarmChanged = false;
        suppressLiveTranscriptPreviewChanged = true;
        ShowLiveTranscriptPreviewCheckBox.IsChecked = settings.ShowLiveTranscriptPreview;
        suppressLiveTranscriptPreviewChanged = false;
        suppressVadChanged = true;
        VoiceActivityDetectionCheckBox.IsChecked = settings.IsVadEnabled;
        suppressVadChanged = false;
        suppressRecorderStyleChanged = true;
        RecorderStyleComboBox.SelectedIndex = RecorderStyleToSelectedIndex(settings.RecorderStyle);
        suppressRecorderStyleChanged = false;
        SoundFeedbackCheckBox.IsChecked = settings.IsSoundFeedbackEnabled;
        customStartSoundPath = settings.CustomStartSoundPath;
        customStopSoundPath = settings.CustomStopSoundPath;
        StartSoundComboBox.SelectedIndex = RecordingSoundModeToSelectedIndex(
            settings.StartSoundMode,
            customStartSoundPath);
        StopSoundComboBox.SelectedIndex = RecordingSoundModeToSelectedIndex(
            settings.StopSoundMode,
            customStopSoundPath);
        RefreshRecordingSoundControls();
        MuteSystemAudioCheckBox.IsChecked = settings.IsSystemMuteEnabled;
        PauseMediaCheckBox.IsChecked = settings.IsPauseMediaEnabled;
        AudioResumptionDelayComboBox.SelectedIndex = AudioResumptionDelayToSelectedIndex(
            settings.AudioResumptionDelaySeconds);
        UpdateRecordingFeedbackSettingControlState();
        var startupWarning = await ApplyStartupStateToUiAsync(settings, cancellationToken);
        RemoveFillerWordsCheckBox.IsChecked = settings.RemoveFillerWords;
        TextFormattingCheckBox.IsChecked = settings.IsTextFormattingEnabled;
        LowercaseTranscriptionCheckBox.IsChecked = settings.LowercaseTranscription;
        AppendTrailingSpaceCheckBox.IsChecked = settings.AppendTrailingSpace;
        PunctuationCleanupComboBox.SelectedIndex = PunctuationCleanupModeToSelectedIndex(settings.PunctuationCleanupMode);
        TranscriptionCleanupCheckBox.IsChecked = settings.IsTranscriptionCleanupEnabled;
        TranscriptionRetentionComboBox.SelectedIndex = TranscriptionRetentionToSelectedIndex(
            settings.TranscriptionRetentionMinutes);
        AudioCleanupCheckBox.IsChecked = settings.IsAudioCleanupEnabled;
        AudioRetentionComboBox.SelectedIndex = AudioRetentionToSelectedIndex(settings.AudioRetentionPeriod);
        UpdateCleanupSettingControlState();
        UpdatePrivacyCleanupTimer(settings);
        AudioInputModeComboBox.SelectedIndex = AudioInputModeToSelectedIndex(
            settings.AudioInputDeviceNumber is not null
                ? AudioInputModeSettings.Custom
                : settings.AudioInputMode);
        prioritizedAudioInputDevices = AudioInputPriorityList.Normalize(settings.PrioritizedAudioInputDevices);
        RefreshAudioInputPriorityControls();
        await RefreshCloudTranscriptionKeyStatusAsync(cancellationToken);
        await RefreshEnhancementKeyStatusAsync(cancellationToken);
        var audioInputWarning = await RefreshAudioInputDevicesAsync(settings, cancellationToken);
        await RefreshDictionaryAsync(cancellationToken);
        await RefreshHistoryAsync(cancellationToken);
        var metricsWarning = await RefreshMetricsBestEffortAsync(cancellationToken);

        return startupWarning ?? audioInputWarning ?? metricsWarning;
    }

    private void ApplySettingsSectionPresentation(SettingsSectionPresentation presentation)
    {
        SettingsHeroTitleTextBlock.Text = presentation.HeroTitle;
        SettingsHeroDescriptionTextBlock.Text = presentation.HeroDescription;
        SettingsOverviewSummaryTextBlock.Text = presentation.OverviewSummary;
        SettingsDataSafetyGuidanceTextBlock.Text = presentation.DataSafetyGuidance;
        SettingsActionSummaryListView.ItemsSource = presentation.ActionSummaries;
        SettingsPreferenceSummaryListView.ItemsSource = presentation.PreferenceSummaries;
        SettingsBackupGuidanceListView.ItemsSource = presentation.BackupGuidanceRows;
        DiagnosticsGuidanceListView.ItemsSource = presentation.DiagnosticsGuidanceRows;

        foreach (var section in presentation.Sections)
        {
            SectionDescriptionTextBlock(section.Key).Text = section.Description;
        }
    }

    private void ApplyEnhancementContextReadinessPresentation(EnhancementContextReadinessPresentation presentation)
    {
        EnhancementContextReadinessTitleTextBlock.Text = presentation.Title;
        EnhancementContextReadinessDescriptionTextBlock.Text = presentation.Description;
        EnhancementContextReadinessListView.ItemsSource = presentation.Rows;
        EnhancementContextPrivacyListView.ItemsSource = presentation.PrivacyRows;
        EnhancementContextActionsListView.ItemsSource = presentation.ActionRows;
    }

    private void RefreshOcrDisplayChoices()
    {
        try
        {
            ocrDisplays = ScreenCaptureDisplayCatalog.FromWindowsScreens();
            OcrDisplayComboBox.ItemsSource = ocrDisplays;
            OcrDisplayComboBox.SelectedIndex = ocrDisplays.Count > 0 ? 0 : -1;
        }
        catch (Exception ex)
        {
            ocrDisplays = [];
            OcrDisplayComboBox.ItemsSource = null;
            OcrDisplayComboBox.PlaceholderText = $"Displays unavailable: {ex.Message}";
        }
    }

    private TextBlock SectionDescriptionTextBlock(string key) =>
        key switch
        {
            "shortcuts" => ShortcutsSectionDescriptionTextBlock,
            "recordingFeedback" => RecordingFeedbackSectionDescriptionTextBlock,
            "interface" => InterfaceSectionDescriptionTextBlock,
            "clipboard" => ClipboardSectionDescriptionTextBlock,
            "cleanup" => CleanupSectionDescriptionTextBlock,
            "privacy" => PrivacySectionDescriptionTextBlock,
            "general" => GeneralSectionDescriptionTextBlock,
            "backup" => BackupSectionDescriptionTextBlock,
            "diagnostics" => DiagnosticsSectionDescriptionTextBlock,
            _ => SettingsHeroDescriptionTextBlock
        };

    private async Task<string?> ApplyStartupStateToUiAsync(
        AppSettings settings,
        CancellationToken cancellationToken)
    {
        var state = await startupRegistrationService.GetStateAsync(cancellationToken);
        suppressLaunchAtLoginChanged = true;
        LaunchAtLoginCheckBox.IsChecked = settings.LaunchAtLogin;
        suppressLaunchAtLoginChanged = false;
        RepairLaunchAtLoginButton.Visibility = state.HasExternalValue
            || (settings.LaunchAtLogin != state.IsEnabled)
                ? Visibility.Visible
                : Visibility.Collapsed;
        if (state.HasExternalValue)
        {
            return "Launch at Login uses a different Windows startup command. Reapply the setting to replace or remove it.";
        }

        if (settings.LaunchAtLogin && !state.IsEnabled)
        {
            return "Launch at Login is enabled in settings but not registered with Windows. Reapply the setting to update it.";
        }

        if (!settings.LaunchAtLogin && state.IsEnabled)
        {
            return "Launch at Login is registered with Windows but disabled in settings. Reapply the setting to update it.";
        }

        return null;
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
        var recommendedModelComboBox = new ComboBox
        {
            Header = "Recommended local model",
            ItemsSource = OnboardingModelCatalogService.BuildRecommendedChoices()
        };
        var recommendedModelIndex = OnboardingModelCatalogService
            .BuildRecommendedChoices()
            .ToList()
            .FindIndex(model => model.Name == OnboardingModelCatalogService.DefaultRecommendedModelName);
        recommendedModelComboBox.SelectedIndex = Math.Max(0, recommendedModelIndex);
        var downloadRecommendedModelButton = new Button
        {
            Content = "Download Recommended Model"
        };

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
        microphoneSettingsButton.Click += (_, _) => OpenOnboardingMicrophoneAction(microphoneSettingsButton.Tag as string);
        var refreshMicrophonesButton = new Button
        {
            Content = "Refresh Microphones"
        };

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
        var onboardingDescriptionTextBlock = new TextBlock
        {
            TextWrapping = TextWrapping.Wrap
        };
        var onboardingProgressTextBlock = new TextBlock
        {
            FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
            TextWrapping = TextWrapping.Wrap
        };
        var onboardingSummaryListView = new ListView
        {
            MaxHeight = 190,
            SelectionMode = ListViewSelectionMode.None
        };
        var onboardingActionListView = new ListView
        {
            MaxHeight = 190,
            SelectionMode = ListViewSelectionMode.None
        };
        var onboardingStagesListView = new ListView
        {
            MaxHeight = 180,
            SelectionMode = ListViewSelectionMode.None
        };
        var onboardingTutorialListView = new ListView
        {
            MaxHeight = 180,
            SelectionMode = ListViewSelectionMode.None
        };
        var onboardingChecklistTextBlock = new TextBlock
        {
            TextWrapping = TextWrapping.Wrap
        };
        var microphoneStatusTextBlock = new TextBlock
        {
            TextWrapping = TextWrapping.Wrap
        };
        var onboardingHealthTextBlock = new TextBlock
        {
            TextWrapping = TextWrapping.Wrap
        };
        RefreshOnboardingStatus(
            statusTextBlock,
            onboardingDescriptionTextBlock,
            onboardingProgressTextBlock,
            onboardingActionListView,
            onboardingSummaryListView,
            onboardingStagesListView,
            onboardingTutorialListView,
            onboardingChecklistTextBlock,
            microphoneStatusTextBlock,
            onboardingHealthTextBlock,
            microphoneSettingsButton,
            modelPathTextBox.Text,
            shortcutTextBox.Text);

        downloadRecommendedModelButton.Click += async (_, _) => await DownloadOnboardingModelAsync(
            recommendedModelComboBox,
            modelPathTextBox,
            statusTextBlock);
        refreshMicrophonesButton.Click += async (_, _) => await RefreshOnboardingAudioInputsAsync(
            audioInputComboBox,
            statusTextBlock,
            onboardingDescriptionTextBlock,
            onboardingProgressTextBlock,
            onboardingActionListView,
            onboardingSummaryListView,
            onboardingStagesListView,
            onboardingTutorialListView,
            onboardingChecklistTextBlock,
            microphoneStatusTextBlock,
            onboardingHealthTextBlock,
            microphoneSettingsButton,
            modelPathTextBox,
            shortcutTextBox);

        modelPathTextBox.TextChanged += (_, _) => RefreshOnboardingStatus(
            statusTextBlock,
            onboardingDescriptionTextBlock,
            onboardingProgressTextBlock,
            onboardingActionListView,
            onboardingSummaryListView,
            onboardingStagesListView,
            onboardingTutorialListView,
            onboardingChecklistTextBlock,
            microphoneStatusTextBlock,
            onboardingHealthTextBlock,
            microphoneSettingsButton,
            modelPathTextBox.Text,
            shortcutTextBox.Text);
        shortcutTextBox.TextChanged += (_, _) => RefreshOnboardingStatus(
            statusTextBlock,
            onboardingDescriptionTextBlock,
            onboardingProgressTextBlock,
            onboardingActionListView,
            onboardingSummaryListView,
            onboardingStagesListView,
            onboardingTutorialListView,
            onboardingChecklistTextBlock,
            microphoneStatusTextBlock,
            onboardingHealthTextBlock,
            microphoneSettingsButton,
            modelPathTextBox.Text,
            shortcutTextBox.Text);

        var content = new StackPanel
        {
            Spacing = 12
        };
        content.Children.Add(onboardingDescriptionTextBlock);
        content.Children.Add(onboardingProgressTextBlock);
        content.Children.Add(onboardingActionListView);
        content.Children.Add(onboardingSummaryListView);
        content.Children.Add(onboardingStagesListView);
        content.Children.Add(onboardingTutorialListView);
        content.Children.Add(onboardingChecklistTextBlock);
        content.Children.Add(modelPathTextBox);
        content.Children.Add(browseModelButton);
        content.Children.Add(recommendedModelComboBox);
        content.Children.Add(downloadRecommendedModelButton);
        content.Children.Add(audioInputComboBox);
        content.Children.Add(microphoneStatusTextBlock);
        content.Children.Add(onboardingHealthTextBlock);
        content.Children.Add(new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 8,
            Children =
            {
                refreshMicrophonesButton,
                microphoneSettingsButton
            }
        });
        content.Children.Add(shortcutTextBox);
        content.Children.Add(statusTextBlock);

        var dialog = new ContentDialog
        {
            Title = OnboardingChecklistPresenter.Present(OnboardingSetupStatusService.Build(
                new AppSettings
                {
                    ModelPath = modelPathTextBox.Text,
                    Hotkey = shortcutTextBox.Text
                },
                HasPhysicalAudioInputChoices())).Title,
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

    private async Task DownloadOnboardingModelAsync(
        ComboBox recommendedModelComboBox,
        TextBox modelPathTextBox,
        TextBlock statusTextBlock)
    {
        if (isDownloadingModel)
        {
            return;
        }

        if (recommendedModelComboBox.SelectedItem is not WhisperModelCatalogEntry model)
        {
            statusTextBlock.Text = "Choose a recommended model to download.";
            return;
        }

        var statusOverride = $"Downloading {model.DisplayName}";
        isDownloadingModel = true;
        modelDownloadCancellation = CancellationTokenSource.CreateLinkedTokenSource(windowLifetime.Token);
        statusTextBlock.Text = $"{statusOverride} ({model.Size})";
        RefreshUiFromControllerState(statusOverride);

        try
        {
            var progress = new Progress<WhisperModelDownloadProgress>(downloadProgress =>
            {
                _ = DispatcherQueue.TryEnqueue(() =>
                    statusTextBlock.Text = $"Downloading {model.DisplayName}: {downloadProgress.FractionComplete:P0}");
            });
            var downloadTask = modelDownloader.DownloadAsync(
                model,
                modelsDirectory,
                progress,
                modelDownloadCancellation.Token);
            modelDownloadTask = downloadTask;
            var downloadedModel = await downloadTask;

            localWhisperModels = LocalWhisperModelService.AddOrReplaceCatalogModel(
                localWhisperModels,
                downloadedModel);
            modelPathTextBox.Text = downloadedModel.Path;
            ModelPathTextBox.Text = downloadedModel.Path;
            RefreshLanguageChoices(downloadedModel.Path, selectedLanguage: SelectedLanguageCode());
            RefreshModelChoices(downloadedModel.Path);
            SelectCatalogModelByName(model.Name);
            statusOverride = $"Downloaded {model.DisplayName}. Save setup to use it.";
            statusTextBlock.Text = statusOverride;
        }
        catch (OperationCanceledException) when (windowLifetime.IsCancellationRequested)
        {
            statusOverride = "Closing";
            statusTextBlock.Text = statusOverride;
        }
        catch (OperationCanceledException)
        {
            statusOverride = "Model download canceled";
            statusTextBlock.Text = statusOverride;
        }
        catch (Exception ex)
        {
            statusOverride = $"Model download failed: {ex.Message}";
            statusTextBlock.Text = statusOverride;
        }
        finally
        {
            modelDownloadCancellation?.Dispose();
            modelDownloadCancellation = null;
            modelDownloadTask = null;
            isDownloadingModel = false;
            RefreshUiFromControllerState(statusOverride);
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

    private async Task ResetOnboardingAsync()
    {
        if (!settingsLoaded || IsOperationActive())
        {
            return;
        }

        var dialog = new ContentDialog
        {
            XamlRoot = Content.XamlRoot,
            Title = "Reset Onboarding?",
            Content = "You'll see the first-run setup again the next time you launch the app.",
            PrimaryButtonText = "Reset",
            SecondaryButtonText = "Cancel",
            DefaultButton = ContentDialogButton.Secondary
        };

        var result = await dialog.ShowAsync();
        if (result != ContentDialogResult.Primary)
        {
            RefreshUiFromControllerState("Onboarding reset canceled");
            return;
        }

        try
        {
            var settings = await settingsStore.LoadAsync(windowLifetime.Token);
            await settingsStore.SaveAsync(settings with { HasCompletedOnboarding = false }, windowLifetime.Token);
            RefreshUiFromControllerState("Onboarding will show on next launch");
        }
        catch (OperationCanceledException) when (windowLifetime.IsCancellationRequested)
        {
            RefreshUiFromControllerState("Closing");
        }
        catch (Exception ex)
        {
            RefreshUiFromControllerState($"Onboarding reset failed: {ex.Message}");
        }
    }

    private async Task ApplyClipboardSettingsAsync()
    {
        if (!settingsLoaded || IsOperationActive())
        {
            return;
        }

        try
        {
            await SaveSettingsAsync(windowLifetime.Token);
            RefreshUiFromControllerState("Clipboard settings saved");
        }
        catch (OperationCanceledException) when (windowLifetime.IsCancellationRequested)
        {
            RefreshUiFromControllerState("Closing");
        }
        catch (Exception ex)
        {
            RefreshUiFromControllerState($"Clipboard settings save failed: {ex.Message}");
        }
    }

    private async Task ApplyRecordingFeedbackSettingsAsync()
    {
        if (!settingsLoaded || IsOperationActive())
        {
            return;
        }

        try
        {
            await SaveSettingsAsync(windowLifetime.Token);
            RefreshUiFromControllerState("Recording feedback settings saved");
        }
        catch (OperationCanceledException) when (windowLifetime.IsCancellationRequested)
        {
            RefreshUiFromControllerState("Closing");
        }
        catch (Exception ex)
        {
            RefreshUiFromControllerState($"Recording feedback settings save failed: {ex.Message}");
        }
    }

    private void TestRecordingSound(RecordingSoundKind kind)
    {
        if (!settingsLoaded || IsOperationActive())
        {
            return;
        }

        var settings = CurrentRecordingSoundPlaybackSettings(kind);
        if (kind == RecordingSoundKind.Start)
        {
            recordingSoundFeedback.PlayStartSound(settings);
            RefreshRecordingSoundControls(startStatus: RecordingSoundStatus(kind, settings, tested: true));
        }
        else
        {
            recordingSoundFeedback.PlayStopSound(settings);
            RefreshRecordingSoundControls(stopStatus: RecordingSoundStatus(kind, settings, tested: true));
        }

        UpdateRecordingFeedbackSettingControlState();
    }

    private async Task ChooseRecordingSoundAsync(RecordingSoundKind kind)
    {
        if (!settingsLoaded || IsOperationActive())
        {
            return;
        }

        var statusPrefix = kind == RecordingSoundKind.Start ? "Start sound" : "Stop sound";
        try
        {
            var picker = new FileOpenPicker
            {
                ViewMode = PickerViewMode.List,
                SuggestedStartLocation = PickerLocationId.MusicLibrary
            };
            picker.FileTypeFilter.Add(".wav");
            picker.FileTypeFilter.Add(".mp3");
            picker.FileTypeFilter.Add(".aiff");
            picker.FileTypeFilter.Add(".aif");
            InitializeWithWindow.Initialize(picker, WindowNative.GetWindowHandle(this));

            var file = await picker.PickSingleFileAsync();
            if (file is null)
            {
                RefreshUiFromControllerState($"{statusPrefix} import canceled");
                return;
            }

            var result = customRecordingSoundImporter.Import(file.Path, kind);
            if (!result.IsSuccess)
            {
                RefreshUiFromControllerState($"{statusPrefix} import failed: {result.ErrorMessage}");
                return;
            }

            if (kind == RecordingSoundKind.Start)
            {
                customStartSoundPath = result.CustomSoundPath;
                StartSoundComboBox.SelectedIndex = 1;
                RefreshRecordingSoundControls(startStatus: $"Custom sound: {result.FileName}");
            }
            else
            {
                customStopSoundPath = result.CustomSoundPath;
                StopSoundComboBox.SelectedIndex = 1;
                RefreshRecordingSoundControls(stopStatus: $"Custom sound: {result.FileName}");
            }

            UpdateRecordingFeedbackSettingControlState();
            await SaveSettingsAsync(windowLifetime.Token);
            RefreshUiFromControllerState($"{statusPrefix} imported");
        }
        catch (OperationCanceledException) when (windowLifetime.IsCancellationRequested)
        {
            RefreshUiFromControllerState("Closing");
        }
        catch (Exception ex)
        {
            RefreshUiFromControllerState($"{statusPrefix} import failed: {ex.Message}");
        }
    }

    private async Task ResetRecordingSoundAsync(RecordingSoundKind kind)
    {
        if (!settingsLoaded || IsOperationActive())
        {
            return;
        }

        var statusPrefix = kind == RecordingSoundKind.Start ? "Start sound" : "Stop sound";
        try
        {
            customRecordingSoundImporter.Reset(kind);
            if (kind == RecordingSoundKind.Start)
            {
                customStartSoundPath = string.Empty;
                StartSoundComboBox.SelectedIndex = 0;
                RefreshRecordingSoundControls(startStatus: "No custom sound");
            }
            else
            {
                customStopSoundPath = string.Empty;
                StopSoundComboBox.SelectedIndex = 0;
                RefreshRecordingSoundControls(stopStatus: "No custom sound");
            }

            UpdateRecordingFeedbackSettingControlState();
            await SaveSettingsAsync(windowLifetime.Token);
            RefreshUiFromControllerState($"{statusPrefix} reset");
        }
        catch (OperationCanceledException) when (windowLifetime.IsCancellationRequested)
        {
            RefreshUiFromControllerState("Closing");
        }
        catch (Exception ex)
        {
            RefreshUiFromControllerState($"{statusPrefix} reset failed: {ex.Message}");
        }
    }

    private async Task ApplyLaunchAtLoginAsync(bool repairRegistration)
    {
        if (!settingsLoaded || IsOperationActive())
        {
            return;
        }

        var previousSettings = await settingsStore.LoadAsync(windowLifetime.Token);
        var previousStartupState = await startupRegistrationService.GetStateAsync(windowLifetime.Token);
        var settings = await CurrentSettingsAsync(windowLifetime.Token, includeShortcutFields: false);

        try
        {
            if (repairRegistration)
            {
                await startupRegistrationService.RepairRegistrationAsync(
                    settings.LaunchAtLogin,
                    windowLifetime.Token);
                try
                {
                    await settingsStore.SaveAsync(settings, windowLifetime.Token);
                }
                catch
                {
                    await startupRegistrationService.RestoreStateAsync(previousStartupState, CancellationToken.None);
                    throw;
                }
            }
            else
            {
                await SaveSettingsAsync(windowLifetime.Token);
            }

            var warning = await ApplyStartupStateToUiAsync(settings, windowLifetime.Token);
            RefreshUiFromControllerState(warning ?? "Launch at Login updated");
        }
        catch (OperationCanceledException) when (windowLifetime.IsCancellationRequested)
        {
            RefreshUiFromControllerState("Closing");
        }
        catch (Exception ex)
        {
            await RestoreLaunchAtLoginUiAsync(previousSettings, previousStartupState);
            RefreshUiFromControllerState($"Launch at Login update failed: {ex.Message}");
        }
    }

    private async Task RestoreLaunchAtLoginUiAsync(
        AppSettings previousSettings,
        StartupRegistrationState previousStartupState)
    {
        try
        {
            await startupRegistrationService.RestoreStateAsync(previousStartupState, CancellationToken.None);
        }
        catch
        {
            // Status already reports the triggering failure; leave the user's external startup state untouched if rollback fails.
        }

        suppressLaunchAtLoginChanged = true;
        LaunchAtLoginCheckBox.IsChecked = previousSettings.LaunchAtLogin;
        suppressLaunchAtLoginChanged = false;

        try
        {
            await ApplyStartupStateToUiAsync(previousSettings, windowLifetime.Token);
        }
        catch
        {
            RepairLaunchAtLoginButton.Visibility = Visibility.Visible;
        }
    }

    private async Task ApplyCleanupSettingsAsync()
    {
        if (!settingsLoaded || IsOperationActive())
        {
            return;
        }

        try
        {
            await SaveSettingsAsync(windowLifetime.Token);
            var cleanupStatus = await RunConfiguredPrivacyCleanupIfNeededAsync(windowLifetime.Token);
            if (cleanupStatus is not null)
            {
                await RefreshHistoryAsync(windowLifetime.Token);
            }

            RefreshUiFromControllerState(cleanupStatus ?? "Cleanup settings saved");
        }
        catch (OperationCanceledException) when (windowLifetime.IsCancellationRequested)
        {
            RefreshUiFromControllerState("Closing");
        }
        catch (Exception ex)
        {
            RefreshUiFromControllerState($"Cleanup settings save failed: {ex.Message}");
        }
    }

    private async Task RunTranscriptCleanupAsync()
    {
        if (!CanUsePrivacyCleanup())
        {
            return;
        }

        var dialog = new ContentDialog
        {
            XamlRoot = Content.XamlRoot,
            Title = "Delete old transcripts?",
            Content = $"This deletes transcript history older than {SelectedTranscriptionRetentionLabel()} and removes their stored audio files.",
            PrimaryButtonText = "Delete",
            SecondaryButtonText = "Cancel",
            DefaultButton = ContentDialogButton.Secondary
        };

        var confirmation = await dialog.ShowAsync();
        if (confirmation != ContentDialogResult.Primary)
        {
            RefreshUiFromControllerState("Transcript cleanup canceled");
            return;
        }

        var statusOverride = "Cleaning up transcripts";
        isRunningPrivacyCleanup = true;
        RefreshUiFromControllerState(statusOverride);
        try
        {
            await SaveSettingsAsync(windowLifetime.Token);
            ClearHistoryAudioPlayer();
            var cleanup = await privacyCleanupService.RunTranscriptCleanupAsync(windowLifetime.Token);
            await RefreshHistoryAsync(windowLifetime.Token);
            statusOverride = TranscriptCleanupStatus(cleanup);
        }
        catch (OperationCanceledException) when (windowLifetime.IsCancellationRequested)
        {
            statusOverride = "Closing";
        }
        catch (Exception ex)
        {
            statusOverride = $"Transcript cleanup failed: {ex.Message}";
        }
        finally
        {
            isRunningPrivacyCleanup = false;
            RefreshUiFromControllerState(statusOverride);
        }
    }

    private async Task RunAudioCleanupAsync()
    {
        if (!CanUsePrivacyCleanup())
        {
            return;
        }

        var statusOverride = "Analyzing audio cleanup";
        isRunningPrivacyCleanup = true;
        RefreshUiFromControllerState(statusOverride);
        try
        {
            await SaveSettingsAsync(windowLifetime.Token);
            var preview = await privacyCleanupService.PreviewAudioCleanupAsync(windowLifetime.Token);
            if (!preview.IsEnabled)
            {
                statusOverride = "Audio cleanup is disabled";
                return;
            }

            if (preview.FileCount == 0)
            {
                statusOverride = $"No audio files found older than {SelectedAudioRetentionLabel()}";
                return;
            }

            isRunningPrivacyCleanup = false;
            RefreshUiFromControllerState();
            var dialog = new ContentDialog
            {
                XamlRoot = Content.XamlRoot,
                Title = "Delete old audio files?",
                Content = $"This will delete {preview.FileCount:N0} audio file(s) ({FormatFileSize(preview.TotalBytes)}) while keeping transcript text.",
                PrimaryButtonText = $"Delete {preview.FileCount:N0} File{(preview.FileCount == 1 ? string.Empty : "s")}",
                SecondaryButtonText = "Cancel",
                DefaultButton = ContentDialogButton.Secondary
            };

            var confirmation = await dialog.ShowAsync();
            if (confirmation != ContentDialogResult.Primary)
            {
                statusOverride = "Audio cleanup canceled";
                return;
            }

            statusOverride = "Cleaning up audio files";
            isRunningPrivacyCleanup = true;
            RefreshUiFromControllerState(statusOverride);
            ClearHistoryAudioPlayer();
            var cleanup = await privacyCleanupService.RunAudioCleanupAsync(windowLifetime.Token);
            await RefreshHistoryAsync(windowLifetime.Token);
            statusOverride = AudioCleanupStatus(cleanup);
        }
        catch (OperationCanceledException) when (windowLifetime.IsCancellationRequested)
        {
            statusOverride = "Closing";
        }
        catch (Exception ex)
        {
            statusOverride = $"Audio cleanup failed: {ex.Message}";
        }
        finally
        {
            isRunningPrivacyCleanup = false;
            RefreshUiFromControllerState(statusOverride);
        }
    }

    private async Task RunConfiguredPrivacyCleanupFromTimerAsync()
    {
        if (!settingsLoaded
            || windowLifetime.IsCancellationRequested
            || IsOperationActive()
            || IsControllerBusy()
            || controller.State == DictationState.Recording)
        {
            return;
        }

        isRunningPrivacyCleanup = true;
        RefreshUiFromControllerState("Running privacy cleanup");
        var statusOverride = "Running privacy cleanup";
        try
        {
            var cleanupStatus = await RunConfiguredPrivacyCleanupIfNeededAsync(windowLifetime.Token);
            if (cleanupStatus is not null)
            {
                await RefreshHistoryAsync(windowLifetime.Token);
                statusOverride = cleanupStatus;
            }
            else
            {
                statusOverride = null;
            }
        }
        catch (OperationCanceledException) when (windowLifetime.IsCancellationRequested)
        {
            statusOverride = "Closing";
        }
        catch (Exception ex)
        {
            statusOverride = $"Privacy cleanup failed: {ex.Message}";
        }
        finally
        {
            isRunningPrivacyCleanup = false;
            RefreshUiFromControllerState(statusOverride);
        }
    }

    private async Task<string?> RunConfiguredPrivacyCleanupIfNeededAsync(CancellationToken cancellationToken)
    {
        var settings = await settingsStore.LoadAsync(cancellationToken);
        UpdatePrivacyCleanupTimer(settings);

        if (settings.IsTranscriptionCleanupEnabled)
        {
            var cleanup = await privacyCleanupService.RunTranscriptCleanupAsync(cancellationToken);
            return HasPrivacyCleanupWork(cleanup)
                ? $"Auto transcript cleanup: {TranscriptCleanupStatus(cleanup).ToLowerInvariant()}"
                : null;
        }

        if (settings.IsAudioCleanupEnabled)
        {
            var cleanup = await privacyCleanupService.RunAudioCleanupAsync(cancellationToken);
            return HasPrivacyCleanupWork(cleanup)
                ? $"Auto audio cleanup: {AudioCleanupStatus(cleanup).ToLowerInvariant()}"
                : null;
        }

        return null;
    }

    private void UpdatePrivacyCleanupTimer(AppSettings settings)
    {
        var shouldRun = settings.IsTranscriptionCleanupEnabled
            || (!settings.IsTranscriptionCleanupEnabled && settings.IsAudioCleanupEnabled);
        if (shouldRun)
        {
            if (!privacyCleanupTimer.IsRunning)
            {
                privacyCleanupTimer.Start();
            }

            return;
        }

        if (privacyCleanupTimer.IsRunning)
        {
            privacyCleanupTimer.Stop();
        }
    }

    private async Task RefreshOnboardingAudioInputsAsync(
        ComboBox audioInputComboBox,
        TextBlock statusTextBlock,
        TextBlock onboardingDescriptionTextBlock,
        TextBlock onboardingProgressTextBlock,
        ListView onboardingActionListView,
        ListView onboardingSummaryListView,
        ListView onboardingStagesListView,
        ListView onboardingTutorialListView,
        TextBlock onboardingChecklistTextBlock,
        TextBlock microphoneStatusTextBlock,
        TextBlock onboardingHealthTextBlock,
        Button microphoneSettingsButton,
        TextBox modelPathTextBox,
        TextBox shortcutTextBox)
    {
        try
        {
            var selectedDevice = audioInputComboBox.SelectedItem as AudioInputDeviceChoice;
            var settings = await CurrentSettingsAsync(windowLifetime.Token, includeShortcutFields: false);
            await RefreshAudioInputDevicesAsync(settings, windowLifetime.Token);
            audioInputComboBox.ItemsSource = audioInputChoices;
            audioInputComboBox.SelectedIndex = selectedDevice is null
                ? AudioInputComboBox.SelectedIndex
                : audioInputChoices.ToList().FindIndex(choice =>
                    choice.DeviceNumber == selectedDevice.DeviceNumber
                    && string.Equals(choice.Name, selectedDevice.Name, StringComparison.OrdinalIgnoreCase));
            if (audioInputComboBox.SelectedIndex < 0 && audioInputChoices.Count > 0)
            {
                audioInputComboBox.SelectedIndex = 0;
            }

            RefreshOnboardingStatus(
                statusTextBlock,
                onboardingDescriptionTextBlock,
                onboardingProgressTextBlock,
                onboardingActionListView,
                onboardingSummaryListView,
                onboardingStagesListView,
                onboardingTutorialListView,
                onboardingChecklistTextBlock,
                microphoneStatusTextBlock,
                onboardingHealthTextBlock,
                microphoneSettingsButton,
                modelPathTextBox.Text,
                shortcutTextBox.Text);
            RefreshUiFromControllerState("Audio inputs refreshed");
        }
        catch (OperationCanceledException) when (windowLifetime.IsCancellationRequested)
        {
            statusTextBlock.Text = "Closing";
        }
        catch (Exception ex)
        {
            statusTextBlock.Text = $"Audio input refresh failed: {ex.Message}";
            RefreshUiFromControllerState(statusTextBlock.Text);
        }
    }

    private void RefreshOnboardingStatus(
        TextBlock statusTextBlock,
        TextBlock? onboardingDescriptionTextBlock,
        TextBlock? onboardingProgressTextBlock,
        ListView? onboardingActionListView,
        ListView? onboardingSummaryListView,
        ListView? onboardingStagesListView,
        ListView? onboardingTutorialListView,
        TextBlock? onboardingChecklistTextBlock,
        TextBlock? microphoneStatusTextBlock,
        TextBlock? onboardingHealthTextBlock,
        Button? microphoneSettingsButton,
        string modelPath,
        string primaryShortcut)
    {
        var status = OnboardingSetupStatusService.Build(
            new AppSettings
            {
                ModelPath = modelPath,
                Hotkey = primaryShortcut
            },
            HasPhysicalAudioInputChoices());
        var presentation = OnboardingChecklistPresenter.Present(status);

        statusTextBlock.Text = presentation.NextAction;
        if (onboardingDescriptionTextBlock is not null)
        {
            onboardingDescriptionTextBlock.Text = presentation.Description;
        }

        if (onboardingProgressTextBlock is not null)
        {
            onboardingProgressTextBlock.Text = presentation.ProgressLabel;
        }

        if (onboardingActionListView is not null)
        {
            onboardingActionListView.ItemsSource = presentation.SetupActions
                .Select(action => $"{action.StatusBadge}: {action.Title} - {action.CommandText}. {action.Description}")
                .ToArray();
        }

        if (onboardingSummaryListView is not null)
        {
            onboardingSummaryListView.ItemsSource = presentation.SummaryRows
                .Select(row => $"{row.StatusBadge}: {row.Title} - {row.Description}")
                .ToArray();
        }

        if (onboardingStagesListView is not null)
        {
            onboardingStagesListView.ItemsSource = presentation.Stages
                .Select(stage => stage.DisplayText)
                .ToArray();
        }

        if (onboardingTutorialListView is not null)
        {
            onboardingTutorialListView.ItemsSource = presentation.TutorialSteps
                .Select(step => $"{step.StatusBadge}: {step.StepNumber}. {step.Title} - {step.Description}")
                .ToArray();
        }

        if (onboardingChecklistTextBlock is not null)
        {
            onboardingChecklistTextBlock.Text = presentation.ChecklistSummary;
        }

        if (microphoneStatusTextBlock is not null)
        {
            microphoneStatusTextBlock.Text = $"{status.MicrophoneStatusTitle}: {status.MicrophoneStatusMessage}";
        }

        if (onboardingHealthTextBlock is not null)
        {
            onboardingHealthTextBlock.Text = status.HealthSummary;
        }

        if (microphoneSettingsButton is not null)
        {
            microphoneSettingsButton.Content = status.MicrophoneActionText;
            microphoneSettingsButton.Tag = status.MicrophoneActionTarget;
        }
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

    private void OpenOnboardingMicrophoneAction(string? actionTarget)
    {
        if (string.Equals(actionTarget, "ms-settings:privacy-microphone", StringComparison.OrdinalIgnoreCase))
        {
            OpenWindowsMicrophoneSettings();
        }
    }

    private async void RefreshPermissionsButton_Click(object sender, RoutedEventArgs e)
    {
        await RefreshPermissionsSectionAsync("Permissions refreshed");
    }

    private void OpenMicrophonePrivacySettingsButton_Click(object sender, RoutedEventArgs e)
    {
        OpenWindowsMicrophoneSettings();
    }

    private async void PermissionActionButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button { Tag: string actionTarget })
        {
            return;
        }

        if (string.Equals(actionTarget, "ms-settings:privacy-microphone", StringComparison.OrdinalIgnoreCase))
        {
            OpenWindowsMicrophoneSettings();
            return;
        }

        if (navigationItemsByTag.ContainsKey(actionTarget))
        {
            ShowShellSection(actionTarget);
            if (actionTarget == MetricsSectionTag)
            {
                await RefreshMetricsWithStatusAsync("Metrics refreshed");
            }
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
            await ScheduleModelWarmupFromCurrentSettingsAsync("model import");
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

        var health = ModelPathHealth(selectedModel.Path);
        if (!health.CanUse)
        {
            RefreshUiFromControllerState(health.Message);
            return;
        }

        try
        {
            await UseLocalModelPathAsync(selectedModel.Path, selectedModel.DisplayName);
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

    private async Task RemoveUnavailableImportedModelsAsync()
    {
        if (!CanEditModelLibrary())
        {
            return;
        }

        var previousModelPath = ModelPathTextBox.Text.Trim();
        var selectedPathWasImported = localWhisperModels.Any(model =>
            string.Equals(model.Path, previousModelPath, StringComparison.OrdinalIgnoreCase));
        var cleaned = LocalWhisperModelService.RemoveUnavailableImportedModels(
            localWhisperModels,
            File.Exists,
            LocalModelFileLength,
            out var removedCount);
        if (removedCount == 0)
        {
            RefreshUiFromControllerState("No unavailable imported models to remove");
            return;
        }

        localWhisperModels = cleaned;
        if (selectedPathWasImported
            && !cleaned.Any(model => string.Equals(model.Path, previousModelPath, StringComparison.OrdinalIgnoreCase)))
        {
            ModelPathTextBox.Text = string.Empty;
        }

        RefreshModelChoices(ModelPathTextBox.Text);
        RefreshLanguageChoices(ModelPathTextBox.Text, selectedLanguage: SelectedLanguageCode());
        await SaveSettingsAsync(windowLifetime.Token);
        RefreshUiFromControllerState(
            removedCount == 1
                ? "Removed 1 unavailable imported model"
                : $"Removed {removedCount} unavailable imported models");
    }

    private async Task RemoveLocalModelAsync(string modelPath, string displayName)
    {
        if (!CanEditModelLibrary())
        {
            return;
        }

        var removal = LocalWhisperModelService.RemoveModel(
            localWhisperModels,
            modelPath,
            ModelPathTextBox.Text,
            modelsDirectory);
        if (!removal.Removed)
        {
            RefreshUiFromControllerState("Model is not in the local model list");
            return;
        }

        var deletesAppFile = removal.FilePathToDelete is { Length: > 0 };
        var dialog = new ContentDialog
        {
            XamlRoot = Content.XamlRoot,
            Title = deletesAppFile ? "Delete downloaded model?" : "Remove model reference?",
            Content = deletesAppFile
                ? $"Delete '{displayName}' from VoiceInk's local model storage? This removes the downloaded .bin file."
                : $"Remove '{displayName}' from VoiceInk? The external .bin file remains on disk.",
            PrimaryButtonText = deletesAppFile ? "Delete" : "Remove",
            CloseButtonText = "Cancel",
            DefaultButton = ContentDialogButton.Secondary
        };

        var confirmation = await dialog.ShowAsync();
        if (confirmation != ContentDialogResult.Primary)
        {
            RefreshUiFromControllerState("Model removal canceled");
            return;
        }

        localWhisperModels = removal.ImportedModels;
        ModelPathTextBox.Text = removal.ModelPath;
        RefreshModelChoices(ModelPathTextBox.Text);
        RefreshLanguageChoices(ModelPathTextBox.Text, selectedLanguage: SelectedLanguageCode());
        await SaveSettingsAsync(windowLifetime.Token);

        if (deletesAppFile)
        {
            try
            {
                File.Delete(removal.FilePathToDelete!);
            }
            catch (Exception ex)
            {
                RefreshUiFromControllerState($"Removed model reference; file delete failed: {ex.Message}");
                return;
            }
        }

        RefreshModelChoices(ModelPathTextBox.Text);
        RefreshUiFromControllerState(deletesAppFile
            ? $"Deleted model: {displayName}"
            : $"Removed model reference: {displayName}");
    }

    private async Task UseSelectedCatalogModelAsync()
    {
        var selectedModel = SelectedCatalogModelItem();
        if (!CanEditModelLibrary())
        {
            return;
        }

        if (selectedModel?.LocalPath is not { Length: > 0 } localPath)
        {
            RefreshUiFromControllerState("Download the model before setting it as default");
            return;
        }

        var health = ModelPathHealth(localPath);
        if (!health.CanUse)
        {
            RefreshUiFromControllerState(health.Message);
            return;
        }

        try
        {
            await UseLocalModelPathAsync(localPath, selectedModel.DisplayName);
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

    private async Task UseLocalModelPathAsync(string modelPath, string displayName)
    {
        var health = ModelPathHealth(modelPath);
        if (!health.CanUse)
        {
            RefreshUiFromControllerState(health.Message);
            return;
        }

        ModelPathTextBox.Text = modelPath;
        RefreshLanguageChoices(modelPath, selectedLanguage: SelectedLanguageCode());
        await SaveSettingsAsync(windowLifetime.Token);
        RefreshModelChoices(modelPath);
        SelectCatalogModelByName(Path.GetFileNameWithoutExtension(modelPath));
        await ScheduleModelWarmupFromCurrentSettingsAsync("model selection");
        RefreshUiFromControllerState($"Default model: {displayName}");
    }

    private async Task UseLocalModelPathFromTrayAsync(string modelPath, string displayName)
    {
        try
        {
            await UseLocalModelPathAsync(modelPath, displayName);
        }
        catch (OperationCanceledException) when (windowLifetime.IsCancellationRequested)
        {
            RefreshUiFromControllerState("Closing");
        }
        catch (Exception ex)
        {
            RefreshUiFromControllerState($"Tray model selection failed: {ex.Message}");
        }
    }

    private async Task DownloadSelectedCatalogModelAsync()
    {
        var selectedModel = SelectedCatalogModelItem();
        if (!CanEditModelLibrary())
        {
            return;
        }

        if (selectedModel is null)
        {
            RefreshUiFromControllerState("Select a model to download");
            return;
        }

        if (selectedModel.IsDownloaded)
        {
            await UseSelectedCatalogModelAsync();
            return;
        }

        var statusOverride = $"Downloading {selectedModel.DisplayName}";
        isDownloadingModel = true;
        modelDownloadCancellation = CancellationTokenSource.CreateLinkedTokenSource(windowLifetime.Token);
        SetModelDownloadProgress(selectedModel.DisplayName, percent: null);
        RefreshUiFromControllerState(statusOverride);

        try
        {
            var selectedName = selectedModel.Name;
            var progress = new Progress<WhisperModelDownloadProgress>(downloadProgress =>
            {
                _ = DispatcherQueue.TryEnqueue(() =>
                    SetModelDownloadProgress(selectedModel.DisplayName, downloadProgress.FractionComplete));
            });
            var downloadTask = modelDownloader.DownloadAsync(
                selectedModel.Model,
                modelsDirectory,
                progress,
                modelDownloadCancellation.Token);
            modelDownloadTask = downloadTask;
            var downloadedModel = await downloadTask;

            localWhisperModels = LocalWhisperModelService.AddOrReplaceCatalogModel(
                localWhisperModels,
                downloadedModel);
            ModelPathTextBox.Text = downloadedModel.Path;
            RefreshLanguageChoices(downloadedModel.Path, selectedLanguage: SelectedLanguageCode());
            await SaveSettingsAsync(windowLifetime.Token);
            RefreshModelChoices(downloadedModel.Path);
            SelectCatalogModelByName(selectedName);
            await ScheduleModelWarmupFromCurrentSettingsAsync("model download");
            statusOverride = $"Downloaded and selected {selectedModel.DisplayName}";
        }
        catch (OperationCanceledException) when (windowLifetime.IsCancellationRequested)
        {
            statusOverride = "Closing";
        }
        catch (OperationCanceledException)
        {
            statusOverride = "Model download canceled";
        }
        catch (Exception ex)
        {
            statusOverride = $"Model download failed: {ex.Message}";
        }
        finally
        {
            modelDownloadCancellation?.Dispose();
            modelDownloadCancellation = null;
            modelDownloadTask = null;
            isDownloadingModel = false;
            HideModelDownloadProgress();
            RefreshUiFromControllerState(statusOverride);
        }
    }

    private async Task CancelModelDownloadAsync()
    {
        if (!isDownloadingModel || modelDownloadCancellation is null)
        {
            return;
        }

        modelDownloadCancellation.Cancel();
        if (modelDownloadTask is not null)
        {
            try
            {
                await modelDownloadTask;
            }
            catch (OperationCanceledException)
            {
            }
            catch
            {
            }
        }
    }

    private async Task ScheduleModelWarmupFromCurrentSettingsAsync(
        string trigger,
        bool updateMainStatus = false)
    {
        if (!settingsLoaded || windowLifetime.IsCancellationRequested)
        {
            return;
        }

        try
        {
            var settings = await CurrentSettingsAsync(windowLifetime.Token, includeShortcutFields: false);
            ScheduleModelWarmup(settings, trigger, updateMainStatus);
        }
        catch (OperationCanceledException) when (windowLifetime.IsCancellationRequested)
        {
        }
        catch (Exception ex)
        {
            UpdateModelWarmupStatus(new WhisperModelWarmupState(
                WhisperModelWarmupStatus.Failed,
                ModelPathTextBox.Text.Trim(),
                Path.GetFileNameWithoutExtension(ModelPathTextBox.Text.Trim()),
                trigger,
                $"Model warmup failed to start: {ex.Message}",
                CompletedAt: DateTimeOffset.Now));
            if (updateMainStatus)
            {
                RefreshUiFromControllerState($"Model warmup failed to start: {ex.Message}");
            }
        }
    }

    private void ScheduleModelWarmup(
        AppSettings settings,
        string trigger,
        bool updateMainStatus)
    {
        var result = modelWarmupCoordinator.TryStart(settings, trigger, windowLifetime.Token);
        if (updateMainStatus)
        {
            RefreshUiFromControllerState(result.Message);
        }
    }

    private void ModelWarmupCoordinator_StateChanged(object? sender, WhisperModelWarmupState state)
    {
        _ = DispatcherQueue.TryEnqueue(() =>
        {
            UpdateModelWarmupStatus(state);
            RefreshUiFromControllerState();
        });
    }

    private void UpdateModelWarmupStatus(WhisperModelWarmupState state)
    {
        ModelWarmupStatusTextBlock.Text = state.Status switch
        {
            WhisperModelWarmupStatus.Idle => "Model warmup idle",
            WhisperModelWarmupStatus.Warming => $"{state.Message}...",
            WhisperModelWarmupStatus.Succeeded when state.Duration is { } duration =>
                $"{state.Message} in {duration.TotalSeconds:0.0}s",
            WhisperModelWarmupStatus.Succeeded => state.Message,
            WhisperModelWarmupStatus.Skipped => state.Message,
            WhisperModelWarmupStatus.Canceled => state.Message,
            WhisperModelWarmupStatus.Failed => state.Message,
            _ => state.Message
        };
    }

    private void ShowSelectedCatalogModel()
    {
        var selectedModel = SelectedCatalogModelItem();
        if (!CanEditModelLibrary() || selectedModel?.LocalPath is not { Length: > 0 } localPath)
        {
            return;
        }

        var health = ModelPathHealth(localPath);
        if (!health.CanUse)
        {
            RefreshUiFromControllerState(health.Message);
            return;
        }

        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "explorer.exe",
                Arguments = $"/select,\"{localPath}\"",
                UseShellExecute = true
            });
            RefreshUiFromControllerState("Model file opened");
        }
        catch (Exception ex)
        {
            RefreshUiFromControllerState($"Open model failed: {ex.Message}");
        }
    }

    private void SetModelDownloadProgress(string displayName, double? percent)
    {
        ModelDownloadProgressBar.Visibility = Visibility.Visible;
        ModelDownloadProgressBar.IsIndeterminate = percent is null;
        ModelDownloadProgressBar.Value = percent is null ? 0 : Math.Round(percent.Value * 100, 0);
        ModelDownloadStatusTextBlock.Text = percent is null
            ? $"Downloading {displayName}"
            : $"Downloading {displayName}: {ModelDownloadProgressBar.Value:0}%";
    }

    private void HideModelDownloadProgress()
    {
        ModelDownloadProgressBar.Visibility = Visibility.Collapsed;
        ModelDownloadProgressBar.IsIndeterminate = false;
        ModelDownloadProgressBar.Value = 0;
        ModelDownloadStatusTextBlock.Text = string.Empty;
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
        var presentation = DictionaryPagePresenter.Present(vocabularyItems, replacementItems);

        ApplyDictionaryPagePresentation(presentation);
        VocabularyListView.SelectedIndex = selectedVocabularyId is null
            ? -1
            : vocabularyItems.ToList().FindIndex(item => item.Id == selectedVocabularyId.Value);
        ReplacementListView.SelectedIndex = selectedReplacementId is null
            ? -1
            : replacementItems.ToList().FindIndex(item => item.Id == selectedReplacementId.Value);
    }

    private void ApplyDictionaryPagePresentation(DictionaryPagePresentation presentation)
    {
        DictionaryHeroTitleTextBlock.Text = presentation.HeroTitle;
        DictionaryHeroDescriptionTextBlock.Text = presentation.HeroDescription;
        DictionaryOverviewSummaryTextBlock.Text = presentation.OverviewSummary;
        DictionaryLocalBackupGuidanceTextBlock.Text = presentation.LocalBackupGuidance;
        DictionarySummaryListView.ItemsSource = presentation.SummaryRows;
        DictionaryRuleGuidanceListView.ItemsSource = presentation.RuleGuidanceRows;
        VocabularySectionTitleTextBlock.Text = presentation.VocabularySectionTitle;
        VocabularySectionDescriptionTextBlock.Text = presentation.VocabularySectionDescription;
        VocabularyCountTextBlock.Text = presentation.VocabularyCountLabel;
        VocabularyEmptyTextBlock.Text = presentation.VocabularyEmptyText;
        VocabularyEmptyTextBlock.Visibility = string.IsNullOrWhiteSpace(presentation.VocabularyEmptyText)
            ? Visibility.Collapsed
            : Visibility.Visible;
        VocabularyListView.ItemsSource = presentation.VocabularyRows;
        ReplacementSectionTitleTextBlock.Text = presentation.ReplacementSectionTitle;
        ReplacementSectionDescriptionTextBlock.Text = presentation.ReplacementSectionDescription;
        ReplacementCountTextBlock.Text = presentation.ReplacementCountLabel;
        ReplacementEmptyTextBlock.Text = presentation.ReplacementEmptyText;
        ReplacementEmptyTextBlock.Visibility = string.IsNullOrWhiteSpace(presentation.ReplacementEmptyText)
            ? Visibility.Collapsed
            : Visibility.Visible;
        ReplacementListView.ItemsSource = presentation.ReplacementRows;
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

    private async Task ExportSettingsBackupAsync()
    {
        if (!CanUseSettingsBackup())
        {
            return;
        }

        var statusOverride = "Preparing settings backup";
        isExportingSettingsBackup = true;
        RefreshUiFromControllerState(statusOverride);

        try
        {
            var picker = new FileSavePicker
            {
                SuggestedStartLocation = PickerLocationId.DocumentsLibrary,
                SuggestedFileName = $"VoiceInk_Settings_Backup_{DateTimeOffset.Now:yyyyMMdd-HHmmss}"
            };
            picker.FileTypeChoices.Add("JSON file", [".json"]);
            InitializeWithWindow.Initialize(picker, WindowNative.GetWindowHandle(this));

            var file = await picker.PickSaveFileAsync();
            if (file is null)
            {
                statusOverride = "Settings export canceled";
                return;
            }

            var settings = await CurrentSettingsAsync(windowLifetime.Token, includeShortcutFields: true);
            var vocabulary = await dictionaryStore.ListVocabularyAsync(windowLifetime.Token);
            var replacements = await dictionaryStore.ListReplacementsAsync(windowLifetime.Token);
            var json = VoiceInkSettingsBackup.Export(
                settings,
                vocabulary,
                replacements,
                DateTimeOffset.UtcNow);

            await FileIO.WriteTextAsync(file, json);
            statusOverride = $"Settings exported: {file.Name}";
        }
        catch (OperationCanceledException) when (windowLifetime.IsCancellationRequested)
        {
            statusOverride = "Closing";
        }
        catch (Exception ex)
        {
            statusOverride = $"Settings export failed: {ex.Message}";
        }
        finally
        {
            isExportingSettingsBackup = false;
            RefreshUiFromControllerState(statusOverride);
        }
    }

    private async Task ImportSettingsBackupAsync()
    {
        if (!CanImportSettingsBackup())
        {
            return;
        }

        var statusOverride = "Opening settings backup";
        isImportingSettingsBackup = true;
        RefreshUiFromControllerState(statusOverride);
        AppSettings? settingsRollbackSnapshot = null;
        StartupRegistrationState? startupRollbackSnapshot = null;
        bool settingsCommitted = false;

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
                statusOverride = "Settings import canceled";
                return;
            }

            var json = await FileIO.ReadTextAsync(file);
            var backup = VoiceInkSettingsBackup.Parse(json);
            var selectedCategories = await ShowSettingsBackupImportOptionsDialogAsync(backup);
            if (selectedCategories is null)
            {
                statusOverride = "Settings import canceled";
                return;
            }

            if (selectedCategories.Count == 0)
            {
                statusOverride = "Select at least one backup category";
                return;
            }

            var currentSettings = await settingsStore.LoadAsync(windowLifetime.Token);
            settingsRollbackSnapshot = currentSettings;
            startupRollbackSnapshot = await startupRegistrationService.GetStateAsync(windowLifetime.Token);
            var importsSettings = ImportsSettingsCategories(selectedCategories);
            AppSettings importedSettings = currentSettings;

            if (importsSettings)
            {
                importedSettings = VoiceInkSettingsBackupMerger.Merge(
                    currentSettings,
                    backup,
                    selectedCategories);

                await SaveImportedSettingsWithRollbackAsync(
                    importedSettings,
                    currentSettings,
                    startupRollbackSnapshot,
                    windowLifetime.Token);
                settingsCommitted = true;
            }

            if (importsSettings)
            {
                modelPathEdited = false;
                var refreshWarning = await ApplySettingsToUiAsync(
                    importedSettings,
                    forceModelPath: true,
                    cancellationToken: windowLifetime.Token);
                if (!AudioInputDeviceChoicesMatch(activeAudioInputDeviceChoice, SelectedAudioInputDeviceChoice()))
                {
                    RecreateController();
                }

                statusOverride = refreshWarning;
            }

            DictionaryImportResult? dictionaryResult = null;
            if (selectedCategories.Contains(VoiceInkSettingsBackupCategory.Dictionary))
            {
                dictionaryResult = await dictionaryStore.ImportBackupAsync(
                    VoiceInkSettingsBackup.ExportDictionaryJson(backup),
                    windowLifetime.Token);
            }

            if (!importsSettings && dictionaryResult is not null)
            {
                await RefreshDictionaryAsync(windowLifetime.Token);
            }

            statusOverride = BuildSettingsImportStatus(selectedCategories, dictionaryResult, statusOverride);
        }
        catch (OperationCanceledException) when (windowLifetime.IsCancellationRequested)
        {
            statusOverride = "Closing";
        }
        catch (Exception ex)
        {
            if (settingsCommitted && settingsRollbackSnapshot is not null)
            {
                var rollbackWarning = await RestoreSettingsAfterFailedImportAsync(
                    settingsRollbackSnapshot,
                    startupRollbackSnapshot,
                    windowLifetime.Token);
                statusOverride = rollbackWarning is null
                    ? $"Settings import failed and previous settings were restored: {ex.Message}"
                    : $"Settings import failed: {ex.Message} Previous settings restore warning: {rollbackWarning}";
            }
            else
            {
                statusOverride = $"Settings import failed: {ex.Message}";
            }
        }
        finally
        {
            isImportingSettingsBackup = false;
            RefreshUiFromControllerState(statusOverride);
        }
    }

    private async Task SaveImportedSettingsWithRollbackAsync(
        AppSettings importedSettings,
        AppSettings previousSettings,
        StartupRegistrationState previousStartupState,
        CancellationToken cancellationToken)
    {
        if (!TryReplaceGlobalHotkeys(importedSettings, previousSettings))
        {
            throw new InvalidOperationException(hotkeyRegistrationError ?? "Imported shortcut is unavailable.");
        }

        try
        {
            await ApplyStartupRegistrationAsync(importedSettings, cancellationToken);
            await settingsStore.SaveAsync(importedSettings, cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception importEx)
        {
            var warnings = new List<string>();
            var rollbackSucceeded = TryReplaceGlobalHotkeys(previousSettings, rollbackSettings: null);
            if (!rollbackSucceeded && hotkeyRegistrationError is not null)
            {
                warnings.Add($"Previous shortcuts could not be restored: {hotkeyRegistrationError}");
            }

            try
            {
                await startupRegistrationService.RestoreStateAsync(previousStartupState, CancellationToken.None);
            }
            catch (Exception rollbackEx)
            {
                warnings.Add($"Previous launch-at-login registration could not be restored: {rollbackEx.Message}");
            }

            if (warnings.Count > 0)
            {
                throw new InvalidOperationException(
                    $"Settings import failed: {importEx.Message} Rollback warning: {string.Join(" ", warnings)}",
                    importEx);
            }

            throw;
        }
    }

    private async Task<string?> RestoreSettingsAfterFailedImportAsync(
        AppSettings previousSettings,
        StartupRegistrationState? previousStartupState,
        CancellationToken cancellationToken)
    {
        var warnings = new List<string>();
        if (!TryReplaceGlobalHotkeys(previousSettings, rollbackSettings: null))
        {
            warnings.Add(hotkeyRegistrationError ?? "Previous shortcuts could not be restored.");
        }

        try
        {
            if (previousStartupState is not null)
            {
                await startupRegistrationService.RestoreStateAsync(previousStartupState, cancellationToken);
            }
            else
            {
                await ApplyStartupRegistrationAsync(previousSettings, cancellationToken);
            }

            await settingsStore.SaveAsync(previousSettings, cancellationToken);
        }
        catch (Exception ex)
        {
            warnings.Add($"Previous settings could not be saved: {ex.Message}");
        }

        try
        {
            modelPathEdited = false;
            await ApplySettingsToUiAsync(
                previousSettings,
                forceModelPath: true,
                cancellationToken);
            if (!AudioInputDeviceChoicesMatch(activeAudioInputDeviceChoice, SelectedAudioInputDeviceChoice()))
            {
                RecreateController();
            }
        }
        catch (Exception ex)
        {
            warnings.Add($"Previous settings could not be refreshed in the UI: {ex.Message}");
        }

        return warnings.Count == 0
            ? null
            : string.Join(" ", warnings);
    }

    private async Task<IReadOnlyList<VoiceInkSettingsBackupCategory>?> ShowSettingsBackupImportOptionsDialogAsync(
        VoiceInkSettingsBackupFile backup)
    {
        var choices = VoiceInkSettingsBackupCategories.All
            .Select(choice => new
            {
                Choice = choice,
                CheckBox = new CheckBox
                {
                    Content = choice.Title,
                    IsChecked = true
                }
            })
            .ToArray();

        var content = new StackPanel { Spacing = 10 };
        content.Children.Add(new TextBlock
        {
            Text = string.Join(
                Environment.NewLine,
                $"Backup version: {backup.Version}",
                $"Platform: {backup.Platform}",
                backup.SecretNotice),
            TextWrapping = TextWrapping.Wrap
        });

        foreach (var choice in choices)
        {
            content.Children.Add(choice.CheckBox);
        }

        var dialog = new ContentDialog
        {
            XamlRoot = Content.XamlRoot,
            Title = "Import Settings",
            Content = content,
            PrimaryButtonText = "Import",
            SecondaryButtonText = "Cancel",
            DefaultButton = ContentDialogButton.Primary
        };

        var result = await dialog.ShowAsync();
        if (result != ContentDialogResult.Primary)
        {
            return null;
        }

        return choices
            .Where(choice => choice.CheckBox.IsChecked == true)
            .Select(choice => choice.Choice.Category)
            .ToArray();
    }

    private async Task ShowQuickAddDictionaryAsync()
    {
        if (!settingsLoaded
            || isStarting
            || isStopping
            || isCanceling
            || isPastingLast
            || isRetryingHistory
            || isReenhancingHistory
            || isCopyingHistory
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

            await AddAudioFilePathsAsync(files.Select(file => file.Path), "Selected");
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

    private async Task AddAudioFilePathsAsync(IEnumerable<string> filePaths, string statusPrefix)
    {
        var update = audioFileQueueService.AddFiles(audioFileQueueItems, filePaths);
        audioFileQueueItems = update.Items;
        RefreshAudioFileQueueListView();
        var persistWarning = await PersistAudioFileQueueSnapshotAsync();

        var status = update.AddedCount == 0
            ? "No supported audio files added"
            : $"{statusPrefix} {update.AddedCount} audio file{Plural(update.AddedCount)}";
        if (update.SkippedCount > 0)
        {
            status += $" ({update.SkippedCount} skipped)";
        }

        RefreshUiFromControllerState(persistWarning ?? status);
    }

    private async Task<string?> RestoreAudioFileQueueSnapshotAsync(CancellationToken cancellationToken)
    {
        try
        {
            var snapshot = await audioFileQueueSnapshotStore.LoadAsync(cancellationToken);
            audioFileQueueItems = AudioFileQueuePersistence.Restore(snapshot);
            RefreshAudioFileQueueListView();
            return audioFileQueueItems.Count == 0
                ? null
                : $"Restored {audioFileQueueItems.Count} queued audio file{Plural(audioFileQueueItems.Count)}";
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            audioFileQueueItems = [];
            RefreshAudioFileQueueListView();
            return $"Audio file queue restore failed: {ex.Message}";
        }
    }

    private async Task<string?> PersistAudioFileQueueSnapshotAsync()
    {
        try
        {
            await audioFileQueueSnapshotStore.SaveAsync(
                AudioFileQueuePersistence.CreateSnapshot(audioFileQueueItems),
                windowLifetime.Token);
            return null;
        }
        catch (OperationCanceledException) when (windowLifetime.IsCancellationRequested)
        {
            return "Closing";
        }
        catch (Exception ex)
        {
            return $"Audio file queue save failed: {ex.Message}";
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
            var metricsWarning = await RefreshMetricsBestEffortAsync(windowLifetime.Token);
            statusOverride = metricsWarning ?? "Audio file queue processed";
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
            statusOverride = await PersistAudioFileQueueSnapshotAsync() ?? statusOverride;
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
        await PersistAudioFileQueueSnapshotAsync();

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
        await PersistAudioFileQueueSnapshotAsync();
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

    private async Task CopySelectedAudioFileQueueTextAsync()
    {
        if (isCopyingAudioFileQueueText)
        {
            return;
        }

        var item = SelectedAudioFileQueueItem();
        if (!AudioFileQueueTextActions.TryGetActionText(item, out var text, out var message))
        {
            RefreshUiFromControllerState(message);
            return;
        }

        isCopyingAudioFileQueueText = true;
        var statusOverride = "Copying audio transcription";
        RefreshUiFromControllerState(statusOverride);
        try
        {
            await textInjectionService.CopyAsync(text, windowLifetime.Token);
            statusOverride = "Audio transcription copied";
        }
        catch (OperationCanceledException) when (windowLifetime.IsCancellationRequested)
        {
            statusOverride = "Closing";
        }
        catch (Exception ex)
        {
            statusOverride = $"Audio transcription copy failed: {ex.Message}";
        }
        finally
        {
            isCopyingAudioFileQueueText = false;
            RefreshUiFromControllerState(statusOverride);
        }
    }

    private async Task SaveSelectedAudioFileQueueTextAsync()
    {
        if (isSavingAudioFileQueueText)
        {
            return;
        }

        var item = SelectedAudioFileQueueItem();
        if (!AudioFileQueueTextActions.TryGetActionText(item, out var text, out var message))
        {
            RefreshUiFromControllerState(message);
            return;
        }

        isSavingAudioFileQueueText = true;
        var statusOverride = "Saving audio transcription";
        RefreshUiFromControllerState(statusOverride);
        try
        {
            var picker = new FileSavePicker
            {
                SuggestedStartLocation = PickerLocationId.DocumentsLibrary,
                SuggestedFileName = AudioFileQueueTextActions.SuggestFileName(text)
            };
            picker.FileTypeChoices.Add("Text file", [".txt"]);
            picker.FileTypeChoices.Add("Markdown file", [".md"]);
            InitializeWithWindow.Initialize(picker, WindowNative.GetWindowHandle(this));

            var file = await picker.PickSaveFileAsync();
            if (file is null)
            {
                statusOverride = "Audio transcription save canceled";
                return;
            }

            var content = string.Equals(file.FileType, ".md", StringComparison.OrdinalIgnoreCase)
                ? AudioFileQueueTextActions.FormatMarkdown(text, item?.HistoryItem?.CreatedAt ?? DateTimeOffset.Now)
                : text;
            await FileIO.WriteTextAsync(file, content);
            statusOverride = $"Audio transcription saved: {file.Name}";
        }
        catch (OperationCanceledException) when (windowLifetime.IsCancellationRequested)
        {
            statusOverride = "Closing";
        }
        catch (Exception ex)
        {
            statusOverride = $"Audio transcription save failed: {ex.Message}";
        }
        finally
        {
            isSavingAudioFileQueueText = false;
            RefreshUiFromControllerState(statusOverride);
        }
    }

    private async Task EnhanceSelectedAudioFileQueueItemAsync()
    {
        if (isEnhancingAudioFileQueueItem)
        {
            return;
        }

        var item = SelectedAudioFileQueueItem();
        if (item?.HistoryItem is null || item.Status != AudioFileQueueStatus.Completed)
        {
            RefreshUiFromControllerState("Select a completed audio transcription to enhance");
            return;
        }

        isEnhancingAudioFileQueueItem = true;
        var statusOverride = "Enhancing audio transcription";
        RefreshUiFromControllerState(statusOverride);
        try
        {
            var result = await historyReenhancementService.ReenhanceAsync(
                item.HistoryItem,
                forceEnhancement: true,
                windowLifetime.Token);
            if (!result.Success || result.Item is null)
            {
                statusOverride = result.Message;
                return;
            }

            UpdateAudioFileQueueItem(item.Id, current => current.MarkCompleted(result.Item));
            RefreshAudioFileQueueListView(item.Id);
            await RefreshHistoryAsync(windowLifetime.Token);
            statusOverride = await PersistAudioFileQueueSnapshotAsync() ?? "Audio transcription enhanced";
        }
        catch (OperationCanceledException) when (windowLifetime.IsCancellationRequested)
        {
            statusOverride = "Closing";
        }
        catch (Exception ex)
        {
            statusOverride = $"Audio transcription enhancement failed: {ex.Message}";
        }
        finally
        {
            isEnhancingAudioFileQueueItem = false;
            RefreshUiFromControllerState(statusOverride);
        }
    }

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

    private async Task RefreshMetricsWithStatusAsync(string status)
    {
        try
        {
            var metricsWarning = await RefreshMetricsBestEffortAsync(windowLifetime.Token);
            RefreshUiFromControllerState(metricsWarning ?? status);
        }
        catch (OperationCanceledException) when (windowLifetime.IsCancellationRequested)
        {
            RefreshUiFromControllerState("Closing");
        }
        catch (Exception ex)
        {
            RefreshUiFromControllerState($"Metrics refresh failed: {ex.Message}");
        }
    }

    private async Task RefreshMetricsAsync(CancellationToken cancellationToken)
    {
        MetricsDatabasePathTextBox.Text = metricsPath;
        if (metricsInitializationWarning is not null)
        {
            ApplyMetricsUnavailable(metricsInitializationWarning);
            TranscriptionModelPerformanceListView.ItemsSource = MetricsUnavailableRows("Metrics are disabled for this session");
            EnhancementModelPerformanceListView.ItemsSource = MetricsUnavailableRows("Metrics are disabled for this session");
            return;
        }

        var filter = SelectedMetricsTimeFilter();
        var since = filter.Since(DateTimeOffset.Now, TimeZoneInfo.Local);
        var summary = await sessionMetricStore.GetSummaryAsync(since, cancellationToken);
        var transcriptionStats = await sessionMetricStore.ListTranscriptionModelPerformanceAsync(
            since,
            cancellationToken);
        var enhancementStats = await sessionMetricStore.ListEnhancementModelPerformanceAsync(
            since,
            cancellationToken);

        ApplyMetricsDashboardPresentation(SessionMetricsDashboardPresenter.Present(filter.Label, summary));
        TranscriptionModelPerformanceListView.ItemsSource = ModelPerformancePresenter
            .PresentTranscription(transcriptionStats);
        EnhancementModelPerformanceListView.ItemsSource = ModelPerformancePresenter
            .PresentEnhancement(enhancementStats);
    }

    private async Task<string?> RefreshMetricsBestEffortAsync(CancellationToken cancellationToken)
    {
        try
        {
            await RefreshMetricsAsync(cancellationToken);
            return metricsInitializationWarning;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            MetricsDatabasePathTextBox.Text = metricsPath;
            ApplyMetricsUnavailable($"Metrics unavailable: {ex.Message}");
            TranscriptionModelPerformanceListView.ItemsSource = MetricsUnavailableRows("Metrics refresh failed");
            EnhancementModelPerformanceListView.ItemsSource = MetricsUnavailableRows("Metrics refresh failed");
            return $"Metrics refresh failed: {ex.Message}";
        }
    }

    private static IReadOnlyList<ModelPerformanceRow> MetricsUnavailableRows(string title) =>
    [
        new(
            title,
            string.Empty,
            "Metrics storage is unavailable in this session.",
            "No data",
            "Unavailable",
            IsEmpty: true)
    ];

    private async Task ExportMetricsAsync()
    {
        if (metricsInitializationWarning is not null)
        {
            RefreshUiFromControllerState(metricsInitializationWarning);
            return;
        }

        try
        {
            var filter = SelectedMetricsTimeFilter();
            var since = filter.Since(DateTimeOffset.Now, TimeZoneInfo.Local);
            var summary = await sessionMetricStore.GetSummaryAsync(since, windowLifetime.Token);
            var transcriptionStats = await sessionMetricStore.ListTranscriptionModelPerformanceAsync(
                since,
                windowLifetime.Token);
            var enhancementStats = await sessionMetricStore.ListEnhancementModelPerformanceAsync(
                since,
                windowLifetime.Token);
            var picker = new FileSavePicker
            {
                SuggestedStartLocation = PickerLocationId.DocumentsLibrary,
                SuggestedFileName = $"VoiceInk-metrics-{filter.Id}-{DateTimeOffset.Now:yyyyMMdd-HHmmss}"
            };
            picker.FileTypeChoices.Add("CSV file", [".csv"]);
            InitializeWithWindow.Initialize(picker, WindowNative.GetWindowHandle(this));

            var file = await picker.PickSaveFileAsync();
            if (file is null)
            {
                RefreshUiFromControllerState("Metrics export canceled");
                return;
            }

            await FileIO.WriteTextAsync(
                file,
                MetricsCsvExporter.Export(filter.Label, summary, transcriptionStats, enhancementStats));
            RefreshUiFromControllerState($"Metrics exported: {file.Name}");
        }
        catch (OperationCanceledException) when (windowLifetime.IsCancellationRequested)
        {
            RefreshUiFromControllerState("Closing");
        }
        catch (Exception ex)
        {
            RefreshUiFromControllerState($"Metrics export failed: {ex.Message}");
        }
    }

    private async Task ResetMetricsAsync()
    {
        if (metricsInitializationWarning is not null)
        {
            RefreshUiFromControllerState(metricsInitializationWarning);
            return;
        }

        if (IsOperationActive())
        {
            RefreshUiFromControllerState("Finish the current operation before resetting metrics");
            return;
        }

        var dialog = new ContentDialog
        {
            XamlRoot = Content.XamlRoot,
            Title = "Reset metrics?",
            Content = "This deletes local session metrics and model performance totals. History, recordings, models, settings, and diagnostics stay unchanged.",
            PrimaryButtonText = "Reset",
            SecondaryButtonText = "Cancel",
            DefaultButton = ContentDialogButton.Secondary
        };

        var result = await dialog.ShowAsync();
        if (result != ContentDialogResult.Primary)
        {
            RefreshUiFromControllerState("Metrics reset canceled");
            return;
        }

        try
        {
            await sessionMetricStore.ClearAsync(windowLifetime.Token);
            await RefreshMetricsAsync(windowLifetime.Token);
            RefreshUiFromControllerState("Metrics reset");
        }
        catch (OperationCanceledException) when (windowLifetime.IsCancellationRequested)
        {
            RefreshUiFromControllerState("Closing");
        }
        catch (Exception ex)
        {
            RefreshUiFromControllerState($"Metrics reset failed: {ex.Message}");
        }
    }

    private void ApplyMetricsDashboardPresentation(SessionMetricsDashboardPresentation presentation)
    {
        MetricsHeroTitleTextBlock.Text = presentation.HeroTitle;
        MetricsHeroSubtitleTextBlock.Text = presentation.IsEmpty ? string.Empty : presentation.HeroSubtitle;
        MetricsEmptyStateTextBlock.Text = presentation.IsEmpty ? presentation.HeroSubtitle : string.Empty;
        MetricsEmptyStateTextBlock.Visibility = presentation.IsEmpty ? Visibility.Visible : Visibility.Collapsed;
        MetricsDashboardCardsListView.ItemsSource = presentation.Cards;
        MetricsDataGuidanceListView.ItemsSource = presentation.DataGuidanceRows;
        MetricsActionListView.ItemsSource = presentation.ActionRows;
        MetricsDiagnosticsListView.ItemsSource = presentation.DiagnosticsRows;
        MetricsAudioDurationTextBlock.Text = $"{presentation.FilterLabel} - {presentation.AudioDurationDisplay}";
    }

    private void ApplyMetricsUnavailable(string message)
    {
        MetricsHeroTitleTextBlock.Text = message;
        MetricsHeroSubtitleTextBlock.Text = string.Empty;
        MetricsEmptyStateTextBlock.Text = string.Empty;
        MetricsEmptyStateTextBlock.Visibility = Visibility.Collapsed;
        MetricsDashboardCardsListView.ItemsSource = Array.Empty<SessionMetricsDashboardCard>();
        MetricsDataGuidanceListView.ItemsSource = Array.Empty<SessionMetricsDataGuidanceRow>();
        MetricsActionListView.ItemsSource = Array.Empty<SessionMetricsActionRow>();
        MetricsDiagnosticsListView.ItemsSource = Array.Empty<SessionMetricsDiagnosticsRow>();
        MetricsAudioDurationTextBlock.Text = string.Empty;
    }

    private SessionMetricsTimeFilter SelectedMetricsTimeFilter() =>
        MetricsTimeFilterComboBox.SelectedItem as SessionMetricsTimeFilter
        ?? SessionMetricsTimeFilter.Last7Days;

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
            var metricsWarning = await RefreshMetricsBestEffortAsync(windowLifetime.Token);
            if (result.Item is not null)
            {
                SelectHistoryItem(result.Item.Id);
            }

            statusOverride = metricsWarning ?? result.Message;
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
            || isReenhancingHistory
            || isCopyingHistory
            || isQuickAdding)
        {
            return;
        }

        try
        {
            if (historyWindow is null)
            {
                historyWindow = new HistoryWindow(
                    historyStore,
                    historyRetryService,
                    historyReenhancementService,
                    textInjectionService,
                    recordingsDirectory);
                historyWindow.Closed += HistoryWindow_Closed;
            }

            historyWindow.Activate();
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

    private void HistoryWindow_Closed(object sender, WindowEventArgs args)
    {
        if (historyWindow is not null)
        {
            historyWindow.Closed -= HistoryWindow_Closed;
            historyWindow = null;
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
            || isReenhancingHistory
            || isCopyingHistory
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
                var metricsWarning = await RefreshMetricsBestEffortAsync(windowLifetime.Token);
                SelectHistoryItem(result.Item.Id);
                await textInjectionService.CopyAsync(result.Item.Text, windowLifetime.Token);
                statusOverride = metricsWarning ?? "Retry transcription copied";
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

    private async Task CopySelectedHistoryTextAsync(HistoryCopyTextKind kind)
    {
        if (isCopyingHistory)
        {
            return;
        }

        var item = SelectedHistoryItem();
        if (item is null)
        {
            RefreshUiFromControllerState("Select a transcription to copy");
            return;
        }

        var result = HistoryCopyTextSelector.Select(item, kind);
        if (!result.Success)
        {
            RefreshUiFromControllerState(result.Message);
            return;
        }

        isCopyingHistory = true;
        var statusOverride = "Copying history text";
        RefreshUiFromControllerState(statusOverride);
        try
        {
            await textInjectionService.CopyAsync(result.Text, windowLifetime.Token);
            statusOverride = result.Message;
        }
        catch (OperationCanceledException) when (windowLifetime.IsCancellationRequested)
        {
            statusOverride = "Closing";
        }
        catch (Exception ex)
        {
            statusOverride = $"Copy failed: {ex.Message}";
        }
        finally
        {
            isCopyingHistory = false;
            RefreshUiFromControllerState(statusOverride);
        }
    }

    private async Task ReenhanceSelectedHistoryAsync()
    {
        if (isReenhancingHistory)
        {
            return;
        }

        var item = SelectedHistoryItem();
        if (item is null)
        {
            RefreshUiFromControllerState("Select a transcription to re-enhance");
            return;
        }

        var statusOverride = "Re-enhancing transcription";
        isReenhancingHistory = true;
        RefreshUiFromControllerState(statusOverride);

        try
        {
            await SaveSettingsAsync(windowLifetime.Token);
            var result = await historyReenhancementService.ReenhanceAsync(item, windowLifetime.Token);
            await RefreshHistoryAsync(windowLifetime.Token);
            var metricsWarning = await RefreshMetricsBestEffortAsync(windowLifetime.Token);
            if (result.Item is not null)
            {
                SelectHistoryItem(result.Item.Id);
            }

            statusOverride = metricsWarning ?? result.Message;
        }
        catch (OperationCanceledException) when (windowLifetime.IsCancellationRequested)
        {
            statusOverride = "Closing";
        }
        catch (Exception ex)
        {
            statusOverride = $"Re-enhance failed: {ex.Message}";
        }
        finally
        {
            isReenhancingHistory = false;
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
        if (isStopping || isCanceling || isPastingLast || isRetryingHistory || isReenhancingHistory || isCopyingHistory)
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
            HistoryAnalysisListView.ItemsSource = null;
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
        HistoryAnalysisListView.ItemsSource = HistoryAnalysisPresenter.Present(item);
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
        RefreshHistoryWaveform(audioPath);
        HistoryPlaybackRateComboBox.SelectedIndex = HistoryPlaybackRatePresenter.SelectedIndexFor(
            HistoryPlaybackRatePresenter.DefaultChoice.Value);
        ApplyHistoryPlaybackRate();
        HistoryAudioPanel.Visibility = Visibility.Visible;
    }

    private void ClearHistoryAudioPlayer()
    {
        HistoryAudioPlayer.Source = null;
        HistoryWaveformItemsControl.ItemsSource = null;
        HistoryWaveformItemsControl.Visibility = Visibility.Collapsed;
        HistoryAudioPanel.Visibility = Visibility.Collapsed;
    }

    private void ApplyHistoryPlaybackRate()
    {
        var rate = HistoryPlaybackRatePresenter.ChoiceAtOrDefault(HistoryPlaybackRateComboBox.SelectedIndex).Value;
        HistoryAudioPlayer.MediaPlayer.PlaybackSession.PlaybackRate = rate;
    }

    private void RefreshHistoryWaveform(string audioPath)
    {
        try
        {
            var peaks = HistoryWaveformPeakExtractor.ExtractPeaks(
                File.ReadAllBytes(audioPath),
                HistoryWaveformPeakCount);
            HistoryWaveformItemsControl.ItemsSource = peaks;
            HistoryWaveformItemsControl.Visibility = peaks.Count > 0
                ? Visibility.Visible
                : Visibility.Collapsed;
        }
        catch
        {
            HistoryWaveformItemsControl.ItemsSource = null;
            HistoryWaveformItemsControl.Visibility = Visibility.Collapsed;
        }
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

    private void RestoreAndShowSection(string sectionTag)
    {
        RestoreAndActivateWindow();
        ShowShellSection(sectionTag);
        RefreshUiFromControllerState();
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
            ScheduleModelWarmup(settings, "provider settings", updateMainStatus: false);
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

    private async Task TestCloudTranscriptionProviderAsync()
    {
        if (!settingsLoaded || IsOperationActive(includeCurrentCloudTranscriptionProviderTest: false))
        {
            return;
        }

        isTestingCloudTranscriptionProvider = true;
        var statusOverride = "Testing cloud transcription provider";
        RefreshUiFromControllerState(statusOverride);
        try
        {
            var settings = await CurrentSettingsAsync(windowLifetime.Token, includeShortcutFields: false);
            var result = await cloudTranscriptionProviderProbeService.ProbeAsync(settings, windowLifetime.Token);
            statusOverride = result.Message;
            await RefreshCloudTranscriptionKeyStatusAsync(windowLifetime.Token);
        }
        catch (OperationCanceledException) when (windowLifetime.IsCancellationRequested)
        {
            statusOverride = "Closing";
        }
        catch (Exception ex)
        {
            statusOverride = $"Cloud transcription provider test failed: {ex.Message}";
        }
        finally
        {
            isTestingCloudTranscriptionProvider = false;
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

            var ocrRegionError = ValidateOcrRegionSettings();
            if (ocrRegionError is not null)
            {
                RefreshUiFromControllerState(ocrRegionError);
                return;
            }

            EnhancementTimeoutTextBox.Text = timeoutSeconds.ToString(CultureInfo.InvariantCulture);
            ShortEnhancementThresholdTextBox.Text = shortThreshold.ToString(CultureInfo.InvariantCulture);
            var configurationError = EnhancementConfiguration.ValidateRequiredSettings(
                EnhancementEnabledCheckBox.IsChecked == true,
                EnhancementEndpointTextBox.Text,
                EnhancementModelTextBox.Text,
                SelectedEnhancementProviderId());
            if (configurationError is not null)
            {
                RefreshUiFromControllerState(configurationError);
                return;
            }

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

    private async Task ToggleEnhancementAsync()
    {
        if (!settingsLoaded || IsOperationActive())
        {
            return;
        }

        try
        {
            var isEnabled = EnhancementEnabledCheckBox.IsChecked != true;
            EnhancementEnabledCheckBox.IsChecked = isEnabled;
            await SaveSettingsAsync(windowLifetime.Token);
            RefreshUiFromControllerState(isEnabled ? "Enhancement enabled" : "Enhancement disabled");
        }
        catch (OperationCanceledException) when (windowLifetime.IsCancellationRequested)
        {
            RefreshUiFromControllerState("Closing");
        }
        catch (Exception ex)
        {
            RefreshUiFromControllerState($"Enhancement toggle failed: {ex.Message}");
        }
    }

    private async Task CyclePowerModeAsync()
    {
        if (!settingsLoaded || IsOperationActive())
        {
            return;
        }

        try
        {
            var settings = await CurrentSettingsAsync(windowLifetime.Token, includeShortcutFields: false);
            selectedPowerModeRuleId = PowerModeShortcutCycler.NextRuleId(settings, powerModeRules);
            await SaveSettingsAsync(windowLifetime.Token);
            var selectedRule = selectedPowerModeRuleId is { } ruleId
                ? powerModeRules.FirstOrDefault(rule => rule.Id == ruleId && rule.IsEnabled)
                : null;
            RefreshPowerModeRulesListView(selectedPowerModeRuleId);
            RefreshUiFromControllerState(
                selectedRule is null
                    ? "Power Mode: Auto"
                    : $"Power Mode: {PowerModeDisplay(selectedRule.Name, selectedRule.Emoji)}");
        }
        catch (OperationCanceledException) when (windowLifetime.IsCancellationRequested)
        {
            RefreshUiFromControllerState("Closing");
        }
        catch (Exception ex)
        {
            RefreshUiFromControllerState($"Power Mode shortcut failed: {ex.Message}");
        }
    }

    private async Task SelectPowerModeRuleShortcutAsync(Guid? ruleId)
    {
        if (!settingsLoaded || IsOperationActive() || ruleId is null)
        {
            return;
        }

        try
        {
            var selectedRule = powerModeRules.FirstOrDefault(rule => rule.Id == ruleId.Value && rule.IsEnabled);
            if (selectedRule is null)
            {
                RefreshUiFromControllerState("Power Mode shortcut target unavailable");
                return;
            }

            selectedPowerModeRuleId = selectedRule.Id;
            await SaveSettingsAsync(windowLifetime.Token);
            RefreshPowerModeRulesListView(selectedPowerModeRuleId);
            RefreshUiFromControllerState($"Power Mode: {PowerModeDisplay(selectedRule.Name, selectedRule.Emoji)}");
        }
        catch (OperationCanceledException) when (windowLifetime.IsCancellationRequested)
        {
            RefreshUiFromControllerState("Closing");
        }
        catch (Exception ex)
        {
            RefreshUiFromControllerState($"Power Mode shortcut failed: {ex.Message}");
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
            var preset = EnhancementProviderPresetCatalog.Resolve(SelectedEnhancementProviderId());
            if (!preset.RequiresApiKey)
            {
                statusOverride = $"{preset.DisplayName} does not require an API key";
                return;
            }

            await secretStore.SaveSecretAsync(
                EnhancementConfiguration.SecretNameForProvider(preset.Id),
                secret,
                windowLifetime.Token);
            EnhancementApiKeyPasswordBox.Password = string.Empty;
            await RefreshEnhancementKeyStatusAsync(windowLifetime.Token);
            statusOverride = $"{preset.DisplayName} API key saved";
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
            var preset = EnhancementProviderPresetCatalog.Resolve(SelectedEnhancementProviderId());
            foreach (var secretName in EnhancementConfiguration.SecretNamesForProvider(preset.Id))
            {
                await secretStore.DeleteSecretAsync(secretName, windowLifetime.Token);
            }

            EnhancementApiKeyPasswordBox.Password = string.Empty;
            await RefreshEnhancementKeyStatusAsync(windowLifetime.Token);
            statusOverride = $"{preset.DisplayName} API key cleared";
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

    private async Task RefreshEnhancementModelsAsync()
    {
        if (!settingsLoaded || IsOperationActive())
        {
            return;
        }

        var preset = EnhancementProviderPresetCatalog.Resolve(SelectedEnhancementProviderId());
        if (!SupportsDynamicEnhancementModels(preset))
        {
            RefreshUiFromControllerState("Select OpenRouter or Ollama to refresh models");
            return;
        }

        isRefreshingEnhancementModels = true;
        var statusOverride = $"Refreshing {preset.DisplayName} models";
        RefreshUiFromControllerState(statusOverride);
        try
        {
            var endpoint = string.IsNullOrWhiteSpace(EnhancementEndpointTextBox.Text)
                ? preset.Endpoint
                : EnhancementEndpointTextBox.Text.Trim();
            var result = await RefreshEnhancementModelsForPresetAsync(preset, endpoint, windowLifetime.Token);
            if (!result.Success)
            {
                statusOverride = result.Message;
                return;
            }

            StoreDynamicEnhancementModelChoices(preset, result.Models);
            var selectedModel = EnhancementModelTextBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(selectedModel)
                || !result.Models.Any(model => string.Equals(
                    model,
                    selectedModel,
                    StringComparison.OrdinalIgnoreCase)))
            {
                selectedModel = PreferredEnhancementModel(preset, result.Models);
                EnhancementModelTextBox.Text = selectedModel;
            }

            RefreshEnhancementModelChoices(selectedModel);
            statusOverride = result.Message;
        }
        catch (OperationCanceledException) when (windowLifetime.IsCancellationRequested)
        {
            statusOverride = "Closing";
        }
        catch (Exception ex)
        {
            statusOverride = $"Ollama model refresh failed: {ex.Message}";
        }
        finally
        {
            isRefreshingEnhancementModels = false;
            RefreshUiFromControllerState(statusOverride);
        }
    }

    private async Task<DynamicEnhancementModelRefreshResult> RefreshEnhancementModelsForPresetAsync(
        EnhancementProviderPreset preset,
        string endpoint,
        CancellationToken cancellationToken)
    {
        if (preset.Id == EnhancementProviderPresetCatalog.OpenRouter.Id)
        {
            var result = await openRouterModelCatalogClient.ListModelsAsync(endpoint, cancellationToken);
            return new DynamicEnhancementModelRefreshResult(result.Success, result.Models, result.Message);
        }

        var ollamaResult = await ollamaModelCatalogClient.ListModelsAsync(endpoint, cancellationToken);
        return new DynamicEnhancementModelRefreshResult(
            ollamaResult.Success,
            ollamaResult.Models,
            ollamaResult.Message);
    }

    private void StoreDynamicEnhancementModelChoices(
        EnhancementProviderPreset preset,
        IReadOnlyList<string> models)
    {
        if (preset.Id == EnhancementProviderPresetCatalog.OpenRouter.Id)
        {
            openRouterEnhancementModelChoices = models;
            return;
        }

        if (preset.Id == EnhancementProviderPresetCatalog.Ollama.Id)
        {
            ollamaEnhancementModelChoices = models;
        }
    }

    private static string PreferredEnhancementModel(
        EnhancementProviderPreset preset,
        IReadOnlyList<string> models) =>
        models.FirstOrDefault(model => string.Equals(
            model,
            preset.DefaultModel,
            StringComparison.OrdinalIgnoreCase))
        ?? models[0];

    private sealed record DynamicEnhancementModelRefreshResult(
        bool Success,
        IReadOnlyList<string> Models,
        string Message);

    private async Task SavePromptAsync()
    {
        if (!settingsLoaded || IsOperationActive())
        {
            return;
        }

        try
        {
            var existingPrompt = PromptEditorPrompt();
            EnhancementPrompt updatedPrompt;
            if (existingPrompt?.IsPredefined == true)
            {
                updatedPrompt = existingPrompt with
                {
                    TriggerWords = EnhancementPromptLibrary.TriggerWordsFromText(PromptTriggerWordsTextBox.Text)
                };
            }
            else
            {
                updatedPrompt = EnhancementPromptLibrary.CreateCustomPrompt(
                    existingPrompt?.Id ?? Guid.NewGuid(),
                    PromptTitleTextBox.Text,
                    PromptInstructionsTextBox.Text,
                    PromptIconTextBox.Text,
                    PromptDescriptionTextBox.Text,
                    PromptTriggerWordsTextBox.Text,
                    PromptUseSystemInstructionsCheckBox.IsChecked == true);
            }

            var nextPrompts = EnhancementPromptLibrary.UpdatePrompt(enhancementPrompts, updatedPrompt);
            await PersistPromptLibraryAsync(nextPrompts, powerModeRules, updatedPrompt.Id);
            RefreshUiFromControllerState($"{updatedPrompt.Title} prompt saved");
        }
        catch (ArgumentException ex)
        {
            RefreshUiFromControllerState(ex.Message);
        }
        catch (OperationCanceledException) when (windowLifetime.IsCancellationRequested)
        {
            RefreshUiFromControllerState("Closing");
        }
        catch (Exception ex)
        {
            RefreshUiFromControllerState($"Prompt save failed: {ex.Message}");
        }
    }

    private async Task DeletePromptAsync()
    {
        if (!settingsLoaded || IsOperationActive())
        {
            return;
        }

        var prompt = PromptEditorPrompt();
        if (prompt is null || prompt.IsPredefined)
        {
            RefreshUiFromControllerState("Only custom prompts can be deleted");
            return;
        }

        try
        {
            var nextPrompts = EnhancementPromptLibrary.DeletePrompt(enhancementPrompts, prompt.Id);
            var nextPowerModeRules = ClearPowerModePromptOverridesForDeletedPrompt(powerModeRules, prompt.Id);
            var selectedPromptId = EnhancementPromptLibrary.ResolveSelectedPromptId(null, nextPrompts);
            await PersistPromptLibraryAsync(nextPrompts, nextPowerModeRules, selectedPromptId);
            RefreshUiFromControllerState($"{prompt.Title} prompt deleted");
        }
        catch (OperationCanceledException) when (windowLifetime.IsCancellationRequested)
        {
            RefreshUiFromControllerState("Closing");
        }
        catch (Exception ex)
        {
            RefreshUiFromControllerState($"Prompt delete failed: {ex.Message}");
        }
    }

    private async Task PersistPromptLibraryAsync(
        IReadOnlyList<EnhancementPrompt> nextPrompts,
        IReadOnlyList<PowerModeRule> nextPowerModeRules,
        Guid selectedPromptId)
    {
        var powerModePromptId = SelectedPowerModePromptOverrideId();
        var previousPrompts = enhancementPrompts;
        var previousPowerModeRules = powerModeRules;

        enhancementPrompts = nextPrompts;
        powerModeRules = nextPowerModeRules;
        RefreshEnhancementPromptChoices(selectedPromptId);
        RefreshPowerModePromptChoices(powerModePromptId);
        RefreshPromptEditorFields();
        try
        {
            await SaveSettingsAsync(windowLifetime.Token);
        }
        catch
        {
            enhancementPrompts = previousPrompts;
            powerModeRules = previousPowerModeRules;
            RefreshEnhancementPromptChoices(selectedPromptId: null);
            RefreshPowerModePromptChoices(powerModePromptId);
            RefreshPromptEditorFields();
            throw;
        }
    }

    private static IReadOnlyList<PowerModeRule> ClearPowerModePromptOverridesForDeletedPrompt(
        IReadOnlyList<PowerModeRule> rules,
        Guid promptId) =>
        rules
            .Select(rule => rule.SelectedEnhancementPromptIdOverride == promptId
                ? rule with { SelectedEnhancementPromptIdOverride = null }
                : rule)
            .ToArray();

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
            RecreateControllerIfAudioInputChanged();
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
            var settings = await CurrentSettingsAsync(windowLifetime.Token, includeShortcutFields: false);
            var result = AudioInputDeviceSelection.BuildChoices(audioInputDevices, settings);
            audioInputChoices = result.Choices;
            audioInputSelectionNotice = result.Notice;
            AudioInputComboBox.ItemsSource = audioInputChoices;
            AudioInputComboBox.SelectedIndex = Math.Clamp(
                result.SelectedIndex,
                0,
                Math.Max(0, audioInputChoices.Count - 1));
            RefreshAudioInputHealthRows();
            UpdateAudioInputStatusNotice();
            RecreateControllerIfAudioInputChanged();

            RefreshUiFromControllerState(result.Warning ?? "Audio input updated");
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

    private async Task RefreshAudioInputDevicesAfterChangeAsync()
    {
        if (windowLifetime.IsCancellationRequested)
        {
            return;
        }

        if (!settingsLoaded)
        {
            return;
        }

        if (!CanRefreshAudioInputsFromDeviceChange())
        {
            pendingAudioInputDeviceChangeRefresh = true;
            RefreshUiFromControllerState("Audio inputs changed; refresh when idle");
            return;
        }

        if (isRefreshingAudioInputsFromDeviceChange)
        {
            pendingAudioInputDeviceChangeRefresh = true;
            return;
        }

        isRefreshingAudioInputsFromDeviceChange = true;
        try
        {
            do
            {
                pendingAudioInputDeviceChangeRefresh = false;
                if (!CanRefreshAudioInputsFromDeviceChange())
                {
                    pendingAudioInputDeviceChangeRefresh = true;
                    RefreshUiFromControllerState("Audio inputs changed; refresh when idle");
                    return;
                }

                var settings = await CurrentSettingsAsync(windowLifetime.Token, includeShortcutFields: false);
                var warning = await RefreshAudioInputDevicesAsync(settings, windowLifetime.Token);
                if (!CanRefreshAudioInputsFromDeviceChange())
                {
                    pendingAudioInputDeviceChangeRefresh = true;
                    RefreshUiFromControllerState("Audio inputs changed; refresh when idle");
                    return;
                }

                RecreateControllerIfAudioInputChanged();
                RefreshUiFromControllerState(warning ?? "Audio inputs changed");
            }
            while (pendingAudioInputDeviceChangeRefresh && CanRefreshAudioInputsFromDeviceChange());
        }
        catch (OperationCanceledException) when (windowLifetime.IsCancellationRequested)
        {
            RefreshUiFromControllerState("Closing");
        }
        catch (Exception ex)
        {
            RefreshUiFromControllerState($"Audio input refresh failed: {ex.Message}");
        }
        finally
        {
            isRefreshingAudioInputsFromDeviceChange = false;
        }
    }

    private async Task<string?> RefreshAudioInputDevicesAsync(
        AppSettings settings,
        CancellationToken cancellationToken)
    {
        var devices = await audioInputDeviceProvider.ListInputDevicesAsync(cancellationToken);
        var result = AudioInputDeviceSelection.BuildChoices(devices, settings);

        audioInputDevices = devices;
        audioInputChoices = result.Choices;
        audioInputSelectionNotice = result.Notice;
        AudioInputComboBox.ItemsSource = audioInputChoices;
        AudioInputComboBox.SelectedIndex = Math.Clamp(
            result.SelectedIndex,
            0,
            Math.Max(0, audioInputChoices.Count - 1));
        RefreshAudioInputHealthRows();
        UpdateAudioInputStatusNotice();

        return result.Warning;
    }

    private void RefreshAudioInputHealthRows()
    {
        if (AudioInputDeviceHealthListView is null)
        {
            return;
        }

        audioInputHealthRows = AudioInputDeviceHealthPresenter.BuildRows(
            audioInputChoices,
            SelectedAudioInputDeviceChoice(),
            prioritizedAudioInputDevices,
            SelectedAudioInputMode());
        AudioInputDeviceHealthListView.ItemsSource = audioInputHealthRows;
    }

    private void UpdateAudioInputStatusNotice()
    {
        var notice = audioInputSelectionNotice;
        if (notice is null)
        {
            AudioInputStatusInfoBar.Title = "Audio input";
            AudioInputStatusInfoBar.Message = "Refresh audio inputs to check microphone availability.";
            AudioInputStatusInfoBar.Severity = InfoBarSeverity.Informational;
            AudioInputStatusActionTextBlock.Text = string.Empty;
            return;
        }

        AudioInputStatusInfoBar.Title = notice.Title;
        AudioInputStatusInfoBar.Message = notice.Message;
        AudioInputStatusInfoBar.Severity = ToInfoBarSeverity(notice.Kind);
        AudioInputStatusActionTextBlock.Text = notice.ActionText;
    }

    private static InfoBarSeverity ToInfoBarSeverity(AudioInputDeviceSelectionNoticeKind kind) =>
        kind switch
        {
            AudioInputDeviceSelectionNoticeKind.Success => InfoBarSeverity.Success,
            AudioInputDeviceSelectionNoticeKind.Warning => InfoBarSeverity.Warning,
            AudioInputDeviceSelectionNoticeKind.Error => InfoBarSeverity.Error,
            _ => InfoBarSeverity.Informational
        };

    private async Task SaveSettingsAsync(CancellationToken cancellationToken)
    {
        var previousStartupState = await startupRegistrationService.GetStateAsync(cancellationToken);
        var settings = await CurrentSettingsAsync(cancellationToken, includeShortcutFields: false);
        await ApplyStartupRegistrationAsync(settings, cancellationToken);
        try
        {
            await settingsStore.SaveAsync(settings, cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception saveEx)
        {
            try
            {
                await startupRegistrationService.RestoreStateAsync(previousStartupState, CancellationToken.None);
            }
            catch (Exception rollbackEx)
            {
                throw new InvalidOperationException(
                    $"Settings save failed: {saveEx.Message} Previous launch-at-login registration could not be restored: {rollbackEx.Message}",
                    saveEx);
            }

            throw;
        }
    }

    private async Task SaveSettingsFromTrayAsync(string status)
    {
        if (!settingsLoaded || IsOperationActive())
        {
            return;
        }

        try
        {
            await SaveSettingsAsync(windowLifetime.Token);
            RefreshUiFromControllerState(status);
        }
        catch (OperationCanceledException) when (windowLifetime.IsCancellationRequested)
        {
            RefreshUiFromControllerState("Closing");
        }
        catch (Exception ex)
        {
            RefreshUiFromControllerState($"Tray setting update failed: {ex.Message}");
        }
    }

    private async Task ApplyStartupRegistrationAsync(
        AppSettings settings,
        CancellationToken cancellationToken)
    {
        await startupRegistrationService.SetEnabledAsync(settings.LaunchAtLogin, cancellationToken);
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
            Language = SelectedLanguageCode(),
            ImportedWhisperModels = localWhisperModels.ToArray(),
            TranscriptionProvider = SelectedTranscriptionProvider(),
            CloudTranscriptionProviderId = SelectedCloudTranscriptionProviderId(),
            CloudTranscriptionEndpoint = CloudTranscriptionEndpointTextBox.Text.Trim(),
            CloudTranscriptionModel = CloudTranscriptionModelTextBox.Text.Trim(),
            Hotkey = includeShortcutFields ? RecordingHotkeyTextBox.Text.Trim() : settings.Hotkey,
            PrimaryRecordingShortcutMode = includeShortcutFields
                ? SelectedPrimaryRecordingShortcutMode()
                : settings.PrimaryRecordingShortcutMode,
            SecondaryRecordingHotkey = includeShortcutFields
                ? SecondaryRecordingHotkeyTextBox.Text.Trim()
                : settings.SecondaryRecordingHotkey,
            SecondaryRecordingShortcutMode = includeShortcutFields
                ? SelectedSecondaryRecordingShortcutMode()
                : settings.SecondaryRecordingShortcutMode,
            IsMiddleClickRecordingEnabled = includeShortcutFields
                ? MiddleClickRecordingCheckBox.IsChecked == true
                : settings.IsMiddleClickRecordingEnabled,
            MiddleClickActivationDelayMilliseconds = includeShortcutFields
                ? CheckedNumberBoxIntValue(MiddleClickDelayNumberBox)
                : settings.MiddleClickActivationDelayMilliseconds,
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
            ToggleEnhancementHotkey = includeShortcutFields
                ? ToggleEnhancementHotkeyTextBox.Text.Trim()
                : settings.ToggleEnhancementHotkey,
            CyclePowerModeHotkey = includeShortcutFields
                ? CyclePowerModeHotkeyTextBox.Text.Trim()
                : settings.CyclePowerModeHotkey,
            AudioInputDeviceNumber = SelectedAudioInputDeviceNumber(),
            AudioInputDeviceName = SelectedAudioInputDeviceName(),
            AudioInputEndpointId = SelectedAudioInputEndpointId(),
            AudioInputMode = SelectedAudioInputMode(),
            PrioritizedAudioInputDevices = prioritizedAudioInputDevices.ToArray(),
            RestoreClipboard = RestoreClipboardCheckBox.IsChecked == true,
            ClipboardRestoreDelaySeconds = SelectedClipboardRestoreDelaySeconds(),
            PasteMethod = SelectedPasteMethod(),
            LaunchAtLogin = LaunchAtLoginCheckBox.IsChecked == true,
            PrewarmModelOnWake = PrewarmModelOnWakeCheckBox.IsChecked == true,
            IsVadEnabled = VoiceActivityDetectionCheckBox.IsChecked == true,
            ShowLiveTranscriptPreview = ShowLiveTranscriptPreviewCheckBox.IsChecked == true,
            RecorderStyle = SelectedRecorderStyle(),
            IsSoundFeedbackEnabled = SoundFeedbackCheckBox.IsChecked == true,
            StartSoundMode = SelectedStartSoundMode(),
            StopSoundMode = SelectedStopSoundMode(),
            CustomStartSoundPath = customStartSoundPath,
            CustomStopSoundPath = customStopSoundPath,
            IsSystemMuteEnabled = MuteSystemAudioCheckBox.IsChecked == true,
            IsPauseMediaEnabled = PauseMediaCheckBox.IsChecked == true,
            AudioResumptionDelaySeconds = SelectedAudioResumptionDelaySeconds(),
            IsEnhancementEnabled = EnhancementEnabledCheckBox.IsChecked == true,
            UseClipboardContext = UseClipboardContextCheckBox.IsChecked == true,
            UseOcrContext = UseOcrContextCheckBox.IsChecked == true,
            UseOcrCaptureRegion = UseOcrCaptureRegionCheckBox.IsChecked == true,
            OcrCaptureRegionLeft = CheckedNumberBoxIntValue(OcrRegionLeftNumberBox),
            OcrCaptureRegionTop = CheckedNumberBoxIntValue(OcrRegionTopNumberBox),
            OcrCaptureRegionWidth = CheckedNumberBoxIntValue(OcrRegionWidthNumberBox),
            OcrCaptureRegionHeight = CheckedNumberBoxIntValue(OcrRegionHeightNumberBox),
            EnhancementProviderId = SelectedEnhancementProviderId(),
            EnhancementEndpoint = EnhancementEndpointTextBox.Text.Trim(),
            EnhancementModel = EnhancementModelTextBox.Text.Trim(),
            CustomEnhancementPrompts = EnhancementPromptLibrary
                .PersistentPrompts(enhancementPrompts)
                .ToArray(),
            SelectedEnhancementPromptId = EnhancementPromptLibrary.ResolveSelectedPromptId(
                SelectedEnhancementPromptId(),
                enhancementPrompts),
            EnhancementTimeoutSeconds = ParsedPositiveOrDefault(EnhancementTimeoutTextBox.Text, 7),
            EnhancementRetryOnTimeout = EnhancementRetryOnTimeoutCheckBox.IsChecked == true,
            SkipShortEnhancement = SkipShortEnhancementCheckBox.IsChecked == true,
            ShortEnhancementWordThreshold = ParsedPositiveOrDefault(ShortEnhancementThresholdTextBox.Text, 3),
            RemoveFillerWords = RemoveFillerWordsCheckBox.IsChecked == true,
            IsTextFormattingEnabled = TextFormattingCheckBox.IsChecked == true,
            LowercaseTranscription = LowercaseTranscriptionCheckBox.IsChecked == true,
            AppendTrailingSpace = AppendTrailingSpaceCheckBox.IsChecked == true,
            PunctuationCleanupMode = SelectedPunctuationCleanupMode(),
            IsTranscriptionCleanupEnabled = TranscriptionCleanupCheckBox.IsChecked == true,
            TranscriptionRetentionMinutes = SelectedTranscriptionRetentionMinutes(),
            IsAudioCleanupEnabled = AudioCleanupCheckBox.IsChecked == true,
            AudioRetentionPeriod = SelectedAudioRetentionDays(),
            SelectedPowerModeRuleId = SelectedPowerModeRuleId(),
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
            powerModeTargetProvider,
            sessionMetricStore,
            recordingFeedback,
            liveTranscriptionPreviewService,
            powerModeAutoSendService,
            new WavVoiceActivityDetector());

    private static (ISessionMetricStore Store, string? Warning) CreateSessionMetricStore(string databasePath)
    {
        try
        {
            return (new SqliteSessionMetricStore(databasePath), null);
        }
        catch (Exception ex)
        {
            return (DisabledSessionMetricStore.Instance, $"Metrics disabled: {ex.Message}");
        }
    }

    private int? SelectedAudioInputDeviceNumber() =>
        SelectedAudioInputMode() == AudioInputModeSettings.Custom
            ? SelectedAudioInputDeviceChoice()?.DeviceNumber
            : null;

    private string SelectedAudioInputDeviceName()
    {
        if (SelectedAudioInputMode() != AudioInputModeSettings.Custom)
        {
            return string.Empty;
        }

        var choice = SelectedAudioInputDeviceChoice();
        return choice?.DeviceNumber is null ? string.Empty : choice.Name;
    }

    private string SelectedAudioInputEndpointId()
    {
        if (SelectedAudioInputMode() != AudioInputModeSettings.Custom)
        {
            return string.Empty;
        }

        var choice = SelectedAudioInputDeviceChoice();
        return choice?.DeviceNumber is null ? string.Empty : choice.EndpointId;
    }

    private AudioInputDeviceChoice? SelectedAudioInputDeviceChoice()
    {
        var selectedIndex = AudioInputComboBox.SelectedIndex;
        return selectedIndex >= 0 && selectedIndex < audioInputChoices.Count
            ? audioInputChoices[selectedIndex]
            : null;
    }

    private string SelectedAudioInputMode() =>
        AudioInputModeComboBox.SelectedIndex switch
        {
            1 => AudioInputModeSettings.Custom,
            2 => AudioInputModeSettings.Prioritized,
            _ => AudioInputModeSettings.SystemDefault
        };

    private static int AudioInputModeToSelectedIndex(string mode) =>
        AudioInputModeSettings.Normalize(mode) switch
        {
            AudioInputModeSettings.Custom => 1,
            AudioInputModeSettings.Prioritized => 2,
            _ => 0
        };

    private void RefreshAudioInputPriorityControls(string? selectedName = null)
    {
        var isPrioritized = SelectedAudioInputMode() == AudioInputModeSettings.Prioritized;
        AudioInputPriorityPanel.Visibility = isPrioritized ? Visibility.Visible : Visibility.Collapsed;
        var priorityItems = prioritizedAudioInputDevices
            .OrderBy(device => device.Priority)
            .Select(device => new AudioInputPriorityItem(
                device.Name,
                (device.Priority + 1).ToString(CultureInfo.InvariantCulture)))
            .ToArray();
        AudioInputPriorityListView.ItemsSource = priorityItems;

        var selectedIndex = selectedName is null
            ? Math.Min(AudioInputPriorityListView.SelectedIndex, priorityItems.Length - 1)
            : Array.FindIndex(priorityItems, item => item.Name == selectedName);
        AudioInputPriorityListView.SelectedIndex = selectedIndex >= 0 ? selectedIndex : -1;
    }

    private AudioInputPriorityItem? SelectedAudioInputPriorityItem()
    {
        var selectedIndex = AudioInputPriorityListView.SelectedIndex;
        if (selectedIndex < 0 || selectedIndex >= AudioInputPriorityListView.Items.Count)
        {
            return null;
        }

        return AudioInputPriorityListView.Items[selectedIndex] as AudioInputPriorityItem;
    }

    private LocalWhisperModel? SelectedLocalWhisperModelChoice()
    {
        var selectedIndex = ModelComboBox.SelectedIndex;
        return selectedIndex >= 0 && selectedIndex < modelChoices.Count
            ? modelChoices[selectedIndex]
            : null;
    }

    private WhisperModelCatalogItem? SelectedCatalogModelItem()
    {
        var selectedIndex = LocalModelCatalogListView.SelectedIndex;
        return selectedIndex >= 0 && selectedIndex < modelCatalogItems.Count
            ? modelCatalogItems[selectedIndex]
            : null;
    }

    private string SelectedLanguageCode()
    {
        var selectedIndex = LanguageComboBox.SelectedIndex;
        return selectedIndex >= 0 && selectedIndex < languageChoices.Count
            ? languageChoices[selectedIndex].Code
            : CompatibleLanguageForSelectedProvider(selectedLanguage: null);
    }

    private TranscriptionProviderKind SelectedTranscriptionProvider() =>
        TranscriptionProviderComboBox.SelectedIndex == 1
            ? TranscriptionProviderKind.OpenAICompatible
            : TranscriptionProviderKind.LocalWhisper;

    private string SelectedCloudTranscriptionProviderId() =>
        SelectedCloudTranscriptionPreset()?.Id ?? TranscriptionProviderPresetCatalog.Custom.Id;

    private TranscriptionProviderPreset? SelectedCloudTranscriptionPreset() =>
        CloudTranscriptionPresetComboBox.SelectedItem as TranscriptionProviderPreset;

    private string SelectedEnhancementProviderId() =>
        SelectedEnhancementPreset()?.Id ?? EnhancementProviderPresetCatalog.Custom.Id;

    private EnhancementProviderPreset? SelectedEnhancementPreset() =>
        EnhancementProviderPresetComboBox.SelectedItem as EnhancementProviderPreset;

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

        RefreshModelCatalogItems(modelPath);
        RefreshLanguageChoices(modelPath, selectedLanguage: SelectedLanguageCode());
    }

    private void RefreshModelCatalogItems(string? selectedPath = null)
    {
        var previouslySelectedName = SelectedCatalogModelItem()?.Name;
        var modelPath = selectedPath ?? ModelPathTextBox.Text;
        modelCatalogItems = LocalWhisperModelService.BuildCatalogItems(localWhisperModels, modelPath);
        LocalModelCatalogListView.ItemsSource = modelCatalogItems;
        var modelLibraryOverview = ModelLibraryOverviewPresenter.Present(
            modelCatalogItems,
            localWhisperModels.Where(model => ModelPathHealth(model.Path).CanUse),
            modelPath,
            UnavailableImportedModelCount());
        ModelLibraryTitleTextBlock.Text = modelLibraryOverview.Title;
        ModelLibrarySummaryTextBlock.Text = modelLibraryOverview.Summary;
        ModelLibraryDefaultTextBlock.Text = modelLibraryOverview.DefaultModelLabel;
        ModelLibraryCleanupHintTextBlock.Text = modelLibraryOverview.CleanupHint;
        ModelLibraryActionListView.ItemsSource = modelLibraryOverview.ActionRows;
        ModelLibraryStorageGuidanceListView.ItemsSource = modelLibraryOverview.StorageGuidanceRows;

        var health = ModelPathHealth(modelPath);
        var healthPresentation = LocalWhisperModelHealthPresenter.Present(health);
        DefaultModelStatusTextBlock.Text = health.Message;
        ModelRepairHintTextBlock.Text = $"{healthPresentation.Title}: {healthPresentation.Guidance}";
        ModelHealthGuidanceListView.ItemsSource = healthPresentation.GuidanceRows;
        RepairModelPathButton.Content = healthPresentation.ActionLabel;
        currentModelRepairAction = healthPresentation.RepairAction;

        if (!string.IsNullOrWhiteSpace(previouslySelectedName))
        {
            SelectCatalogModelByName(previouslySelectedName);
        }
        else if (!string.IsNullOrWhiteSpace(modelPath))
        {
            SelectCatalogModelByName(Path.GetFileNameWithoutExtension(modelPath));
        }
    }

    private static LocalWhisperModelHealth ModelPathHealth(string? modelPath) =>
        LocalWhisperModelService.CheckPathHealth(
            modelPath,
            File.Exists,
            LocalModelFileLength,
            LocalModelHeader);

    private static long LocalModelFileLength(string path)
    {
        try
        {
            return new FileInfo(path).Length;
        }
        catch
        {
            return 0;
        }
    }

    private static byte[] LocalModelHeader(string path)
    {
        try
        {
            using var stream = File.OpenRead(path);
            var header = new byte[4];
            var bytesRead = stream.Read(header, 0, header.Length);
            return bytesRead == header.Length ? header : [];
        }
        catch
        {
            return [];
        }
    }

    private static bool CanUseModelPath(string? modelPath) =>
        ModelPathHealth(modelPath).CanUse;

    private int UnavailableImportedModelCount() =>
        localWhisperModels.Count(model => !ModelPathHealth(model.Path).CanUse);

    private void SelectCatalogModelByName(string? modelName)
    {
        if (string.IsNullOrWhiteSpace(modelName))
        {
            LocalModelCatalogListView.SelectedIndex = -1;
            return;
        }

        var selectedIndex = modelCatalogItems
            .ToList()
            .FindIndex(model => string.Equals(model.Name, modelName, StringComparison.OrdinalIgnoreCase));
        LocalModelCatalogListView.SelectedIndex = selectedIndex;
    }

    private void RefreshLanguageChoices(string? selectedPath = null, string? selectedLanguage = null)
    {
        var modelPath = selectedPath ?? ModelPathTextBox.Text;
        languageChoices = SelectedTranscriptionProvider() == TranscriptionProviderKind.LocalWhisper
            ? WhisperLanguageCatalog.ChoicesForModelPath(modelPath, localWhisperModels)
            : WhisperLanguageCatalog.MultilingualChoices;
        var compatibleLanguage = CompatibleLanguageForSelectedProvider(selectedLanguage, modelPath);

        suppressLanguageChanged = true;
        LanguageComboBox.ItemsSource = languageChoices;
        LanguageComboBox.SelectedIndex = languageChoices
            .ToList()
            .FindIndex(choice => string.Equals(choice.Code, compatibleLanguage, StringComparison.OrdinalIgnoreCase));
        suppressLanguageChanged = false;

        var isEnglishOnly = languageChoices.Count == 1 && languageChoices[0].Code == "en";
        LanguageDescriptionTextBlock.Text = isEnglishOnly
            ? "This is an English-optimized model and only supports English transcription."
            : SelectedTranscriptionProvider() == TranscriptionProviderKind.LocalWhisper
                ? "This model supports multiple languages. Select a specific language or auto-detect."
                : "Cloud transcription language is provider-dependent. Select a specific language or auto-detect.";
    }

    private string CompatibleLanguageForSelectedProvider(string? selectedLanguage, string? modelPath = null)
    {
        if (SelectedTranscriptionProvider() == TranscriptionProviderKind.LocalWhisper)
        {
            return WhisperLanguageCatalog.CompatibleLanguageOrFallback(
                modelPath ?? ModelPathTextBox.Text,
                localWhisperModels,
                selectedLanguage);
        }

        var normalizedLanguage = selectedLanguage?.Trim() ?? string.Empty;
        return languageChoices.Any(choice =>
            string.Equals(choice.Code, normalizedLanguage, StringComparison.OrdinalIgnoreCase))
            ? languageChoices.First(choice =>
                string.Equals(choice.Code, normalizedLanguage, StringComparison.OrdinalIgnoreCase)).Code
            : "auto";
    }

    private void RefreshEnhancementPromptChoices(Guid? selectedPromptId)
    {
        suppressEnhancementPromptChanged = true;
        EnhancementPromptComboBox.ItemsSource = enhancementPrompts
            .Select(prompt => prompt.Title)
            .ToArray();

        var promptId = EnhancementPromptLibrary.ResolveSelectedPromptId(selectedPromptId, enhancementPrompts);
        var selectedIndex = enhancementPrompts.ToList().FindIndex(prompt => prompt.Id == promptId);
        EnhancementPromptComboBox.SelectedIndex = selectedIndex >= 0 ? selectedIndex : 0;
        suppressEnhancementPromptChanged = false;
    }

    private EnhancementPrompt? SelectedEnhancementPrompt()
    {
        var selectedIndex = EnhancementPromptComboBox.SelectedIndex;
        return selectedIndex >= 0 && selectedIndex < enhancementPrompts.Count
            ? enhancementPrompts[selectedIndex]
            : null;
    }

    private Guid? SelectedPowerModeRuleId() =>
        selectedPowerModeRuleId is { } ruleId
        && powerModeRules.Any(rule => rule.IsEnabled && rule.Id == ruleId)
            ? ruleId
            : null;

    private EnhancementPrompt? PromptEditorPrompt() =>
        promptEditorPromptId is { } promptId
            ? enhancementPrompts.FirstOrDefault(prompt => prompt.Id == promptId)
            : null;

    private void RefreshPromptEditorFields()
    {
        var prompt = SelectedEnhancementPrompt();
        promptEditorPromptId = prompt?.Id;
        PromptTitleTextBox.Text = prompt?.Title ?? string.Empty;
        PromptIconTextBox.Text = prompt?.Icon ?? string.Empty;
        PromptDescriptionTextBox.Text = prompt?.Description ?? string.Empty;
        PromptInstructionsTextBox.Text = prompt?.PromptText ?? string.Empty;
        PromptTriggerWordsTextBox.Text = prompt is null
            ? string.Empty
            : EnhancementPromptLibrary.TriggerWordsText(prompt);
        PromptUseSystemInstructionsCheckBox.IsChecked = prompt?.UseSystemInstructions ?? true;
    }

    private void SetPromptEditorForNewPrompt()
    {
        promptEditorPromptId = null;
        suppressEnhancementPromptChanged = true;
        EnhancementPromptComboBox.SelectedIndex = -1;
        suppressEnhancementPromptChanged = false;
        PromptTitleTextBox.Text = string.Empty;
        PromptIconTextBox.Text = "doc.text.fill";
        PromptDescriptionTextBox.Text = string.Empty;
        PromptInstructionsTextBox.Text = string.Empty;
        PromptTriggerWordsTextBox.Text = string.Empty;
        PromptUseSystemInstructionsCheckBox.IsChecked = true;
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

            PowerModeActiveWindowTextBlock.Text = string.IsNullOrWhiteSpace(target.BrowserUrl)
                ? $"Process: {target.ProcessName}; Title: {target.WindowTitle}"
                : $"Process: {target.ProcessName}; Title: {target.WindowTitle}; URL: {target.BrowserUrl}";
            if (fillRuleFields)
            {
                PowerModeProcessTextBox.Text = target.ProcessName;
                PowerModeWindowTitleTextBox.Text = target.WindowTitle;
                PowerModeBrowserUrlTextBox.Text = target.BrowserUrl;
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
        var validation = PowerModeRuleValidator.Validate(rule);
        if (!validation.IsValid)
        {
            ShowPowerModeValidationResult(validation, "Fix Power Mode rule");
            RefreshUiFromControllerState("Power Mode rule needs a match");
            return;
        }

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
        var validation = PowerModeRuleValidator.Validate(updatedRule);
        if (!validation.IsValid)
        {
            ShowPowerModeValidationResult(validation, "Fix Power Mode rule");
            RefreshUiFromControllerState("Power Mode rule needs a match");
            return;
        }

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
        if (selectedPowerModeRuleId == selectedRule.Id)
        {
            selectedPowerModeRuleId = null;
        }

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
        var presentation = PowerModePagePresenter.Present(powerModeRules);
        ApplyPowerModePagePresentation(presentation);
        PowerModeRulesListView.ItemsSource = presentation.RuleRows;

        PowerModeRulesListView.SelectedIndex = selectedId is null
            ? -1
            : powerModeRules.ToList().FindIndex(rule => rule.Id == selectedId.Value);
        FillPowerModeFormFromSelection();
        RefreshPowerModeValidationStatus();
    }

    private void ApplyPowerModePagePresentation(PowerModePagePresentation presentation)
    {
        PowerModeTitleTextBlock.Text = presentation.Title;
        PowerModeDescriptionTextBlock.Text = presentation.Description;
        PowerModeManualSwitchingTextBlock.Text = presentation.ManualSwitchingSummary;
        PowerModeMatchGuidanceTextBlock.Text = presentation.MatchGuidance;
        PowerModeOverrideGuidanceTextBlock.Text = presentation.OverrideGuidance;
        PowerModeCountTextBlock.Text = presentation.CountLabel;
        PowerModeEmptyTextBlock.Text = presentation.IsEmpty
            ? $"{presentation.EmptyTitle}. {presentation.EmptyDescription}"
            : string.Empty;
        PowerModeEmptyTextBlock.Visibility = presentation.IsEmpty ? Visibility.Visible : Visibility.Collapsed;
    }

    private void FillPowerModeFormFromSelection()
    {
        var rule = SelectedPowerModeRule();
        PowerModeNameTextBox.Text = rule?.Name ?? string.Empty;
        PowerModeEmojiTextBox.Text = rule?.Emoji ?? string.Empty;
        PowerModeProcessTextBox.Text = rule?.ProcessNamePattern ?? string.Empty;
        PowerModeWindowTitleTextBox.Text = rule?.WindowTitlePattern ?? string.Empty;
        PowerModeBrowserUrlTextBox.Text = rule?.BrowserUrlPattern ?? string.Empty;
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
        PowerModeTextFormattingCheckBox.IsChecked = rule?.IsTextFormattingEnabledOverride;
        PowerModeLowercaseCheckBox.IsChecked = rule?.LowercaseTranscriptionOverride;
        PowerModePunctuationCleanupComboBox.SelectedIndex = rule?.PunctuationCleanupModeOverride switch
        {
            PunctuationCleanupMode.Keep => 1,
            PunctuationCleanupMode.RemoveAll => 2,
            PunctuationCleanupMode.RemoveTrailingPeriod => 3,
            _ => 0
        };
        PowerModeAutoSendComboBox.SelectedIndex = PowerModeAutoSendKeyPresenter.SelectedIndexFor(
            rule?.AutoSendKey ?? PowerModeAutoSendKey.None);
        PowerModeShortcutTextBox.Text = rule?.Shortcut ?? string.Empty;
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
            BrowserUrlPattern = PowerModeBrowserUrlTextBox.Text.Trim(),
            ModelPathOverride = TrimToNull(PowerModeModelPathTextBox.Text),
            LanguageOverride = TrimToNull(PowerModeLanguageTextBox.Text),
            IsEnhancementEnabledOverride = SelectedEnhancementOverride(),
            SelectedEnhancementPromptIdOverride = SelectedPowerModePromptOverrideId(),
            AppendTrailingSpaceOverride = PowerModeAppendTrailingSpaceCheckBox.IsChecked,
            RemoveFillerWordsOverride = PowerModeRemoveFillerWordsCheckBox.IsChecked,
            IsTextFormattingEnabledOverride = PowerModeTextFormattingCheckBox.IsChecked,
            LowercaseTranscriptionOverride = PowerModeLowercaseCheckBox.IsChecked,
            PunctuationCleanupModeOverride = SelectedPowerModePunctuationCleanupMode(),
            AutoSendKey = PowerModeAutoSendKeyPresenter.KeyForSelectedIndex(PowerModeAutoSendComboBox.SelectedIndex),
            Shortcut = PowerModeShortcutTextBox.Text.Trim()
        };
    }

    private static IReadOnlyList<PowerModeRule> NormalizeDefaultRule(
        IReadOnlyList<PowerModeRule> rules,
        PowerModeRule changedRule) =>
        changedRule.IsDefault
            ? rules.Select(rule => rule.Id == changedRule.Id ? rule : rule with { IsDefault = false }).ToArray()
            : rules;

    private void RefreshPowerModeValidationStatus()
    {
        if (!settingsLoaded)
        {
            PowerModeValidationInfoBar.IsOpen = false;
            return;
        }

        var validation = PowerModeRuleValidator.Validate(powerModeRules);
        if (validation.Errors.Count == 0 && validation.Warnings.Count == 0)
        {
            PowerModeValidationInfoBar.IsOpen = false;
            return;
        }

        ShowPowerModeValidationResult(
            validation,
            validation.Errors.Count > 0 ? "Power Mode rules need attention" : "Power Mode rule warning");
    }

    private void ShowPowerModeValidationResult(
        PowerModeRuleValidationResult validation,
        string title)
    {
        PowerModeValidationInfoBar.Title = title;
        PowerModeValidationInfoBar.Message = string.Join(
            Environment.NewLine,
            validation.Errors.Concat(validation.Warnings));
        PowerModeValidationInfoBar.Severity = validation.Errors.Count > 0
            ? InfoBarSeverity.Error
            : InfoBarSeverity.Warning;
        PowerModeValidationInfoBar.IsOpen = true;
    }

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
        var preset = EnhancementProviderPresetCatalog.Resolve(SelectedEnhancementProviderId());
        if (!preset.RequiresApiKey)
        {
            EnhancementKeyStatusTextBlock.Text = $"{preset.DisplayName} does not require an API key";
            return;
        }

        var hasKey = await HasEnhancementApiKeyAsync(preset.Id, cancellationToken);
        EnhancementKeyStatusTextBlock.Text = hasKey
            ? $"{preset.DisplayName} API key stored in Windows Credential Manager"
            : $"No {preset.DisplayName} API key stored";
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

    private async Task<bool> HasEnhancementApiKeyAsync(
        string providerId,
        CancellationToken cancellationToken)
    {
        foreach (var secretName in EnhancementConfiguration.SecretNamesForProvider(providerId))
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

    private void SelectEnhancementPreset(string? providerId)
    {
        var preset = EnhancementProviderPresetCatalog.Resolve(providerId);
        var index = EnhancementProviderPresetCatalog.All.ToList().FindIndex(item => item.Id == preset.Id);
        EnhancementProviderPresetComboBox.SelectedIndex = Math.Max(0, index);
    }

    private void ApplySelectedEnhancementPreset(bool fillConfiguration)
    {
        var preset = EnhancementProviderPresetCatalog.Resolve(SelectedEnhancementProviderId());
        if (preset.Id != EnhancementProviderPresetCatalog.Ollama.Id)
        {
            ollamaEnhancementModelChoices = [];
        }

        if (preset.Id != EnhancementProviderPresetCatalog.OpenRouter.Id)
        {
            openRouterEnhancementModelChoices = [];
        }

        if (fillConfiguration && preset.Id != EnhancementProviderPresetCatalog.Custom.Id)
        {
            EnhancementEndpointTextBox.Text = preset.Endpoint;
            if (string.IsNullOrWhiteSpace(EnhancementModelTextBox.Text)
                || preset.ModelIds.Count == 0
                || (preset.ModelIds.Count > 0
                    && !preset.ModelIds.Any(model => string.Equals(
                        model,
                        EnhancementModelTextBox.Text.Trim(),
                        StringComparison.OrdinalIgnoreCase))))
            {
                EnhancementModelTextBox.Text = preset.DefaultModel;
            }
        }

        RefreshEnhancementModelChoices(EnhancementModelTextBox.Text);
    }

    private void RefreshEnhancementModelChoices(string selectedModel)
    {
        var preset = EnhancementProviderPresetCatalog.Resolve(SelectedEnhancementProviderId());
        var modelChoices = EnhancementModelChoicesForPreset(preset);
        suppressEnhancementModelChanged = true;
        EnhancementModelComboBox.ItemsSource = modelChoices.ToArray();
        if (modelChoices.Count == 0)
        {
            EnhancementModelComboBox.SelectedIndex = -1;
        }
        else
        {
            var index = modelChoices.ToList().FindIndex(model => string.Equals(
                model,
                selectedModel.Trim(),
                StringComparison.OrdinalIgnoreCase));
            EnhancementModelComboBox.SelectedIndex = index >= 0 ? index : 0;
        }

        suppressEnhancementModelChanged = false;
    }

    private IReadOnlyList<string> EnhancementModelChoicesForPreset(EnhancementProviderPreset preset) =>
        preset.Id == EnhancementProviderPresetCatalog.OpenRouter.Id
            && openRouterEnhancementModelChoices.Count > 0
                ? openRouterEnhancementModelChoices
            : preset.Id == EnhancementProviderPresetCatalog.Ollama.Id
            && ollamaEnhancementModelChoices.Count > 0
                ? ollamaEnhancementModelChoices
                : preset.ModelIds;

    private static bool SupportsDynamicEnhancementModels(EnhancementProviderPreset preset) =>
        preset.Id == EnhancementProviderPresetCatalog.OpenRouter.Id
        || preset.Id == EnhancementProviderPresetCatalog.Ollama.Id;

    private bool IsOperationActive(
        bool includeCurrentModelImport = true,
        bool includeCurrentEnhancementKeySave = true,
        bool includeCurrentCloudTranscriptionKeySave = true,
        bool includeCurrentCloudTranscriptionProviderTest = true,
        bool includeCurrentPrivacyCleanup = true,
        bool includeCurrentModelDownload = true) =>
        isStarting
        || isStopping
        || isCanceling
        || isPastingLast
        || isRetryingHistory
        || isReenhancingHistory
        || isCopyingHistory
        || isCopyingAudioFileQueueText
        || isEnhancingAudioFileQueueItem
        || isQuickAdding
        || isOnboardingOpen
        || isTranscribingAudioFiles
        || isSavingAudioFileQueueText
        || isExportingSettingsBackup
        || isImportingSettingsBackup
        || isExportingDiagnosticLogs
        || isRefreshingEnhancementModels
        || (includeCurrentPrivacyCleanup && isRunningPrivacyCleanup)
        || (includeCurrentEnhancementKeySave && isSavingEnhancementKey)
        || (includeCurrentCloudTranscriptionKeySave && isSavingCloudTranscriptionKey)
        || (includeCurrentCloudTranscriptionProviderTest && isTestingCloudTranscriptionProvider)
        || (includeCurrentModelDownload && isDownloadingModel)
        || (includeCurrentModelImport && isImportingModel);

    private bool IsControllerBusy() =>
        controller.State is DictationState.Transcribing or DictationState.Inserting;

    private bool CanRefreshAudioInputsFromDeviceChange() =>
        settingsLoaded
        && !IsOperationActive()
        && !IsControllerBusy()
        && controller.State != DictationState.Recording
        && !windowLifetime.IsCancellationRequested;

    private bool CanEditModelLibrary(bool includeCurrentModelImport = true) =>
        settingsLoaded
        && !IsOperationActive(includeCurrentModelImport)
        && !IsControllerBusy()
        && controller.State != DictationState.Recording;

    private bool CanUseSettingsBackup() =>
        settingsLoaded
        && !IsOperationActive()
        && !IsControllerBusy()
        && controller.State != DictationState.Recording;

    private bool CanImportSettingsBackup() => CanUseSettingsBackup();

    private bool CanUsePrivacyCleanup() =>
        settingsLoaded
        && !IsOperationActive()
        && !IsControllerBusy()
        && controller.State != DictationState.Recording;

    private static bool ImportsSettingsCategories(
        IReadOnlyCollection<VoiceInkSettingsBackupCategory> categories) =>
        categories.Any(category => category != VoiceInkSettingsBackupCategory.Dictionary);

    private static string BuildSettingsImportStatus(
        IReadOnlyCollection<VoiceInkSettingsBackupCategory> categories,
        DictionaryImportResult? dictionaryResult,
        string? warning)
    {
        var imported = string.Join(", ", categories.Select(SettingsBackupCategoryTitle));
        var status = $"Settings imported: {imported}";
        if (dictionaryResult is not null)
        {
            status += $" ({dictionaryResult.ImportedVocabularyCount} vocabulary, "
                + $"{dictionaryResult.ImportedReplacementCount} replacements, "
                + $"{dictionaryResult.SkippedDuplicateCount} skipped)";
        }

        if (SettingsImportNeedsApiKeyReminder(categories))
        {
            status += ". Reconfigure API keys locally.";
        }

        if (!string.IsNullOrWhiteSpace(warning))
        {
            status += $" {warning}";
        }

        return status;
    }

    private static bool SettingsImportNeedsApiKeyReminder(
        IReadOnlyCollection<VoiceInkSettingsBackupCategory> categories) =>
        categories.Contains(VoiceInkSettingsBackupCategory.General)
        || categories.Contains(VoiceInkSettingsBackupCategory.CustomPrompts)
        || categories.Contains(VoiceInkSettingsBackupCategory.CustomModelDefinitions);

    private static string SettingsBackupCategoryTitle(VoiceInkSettingsBackupCategory category) =>
        VoiceInkSettingsBackupCategories.All
            .First(choice => choice.Category == category)
            .Title;

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
        PermissionsSectionPanel.Visibility = tag == PermissionsSectionTag ? Visibility.Visible : Visibility.Collapsed;
        AudioInputSectionPanel.Visibility = tag == AudioInputSectionTag ? Visibility.Visible : Visibility.Collapsed;
        DictionarySectionPanel.Visibility = tag == DictionarySectionTag ? Visibility.Visible : Visibility.Collapsed;
        HistorySectionPanel.Visibility = tag == HistorySectionTag ? Visibility.Visible : Visibility.Collapsed;
        MetricsSectionPanel.Visibility = tag == MetricsSectionTag ? Visibility.Visible : Visibility.Collapsed;
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

    private async Task RefreshPermissionsSectionAsync(string? statusOverride = null)
    {
        try
        {
            var settings = await settingsStore.LoadAsync(windowLifetime.Token);
            var status = PermissionsReadinessPresenter.Build(settings, HasPhysicalAudioInputChoices());

            PermissionsStatusInfoBar.Title = status.SummaryTitle;
            PermissionsStatusInfoBar.Message = status.SummaryMessage;
            PermissionsStatusInfoBar.Severity = status.SummarySeverity == "Ready"
                ? InfoBarSeverity.Success
                : InfoBarSeverity.Warning;
            PermissionsChecklistListView.ItemsSource = status.Items;

            RefreshUiFromControllerState(statusOverride ?? status.SummaryMessage);
        }
        catch (Exception ex)
        {
            PermissionsStatusInfoBar.Title = "Permission check failed";
            PermissionsStatusInfoBar.Message = ex.Message;
            PermissionsStatusInfoBar.Severity = InfoBarSeverity.Error;
            RefreshUiFromControllerState($"Permission check failed: {ex.Message}");
        }
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
            var summary = BuildDiagnosticReport();
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

    private async Task ExportDiagnosticLogsAsync()
    {
        if (IsOperationActive())
        {
            return;
        }

        var statusOverride = "Preparing diagnostic logs";
        isExportingDiagnosticLogs = true;
        RefreshUiFromControllerState(statusOverride);

        try
        {
            var picker = new FileSavePicker
            {
                SuggestedStartLocation = PickerLocationId.DocumentsLibrary,
                SuggestedFileName = $"VoiceInk_Diagnostic_Logs_{DateTimeOffset.Now:yyyyMMdd-HHmmss}"
            };
            picker.FileTypeChoices.Add("Log file", [".log"]);
            InitializeWithWindow.Initialize(picker, WindowNative.GetWindowHandle(this));

            var file = await picker.PickSaveFileAsync();
            if (file is null)
            {
                statusOverride = "Diagnostic log export canceled";
                return;
            }

            await FileIO.WriteTextAsync(file, BuildDiagnosticReport());
            statusOverride = $"Diagnostic logs exported: {file.Name}";
        }
        catch (Exception ex)
        {
            statusOverride = $"Diagnostic log export failed: {ex.Message}";
        }
        finally
        {
            isExportingDiagnosticLogs = false;
            RefreshUiFromControllerState(statusOverride);
        }
    }

    private string BuildDiagnosticReport()
    {
        var request = new DiagnosticReportRequest
        {
            ExportedAtUtc = DateTimeOffset.UtcNow,
            AppVersion = typeof(MainWindow).Assembly.GetName().Version?.ToString() ?? "source build",
            OsDescription = RuntimeInformation.OSDescription,
            RuntimeDescription = RuntimeInformation.FrameworkDescription,
            ProcessArchitecture = RuntimeInformation.ProcessArchitecture.ToString(),
            AppBaseDirectory = AppContext.BaseDirectory,
            AppDataDirectory = appDataDirectory,
            RecordingsDirectory = recordingsDirectory,
            ActiveSection = activeSectionTag,
            DictationState = controller.State.ToString(),
            SelectedModelPath = ModelPathTextBox.Text,
            Files = BuildDiagnosticFileEntries(),
            RecentEvents = diagnosticEvents.ToArray()
        };

        return DiagnosticReportBuilder.Build(request);
    }

    private IReadOnlyList<DiagnosticFileEntry> BuildDiagnosticFileEntries() =>
    [
        DiagnosticFile("Settings", settingsPath),
        DiagnosticFile("History", historyPath),
        DiagnosticFile("Metrics", metricsPath),
        DiagnosticFile("Dictionary", dictionaryPath)
    ];

    private static DiagnosticFileEntry DiagnosticFile(string label, string path)
    {
        var info = new FileInfo(path);
        return new DiagnosticFileEntry(
            label,
            path,
            info.Exists,
            info.Exists ? info.Length : null);
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

            ReplaceGlobalHotkeyService(shortcutRegistration.Registrations, settings);
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
                ReplaceGlobalHotkeyService(shortcutRegistration.Registrations, settings);
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

    private void ReplaceGlobalHotkeyService(
        IReadOnlyList<GlobalShortcutRegistration> registrations,
        AppSettings settings)
    {
        DisposeGlobalHotkeyService();
        var windowHandle = WindowNative.GetWindowHandle(this);
        powerModeTargetProvider.ExcludeWindowHandle(windowHandle);
        var newHotkeyService = new GlobalHotkeyService(windowHandle);
        try
        {
            newHotkeyService.HotkeyPressed += HotkeyService_HotkeyPressed;
            newHotkeyService.MiniRecorderShortcutPressed += HotkeyService_MiniRecorderShortcutPressed;
            newHotkeyService.RegisterHotkeys(
                registrations,
                settings.IsMiddleClickRecordingEnabled,
                settings.MiddleClickActivationDelayMilliseconds);
            hotkeyService = newHotkeyService;
        }
        catch
        {
            newHotkeyService.HotkeyPressed -= HotkeyService_HotkeyPressed;
            newHotkeyService.MiniRecorderShortcutPressed -= HotkeyService_MiniRecorderShortcutPressed;
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
        hotkeyService.MiniRecorderShortcutPressed -= HotkeyService_MiniRecorderShortcutPressed;
        hotkeyService.Dispose();
        hotkeyService = null;
    }

    private void RecreateController()
    {
        var selectedAudioInputDeviceChoice = SelectedAudioInputDeviceChoice();
        audioCapture.LevelAvailable -= AudioCapture_LevelAvailable;
        audioCapture.Dispose();
        audioCapture = new NAudioCaptureService(recordingsDirectory, selectedAudioInputDeviceChoice?.DeviceNumber);
        audioCapture.LevelAvailable += AudioCapture_LevelAvailable;
        Interlocked.Exchange(ref latestRecordingInputLevel, 0);
        activeAudioInputDeviceChoice = selectedAudioInputDeviceChoice;
        controller = CreateController(audioCapture);
    }

    private void RecreateControllerIfAudioInputChanged()
    {
        if (!AudioInputDeviceChoicesMatch(activeAudioInputDeviceChoice, SelectedAudioInputDeviceChoice()))
        {
            RecreateController();
        }
    }

    private static bool AudioInputDeviceChoicesMatch(
        AudioInputDeviceChoice? first,
        AudioInputDeviceChoice? second) =>
        first?.DeviceNumber == second?.DeviceNumber
        && string.Equals(first?.Name, second?.Name, StringComparison.Ordinal)
        && string.Equals(first?.EndpointId, second?.EndpointId, StringComparison.Ordinal);

    private void RefreshUiFromControllerState(string? statusOverride = null)
    {
        var operationActive = IsOperationActive();
        var controllerBusy = IsControllerBusy();
        var modelControlsEnabled = CanEditModelLibrary();
        var modelWarmupActive = modelWarmupCoordinator.State.Status == WhisperModelWarmupStatus.Warming;
        var selectedCatalogModel = SelectedCatalogModelItem();
        var cloudTranscriptionControlsEnabled = modelControlsEnabled
            && SelectedTranscriptionProvider() == TranscriptionProviderKind.OpenAICompatible;
        var cloudPresetHasModelChoices = SelectedCloudTranscriptionPreset()?.ModelIds.Count > 0;
        var enhancementPreset = EnhancementProviderPresetCatalog.Resolve(SelectedEnhancementProviderId());
        var enhancementPresetHasModelChoices = EnhancementModelChoicesForPreset(enhancementPreset).Count > 0;
        var promptEditorPrompt = PromptEditorPrompt();
        var promptEditorIsPredefined = promptEditorPrompt?.IsPredefined == true;
        var enhancementControlsEnabled = settingsLoaded
            && !operationActive
            && !controllerBusy
            && controller.State != DictationState.Recording;
        var powerModeControlsEnabled = CanEditPowerModeRules();
        var historyAudioAvailable = SelectedHistoryAudioPath() is not null;
        var selectedHistory = SelectedHistoryItem();
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
            && (!operationActive || isStopping)
            && controller.State is DictationState.Recording or DictationState.Transcribing or DictationState.Inserting;
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
        RefreshMetricsButton.IsEnabled = settingsLoaded && !operationActive;
        MetricsTimeFilterComboBox.IsEnabled = settingsLoaded && !operationActive;
        ExportMetricsButton.IsEnabled = settingsLoaded && !operationActive;
        ResetMetricsButton.IsEnabled = settingsLoaded && !operationActive;
        ExportHistoryButton.IsEnabled = settingsLoaded && !operationActive;
        ApplyShortcutsButton.IsEnabled = settingsLoaded && !operationActive;
        ExportSettingsBackupButton.IsEnabled = settingsLoaded && !operationActive;
        ImportSettingsBackupButton.IsEnabled = settingsLoaded
            && !operationActive
            && !controllerBusy
            && controller.State != DictationState.Recording;
        ExportDiagnosticLogsButton.IsEnabled = settingsLoaded && !operationActive;
        ApplyRecordingFeedbackSettingsButton.IsEnabled = settingsLoaded && !operationActive;
        SoundFeedbackCheckBox.IsEnabled = settingsLoaded && !operationActive;
        MuteSystemAudioCheckBox.IsEnabled = settingsLoaded && !operationActive;
        PauseMediaCheckBox.IsEnabled = settingsLoaded && !operationActive;
        UpdateRecordingFeedbackSettingControlState();
        ApplyClipboardSettingsButton.IsEnabled = settingsLoaded && !operationActive;
        RestoreClipboardCheckBox.IsEnabled = settingsLoaded && !operationActive;
        PasteMethodComboBox.IsEnabled = settingsLoaded && !operationActive;
        UpdateClipboardSettingControlState();
        ApplyCleanupSettingsButton.IsEnabled = settingsLoaded && !operationActive;
        ResetOnboardingButton.IsEnabled = settingsLoaded && !operationActive;
        LaunchAtLoginCheckBox.IsEnabled = settingsLoaded && !operationActive;
        RepairLaunchAtLoginButton.IsEnabled = settingsLoaded && !operationActive;
        TranscriptionCleanupCheckBox.IsEnabled = settingsLoaded && !operationActive;
        AudioCleanupCheckBox.IsEnabled = settingsLoaded && !operationActive;
        UpdateCleanupSettingControlState();
        RefreshAudioInputsButton.IsEnabled = settingsLoaded
            && !operationActive
            && !controllerBusy
            && controller.State != DictationState.Recording;
        ApplyAudioInputButton.IsEnabled = settingsLoaded
            && !operationActive
            && !controllerBusy
            && controller.State != DictationState.Recording;
        var audioInputControlsEnabled = ApplyAudioInputButton.IsEnabled;
        AudioInputModeComboBox.IsEnabled = audioInputControlsEnabled;
        AudioInputComboBox.IsEnabled = audioInputControlsEnabled;
        AudioInputPriorityListView.IsEnabled = audioInputControlsEnabled;
        AddAudioInputPriorityButton.IsEnabled = audioInputControlsEnabled
            && SelectedAudioInputDeviceChoice()?.DeviceNumber is not null;
        var selectedPriorityItem = SelectedAudioInputPriorityItem();
        RemoveAudioInputPriorityButton.IsEnabled = audioInputControlsEnabled && selectedPriorityItem is not null;
        MoveAudioInputPriorityUpButton.IsEnabled = audioInputControlsEnabled
            && selectedPriorityItem is not null
            && AudioInputPriorityListView.SelectedIndex > 0;
        MoveAudioInputPriorityDownButton.IsEnabled = audioInputControlsEnabled
            && selectedPriorityItem is not null
            && AudioInputPriorityListView.SelectedIndex >= 0
            && AudioInputPriorityListView.SelectedIndex < AudioInputPriorityListView.Items.Count - 1;
        ModelPathTextBox.IsEnabled = modelControlsEnabled;
        LanguageComboBox.IsEnabled = modelControlsEnabled && languageChoices.Count > 1;
        LocalModelCatalogListView.IsEnabled = modelControlsEnabled;
        DownloadCatalogModelButton.IsEnabled = modelControlsEnabled
            && selectedCatalogModel is not null
            && !selectedCatalogModel.IsDownloaded;
        UseCatalogModelButton.IsEnabled = modelControlsEnabled
            && selectedCatalogModel?.IsDownloaded == true
            && !selectedCatalogModel.IsDefault
            && CanUseModelPath(selectedCatalogModel.LocalPath);
        ShowCatalogModelButton.IsEnabled = modelControlsEnabled
            && selectedCatalogModel?.IsDownloaded == true
            && CanUseModelPath(selectedCatalogModel.LocalPath);
        DeleteCatalogModelButton.IsEnabled = modelControlsEnabled
            && selectedCatalogModel?.IsDownloaded == true;
        CancelModelDownloadButton.IsEnabled = settingsLoaded && isDownloadingModel;
        PrewarmModelOnWakeCheckBox.IsEnabled = modelControlsEnabled;
        ShowLiveTranscriptPreviewCheckBox.IsEnabled = modelControlsEnabled;
        VoiceActivityDetectionCheckBox.IsEnabled = modelControlsEnabled;
        RecorderStyleComboBox.IsEnabled = settingsLoaded
            && !operationActive
            && !controllerBusy
            && controller.State != DictationState.Recording;
        WarmupSelectedModelButton.IsEnabled = modelControlsEnabled
            && !modelWarmupActive
            && SelectedTranscriptionProvider() == TranscriptionProviderKind.LocalWhisper
            && CanUseModelPath(ModelPathTextBox.Text);
        RepairModelPathButton.IsEnabled = modelControlsEnabled
            && (currentModelRepairAction != LocalWhisperModelRepairAction.Warmup
                || (SelectedTranscriptionProvider() == TranscriptionProviderKind.LocalWhisper
                    && !modelWarmupActive
                    && CanUseModelPath(ModelPathTextBox.Text)));
        ModelComboBox.IsEnabled = modelControlsEnabled;
        ImportModelButton.IsEnabled = modelControlsEnabled;
        UseSelectedModelButton.IsEnabled = modelControlsEnabled
            && CanUseModelPath(SelectedLocalWhisperModelChoice()?.Path);
        RemoveSelectedModelButton.IsEnabled = modelControlsEnabled
            && SelectedLocalWhisperModelChoice() is not null;
        RemoveUnavailableModelsButton.IsEnabled = modelControlsEnabled
            && UnavailableImportedModelCount() > 0;
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
        TestCloudTranscriptionProviderButton.IsEnabled = cloudTranscriptionControlsEnabled;
        EnhancementEnabledCheckBox.IsEnabled = enhancementControlsEnabled;
        UseClipboardContextCheckBox.IsEnabled = enhancementControlsEnabled;
        UseOcrContextCheckBox.IsEnabled = enhancementControlsEnabled;
        UseOcrCaptureRegionCheckBox.IsEnabled = enhancementControlsEnabled
            && UseOcrContextCheckBox.IsChecked == true;
        var ocrRegionControlsEnabled = enhancementControlsEnabled
            && UseOcrContextCheckBox.IsChecked == true
            && UseOcrCaptureRegionCheckBox.IsChecked == true;
        OcrRegionLeftNumberBox.IsEnabled = ocrRegionControlsEnabled;
        OcrRegionTopNumberBox.IsEnabled = ocrRegionControlsEnabled;
        OcrRegionWidthNumberBox.IsEnabled = ocrRegionControlsEnabled;
        OcrRegionHeightNumberBox.IsEnabled = ocrRegionControlsEnabled;
        OcrDisplayComboBox.IsEnabled = enhancementControlsEnabled && ocrDisplays.Count > 0;
        PickOcrRegionButton.IsEnabled = enhancementControlsEnabled;
        EnhancementProviderPresetComboBox.IsEnabled = enhancementControlsEnabled;
        EnhancementEndpointTextBox.IsEnabled = enhancementControlsEnabled;
        EnhancementModelTextBox.IsEnabled = enhancementControlsEnabled;
        EnhancementModelComboBox.IsEnabled = enhancementControlsEnabled && enhancementPresetHasModelChoices;
        RefreshEnhancementModelsButton.IsEnabled = enhancementControlsEnabled
            && SupportsDynamicEnhancementModels(enhancementPreset);
        EnhancementApiKeyPasswordBox.IsEnabled = enhancementControlsEnabled && enhancementPreset.RequiresApiKey;
        SaveEnhancementKeyButton.IsEnabled = enhancementControlsEnabled && enhancementPreset.RequiresApiKey;
        ClearEnhancementKeyButton.IsEnabled = enhancementControlsEnabled && enhancementPreset.RequiresApiKey;
        EnhancementPromptComboBox.IsEnabled = enhancementControlsEnabled;
        PromptTitleTextBox.IsEnabled = enhancementControlsEnabled && !promptEditorIsPredefined;
        PromptIconTextBox.IsEnabled = enhancementControlsEnabled && !promptEditorIsPredefined;
        PromptDescriptionTextBox.IsEnabled = enhancementControlsEnabled && !promptEditorIsPredefined;
        PromptInstructionsTextBox.IsEnabled = enhancementControlsEnabled && !promptEditorIsPredefined;
        PromptTriggerWordsTextBox.IsEnabled = enhancementControlsEnabled;
        PromptUseSystemInstructionsCheckBox.IsEnabled = enhancementControlsEnabled && !promptEditorIsPredefined;
        NewPromptButton.IsEnabled = enhancementControlsEnabled;
        SavePromptButton.IsEnabled = enhancementControlsEnabled;
        DeletePromptButton.IsEnabled = enhancementControlsEnabled
            && promptEditorPrompt is not null
            && !promptEditorPrompt.IsPredefined;
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
        PowerModeBrowserUrlTextBox.IsEnabled = powerModeControlsEnabled;
        PowerModeEnabledCheckBox.IsEnabled = powerModeControlsEnabled;
        PowerModeDefaultCheckBox.IsEnabled = powerModeControlsEnabled;
        PowerModeModelPathTextBox.IsEnabled = powerModeControlsEnabled;
        PowerModeLanguageTextBox.IsEnabled = powerModeControlsEnabled;
        PowerModeEnhancementOverrideComboBox.IsEnabled = powerModeControlsEnabled;
        PowerModePromptOverrideComboBox.IsEnabled = powerModeControlsEnabled;
        PowerModeAppendTrailingSpaceCheckBox.IsEnabled = powerModeControlsEnabled;
        PowerModeRemoveFillerWordsCheckBox.IsEnabled = powerModeControlsEnabled;
        PowerModeTextFormattingCheckBox.IsEnabled = powerModeControlsEnabled;
        PowerModeLowercaseCheckBox.IsEnabled = powerModeControlsEnabled;
        PowerModePunctuationCleanupComboBox.IsEnabled = powerModeControlsEnabled;
        PowerModeAutoSendComboBox.IsEnabled = powerModeControlsEnabled;
        PowerModeShortcutTextBox.IsEnabled = powerModeControlsEnabled;
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
        var canUseSelectedAudioFileQueueText = !operationActive
            && AudioFileQueueTextActions.TryGetActionText(selectedAudioFileQueueItem, out _, out _);
        CopyAudioFileQueueTextButton.IsEnabled = canUseSelectedAudioFileQueueText;
        SaveAudioFileQueueTextButton.IsEnabled = canUseSelectedAudioFileQueueText;
        EnhanceAudioFileQueueTextButton.IsEnabled = canUseSelectedAudioFileQueueText;
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
        ReEnhanceHistoryButton.IsEnabled = settingsLoaded
            && !operationActive
            && !controllerBusy
            && controller.State != DictationState.Recording
            && selectedHistory?.Status == TranscriptionHistoryStatus.Completed;
        CopyHistoryOriginalButton.IsEnabled = settingsLoaded
            && !operationActive
            && HistoryCopyTextSelector.HasText(selectedHistory, HistoryCopyTextKind.Original);
        CopyHistoryFinalButton.IsEnabled = settingsLoaded
            && !operationActive
            && HistoryCopyTextSelector.HasText(selectedHistory, HistoryCopyTextKind.Final);
        CopyHistoryEnhancedButton.IsEnabled = settingsLoaded
            && !operationActive
            && HistoryCopyTextSelector.HasText(selectedHistory, HistoryCopyTextKind.Enhanced);
        CopyHistoryAiRequestButton.IsEnabled = settingsLoaded
            && !operationActive
            && HistoryCopyTextSelector.HasText(selectedHistory, HistoryCopyTextKind.AiRequest);
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
        ShowAppNotification(statusOverride ?? controller.LastError ?? controller.LastWarning ?? idleHotkeyWarning);
        RecordDiagnosticEvent(displayStatus);
        StatusTextBlock.Text = displayStatus;
        UpdateFloatingRecorder(displayStatus, operationActive);
        UpdateTrayFromControllerState(displayStatus, operationActive);
        QueuePendingAudioInputDeviceChangeRefreshIfIdle();
    }

    private void ShowAppNotification(string? status)
    {
        var presentation = AppNotificationPresenter.FromStatus(status);
        if (presentation is null)
        {
            return;
        }

        if (AppNotificationInfoBar.IsOpen
            && string.Equals(lastAppNotificationMessage, presentation.Message, StringComparison.Ordinal))
        {
            return;
        }

        lastAppNotificationMessage = presentation.Message;
        AppNotificationInfoBar.Message = presentation.Message;
        AppNotificationInfoBar.Severity = InfoBarSeverityFor(presentation.Kind);
        AppNotificationInfoBar.IsOpen = true;
        appNotificationTimer.Stop();
        appNotificationTimer.Interval = presentation.Duration;
        appNotificationTimer.Start();
    }

    private void HideAppNotification()
    {
        appNotificationTimer.Stop();
        AppNotificationInfoBar.IsOpen = false;
        lastAppNotificationMessage = null;
    }

    private static InfoBarSeverity InfoBarSeverityFor(AppNotificationKind kind) =>
        kind switch
        {
            AppNotificationKind.Success => InfoBarSeverity.Success,
            AppNotificationKind.Warning => InfoBarSeverity.Warning,
            AppNotificationKind.Error => InfoBarSeverity.Error,
            _ => InfoBarSeverity.Informational
        };

    private void QueuePendingAudioInputDeviceChangeRefreshIfIdle()
    {
        if (!pendingAudioInputDeviceChangeRefresh
            || isRefreshingAudioInputsFromDeviceChange
            || !CanRefreshAudioInputsFromDeviceChange())
        {
            return;
        }

        QueueAudioInputDeviceChangeRefresh();
    }

    private void QueueAudioInputDeviceChangeRefresh()
    {
        if (audioInputDeviceChangeRefreshQueued)
        {
            pendingAudioInputDeviceChangeRefresh = true;
            return;
        }

        audioInputDeviceChangeRefreshQueued = true;
        var queued = DispatcherQueue.TryEnqueue(async () =>
        {
            try
            {
                await RefreshAudioInputDevicesAfterChangeAsync();
            }
            finally
            {
                audioInputDeviceChangeRefreshQueued = false;
                QueuePendingAudioInputDeviceChangeRefreshIfIdle();
            }
        });
        if (!queued)
        {
            audioInputDeviceChangeRefreshQueued = false;
        }
    }

    private void RecordDiagnosticEvent(string status)
    {
        var diagnosticStatus = DiagnosticEventSanitizer.Normalize(status);
        if (string.IsNullOrWhiteSpace(diagnosticStatus)
            || string.Equals(diagnosticStatus, lastDiagnosticStatus, StringComparison.Ordinal))
        {
            return;
        }

        lastDiagnosticStatus = diagnosticStatus;
        diagnosticEvents.Add($"{DateTimeOffset.UtcNow:O} {diagnosticStatus}");
        if (diagnosticEvents.Count > MaxDiagnosticEvents)
        {
            diagnosticEvents.RemoveRange(0, diagnosticEvents.Count - MaxDiagnosticEvents);
        }
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
            Interlocked.Exchange(ref latestRecordingInputLevel, 0);
            Interlocked.Exchange(ref meterRefreshQueued, 0);
        }

        var elapsed = recordingStartedAt is null
            ? TimeSpan.Zero
            : DateTimeOffset.Now - recordingStartedAt.Value;
        var inputLevel = controller.State == DictationState.Recording
            ? Interlocked.CompareExchange(ref latestRecordingInputLevel, 0, 0)
            : 0;
        var state = FloatingRecorderPresenter.FromState(
            controller.State,
            elapsed,
            displayStatus,
            recorderActivityActive && operationActive,
            inputLevel,
            controller.PartialTranscript,
            ShowLiveTranscriptPreviewCheckBox.IsChecked == true,
            SelectedRecorderStyle());

        if (!state.IsVisible && floatingRecorderWindow is null)
        {
            UpdateFloatingRecorderRefreshTimer(state);
            return;
        }

        var floatingWindow = EnsureFloatingRecorderWindow();
        floatingWindow.ApplyControls(
            BuildFloatingRecorderControlState(),
            CanUseFloatingRecorderControls());
        floatingWindow.Apply(state);
        UpdateFloatingRecorderRefreshTimer(state);
    }

    private FloatingRecorderControlState BuildFloatingRecorderControlState()
    {
        var settings = new AppSettings
        {
            IsEnhancementEnabled = EnhancementEnabledCheckBox.IsChecked == true,
            SelectedEnhancementPromptId = SelectedEnhancementPromptId(),
            SelectedPowerModeRuleId = SelectedPowerModeRuleId()
        };

        return FloatingRecorderControlPresenter.FromSettings(
            settings,
            enhancementPrompts,
            powerModeRules);
    }

    private void QueueMeterRefresh()
    {
        if (controller.State != DictationState.Recording
            || windowLifetime.IsCancellationRequested
            || Interlocked.Exchange(ref meterRefreshQueued, 1) == 1)
        {
            return;
        }

        if (!DispatcherQueue.TryEnqueue(() =>
            {
                Interlocked.Exchange(ref meterRefreshQueued, 0);
                if (controller.State == DictationState.Recording && !windowLifetime.IsCancellationRequested)
                {
                    RefreshUiFromControllerState();
                }
            }))
        {
            Interlocked.Exchange(ref meterRefreshQueued, 0);
        }
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

        floatingRecorderWindow = new FloatingRecorderWindow
        {
            StopRequested = StopCurrentRecordingAsync,
            CancelRequested = CancelCurrentRecordingAsync,
            PromptEnhancementToggled = SetFloatingRecorderEnhancementAsync,
            PromptChoiceRequested = SelectFloatingRecorderPromptAsync,
            PowerModeChoiceRequested = SelectFloatingRecorderPowerModeAsync
        };
        floatingRecorderWindow.Closed += (_, _) => floatingRecorderWindow = null;
        return floatingRecorderWindow;
    }

    private async Task SetFloatingRecorderEnhancementAsync(bool isEnabled)
    {
        if (!CanUseFloatingRecorderControls())
        {
            return;
        }

        EnhancementEnabledCheckBox.IsChecked = isEnabled;
        await SaveFloatingRecorderControlSettingsAsync(
            isEnabled ? "AI enhancement enabled" : "AI enhancement disabled");
    }

    private async Task SelectFloatingRecorderPromptAsync(Guid promptId)
    {
        if (!CanUseFloatingRecorderControls())
        {
            return;
        }

        var state = BuildFloatingRecorderControlState();
        var selectedChoice = state.PromptChoices.FirstOrDefault(choice => choice.Id == promptId);
        if (selectedChoice is null)
        {
            return;
        }

        EnhancementEnabledCheckBox.IsChecked = true;
        SelectEnhancementPrompt(promptId);
        await SaveFloatingRecorderControlSettingsAsync($"Prompt: {selectedChoice.Title}");
    }

    private async Task SelectFloatingRecorderPowerModeAsync(Guid? ruleId)
    {
        if (!CanUseFloatingRecorderControls())
        {
            return;
        }

        var state = BuildFloatingRecorderControlState();
        var selectedChoice = state.PowerModeChoices.FirstOrDefault(choice => choice.Id == ruleId);
        if (selectedChoice is null)
        {
            return;
        }

        selectedPowerModeRuleId = ruleId;
        await SaveFloatingRecorderControlSettingsAsync(
            ruleId is null
                ? "Power Mode: Auto"
                : $"Power Mode: {selectedChoice.Title}");
    }

    private async void HotkeyService_MiniRecorderShortcutPressed(object? sender, MiniRecorderShortcutPressedEventArgs e)
    {
        try
        {
            if (!CanUseFloatingRecorderControls())
            {
                return;
            }

            var state = BuildFloatingRecorderControlState();
            if (e.Kind == MiniRecorderShortcutKind.Prompt)
            {
                if (e.SlotIndex >= state.PromptChoices.Count)
                {
                    return;
                }

                await SelectFloatingRecorderPromptAsync(state.PromptChoices[e.SlotIndex].Id);
                return;
            }

            var powerModeChoices = state.PowerModeChoices
                .Where(choice => choice.Id is not null)
                .ToArray();
            if (e.SlotIndex >= powerModeChoices.Length)
            {
                return;
            }

            await SelectFloatingRecorderPowerModeAsync(powerModeChoices[e.SlotIndex].Id);
        }
        catch (Exception ex)
        {
            RefreshUiFromControllerState($"Mini recorder shortcut failed: {ex.Message}");
        }
    }

    private async Task SaveFloatingRecorderControlSettingsAsync(string status)
    {
        try
        {
            await floatingRecorderControlUpdates.RunUpdateAsync(
                async cancellationToken =>
                {
                    try
                    {
                        RefreshUiFromControllerState(status);
                        await SaveSettingsAsync(cancellationToken);
                        RefreshUiFromControllerState(status);
                    }
                    catch (OperationCanceledException) when (windowLifetime.IsCancellationRequested)
                    {
                        RefreshUiFromControllerState("Closing");
                    }
                    catch (Exception ex)
                    {
                        RefreshUiFromControllerState($"Recorder control update failed: {ex.Message}");
                    }
                },
                windowLifetime.Token);
        }
        catch (OperationCanceledException) when (windowLifetime.IsCancellationRequested)
        {
            RefreshUiFromControllerState("Closing");
        }
        catch (Exception ex)
        {
            RefreshUiFromControllerState($"Recorder control update failed: {ex.Message}");
        }
    }

    private bool CanUseFloatingRecorderControls() =>
        settingsLoaded
        && controller.State == DictationState.Recording
        && !IsOperationActive()
        && !floatingRecorderControlUpdates.IsUpdating
        && !windowLifetime.IsCancellationRequested;

    private void SelectEnhancementPrompt(Guid promptId)
    {
        var selectedIndex = enhancementPrompts
            .ToList()
            .FindIndex(prompt => prompt.Id == promptId);
        if (selectedIndex >= 0)
        {
            EnhancementPromptComboBox.SelectedIndex = selectedIndex;
        }
    }

    private void UpdateTrayFromControllerState(string displayStatus, bool operationActive)
    {
        var trayState = TrayShellPresenter.FromState(
            settingsLoaded,
            controller.State,
            operationActive,
            displayStatus);
        trayIconService?.UpdateState(trayState);
        trayIconService?.UpdateQuickSettings(BuildTrayQuickSettingsState());
    }

    private TrayQuickSettingsState BuildTrayQuickSettingsState()
    {
        if (!settingsLoaded)
        {
            return TrayQuickSettingsState.Empty;
        }

        var selectedModelPath = ModelPathTextBox.Text.Trim();
        var selectedCloudProviderId = SelectedCloudTranscriptionProviderId();
        var selectedEnhancementProviderId = SelectedEnhancementProviderId();
        var selectedEnhancementModel = EnhancementModelTextBox.Text.Trim();
        var selectedEnhancementPreset = EnhancementProviderPresetCatalog.Resolve(selectedEnhancementProviderId);
        var selectedPromptId = SelectedEnhancementPromptId();
        var selectedLanguage = SelectedLanguageCode();
        var selectedAudioDeviceNumber = SelectedAudioInputDeviceNumber();
        var selectedAudioMode = SelectedAudioInputMode();

        return new TrayQuickSettingsState(
            TranscriptionModels: modelChoices
                .Select(model => new TrayMenuOption(
                    model.Path,
                    model.DisplayName,
                    string.Equals(model.Path, selectedModelPath, StringComparison.OrdinalIgnoreCase)))
                .ToArray(),
            TranscriptionProviders:
            [
                new TrayMenuOption(
                    "local",
                    "Local Whisper",
                    SelectedTranscriptionProvider() == TranscriptionProviderKind.LocalWhisper),
                .. TranscriptionProviderPresetCatalog.All.Select(provider => new TrayMenuOption(
                    provider.Id,
                    provider.DisplayName,
                    SelectedTranscriptionProvider() == TranscriptionProviderKind.OpenAICompatible
                        && string.Equals(provider.Id, selectedCloudProviderId, StringComparison.OrdinalIgnoreCase)))
            ],
            EnhancementPrompts: enhancementPrompts
                .Select(prompt => new TrayMenuOption(
                    prompt.Id.ToString(),
                    prompt.Title,
                    prompt.Id == selectedPromptId))
                .ToArray(),
            EnhancementProviders: EnhancementProviderPresetCatalog.All
                .Select(provider => new TrayMenuOption(
                    provider.Id,
                    provider.DisplayName,
                    string.Equals(provider.Id, selectedEnhancementProviderId, StringComparison.OrdinalIgnoreCase)))
                .ToArray(),
            EnhancementModels: TrayEnhancementModelOptionsPresenter.FromModels(
                EnhancementModelChoicesForPreset(selectedEnhancementPreset),
                selectedEnhancementModel),
            Languages: languageChoices
                .Select(choice => new TrayMenuOption(
                    choice.Code,
                    choice.DisplayName,
                    string.Equals(choice.Code, selectedLanguage, StringComparison.OrdinalIgnoreCase)))
                .ToArray(),
            AudioInputs: audioInputChoices
                .Select(choice => new TrayMenuOption(
                    choice.DeviceNumber?.ToString(CultureInfo.InvariantCulture) ?? AudioInputModeSettings.SystemDefault,
                    choice.DisplayText,
                    choice.DeviceNumber == selectedAudioDeviceNumber
                        || (choice.DeviceNumber is null
                            && selectedAudioMode == AudioInputModeSettings.SystemDefault)))
                .ToArray(),
            PowerModes:
            [
                new TrayMenuOption(string.Empty, "Auto", selectedPowerModeRuleId is null),
                .. powerModeRules
                    .Where(rule => rule.IsEnabled)
                    .Select(rule => new TrayMenuOption(
                        rule.Id.ToString(),
                        PowerModeDisplay(rule.Name, rule.Emoji),
                        rule.Id == selectedPowerModeRuleId))
            ],
            IsEnhancementEnabled: EnhancementEnabledCheckBox.IsChecked == true,
            UseClipboardContext: UseClipboardContextCheckBox.IsChecked == true,
            UseOcrContext: UseOcrContextCheckBox.IsChecked == true);
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

    private double SelectedClipboardRestoreDelaySeconds() =>
        DoubleChoiceAtOrDefault(ClipboardRestoreDelayChoices, ClipboardRestoreDelayComboBox.SelectedIndex, 2.0);

    private double SelectedAudioResumptionDelaySeconds() =>
        DoubleChoiceAtOrDefault(AudioResumptionDelayChoices, AudioResumptionDelayComboBox.SelectedIndex, 0.0);

    private string? ValidateOcrRegionSettings()
    {
        if (UseOcrContextCheckBox.IsChecked != true || UseOcrCaptureRegionCheckBox.IsChecked != true)
        {
            return null;
        }

        if (!NumberBoxHasFiniteValue(OcrRegionLeftNumberBox)
            || !NumberBoxHasFiniteValue(OcrRegionTopNumberBox)
            || !NumberBoxHasFiniteValue(OcrRegionWidthNumberBox)
            || !NumberBoxHasFiniteValue(OcrRegionHeightNumberBox))
        {
            return "OCR region values must be valid numbers.";
        }

        if (OcrRegionWidthNumberBox.Value <= 0 || OcrRegionHeightNumberBox.Value <= 0)
        {
            return "OCR region width and height must be positive.";
        }

        return null;
    }

    private static bool NumberBoxHasFiniteValue(NumberBox numberBox) =>
        !double.IsNaN(numberBox.Value) && !double.IsInfinity(numberBox.Value);

    private static int CheckedNumberBoxIntValue(NumberBox numberBox) =>
        NumberBoxHasFiniteValue(numberBox)
            ? (int)Math.Round(numberBox.Value, MidpointRounding.AwayFromZero)
            : 0;

    private async Task PickOcrRegionAsync()
    {
        if (!settingsLoaded || IsOperationActive())
        {
            return;
        }

        try
        {
            var picker = new OcrRegionPickerWindow(OcrDisplayComboBox.SelectedItem as ScreenCaptureDisplay);
            var region = await picker.PickAsync();
            if (region is null)
            {
                RefreshUiFromControllerState("OCR region selection canceled");
                return;
            }

            UseOcrContextCheckBox.IsChecked = true;
            UseOcrCaptureRegionCheckBox.IsChecked = true;
            OcrRegionLeftNumberBox.Value = region.Left;
            OcrRegionTopNumberBox.Value = region.Top;
            OcrRegionWidthNumberBox.Value = region.Width;
            OcrRegionHeightNumberBox.Value = region.Height;

            var validationError = ValidateOcrRegionSettings();
            if (validationError is not null)
            {
                RefreshUiFromControllerState(validationError);
                return;
            }

            await SaveSettingsAsync(windowLifetime.Token);
            RefreshUiFromControllerState("OCR region selected");
        }
        catch (OperationCanceledException) when (windowLifetime.IsCancellationRequested)
        {
            RefreshUiFromControllerState("Closing");
        }
        catch (Exception ex)
        {
            RefreshUiFromControllerState($"OCR region selection failed: {ex.Message}");
        }
    }

    private string SelectedPasteMethod() =>
        PasteMethodComboBox.SelectedIndex == 1
            ? PasteMethodSettings.DirectText
            : PasteMethodSettings.Default;

    private string SelectedPrimaryRecordingShortcutMode() =>
        RecordingShortcutModeFromSelectedIndex(PrimaryRecordingShortcutModeComboBox.SelectedIndex);

    private string SelectedSecondaryRecordingShortcutMode() =>
        RecordingShortcutModeFromSelectedIndex(SecondaryRecordingShortcutModeComboBox.SelectedIndex);

    private static string RecordingShortcutModeFromSelectedIndex(int selectedIndex) =>
        selectedIndex switch
        {
            1 => RecordingShortcutModeSettings.PushToTalk,
            2 => RecordingShortcutModeSettings.Hybrid,
            _ => RecordingShortcutModeSettings.Toggle
        };

    private string SelectedRecorderStyle() =>
        RecorderStyleComboBox.SelectedIndex == 1
            ? RecorderStyleSettings.Notch
            : RecorderStyleSettings.Mini;

    private string SelectedStartSoundMode() =>
        StartSoundComboBox.SelectedIndex == 1
            ? RecordingSoundModeSettings.Custom
            : RecordingSoundModeSettings.SystemDefault;

    private string SelectedStopSoundMode() =>
        StopSoundComboBox.SelectedIndex == 1
            ? RecordingSoundModeSettings.Custom
            : RecordingSoundModeSettings.SystemDefault;

    private RecordingSoundPlaybackSettings CurrentRecordingSoundPlaybackSettings(RecordingSoundKind kind) =>
        kind == RecordingSoundKind.Start
            ? new RecordingSoundPlaybackSettings(SelectedStartSoundMode(), customStartSoundPath)
            : new RecordingSoundPlaybackSettings(SelectedStopSoundMode(), customStopSoundPath);

    private static int RecordingSoundModeToSelectedIndex(string? mode, string customSoundPath) =>
        RecordingSoundModeSettings.Normalize(mode) == RecordingSoundModeSettings.Custom
            && !string.IsNullOrWhiteSpace(customSoundPath)
                ? 1
                : 0;

    private static string RecordingSoundStatus(
        RecordingSoundKind kind,
        RecordingSoundPlaybackSettings settings,
        bool tested = false)
    {
        var soundName = kind == RecordingSoundKind.Start ? "Start" : "Stop";
        if (!settings.UsesCustomSound)
        {
            return tested
                ? $"{soundName} system sound tested"
                : "No custom sound";
        }

        var fileName = Path.GetFileName(settings.CustomSoundPath);
        return tested
            ? $"{soundName} custom sound tested: {fileName}"
            : $"Custom sound: {fileName}";
    }

    private static int ClipboardRestoreDelayToSelectedIndex(double seconds)
    {
        var index = Array.FindIndex(
            ClipboardRestoreDelayChoices,
            choice => Math.Abs(choice - seconds) < 0.001);
        return index >= 0 ? index : 3;
    }

    private static int AudioResumptionDelayToSelectedIndex(double seconds)
    {
        var normalized = double.IsFinite(seconds) ? Math.Clamp(seconds, 0, 5) : 0;
        var index = Array.FindIndex(
            AudioResumptionDelayChoices,
            choice => Math.Abs(choice - normalized) < 0.001);
        return index >= 0 ? index : 0;
    }

    private static int PasteMethodToSelectedIndex(string method) =>
        PasteMethodSettings.Normalize(method) == PasteMethodSettings.DirectText ? 1 : 0;

    private static int RecordingShortcutModeToSelectedIndex(string? mode) =>
        RecordingShortcutModeSettings.Normalize(mode) switch
        {
            RecordingShortcutModeSettings.PushToTalk => 1,
            RecordingShortcutModeSettings.Hybrid => 2,
            _ => 0
        };

    private static int RecorderStyleToSelectedIndex(string? style) =>
        RecorderStyleSettings.Normalize(style) == RecorderStyleSettings.Notch ? 1 : 0;

    private int SelectedTranscriptionRetentionMinutes() =>
        ChoiceAtOrDefault(
            TranscriptionRetentionMinuteChoices,
            TranscriptionRetentionComboBox.SelectedIndex,
            24 * 60);

    private int SelectedAudioRetentionDays() =>
        ChoiceAtOrDefault(AudioRetentionDayChoices, AudioRetentionComboBox.SelectedIndex, 7);

    private static int TranscriptionRetentionToSelectedIndex(int minutes) =>
        ChoiceIndexOrDefault(TranscriptionRetentionMinuteChoices, minutes, 2);

    private static int AudioRetentionToSelectedIndex(int days) =>
        ChoiceIndexOrDefault(AudioRetentionDayChoices, days, 2);

    private string SelectedTranscriptionRetentionLabel() =>
        SelectedTranscriptionRetentionMinutes() switch
        {
            0 => "immediately",
            60 => "1 hour",
            24 * 60 => "1 day",
            3 * 24 * 60 => "3 days",
            7 * 24 * 60 => "7 days",
            var minutes => $"{minutes:N0} minutes"
        };

    private string SelectedAudioRetentionLabel() =>
        SelectedAudioRetentionDays() == 1 ? "1 day" : $"{SelectedAudioRetentionDays():N0} days";

    private static int ChoiceAtOrDefault(IReadOnlyList<int> choices, int index, int fallback) =>
        index >= 0 && index < choices.Count ? choices[index] : fallback;

    private static double DoubleChoiceAtOrDefault(IReadOnlyList<double> choices, int index, double fallback) =>
        index >= 0 && index < choices.Count ? choices[index] : fallback;

    private static int ChoiceIndexOrDefault(IReadOnlyList<int> choices, int value, int fallbackIndex)
    {
        var index = Array.IndexOf(choices.ToArray(), value);
        return index >= 0 ? index : fallbackIndex;
    }

    private void UpdateCleanupSettingControlState()
    {
        var canUsePrivacyCleanup = CanUsePrivacyCleanup();
        var transcriptCleanupEnabled = TranscriptionCleanupCheckBox.IsChecked == true;
        var audioCleanupEnabled = AudioCleanupCheckBox.IsChecked == true && !transcriptCleanupEnabled;

        TranscriptionRetentionComboBox.IsEnabled = settingsLoaded
            && !isRunningPrivacyCleanup
            && transcriptCleanupEnabled;
        RunTranscriptCleanupButton.IsEnabled = canUsePrivacyCleanup && transcriptCleanupEnabled;
        AudioCleanupSettingsPanel.Visibility = transcriptCleanupEnabled
            ? Visibility.Collapsed
            : Visibility.Visible;
        AudioRetentionComboBox.IsEnabled = settingsLoaded
            && !isRunningPrivacyCleanup
            && audioCleanupEnabled;
        RunAudioCleanupButton.IsEnabled = canUsePrivacyCleanup && audioCleanupEnabled;
    }

    private void UpdateClipboardSettingControlState()
    {
        ClipboardRestoreDelayComboBox.IsEnabled = settingsLoaded
            && !IsOperationActive()
            && RestoreClipboardCheckBox.IsChecked == true
            && PasteMethodToSelectedIndex(SelectedPasteMethod()) == 0;
    }

    private void RefreshRecordingSoundControls(string? startStatus = null, string? stopStatus = null)
    {
        var hasStartCustomSound = !string.IsNullOrWhiteSpace(customStartSoundPath);
        var hasStopCustomSound = !string.IsNullOrWhiteSpace(customStopSoundPath);

        StartCustomSoundComboBoxItem.Content = hasStartCustomSound
            ? $"Custom: {Path.GetFileName(customStartSoundPath)}"
            : "Custom Sound";
        StopCustomSoundComboBoxItem.Content = hasStopCustomSound
            ? $"Custom: {Path.GetFileName(customStopSoundPath)}"
            : "Custom Sound";
        StartCustomSoundComboBoxItem.Visibility = hasStartCustomSound
            ? Visibility.Visible
            : Visibility.Collapsed;
        StopCustomSoundComboBoxItem.Visibility = hasStopCustomSound
            ? Visibility.Visible
            : Visibility.Collapsed;

        if (!hasStartCustomSound && StartSoundComboBox.SelectedIndex == 1)
        {
            StartSoundComboBox.SelectedIndex = 0;
        }

        if (!hasStopCustomSound && StopSoundComboBox.SelectedIndex == 1)
        {
            StopSoundComboBox.SelectedIndex = 0;
        }

        StartSoundStatusTextBlock.Text = startStatus
            ?? RecordingSoundStatus(RecordingSoundKind.Start, CurrentRecordingSoundPlaybackSettings(RecordingSoundKind.Start));
        StopSoundStatusTextBlock.Text = stopStatus
            ?? RecordingSoundStatus(RecordingSoundKind.Stop, CurrentRecordingSoundPlaybackSettings(RecordingSoundKind.Stop));
    }

    private void UpdateRecordingFeedbackSettingControlState()
    {
        var canEditSounds = settingsLoaded && !IsOperationActive();
        StartSoundComboBox.IsEnabled = canEditSounds;
        StopSoundComboBox.IsEnabled = canEditSounds;
        TestStartSoundButton.IsEnabled = canEditSounds;
        TestStopSoundButton.IsEnabled = canEditSounds;
        ChooseStartSoundButton.IsEnabled = canEditSounds;
        ChooseStopSoundButton.IsEnabled = canEditSounds;
        ResetStartSoundButton.IsEnabled = canEditSounds && !string.IsNullOrWhiteSpace(customStartSoundPath);
        ResetStopSoundButton.IsEnabled = canEditSounds && !string.IsNullOrWhiteSpace(customStopSoundPath);

        AudioResumptionDelayComboBox.IsEnabled = settingsLoaded
            && !IsOperationActive()
            && (MuteSystemAudioCheckBox.IsChecked == true || PauseMediaCheckBox.IsChecked == true);
    }

    private static string TranscriptCleanupStatus(PrivacyCleanupResult cleanup)
    {
        if (!cleanup.IsEnabled)
        {
            return "Transcript cleanup is disabled";
        }

        var status = $"Deleted {cleanup.DeletedTranscriptionCount:N0} transcript(s)"
            + $" and {cleanup.DeletedAudioFileCount:N0} audio file(s)";
        if (cleanup.FailedAudioFileCount > 0)
        {
            status += $" ({cleanup.FailedAudioFileCount:N0} audio file(s) could not be deleted)";
        }

        return status;
    }

    private static string AudioCleanupStatus(PrivacyCleanupResult cleanup)
    {
        if (!cleanup.IsEnabled)
        {
            return "Audio cleanup is disabled";
        }

        var status = $"Deleted {cleanup.DeletedAudioFileCount:N0} audio file(s)"
            + $" and cleared {cleanup.ClearedAudioReferenceCount:N0} history reference(s)";
        if (cleanup.FailedAudioFileCount > 0)
        {
            status += $" ({cleanup.FailedAudioFileCount:N0} audio file(s) could not be deleted)";
        }

        return status;
    }

    private static bool HasPrivacyCleanupWork(PrivacyCleanupResult cleanup) =>
        cleanup.DeletedTranscriptionCount > 0
        || cleanup.DeletedAudioFileCount > 0
        || cleanup.FailedAudioFileCount > 0
        || cleanup.ClearedAudioReferenceCount > 0;

    private static string FormatFileSize(long bytes)
    {
        string[] units = ["B", "KB", "MB", "GB"];
        var value = (double)Math.Max(0, bytes);
        var unitIndex = 0;
        while (value >= 1024 && unitIndex < units.Length - 1)
        {
            value /= 1024;
            unitIndex++;
        }

        return unitIndex == 0
            ? $"{value:0} {units[unitIndex]}"
            : $"{value:0.#} {units[unitIndex]}";
    }

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
        UnsubscribePowerModeEvents();
        modelWarmupCoordinator.StateChanged -= ModelWarmupCoordinator_StateChanged;
        modelWarmupCoordinator.CancelCurrentAsync().GetAwaiter().GetResult();
        audioFileQueueCancellation?.Cancel();
        audioFileQueueCancellation?.Dispose();
        CancelModelDownloadAsync().GetAwaiter().GetResult();
        stopOperationCancellation?.Cancel();
        stopOperationCancellation?.Dispose();
        DisposeGlobalHotkeyService();
        DisposeTrayIconService();
        ClearHistoryAudioPlayer();
        floatingRecorderRefreshTimer.Stop();
        privacyCleanupTimer.Stop();
        if (historyWindow is not null)
        {
            historyWindow.Closed -= HistoryWindow_Closed;
            historyWindow.Close();
            historyWindow = null;
        }

        floatingRecorderWindow?.Close();
        CancelRecordingFeedbackSessionAsync(immediate: true).GetAwaiter().GetResult();
        recordingFeedback.RestorePendingImmediatelyAsync().GetAwaiter().GetResult();

        audioCapture.LevelAvailable -= AudioCapture_LevelAvailable;
        audioCapture.Dispose();
        audioInputDeviceChangeWatcher.DevicesChanged -= AudioInputDeviceChangeWatcher_DevicesChanged;
        audioInputDeviceChangeWatcher.Dispose();
        recordingSoundFeedback.Dispose();
        systemAudioFeedback.Dispose();
        modelDownloadHttpClient.Dispose();
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
        trayIconService.SelectTranscriptionModelRequested += TrayIconService_SelectTranscriptionModelRequested;
        trayIconService.SelectTranscriptionProviderRequested += TrayIconService_SelectTranscriptionProviderRequested;
        trayIconService.ToggleEnhancementRequested += TrayIconService_ToggleEnhancementRequested;
        trayIconService.SelectEnhancementPromptRequested += TrayIconService_SelectEnhancementPromptRequested;
        trayIconService.SelectEnhancementProviderRequested += TrayIconService_SelectEnhancementProviderRequested;
        trayIconService.SelectEnhancementModelRequested += TrayIconService_SelectEnhancementModelRequested;
        trayIconService.SelectLanguageRequested += TrayIconService_SelectLanguageRequested;
        trayIconService.SelectAudioInputRequested += TrayIconService_SelectAudioInputRequested;
        trayIconService.SelectPowerModeRequested += TrayIconService_SelectPowerModeRequested;
        trayIconService.ToggleClipboardContextRequested += TrayIconService_ToggleClipboardContextRequested;
        trayIconService.ToggleOcrContextRequested += TrayIconService_ToggleOcrContextRequested;
        trayIconService.OpenModelsRequested += TrayIconService_OpenModelsRequested;
        trayIconService.OpenEnhancementRequested += TrayIconService_OpenEnhancementRequested;
        trayIconService.OpenAudioInputRequested += TrayIconService_OpenAudioInputRequested;
        trayIconService.OpenSettingsRequested += TrayIconService_OpenSettingsRequested;
        trayIconService.QuickAddDictionaryRequested += TrayIconService_QuickAddDictionaryRequested;
        trayIconService.OpenHistoryRequested += TrayIconService_OpenHistoryRequested;
        trayIconService.ExitRequested += TrayIconService_ExitRequested;
    }

    private void TrySubscribePowerModeEvents()
    {
        try
        {
            SystemEvents.PowerModeChanged += SystemEvents_PowerModeChanged;
            powerModeEventsSubscribed = true;
        }
        catch (Exception ex)
        {
            UpdateModelWarmupStatus(new WhisperModelWarmupState(
                WhisperModelWarmupStatus.Skipped,
                string.Empty,
                string.Empty,
                "resume",
                $"Resume warmup unavailable: {ex.Message}"));
        }
    }

    private void SystemEvents_PowerModeChanged(object sender, PowerModeChangedEventArgs e)
    {
        if (e.Mode != PowerModes.Resume || windowLifetime.IsCancellationRequested)
        {
            return;
        }

        _ = DispatcherQueue.TryEnqueue(() =>
        {
            _ = ScheduleModelWarmupFromCurrentSettingsAsync("resume");
        });
    }

    private void UnsubscribePowerModeEvents()
    {
        if (!powerModeEventsSubscribed)
        {
            return;
        }

        SystemEvents.PowerModeChanged -= SystemEvents_PowerModeChanged;
        powerModeEventsSubscribed = false;
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
        trayIconService.SelectTranscriptionModelRequested -= TrayIconService_SelectTranscriptionModelRequested;
        trayIconService.SelectTranscriptionProviderRequested -= TrayIconService_SelectTranscriptionProviderRequested;
        trayIconService.ToggleEnhancementRequested -= TrayIconService_ToggleEnhancementRequested;
        trayIconService.SelectEnhancementPromptRequested -= TrayIconService_SelectEnhancementPromptRequested;
        trayIconService.SelectEnhancementProviderRequested -= TrayIconService_SelectEnhancementProviderRequested;
        trayIconService.SelectEnhancementModelRequested -= TrayIconService_SelectEnhancementModelRequested;
        trayIconService.SelectLanguageRequested -= TrayIconService_SelectLanguageRequested;
        trayIconService.SelectAudioInputRequested -= TrayIconService_SelectAudioInputRequested;
        trayIconService.SelectPowerModeRequested -= TrayIconService_SelectPowerModeRequested;
        trayIconService.ToggleClipboardContextRequested -= TrayIconService_ToggleClipboardContextRequested;
        trayIconService.ToggleOcrContextRequested -= TrayIconService_ToggleOcrContextRequested;
        trayIconService.OpenModelsRequested -= TrayIconService_OpenModelsRequested;
        trayIconService.OpenEnhancementRequested -= TrayIconService_OpenEnhancementRequested;
        trayIconService.OpenAudioInputRequested -= TrayIconService_OpenAudioInputRequested;
        trayIconService.OpenSettingsRequested -= TrayIconService_OpenSettingsRequested;
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
    private const int VirtualKeyShift = 0x10;
    private const int VirtualKeyControl = 0x11;
    private const int VirtualKeyMenu = 0x12;
    private const int VirtualKeyEscape = 0x1B;

    private static bool IsShortcutModifierKey(int virtualKey) =>
        virtualKey is VirtualKeyShift or VirtualKeyControl or VirtualKeyMenu;

    private bool IsRecordingShortcutTextBox(TextBox textBox) =>
        ReferenceEquals(textBox, RecordingHotkeyTextBox)
        || ReferenceEquals(textBox, SecondaryRecordingHotkeyTextBox);

    private static bool IsKeyDown(int virtualKey) =>
        (GetKeyState(virtualKey) & 0x8000) != 0;

    private sealed record AudioInputPriorityItem(string Name, string PriorityDisplay);

    [DllImport("user32.dll")]
    private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    [DllImport("user32.dll")]
    private static extern bool SetForegroundWindow(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern bool IsIconic(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern short GetKeyState(int nVirtKey);
}
