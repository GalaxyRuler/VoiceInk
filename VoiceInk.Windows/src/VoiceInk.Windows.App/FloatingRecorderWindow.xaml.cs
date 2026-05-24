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
    private readonly DispatcherQueueTimer pulseTimer;
    private bool isShown;
    private int pulseStep;

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
        SetPulseVisible(state.ShowPulse);

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
        ApplyPulse(PulseBar1, 12 + ((pulseStep + 0) % 3) * 8, 0.5 + ((pulseStep + 0) % 3) * 0.2);
        ApplyPulse(PulseBar2, 12 + ((pulseStep + 1) % 3) * 8, 0.5 + ((pulseStep + 1) % 3) * 0.2);
        ApplyPulse(PulseBar3, 12 + ((pulseStep + 2) % 3) * 8, 0.5 + ((pulseStep + 2) % 3) * 0.2);
    }

    private static void ApplyPulse(FrameworkElement bar, double height, double opacity)
    {
        bar.Height = height;
        bar.Opacity = opacity;
    }
}
