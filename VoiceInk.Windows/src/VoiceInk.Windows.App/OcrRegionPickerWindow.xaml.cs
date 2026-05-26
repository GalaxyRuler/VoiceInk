using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using VoiceInk.Windows.Native.Text;
using Windows.Foundation;
using Windows.Graphics;
using Windows.System;

namespace VoiceInk.Windows.App;

public sealed partial class OcrRegionPickerWindow : Window
{
    private readonly ScreenCaptureDisplay? targetDisplay;
    private TaskCompletionSource<ScreenCaptureRegion?>? completion;
    private bool isDragging;
    private bool isCompleted;
    private Point dragStart;

    public OcrRegionPickerWindow(ScreenCaptureDisplay? targetDisplay = null)
    {
        this.targetDisplay = targetDisplay;
        InitializeComponent();
        Closed += OcrRegionPickerWindow_Closed;
    }

    public Task<ScreenCaptureRegion?> PickAsync()
    {
        completion = new TaskCompletionSource<ScreenCaptureRegion?>(
            TaskCreationOptions.RunContinuationsAsynchronously);
        Activate();
        return completion.Task;
    }

    private void PickerRoot_Loaded(object sender, RoutedEventArgs e)
    {
        if (targetDisplay is not null)
        {
            AppWindow.MoveAndResize(new RectInt32(
                targetDisplay.Left,
                targetDisplay.Top,
                targetDisplay.Width,
                targetDisplay.Height));
        }

        AppWindow.SetPresenter(FullScreenPresenter.Create());
        PickerRoot.Focus(FocusState.Programmatic);
    }

    private void PickerRoot_PointerPressed(object sender, PointerRoutedEventArgs e)
    {
        var point = e.GetCurrentPoint(SelectionCanvas);
        dragStart = point.Position;
        isDragging = true;
        SelectionRectangle.Visibility = Visibility.Visible;
        UpdateSelectionRectangle(dragStart, dragStart);
        PickerRoot.CapturePointer(e.Pointer);
    }

    private void PickerRoot_PointerMoved(object sender, PointerRoutedEventArgs e)
    {
        if (!isDragging)
        {
            return;
        }

        UpdateSelectionRectangle(dragStart, e.GetCurrentPoint(SelectionCanvas).Position);
    }

    private void PickerRoot_PointerReleased(object sender, PointerRoutedEventArgs e)
    {
        if (!isDragging)
        {
            return;
        }

        isDragging = false;
        PickerRoot.ReleasePointerCapture(e.Pointer);
        var dragEnd = e.GetCurrentPoint(SelectionCanvas).Position;
        var scale = PickerRoot.XamlRoot?.RasterizationScale ?? 1;
        var origin = AppWindow.Position;
        var region = ScreenCaptureRegionSelection.FromDrag(
            dragStart.X,
            dragStart.Y,
            dragEnd.X,
            dragEnd.Y,
            origin.X,
            origin.Y,
            scale);
        Complete(region);
    }

    private void PickerRoot_KeyDown(object sender, KeyRoutedEventArgs e)
    {
        if (e.Key == VirtualKey.Escape)
        {
            e.Handled = true;
            Complete(null);
        }
    }

    private void OcrRegionPickerWindow_Closed(object sender, WindowEventArgs args)
    {
        if (!isCompleted)
        {
            Complete(null, closeWindow: false);
        }
    }

    private void UpdateSelectionRectangle(Point start, Point end)
    {
        var left = Math.Min(start.X, end.X);
        var top = Math.Min(start.Y, end.Y);
        var width = Math.Abs(end.X - start.X);
        var height = Math.Abs(end.Y - start.Y);

        Canvas.SetLeft(SelectionRectangle, left);
        Canvas.SetTop(SelectionRectangle, top);
        SelectionRectangle.Width = width;
        SelectionRectangle.Height = height;
    }

    private void Complete(ScreenCaptureRegion? region, bool closeWindow = true)
    {
        if (isCompleted)
        {
            return;
        }

        isCompleted = true;
        completion?.TrySetResult(region);
        if (closeWindow)
        {
            Close();
        }
    }
}
