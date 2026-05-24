using System.Drawing;
using System.Windows.Forms;
using VoiceInk.Windows.Core.Shell;

namespace VoiceInk.Windows.Native.Tray;

public sealed class TrayIconService : IDisposable
{
    private const int MaxTooltipLength = 63;

    private readonly Icon icon;
    private readonly NotifyIcon notifyIcon;
    private readonly ContextMenuStrip contextMenu;
    private readonly ToolStripMenuItem showItem;
    private readonly ToolStripMenuItem hideItem;
    private readonly ToolStripMenuItem toggleRecordingItem;
    private readonly ToolStripMenuItem quickAddDictionaryItem;
    private readonly ToolStripMenuItem historyItem;
    private readonly ToolStripMenuItem quitItem;
    private bool disposed;

    public TrayIconService()
    {
        icon = LoadIcon();
        showItem = new ToolStripMenuItem("Show VoiceInk", image: null, (_, _) => ShowRequested?.Invoke(this, EventArgs.Empty));
        hideItem = new ToolStripMenuItem("Hide VoiceInk", image: null, (_, _) => HideRequested?.Invoke(this, EventArgs.Empty));
        toggleRecordingItem = new ToolStripMenuItem("Start Recording", image: null, (_, _) => ToggleRecordingRequested?.Invoke(this, EventArgs.Empty));
        quickAddDictionaryItem = new ToolStripMenuItem("Quick Add to Dictionary", image: null, (_, _) => QuickAddDictionaryRequested?.Invoke(this, EventArgs.Empty));
        historyItem = new ToolStripMenuItem("History", image: null, (_, _) => OpenHistoryRequested?.Invoke(this, EventArgs.Empty));
        quitItem = new ToolStripMenuItem("Quit VoiceInk", image: null, (_, _) => ExitRequested?.Invoke(this, EventArgs.Empty));

        contextMenu = new ContextMenuStrip();
        contextMenu.Items.Add(showItem);
        contextMenu.Items.Add(hideItem);
        contextMenu.Items.Add(new ToolStripSeparator());
        contextMenu.Items.Add(toggleRecordingItem);
        contextMenu.Items.Add(quickAddDictionaryItem);
        contextMenu.Items.Add(historyItem);
        contextMenu.Items.Add(new ToolStripSeparator());
        contextMenu.Items.Add(quitItem);

        notifyIcon = new NotifyIcon
        {
            ContextMenuStrip = contextMenu,
            Icon = icon,
            Text = "VoiceInk",
            Visible = true
        };
        notifyIcon.DoubleClick += NotifyIcon_DoubleClick;
    }

    public event EventHandler? ShowRequested;
    public event EventHandler? HideRequested;
    public event EventHandler? ToggleRecordingRequested;
    public event EventHandler? QuickAddDictionaryRequested;
    public event EventHandler? OpenHistoryRequested;
    public event EventHandler? ExitRequested;

    public void UpdateState(TrayShellState state)
    {
        ObjectDisposedException.ThrowIf(disposed, this);

        toggleRecordingItem.Text = state.ToggleRecordingLabel;
        toggleRecordingItem.Enabled = state.CanToggleRecording;
        quickAddDictionaryItem.Enabled = state.CanQuickAddDictionary;
        historyItem.Enabled = state.CanOpenHistory;
        notifyIcon.Text = Truncate($"VoiceInk - {state.StatusText}", MaxTooltipLength);
    }

    public void Dispose()
    {
        if (disposed)
        {
            return;
        }

        notifyIcon.DoubleClick -= NotifyIcon_DoubleClick;
        notifyIcon.Visible = false;
        notifyIcon.Dispose();
        contextMenu.Dispose();
        icon.Dispose();
        disposed = true;
    }

    private void NotifyIcon_DoubleClick(object? sender, EventArgs e)
    {
        ShowRequested?.Invoke(this, EventArgs.Empty);
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
}
