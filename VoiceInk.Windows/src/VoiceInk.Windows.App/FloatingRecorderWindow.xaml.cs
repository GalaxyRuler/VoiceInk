using Microsoft.UI;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Automation;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System.Runtime.InteropServices;
using VoiceInk.Windows.Core.Recorder;
using Windows.Graphics;
using WinRT.Interop;

namespace VoiceInk.Windows.App;

public sealed partial class FloatingRecorderWindow : Window
{
    private const uint WindowMessageMouseActivate = 0x0021;
    private const int MouseActivateNoActivate = 3;
    private const nuint NoActivateSubclassId = 1;
    private const int MiniRecorderWindowWidth = 384;
    private const int MiniRecorderCollapsedHeight = 104;
    private const int MiniRecorderLiveTranscriptHeight = 176;
    private const int MiniRecorderExpandedHeight = 352;
    private const int MiniRecorderExpandedWithLiveTranscriptHeight = 424;
    private const int NotchRecorderWindowWidth = 360;
    private const int NotchRecorderLiveTranscriptWindowWidth = 400;
    private const int NotchRecorderCollapsedHeight = 88;
    private const int NotchRecorderLiveTranscriptHeight = 160;
    private const int NotchRecorderExpandedHeight = 328;
    private const int NotchRecorderExpandedWithLiveTranscriptHeight = 400;
    private static readonly TimeSpan PopoverDismissalDelay = TimeSpan.FromMilliseconds(250);
    private readonly DispatcherQueueTimer pulseTimer;
    private readonly DispatcherQueueTimer popoverDismissalTimer;
    private readonly List<Border> waveformBars = [];
    private readonly FloatingRecorderPopoverHoverController promptPopoverHover = new();
    private readonly FloatingRecorderPopoverHoverController powerModePopoverHover = new();
    private readonly SubclassProc subclassProc;
    private FloatingRecorderControlState? latestControlState;
    private RecorderControlPopover activePopover = RecorderControlPopover.None;
    private bool isShown;
    private bool subclassInstalled;
    private bool suppressPromptEnhancementChanged;
    private bool canUseRecorderControls;
    private bool hasLiveTranscript;
    private string recorderStyle = RecorderStyleSettings.Mini;
    private RecorderControlPopover pendingDismissalPopover = RecorderControlPopover.None;
    private int pulseStep;
    private double inputLevel;

    public FloatingRecorderWindow()
    {
        InitializeComponent();
        InitializeWaveformBars();
        subclassProc = RecorderSubclassProc;
        ConfigureWindow();
        TryInstallNoActivateSubclass();
        Closed += FloatingRecorderWindow_Closed;

        pulseTimer = DispatcherQueue.CreateTimer();
        pulseTimer.Interval = TimeSpan.FromMilliseconds(180);
        pulseTimer.Tick += (_, _) => AdvancePulse();

        popoverDismissalTimer = DispatcherQueue.CreateTimer();
        popoverDismissalTimer.Interval = PopoverDismissalDelay;
        popoverDismissalTimer.IsRepeating = false;
        popoverDismissalTimer.Tick += (_, _) => DismissalTimer_Tick();
    }

    public Func<Task>? StopRequested { get; set; }

    public Func<Task>? CancelRequested { get; set; }

    public Func<bool, Task>? PromptEnhancementToggled { get; set; }

    public Func<Guid, Task>? PromptChoiceRequested { get; set; }

    public Func<Guid?, Task>? PowerModeChoiceRequested { get; set; }

    public void Apply(FloatingRecorderViewState state)
    {
        TitleTextBlock.Text = state.Title;
        DetailTextBlock.Text = state.Detail;
        ElapsedTextBlock.Text = state.Elapsed;
        ShortcutHintTextBlock.Text = state.FooterHint;
        StopRecordingButton.IsEnabled = state.CanStop;
        CancelRecordingButton.IsEnabled = state.CanCancel;
        var showLiveTranscript = state.HasLiveTranscript;
        hasLiveTranscript = showLiveTranscript;
        recorderStyle = RecorderStyleSettings.Normalize(state.RecorderStyle);
        LiveTranscriptPanel.Visibility = showLiveTranscript ? Visibility.Visible : Visibility.Collapsed;
        LiveTranscriptTextBlock.Text = showLiveTranscript ? state.LiveTranscript : string.Empty;
        LiveTranscriptDetailTextBlock.Text = showLiveTranscript ? state.LiveTranscriptDetail : string.Empty;
        inputLevel = double.IsFinite(state.InputLevel) ? Math.Clamp(state.InputLevel, 0, 1) : 0;
        SetPulseVisible(state.ShowPulse);
        ApplyMeter();
        ResizeForCurrentContent();

        if (state.IsVisible)
        {
            ShowWindow();
            return;
        }

        SetActivePopover(RecorderControlPopover.None);
        HideWindow();
    }

    public void ApplyControls(FloatingRecorderControlState state, bool canUseControls)
    {
        latestControlState = state;
        canUseRecorderControls = canUseControls;

        PromptButtonTextBlock.Text = state.IsEnhancementEnabled ? state.PromptTitle : "Prompt";
        PromptButton.IsEnabled = canUseControls && state.CanOpenPromptControls;
        ToolTipService.SetToolTip(
            PromptButton,
            state.IsEnhancementEnabled
                ? $"Prompt: {state.PromptTitle}"
                : "Prompt chooser");

        suppressPromptEnhancementChanged = true;
        PromptEnhancementCheckBox.Content = state.PromptHeaderTitle;
        PromptEnhancementCheckBox.IsEnabled = canUseControls && state.CanToggleEnhancement;
        PromptEnhancementCheckBox.IsChecked = state.IsEnhancementEnabled;
        suppressPromptEnhancementChanged = false;
        RenderPromptChoices(state, canUseControls);

        var powerModeLabel = state.PowerModeButtonLabel;
        PowerModeButtonTextBlock.Text = powerModeLabel;
        PowerModeButton.IsEnabled = canUseControls;
        ToolTipService.SetToolTip(
            PowerModeButton,
            state.CanOpenPowerModeControls
                ? $"Power Mode: {powerModeLabel}"
                : "No Power Modes available");

        PowerModePopoverHeaderTextBlock.Text = state.PowerModeHeaderTitle;
        RenderPowerModeChoices(state, canUseControls);

        if (!canUseControls)
        {
            SetActivePopover(RecorderControlPopover.None);
            CancelPendingPopoverDismissal();
        }
    }

    private void RenderPromptChoices(FloatingRecorderControlState state, bool canUseControls)
    {
        PromptChoicesStackPanel.Children.Clear();
        foreach (var choice in state.PromptChoices)
        {
            PromptChoicesStackPanel.Children.Add(CreateChoiceButton(
                choice.Title,
                prefix: null,
                choice.IsSelected,
                choice.IsDisabled,
                choice.IsDisabled ? "Selecting this prompt will enable AI enhancement" : null,
                canUseControls,
                async () => await RequestPromptChoiceAsync(choice.Id)));
        }
    }

    private void RenderPowerModeChoices(FloatingRecorderControlState state, bool canUseControls)
    {
        PowerModeChoicesStackPanel.Children.Clear();
        if (!state.CanOpenPowerModeControls)
        {
            PowerModeChoicesStackPanel.Children.Add(new TextBlock
            {
                Text = state.PowerModeEmptyTitle,
                Foreground = Brush(204, 255, 255, 255),
                FontSize = 13,
                TextAlignment = TextAlignment.Center,
                TextTrimming = TextTrimming.CharacterEllipsis,
                Margin = new Thickness(0, 16, 0, 16)
            });
            return;
        }

        foreach (var choice in state.PowerModeChoices)
        {
            PowerModeChoicesStackPanel.Children.Add(CreateChoiceButton(
                choice.Title,
                choice.Id is null ? null : choice.Emoji,
                choice.IsSelected,
                isDimmed: false,
                semanticHint: choice.Id is null ? "Automatic Power Mode selection" : null,
                canUseControls,
                async () => await RequestPowerModeChoiceAsync(choice.Id)));
        }
    }

    private static Button CreateChoiceButton(
        string title,
        string? prefix,
        bool isSelected,
        bool isDimmed,
        string? semanticHint,
        bool isEnabled,
        Func<Task> action)
    {
        var grid = new Grid
        {
            ColumnSpacing = 8
        };
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

        var label = string.IsNullOrWhiteSpace(prefix) ? title : $"{prefix} {title}";
        grid.Children.Add(new TextBlock
        {
            Text = label,
            Foreground = isDimmed ? Brush(102, 255, 255, 255) : Brush(230, 255, 255, 255),
            FontSize = 13,
            TextTrimming = TextTrimming.CharacterEllipsis,
            VerticalAlignment = VerticalAlignment.Center
        });

        if (isSelected)
        {
            var check = new SymbolIcon(Symbol.Accept)
            {
                Foreground = Brush(255, 124, 219, 138),
                Width = 14,
                Height = 14
            };
            Grid.SetColumn(check, 1);
            grid.Children.Add(check);
        }

        var button = new Button
        {
            HorizontalAlignment = HorizontalAlignment.Stretch,
            HorizontalContentAlignment = HorizontalAlignment.Stretch,
            Padding = new Thickness(8, 4, 8, 4),
            IsEnabled = isEnabled,
            BorderThickness = new Thickness(0),
            Background = isSelected ? Brush(26, 255, 255, 255) : Brush(0, 255, 255, 255),
            Content = grid
        };
        AutomationProperties.SetName(
            button,
            string.Join(
                ", ",
                new[] { label, isSelected ? "selected" : null, semanticHint }
                    .Where(value => !string.IsNullOrWhiteSpace(value))));
        ToolTipService.SetToolTip(button, semanticHint ?? label);
        button.Click += async (_, _) => await action();
        return button;
    }

    private void SetActivePopover(RecorderControlPopover popover)
    {
        if (activePopover == popover)
        {
            return;
        }

        CancelPendingPopoverDismissal();
        activePopover = popover;
        if (popover == RecorderControlPopover.None)
        {
            promptPopoverHover.Reset();
            powerModePopoverHover.Reset();
        }
        else if (popover == RecorderControlPopover.Prompt)
        {
            powerModePopoverHover.Reset();
        }
        else
        {
            promptPopoverHover.Reset();
        }

        PromptPopoverPanel.Visibility = popover == RecorderControlPopover.Prompt
            ? Visibility.Visible
            : Visibility.Collapsed;
        PowerModePopoverPanel.Visibility = popover == RecorderControlPopover.PowerMode
            ? Visibility.Visible
            : Visibility.Collapsed;
        ResizeForCurrentContent();
    }

    private void ResizeForCurrentContent()
    {
        var width = RecorderWindowWidthForCurrentContent();
        var height = RecorderWindowHeightForCurrentContent();
        ApplyRecorderStyle(width);
        AppWindow.Resize(new SizeInt32(width, height));
        if (isShown)
        {
            MoveForCurrentRecorderStyle();
        }
    }

    private int RecorderWindowWidthForCurrentContent() =>
        IsNotchRecorder
            ? hasLiveTranscript
                ? NotchRecorderLiveTranscriptWindowWidth
                : NotchRecorderWindowWidth
            : MiniRecorderWindowWidth;

    private int RecorderWindowHeightForCurrentContent()
    {
        if (!IsNotchRecorder)
        {
            return activePopover == RecorderControlPopover.None
                ? hasLiveTranscript
                    ? MiniRecorderLiveTranscriptHeight
                    : MiniRecorderCollapsedHeight
                : hasLiveTranscript
                    ? MiniRecorderExpandedWithLiveTranscriptHeight
                    : MiniRecorderExpandedHeight;
        }

        return activePopover == RecorderControlPopover.None
            ? hasLiveTranscript
                ? NotchRecorderLiveTranscriptHeight
                : NotchRecorderCollapsedHeight
            : hasLiveTranscript
                ? NotchRecorderExpandedWithLiveTranscriptHeight
                : NotchRecorderExpandedHeight;
    }

    private bool IsNotchRecorder =>
        RecorderStyleSettings.Normalize(recorderStyle) == RecorderStyleSettings.Notch;

    private void ApplyRecorderStyle(int width)
    {
        RecorderChrome.Width = width;
        LiveTranscriptPanel.Width = width;
        Grid.SetRow(RecorderChrome, IsNotchRecorder ? 0 : 2);
        Grid.SetRow(PromptPopoverPanel, IsNotchRecorder ? 2 : 0);
        Grid.SetRow(PowerModePopoverPanel, IsNotchRecorder ? 2 : 0);
        Grid.SetRow(LiveTranscriptPanel, 1);

        if (IsNotchRecorder)
        {
            RecorderChrome.Height = NotchRecorderCollapsedHeight;
            RecorderChrome.Padding = new Thickness(12, 10, 12, 10);
            RecorderChrome.CornerRadius = new CornerRadius(0, 0, 20, 20);
            RecorderChrome.BorderThickness = new Thickness(0, 0, 0, 1);
            LiveTranscriptPanel.CornerRadius = new CornerRadius(0, 0, 14, 14);
            PromptPopoverPanel.VerticalAlignment = VerticalAlignment.Top;
            PowerModePopoverPanel.VerticalAlignment = VerticalAlignment.Top;
            DetailTextBlock.Visibility = Visibility.Collapsed;
            ShortcutHintTextBlock.Visibility = Visibility.Collapsed;
            TitleTextBlock.FontSize = 13;
            return;
        }

        RecorderChrome.Height = MiniRecorderCollapsedHeight;
        RecorderChrome.Padding = new Thickness(14);
        RecorderChrome.CornerRadius = new CornerRadius(18);
        RecorderChrome.BorderThickness = new Thickness(1);
        LiveTranscriptPanel.CornerRadius = new CornerRadius(14);
        PromptPopoverPanel.VerticalAlignment = VerticalAlignment.Bottom;
        PowerModePopoverPanel.VerticalAlignment = VerticalAlignment.Bottom;
        DetailTextBlock.Visibility = Visibility.Visible;
        ShortcutHintTextBlock.Visibility = Visibility.Visible;
        TitleTextBlock.FontSize = 16;
    }

    private void ApplyPromptHoverAction(FloatingRecorderPopoverHoverAction action) =>
        ApplyHoverAction(RecorderControlPopover.Prompt, action);

    private void ApplyPowerModeHoverAction(FloatingRecorderPopoverHoverAction action) =>
        ApplyHoverAction(RecorderControlPopover.PowerMode, action);

    private void ApplyHoverAction(
        RecorderControlPopover popover,
        FloatingRecorderPopoverHoverAction action)
    {
        switch (action)
        {
            case FloatingRecorderPopoverHoverAction.Open:
                if (CanOpenPopover(popover))
                {
                    CancelPendingPopoverDismissal();
                    SetActivePopover(popover);
                }

                break;
            case FloatingRecorderPopoverHoverAction.ScheduleDismissal:
                if (activePopover == popover)
                {
                    SchedulePopoverDismissal(popover);
                }

                break;
            case FloatingRecorderPopoverHoverAction.CancelDismissal:
                CancelPendingPopoverDismissal();
                break;
            case FloatingRecorderPopoverHoverAction.Close:
                if (activePopover == popover)
                {
                    SetActivePopover(RecorderControlPopover.None);
                }

                break;
            case FloatingRecorderPopoverHoverAction.None:
            default:
                break;
        }
    }

    private bool CanOpenPopover(RecorderControlPopover popover) =>
        canUseRecorderControls
        && latestControlState is not null
        && (popover != RecorderControlPopover.Prompt || latestControlState.CanOpenPromptControls);

    private void SchedulePopoverDismissal(RecorderControlPopover popover)
    {
        pendingDismissalPopover = popover;
        popoverDismissalTimer.Stop();
        popoverDismissalTimer.Start();
    }

    private void CancelPendingPopoverDismissal()
    {
        pendingDismissalPopover = RecorderControlPopover.None;
        if (popoverDismissalTimer.IsRunning)
        {
            popoverDismissalTimer.Stop();
        }
    }

    private void DismissalTimer_Tick()
    {
        var popover = pendingDismissalPopover;
        pendingDismissalPopover = RecorderControlPopover.None;
        switch (popover)
        {
            case RecorderControlPopover.Prompt:
                ApplyPromptHoverAction(promptPopoverHover.DismissalTimerElapsed());
                break;
            case RecorderControlPopover.PowerMode:
                ApplyPowerModeHoverAction(powerModePopoverHover.DismissalTimerElapsed());
                break;
        }
    }

    private async Task RequestPromptChoiceAsync(Guid promptId)
    {
        if (!canUseRecorderControls || PromptChoiceRequested is null)
        {
            return;
        }

        await PromptChoiceRequested(promptId);
        SetActivePopover(RecorderControlPopover.None);
    }

    private async Task RequestPowerModeChoiceAsync(Guid? ruleId)
    {
        if (!canUseRecorderControls || PowerModeChoiceRequested is null)
        {
            return;
        }

        await PowerModeChoiceRequested(ruleId);
        SetActivePopover(RecorderControlPopover.None);
    }

    private static SolidColorBrush Brush(byte alpha, byte red, byte green, byte blue) =>
        new(ColorHelper.FromArgb(alpha, red, green, blue));

    private void ConfigureWindow()
    {
        ResizeForCurrentContent();
        if (AppWindow.Presenter is OverlappedPresenter presenter)
        {
            presenter.IsAlwaysOnTop = true;
            presenter.IsResizable = false;
            presenter.IsMaximizable = false;
            presenter.IsMinimizable = false;
            presenter.SetBorderAndTitleBar(false, false);
        }

        MoveForCurrentRecorderStyle();
    }

    private void MoveForCurrentRecorderStyle()
    {
        if (IsNotchRecorder)
        {
            MoveTopCenter();
            return;
        }

        MoveBottomCenter();
    }

    private DisplayArea CurrentDisplayArea()
    {
        var windowId = Win32Interop.GetWindowIdFromWindow(WindowNative.GetWindowHandle(this));
        return DisplayArea.GetFromWindowId(windowId, DisplayAreaFallback.Primary);
    }

    private void MoveBottomCenter()
    {
        var displayArea = CurrentDisplayArea();
        var workArea = displayArea.WorkArea;
        var x = workArea.X + Math.Max(0, (workArea.Width - AppWindow.Size.Width) / 2);
        var y = workArea.Y + workArea.Height - AppWindow.Size.Height - 24;
        AppWindow.Move(new PointInt32(x, y));
    }

    private void MoveTopCenter()
    {
        var displayArea = CurrentDisplayArea();
        var workArea = displayArea.WorkArea;
        var x = workArea.X + Math.Max(0, (workArea.Width - AppWindow.Size.Width) / 2);
        AppWindow.Move(new PointInt32(x, workArea.Y));
    }

    private void ShowWindow()
    {
        if (!isShown)
        {
            MoveForCurrentRecorderStyle();
            AppWindow.Show(activateWindow: false);
            isShown = true;
        }
    }

    private void HideWindow()
    {
        if (!isShown)
        {
            return;
        }

        AppWindow.Hide();
        isShown = false;
    }

    private void SetPulseVisible(bool isVisible)
    {
        var visibility = isVisible ? Visibility.Visible : Visibility.Collapsed;
        foreach (var bar in waveformBars)
        {
            bar.Visibility = visibility;
        }

        if (isVisible && !pulseTimer.IsRunning)
        {
            pulseTimer.Start();
        }
        else if (!isVisible && pulseTimer.IsRunning)
        {
            pulseTimer.Stop();
        }
    }

    private void AdvancePulse()
    {
        pulseStep = (pulseStep + 1) % 6;
        ApplyMeter();
    }

    private void ApplyMeter()
    {
        var bars = FloatingRecorderWaveformPresenter.Present(inputLevel, pulseStep);
        for (var index = 0; index < waveformBars.Count && index < bars.Count; index++)
        {
            ApplyPulse(waveformBars[index], bars[index].Height, bars[index].Opacity);
        }
    }

    private void InitializeWaveformBars()
    {
        var brushes = new[]
        {
            Brush(255, 76, 194, 255),
            Brush(255, 124, 219, 138),
            Brush(255, 255, 209, 102),
            Brush(255, 255, 122, 144),
            Brush(255, 185, 167, 255)
        };
        for (var index = 0; index < FloatingRecorderWaveformPresenter.BarCount; index++)
        {
            var bar = new Border
            {
                Width = 3,
                Height = FloatingRecorderWaveformPresenter.MinimumHeight,
                CornerRadius = new CornerRadius(1.5),
                Background = brushes[index % brushes.Length],
                VerticalAlignment = VerticalAlignment.Center
            };
            waveformBars.Add(bar);
            WaveformBarsStackPanel.Children.Add(bar);
        }
    }

    private static void ApplyPulse(FrameworkElement bar, double height, double opacity)
    {
        if (bar.Visibility != Visibility.Visible)
        {
            return;
        }

        bar.Height = height;
        bar.Opacity = opacity;
    }

    private async void StopRecordingButton_Click(object sender, RoutedEventArgs e)
    {
        if (StopRequested is null)
        {
            return;
        }

        await StopRequested();
    }

    private async void CancelRecordingButton_Click(object sender, RoutedEventArgs e)
    {
        if (CancelRequested is null)
        {
            return;
        }

        await CancelRequested();
    }

    private void PromptButton_Click(object sender, RoutedEventArgs e)
    {
        if (!canUseRecorderControls || latestControlState?.CanOpenPromptControls != true)
        {
            return;
        }

        promptPopoverHover.Reset();
        SetActivePopover(activePopover == RecorderControlPopover.Prompt
            ? RecorderControlPopover.None
            : RecorderControlPopover.Prompt);
    }

    private void PowerModeButton_Click(object sender, RoutedEventArgs e)
    {
        if (!canUseRecorderControls)
        {
            return;
        }

        powerModePopoverHover.Reset();
        SetActivePopover(activePopover == RecorderControlPopover.PowerMode
            ? RecorderControlPopover.None
            : RecorderControlPopover.PowerMode);
    }

    private void PromptButton_PointerEntered(object sender, RoutedEventArgs e) =>
        ApplyPromptHoverAction(promptPopoverHover.ButtonEntered());

    private void PromptButton_PointerExited(object sender, RoutedEventArgs e) =>
        ApplyPromptHoverAction(promptPopoverHover.ButtonExited());

    private void PromptPopoverPanel_PointerEntered(object sender, RoutedEventArgs e) =>
        ApplyPromptHoverAction(promptPopoverHover.PanelEntered());

    private void PromptPopoverPanel_PointerExited(object sender, RoutedEventArgs e) =>
        ApplyPromptHoverAction(promptPopoverHover.PanelExited());

    private void PowerModeButton_PointerEntered(object sender, RoutedEventArgs e) =>
        ApplyPowerModeHoverAction(powerModePopoverHover.ButtonEntered());

    private void PowerModeButton_PointerExited(object sender, RoutedEventArgs e) =>
        ApplyPowerModeHoverAction(powerModePopoverHover.ButtonExited());

    private void PowerModePopoverPanel_PointerEntered(object sender, RoutedEventArgs e) =>
        ApplyPowerModeHoverAction(powerModePopoverHover.PanelEntered());

    private void PowerModePopoverPanel_PointerExited(object sender, RoutedEventArgs e) =>
        ApplyPowerModeHoverAction(powerModePopoverHover.PanelExited());

    private async void PromptEnhancementCheckBox_Changed(object sender, RoutedEventArgs e)
    {
        if (suppressPromptEnhancementChanged
            || !canUseRecorderControls
            || PromptEnhancementToggled is null)
        {
            return;
        }

        await PromptEnhancementToggled(PromptEnhancementCheckBox.IsChecked == true);
    }

    private void TryInstallNoActivateSubclass()
    {
        var hwnd = WindowNative.GetWindowHandle(this);
        if (hwnd == IntPtr.Zero)
        {
            return;
        }

        subclassInstalled = SetWindowSubclass(hwnd, subclassProc, NoActivateSubclassId, IntPtr.Zero);
    }

    private void FloatingRecorderWindow_Closed(object sender, WindowEventArgs args)
    {
        pulseTimer.Stop();
        popoverDismissalTimer.Stop();
        StopRequested = null;
        CancelRequested = null;
        PromptEnhancementToggled = null;
        PromptChoiceRequested = null;
        PowerModeChoiceRequested = null;

        var hwnd = WindowNative.GetWindowHandle(this);
        if (subclassInstalled && hwnd != IntPtr.Zero)
        {
            RemoveWindowSubclass(hwnd, subclassProc, NoActivateSubclassId);
            subclassInstalled = false;
        }
    }

    private static IntPtr RecorderSubclassProc(
        IntPtr hwnd,
        uint message,
        IntPtr wParam,
        IntPtr lParam,
        nuint subclassId,
        IntPtr referenceData)
    {
        return message == WindowMessageMouseActivate
            ? new IntPtr(MouseActivateNoActivate)
            : DefSubclassProc(hwnd, message, wParam, lParam);
    }

    private delegate IntPtr SubclassProc(
        IntPtr hwnd,
        uint message,
        IntPtr wParam,
        IntPtr lParam,
        nuint subclassId,
        IntPtr referenceData);

    private enum RecorderControlPopover
    {
        None,
        Prompt,
        PowerMode
    }

    [DllImport("comctl32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool SetWindowSubclass(
        IntPtr hwnd,
        SubclassProc subclassProc,
        nuint subclassId,
        IntPtr referenceData);

    [DllImport("comctl32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool RemoveWindowSubclass(
        IntPtr hwnd,
        SubclassProc subclassProc,
        nuint subclassId);

    [DllImport("comctl32.dll")]
    private static extern IntPtr DefSubclassProc(
        IntPtr hwnd,
        uint message,
        IntPtr wParam,
        IntPtr lParam);
}
