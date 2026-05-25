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
    private const int RecorderWindowWidth = 384;
    private const int RecorderWindowCollapsedHeight = 104;
    private const int RecorderWindowExpandedHeight = 352;
    private const double MinimumBarHeight = 8;
    private const double MaximumBarHeight = 32;
    private readonly DispatcherQueueTimer pulseTimer;
    private readonly SubclassProc subclassProc;
    private FloatingRecorderControlState? latestControlState;
    private RecorderControlPopover activePopover = RecorderControlPopover.None;
    private bool isShown;
    private bool subclassInstalled;
    private bool suppressPromptEnhancementChanged;
    private bool canUseRecorderControls;
    private int pulseStep;
    private double inputLevel;

    public FloatingRecorderWindow()
    {
        InitializeComponent();
        subclassProc = RecorderSubclassProc;
        ConfigureWindow();
        TryInstallNoActivateSubclass();
        Closed += FloatingRecorderWindow_Closed;

        pulseTimer = DispatcherQueue.CreateTimer();
        pulseTimer.Interval = TimeSpan.FromMilliseconds(180);
        pulseTimer.Tick += (_, _) => AdvancePulse();
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
        StopRecordingButton.IsEnabled = state.CanStop;
        CancelRecordingButton.IsEnabled = state.CanCancel;
        inputLevel = double.IsFinite(state.InputLevel) ? Math.Clamp(state.InputLevel, 0, 1) : 0;
        SetPulseVisible(state.ShowPulse);
        ApplyMeter();

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

        var powerModeLabel = state.PowerModeTitle == "Auto"
            ? "Auto"
            : $"{state.PowerModeEmoji} {state.PowerModeTitle}";
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

        activePopover = popover;
        PromptPopoverPanel.Visibility = popover == RecorderControlPopover.Prompt
            ? Visibility.Visible
            : Visibility.Collapsed;
        PowerModePopoverPanel.Visibility = popover == RecorderControlPopover.PowerMode
            ? Visibility.Visible
            : Visibility.Collapsed;
        AppWindow.Resize(new SizeInt32(
            RecorderWindowWidth,
            popover == RecorderControlPopover.None
                ? RecorderWindowCollapsedHeight
                : RecorderWindowExpandedHeight));
        if (isShown)
        {
            MoveBottomCenter();
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
        AppWindow.Resize(new SizeInt32(RecorderWindowWidth, RecorderWindowCollapsedHeight));
        if (AppWindow.Presenter is OverlappedPresenter presenter)
        {
            presenter.IsAlwaysOnTop = true;
            presenter.IsResizable = false;
            presenter.IsMaximizable = false;
            presenter.IsMinimizable = false;
            presenter.SetBorderAndTitleBar(false, false);
        }

        MoveBottomCenter();
    }

    private void MoveBottomCenter()
    {
        var windowId = Win32Interop.GetWindowIdFromWindow(WindowNative.GetWindowHandle(this));
        var displayArea = DisplayArea.GetFromWindowId(windowId, DisplayAreaFallback.Primary);
        var workArea = displayArea.WorkArea;
        var x = workArea.X + (workArea.Width - AppWindow.Size.Width) / 2;
        var y = workArea.Y + workArea.Height - AppWindow.Size.Height - 24;
        AppWindow.Move(new PointInt32(x, y));
    }

    private void ShowWindow()
    {
        if (!isShown)
        {
            MoveBottomCenter();
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
        PulseBar1.Visibility = visibility;
        PulseBar2.Visibility = visibility;
        PulseBar3.Visibility = visibility;
        PulseBar4.Visibility = visibility;
        PulseBar5.Visibility = visibility;

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
        if (inputLevel > 0.01)
        {
            ApplyLevelBar(PulseBar1, 0.65);
            ApplyLevelBar(PulseBar2, 0.9);
            ApplyLevelBar(PulseBar3, 1.0);
            ApplyLevelBar(PulseBar4, 0.85);
            ApplyLevelBar(PulseBar5, 0.6);
            return;
        }

        ApplyPulse(PulseBar1, 10 + ((pulseStep + 0) % 3) * 7, 0.45 + ((pulseStep + 0) % 3) * 0.2);
        ApplyPulse(PulseBar2, 10 + ((pulseStep + 1) % 3) * 7, 0.45 + ((pulseStep + 1) % 3) * 0.2);
        ApplyPulse(PulseBar3, 10 + ((pulseStep + 2) % 3) * 7, 0.45 + ((pulseStep + 2) % 3) * 0.2);
        ApplyPulse(PulseBar4, 10 + ((pulseStep + 1) % 3) * 7, 0.45 + ((pulseStep + 1) % 3) * 0.2);
        ApplyPulse(PulseBar5, 10 + ((pulseStep + 0) % 3) * 7, 0.45 + ((pulseStep + 0) % 3) * 0.2);
    }

    private void ApplyLevelBar(FrameworkElement bar, double weight)
    {
        var normalized = Math.Clamp(inputLevel * weight, 0, 1);
        var height = MinimumBarHeight + normalized * (MaximumBarHeight - MinimumBarHeight);
        ApplyPulse(bar, height, 0.55 + normalized * 0.45);
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

        SetActivePopover(activePopover == RecorderControlPopover.PowerMode
            ? RecorderControlPopover.None
            : RecorderControlPopover.PowerMode);
    }

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
