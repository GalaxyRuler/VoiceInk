using Microsoft.UI.Dispatching;
using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using VoiceInk.Windows.Core.Recorder;
using Windows.Graphics;
using WinRT.Interop;

namespace VoiceInk.Windows.App;

public sealed partial class FloatingRecorderWindow : Window
{
    private const double MinimumBarHeight = 8;
    private const double MaximumBarHeight = 32;
    private readonly DispatcherQueueTimer pulseTimer;
    private bool isShown;
    private int pulseStep;
    private double inputLevel;

    public FloatingRecorderWindow()
    {
        InitializeComponent();
        ConfigureWindow();

        pulseTimer = DispatcherQueue.CreateTimer();
        pulseTimer.Interval = TimeSpan.FromMilliseconds(180);
        pulseTimer.Tick += (_, _) => AdvancePulse();
    }

    public void Apply(FloatingRecorderViewState state)
    {
        TitleTextBlock.Text = state.Title;
        DetailTextBlock.Text = state.Detail;
        ElapsedTextBlock.Text = state.Elapsed;
        inputLevel = double.IsFinite(state.InputLevel) ? Math.Clamp(state.InputLevel, 0, 1) : 0;
        SetPulseVisible(state.ShowPulse);
        ApplyMeter();

        if (state.IsVisible)
        {
            ShowWindow();
            return;
        }

        HideWindow();
    }

    private void ConfigureWindow()
    {
        AppWindow.Resize(new SizeInt32(320, 104));
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
}
