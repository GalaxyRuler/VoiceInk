using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using VoiceInk.Windows.Core.Shell;

namespace VoiceInk.Windows.Native.Tray;

public sealed class TrayIconService : IDisposable
{
    private const int MaxTooltipLength = 63;

    private readonly Icon icon;
    private readonly NotifyIcon notifyIcon;
    private readonly TaskbarCreatedMessageWindow taskbarCreatedMessageWindow;
    private readonly ContextMenuStrip trayContextMenu;
    private readonly ToolStripMenuItem showItem;
    private readonly ToolStripMenuItem hideItem;
    private readonly ToolStripMenuItem statusItem;
    private readonly ToolStripMenuItem visibilityGuidanceItem;
    private readonly ToolStripMenuItem toggleRecordingItem;
    private readonly ToolStripMenuItem transcriptionModelMenu;
    private readonly ToolStripMenuItem transcriptionProviderMenu;
    private readonly ToolStripMenuItem enhancementEnabledItem;
    private readonly ToolStripMenuItem enhancementPromptMenu;
    private readonly ToolStripMenuItem enhancementProviderMenu;
    private readonly ToolStripMenuItem enhancementModelMenu;
    private readonly ToolStripMenuItem languageMenu;
    private readonly ToolStripMenuItem audioInputMenu;
    private readonly ToolStripMenuItem powerModeMenu;
    private readonly ToolStripMenuItem contextAwarenessMenu;
    private readonly ToolStripMenuItem clipboardContextItem;
    private readonly ToolStripMenuItem ocrContextItem;
    private readonly ToolStripMenuItem manageModelsItem;
    private readonly ToolStripMenuItem enhancementSettingsItem;
    private readonly ToolStripMenuItem audioInputSettingsItem;
    private readonly ToolStripMenuItem settingsItem;
    private readonly ToolStripMenuItem pasteLastItem;
    private readonly ToolStripMenuItem pasteLastEnhancedItem;
    private readonly ToolStripMenuItem retryLastItem;
    private readonly ToolStripMenuItem quickAddDictionaryItem;
    private readonly ToolStripMenuItem historyItem;
    private readonly ToolStripMenuItem quitItem;
    private bool disposed;

    public TrayIconService()
    {
        icon = LoadIcon();
        showItem = new ToolStripMenuItem("Show VoiceInk", image: null, (_, _) => ShowRequested?.Invoke(this, EventArgs.Empty));
        hideItem = new ToolStripMenuItem("Hide VoiceInk", image: null, (_, _) => HideRequested?.Invoke(this, EventArgs.Empty));
        statusItem = new ToolStripMenuItem("Status: Loading settings") { Enabled = false };
        visibilityGuidanceItem = new ToolStripMenuItem("Taskbar settings: Other system tray icons", image: null, (_, _) => OpenTaskbarSettingsRequested?.Invoke(this, EventArgs.Empty))
        {
            Enabled = true
        };
        toggleRecordingItem = new ToolStripMenuItem("Start Recording", image: null, (_, _) => ToggleRecordingRequested?.Invoke(this, EventArgs.Empty));
        transcriptionModelMenu = new ToolStripMenuItem("Transcription Model");
        transcriptionProviderMenu = new ToolStripMenuItem("Transcription Provider");
        enhancementEnabledItem = new ToolStripMenuItem("AI Enhancement", image: null, (_, _) => ToggleEnhancementRequested?.Invoke(this, EventArgs.Empty));
        enhancementPromptMenu = new ToolStripMenuItem("Prompt");
        enhancementProviderMenu = new ToolStripMenuItem("AI Provider");
        enhancementModelMenu = new ToolStripMenuItem("AI Model");
        languageMenu = new ToolStripMenuItem("Language");
        audioInputMenu = new ToolStripMenuItem("Audio Input");
        powerModeMenu = new ToolStripMenuItem("Power Mode");
        contextAwarenessMenu = new ToolStripMenuItem("Additional");
        clipboardContextItem = new ToolStripMenuItem("Clipboard Context", image: null, (_, _) => ToggleClipboardContextRequested?.Invoke(this, EventArgs.Empty));
        ocrContextItem = new ToolStripMenuItem("Context Awareness", image: null, (_, _) => ToggleOcrContextRequested?.Invoke(this, EventArgs.Empty));
        manageModelsItem = new ToolStripMenuItem("Manage Models", image: null, (_, _) => OpenModelsRequested?.Invoke(this, EventArgs.Empty));
        enhancementSettingsItem = new ToolStripMenuItem("Enhancement Settings", image: null, (_, _) => OpenEnhancementRequested?.Invoke(this, EventArgs.Empty));
        audioInputSettingsItem = new ToolStripMenuItem("Audio Input Settings", image: null, (_, _) => OpenAudioInputRequested?.Invoke(this, EventArgs.Empty));
        settingsItem = new ToolStripMenuItem("Settings", image: null, (_, _) => OpenSettingsRequested?.Invoke(this, EventArgs.Empty));
        pasteLastItem = new ToolStripMenuItem("Paste Last Transcription", image: null, (_, _) => PasteLastTranscriptionRequested?.Invoke(this, EventArgs.Empty));
        pasteLastEnhancedItem = new ToolStripMenuItem("Paste Last Enhanced", image: null, (_, _) => PasteLastEnhancedTranscriptionRequested?.Invoke(this, EventArgs.Empty));
        retryLastItem = new ToolStripMenuItem("Retry Last Transcription", image: null, (_, _) => RetryLastTranscriptionRequested?.Invoke(this, EventArgs.Empty));
        quickAddDictionaryItem = new ToolStripMenuItem("Quick Add to Dictionary", image: null, (_, _) => QuickAddDictionaryRequested?.Invoke(this, EventArgs.Empty));
        historyItem = new ToolStripMenuItem("History", image: null, (_, _) => OpenHistoryRequested?.Invoke(this, EventArgs.Empty));
        quitItem = new ToolStripMenuItem("Quit VoiceInk", image: null, (_, _) => ExitRequested?.Invoke(this, EventArgs.Empty));

        contextAwarenessMenu.DropDownItems.Add(clipboardContextItem);
        contextAwarenessMenu.DropDownItems.Add(ocrContextItem);

        trayContextMenu = new ContextMenuStrip();
        trayContextMenu.Items.Add(showItem);
        trayContextMenu.Items.Add(hideItem);
        trayContextMenu.Items.Add(statusItem);
        trayContextMenu.Items.Add(visibilityGuidanceItem);
        trayContextMenu.Items.Add(new ToolStripSeparator());
        trayContextMenu.Items.Add(toggleRecordingItem);
        trayContextMenu.Items.Add(new ToolStripSeparator());
        trayContextMenu.Items.Add(transcriptionModelMenu);
        trayContextMenu.Items.Add(transcriptionProviderMenu);
        trayContextMenu.Items.Add(languageMenu);
        trayContextMenu.Items.Add(new ToolStripSeparator());
        trayContextMenu.Items.Add(enhancementEnabledItem);
        trayContextMenu.Items.Add(enhancementPromptMenu);
        trayContextMenu.Items.Add(enhancementProviderMenu);
        trayContextMenu.Items.Add(enhancementModelMenu);
        trayContextMenu.Items.Add(contextAwarenessMenu);
        trayContextMenu.Items.Add(powerModeMenu);
        trayContextMenu.Items.Add(audioInputMenu);
        trayContextMenu.Items.Add(new ToolStripSeparator());
        trayContextMenu.Items.Add(manageModelsItem);
        trayContextMenu.Items.Add(enhancementSettingsItem);
        trayContextMenu.Items.Add(audioInputSettingsItem);
        trayContextMenu.Items.Add(settingsItem);
        trayContextMenu.Items.Add(new ToolStripSeparator());
        trayContextMenu.Items.Add(pasteLastItem);
        trayContextMenu.Items.Add(pasteLastEnhancedItem);
        trayContextMenu.Items.Add(retryLastItem);
        trayContextMenu.Items.Add(quickAddDictionaryItem);
        trayContextMenu.Items.Add(historyItem);
        trayContextMenu.Items.Add(new ToolStripSeparator());
        trayContextMenu.Items.Add(quitItem);

        notifyIcon = new NotifyIcon
        {
            ContextMenuStrip = trayContextMenu,
            Icon = icon,
            Text = "VoiceInk",
            Visible = true
        };
        notifyIcon.DoubleClick += NotifyIcon_DoubleClick;
        taskbarCreatedMessageWindow = new TaskbarCreatedMessageWindow(RestoreNotifyIconAfterTaskbarCreated);
    }

    public event EventHandler? ShowRequested;
    public event EventHandler? HideRequested;
    public event EventHandler? ToggleRecordingRequested;
    public event EventHandler<TrayMenuOptionEventArgs>? SelectTranscriptionModelRequested;
    public event EventHandler<TrayMenuOptionEventArgs>? SelectTranscriptionProviderRequested;
    public event EventHandler? ToggleEnhancementRequested;
    public event EventHandler<TrayMenuOptionEventArgs>? SelectEnhancementPromptRequested;
    public event EventHandler<TrayMenuOptionEventArgs>? SelectEnhancementProviderRequested;
    public event EventHandler<TrayMenuOptionEventArgs>? SelectEnhancementModelRequested;
    public event EventHandler<TrayMenuOptionEventArgs>? SelectLanguageRequested;
    public event EventHandler<TrayMenuOptionEventArgs>? SelectAudioInputRequested;
    public event EventHandler<TrayMenuOptionEventArgs>? SelectPowerModeRequested;
    public event EventHandler? ToggleClipboardContextRequested;
    public event EventHandler? ToggleOcrContextRequested;
    public event EventHandler? OpenModelsRequested;
    public event EventHandler? OpenEnhancementRequested;
    public event EventHandler? OpenAudioInputRequested;
    public event EventHandler? OpenSettingsRequested;
    public event EventHandler? OpenTaskbarSettingsRequested;
    public event EventHandler? PasteLastTranscriptionRequested;
    public event EventHandler? PasteLastEnhancedTranscriptionRequested;
    public event EventHandler? RetryLastTranscriptionRequested;
    public event EventHandler? QuickAddDictionaryRequested;
    public event EventHandler? OpenHistoryRequested;
    public event EventHandler? ExitRequested;

    public void UpdateState(TrayShellState state)
    {
        ObjectDisposedException.ThrowIf(disposed, this);

        toggleRecordingItem.Text = state.ToggleRecordingLabel;
        toggleRecordingItem.Enabled = state.CanToggleRecording;
        pasteLastItem.Enabled = state.CanPasteLastTranscription;
        pasteLastEnhancedItem.Enabled = state.CanPasteLastEnhancedTranscription;
        retryLastItem.Enabled = state.CanRetryLastTranscription;
        quickAddDictionaryItem.Enabled = state.CanQuickAddDictionary;
        historyItem.Enabled = state.CanOpenHistory;
        SetQuickSettingsEnabled(state.CanUseQuickSettings);
        statusItem.Text = state.StatusMenuText;
        visibilityGuidanceItem.Text = state.VisibilityMenuText;
        notifyIcon.Text = Truncate(state.TooltipText, MaxTooltipLength);
    }

    public void UpdateQuickSettings(TrayQuickSettingsState state)
    {
        ObjectDisposedException.ThrowIf(disposed, this);

        PopulateMenu(
            transcriptionModelMenu,
            state.TranscriptionModels,
            SelectTranscriptionModelRequested,
            "No local models imported");
        PopulateMenu(
            transcriptionProviderMenu,
            state.TranscriptionProviders,
            SelectTranscriptionProviderRequested,
            "No providers available");
        PopulateMenu(
            enhancementPromptMenu,
            state.EnhancementPrompts,
            SelectEnhancementPromptRequested,
            "No prompts available");
        PopulateMenu(
            enhancementProviderMenu,
            state.EnhancementProviders,
            SelectEnhancementProviderRequested,
            "No providers available");
        PopulateMenu(
            enhancementModelMenu,
            state.EnhancementModels,
            SelectEnhancementModelRequested,
            "No models available");
        PopulateMenu(
            languageMenu,
            state.Languages,
            SelectLanguageRequested,
            "No languages available");
        PopulateMenu(
            audioInputMenu,
            state.AudioInputs,
            SelectAudioInputRequested,
            "No devices available");
        PopulateMenu(
            powerModeMenu,
            state.PowerModes,
            SelectPowerModeRequested,
            "No Power Modes configured");

        enhancementEnabledItem.Checked = state.IsEnhancementEnabled;
        clipboardContextItem.Checked = state.UseClipboardContext;
        ocrContextItem.Checked = state.UseOcrContext;
    }

    public void Dispose()
    {
        if (disposed)
        {
            return;
        }

        notifyIcon.DoubleClick -= NotifyIcon_DoubleClick;
        taskbarCreatedMessageWindow.Dispose();
        notifyIcon.Visible = false;
        notifyIcon.Dispose();
        trayContextMenu.Dispose();
        icon.Dispose();
        disposed = true;
    }

    private void NotifyIcon_DoubleClick(object? sender, EventArgs e)
    {
        ShowRequested?.Invoke(this, EventArgs.Empty);
    }

    private void RestoreNotifyIconAfterTaskbarCreated()
    {
        if (disposed)
        {
            return;
        }

        notifyIcon.Visible = false;
        notifyIcon.Icon = icon;
        notifyIcon.ContextMenuStrip = trayContextMenu;
        notifyIcon.Visible = true;
    }

    private static Icon LoadIcon()
    {
        if (!string.IsNullOrWhiteSpace(Environment.ProcessPath))
        {
            var extractedIcon = Icon.ExtractAssociatedIcon(Environment.ProcessPath);
            if (extractedIcon is not null)
            {
                using (extractedIcon)
                {
                    return (Icon)extractedIcon.Clone();
                }
            }
        }

        return (Icon)SystemIcons.Application.Clone();
    }

    private static string Truncate(string text, int maxLength) =>
        text.Length <= maxLength
            ? text
            : text[..maxLength];

    private void SetQuickSettingsEnabled(bool isEnabled)
    {
        transcriptionModelMenu.Enabled = isEnabled;
        transcriptionProviderMenu.Enabled = isEnabled;
        enhancementEnabledItem.Enabled = isEnabled;
        enhancementPromptMenu.Enabled = isEnabled;
        enhancementProviderMenu.Enabled = isEnabled;
        enhancementModelMenu.Enabled = isEnabled;
        languageMenu.Enabled = isEnabled;
        audioInputMenu.Enabled = isEnabled;
        powerModeMenu.Enabled = isEnabled;
        contextAwarenessMenu.Enabled = isEnabled;
    }

    private void PopulateMenu(
        ToolStripMenuItem menu,
        IReadOnlyList<TrayMenuOption> options,
        EventHandler<TrayMenuOptionEventArgs>? requested,
        string emptyText)
    {
        menu.DropDownItems.Clear();
        if (options.Count == 0)
        {
            menu.DropDownItems.Add(new ToolStripMenuItem(emptyText) { Enabled = false });
            return;
        }

        foreach (var option in options)
        {
            var item = new ToolStripMenuItem(option.Label)
            {
                Checked = option.IsChecked,
                Enabled = option.IsEnabled
            };
            item.Click += (_, _) => requested?.Invoke(this, new TrayMenuOptionEventArgs(option.Id));
            menu.DropDownItems.Add(item);
        }
    }
}

internal sealed class TaskbarCreatedMessageWindow : NativeWindow, IDisposable
{
    private readonly int taskbarCreatedMessage;
    private readonly Action recoverTrayIcon;
    private bool disposed;

    public TaskbarCreatedMessageWindow(Action recoverTrayIcon)
    {
        this.recoverTrayIcon = recoverTrayIcon;
        taskbarCreatedMessage = RegisterWindowMessage("TaskbarCreated");
        CreateHandle(new CreateParams());
    }

    public void Dispose()
    {
        if (disposed)
        {
            return;
        }

        DestroyHandle();
        disposed = true;
        GC.SuppressFinalize(this);
    }

    protected override void WndProc(ref Message m)
    {
        if (TrayIconRecoveryPolicy.ShouldRecover(m.Msg, taskbarCreatedMessage))
        {
            recoverTrayIcon();
        }

        base.WndProc(ref m);
    }

    [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern int RegisterWindowMessage(string lpString);
}

public sealed class TrayMenuOptionEventArgs(string id) : EventArgs
{
    public string Id { get; } = id;
}
