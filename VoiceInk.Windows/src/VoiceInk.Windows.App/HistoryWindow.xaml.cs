using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using VoiceInk.Windows.Core.History;
using VoiceInk.Windows.Core.Services;
using VoiceInk.Windows.Native.Text;
using Windows.Graphics;
using Windows.Media.Core;
using Windows.Storage;
using Windows.Storage.Pickers;
using WinRT.Interop;

namespace VoiceInk.Windows.App;

public sealed partial class HistoryWindow : Window
{
    private const int HistoryPageSize = 100;
    private const int WaveformPeakCount = 64;
    private readonly IHistoryStore historyStore;
    private readonly HistoryRetryService historyRetryService;
    private readonly HistoryReenhancementService historyReenhancementService;
    private readonly ClipboardTextInjectionService textInjectionService;
    private readonly string recordingsDirectory;
    private readonly CancellationTokenSource lifetime = new();
    private IReadOnlyList<TranscriptionHistoryItem> historyItems = [];
    private IReadOnlyList<HistoryWindowListRow> historyRows = [];
    private HistoryPageCursor? nextHistoryCursor;
    private string currentHistoryQuery = string.Empty;
    private bool hasMoreHistory;
    private bool isBusy;

    public HistoryWindow(
        IHistoryStore historyStore,
        HistoryRetryService historyRetryService,
        HistoryReenhancementService historyReenhancementService,
        ClipboardTextInjectionService textInjectionService,
        string recordingsDirectory)
    {
        this.historyStore = historyStore;
        this.historyRetryService = historyRetryService;
        this.historyReenhancementService = historyReenhancementService;
        this.textInjectionService = textInjectionService;
        this.recordingsDirectory = recordingsDirectory;

        InitializeComponent();
        Title = "VoiceInk - Transcription History";
        PlaybackRateComboBox.ItemsSource = HistoryPlaybackRatePresenter.Choices;
        PlaybackRateComboBox.SelectedIndex = HistoryPlaybackRatePresenter.SelectedIndexFor(
            HistoryPlaybackRatePresenter.DefaultChoice.Value);
        AppWindow.Resize(new SizeInt32(1250, 750));
        Closed += HistoryWindow_Closed;
    }

    private async void RootGrid_Loaded(object sender, RoutedEventArgs e)
    {
        await RefreshHistoryAsync();
        SearchTextBox.Focus(Microsoft.UI.Xaml.FocusState.Programmatic);
    }

    private void HistoryWindow_Closed(object sender, WindowEventArgs args)
    {
        lifetime.Cancel();
        lifetime.Dispose();
    }

    private async void SearchButton_Click(object sender, RoutedEventArgs e)
    {
        await RefreshHistoryAsync();
    }

    private async void ClearSearchButton_Click(object sender, RoutedEventArgs e)
    {
        SearchTextBox.Text = string.Empty;
        await RefreshHistoryAsync();
    }

    private async void RefreshButton_Click(object sender, RoutedEventArgs e)
    {
        await RefreshHistoryAsync();
    }

    private async void LoadMoreHistoryButton_Click(object sender, RoutedEventArgs e)
    {
        await LoadMoreHistoryAsync();
    }

    private async void ExportButton_Click(object sender, RoutedEventArgs e)
    {
        await ExportHistoryAsync();
    }

    private void SelectAllButton_Click(object sender, RoutedEventArgs e)
    {
        HistoryListView.SelectAll();
        RefreshSelectedHistoryDetails();
    }

    private void ClearSelectionButton_Click(object sender, RoutedEventArgs e)
    {
        HistoryListView.SelectedItems.Clear();
        RefreshSelectedHistoryDetails();
    }

    private async void ExportSelectedButton_Click(object sender, RoutedEventArgs e)
    {
        await ExportSelectedHistoryAsync();
    }

    private async void DeleteSelectedButton_Click(object sender, RoutedEventArgs e)
    {
        await DeleteSelectedHistoryAsync();
    }

    private async void RetryButton_Click(object sender, RoutedEventArgs e)
    {
        await RetrySelectedHistoryAsync();
    }

    private async void ReEnhanceButton_Click(object sender, RoutedEventArgs e)
    {
        await ReenhanceSelectedHistoryAsync();
    }

    private void OpenAudioButton_Click(object sender, RoutedEventArgs e)
    {
        OpenSelectedHistoryAudio();
    }

    private async void DeleteButton_Click(object sender, RoutedEventArgs e)
    {
        await DeleteActiveHistoryAsync();
    }

    private async void CopyOriginalButton_Click(object sender, RoutedEventArgs e)
    {
        await CopySelectedHistoryTextAsync(HistoryCopyTextKind.Original);
    }

    private async void CopyFinalButton_Click(object sender, RoutedEventArgs e)
    {
        await CopySelectedHistoryTextAsync(HistoryCopyTextKind.Final);
    }

    private async void CopyEnhancedButton_Click(object sender, RoutedEventArgs e)
    {
        await CopySelectedHistoryTextAsync(HistoryCopyTextKind.Enhanced);
    }

    private async void CopyAiRequestButton_Click(object sender, RoutedEventArgs e)
    {
        await CopySelectedHistoryTextAsync(HistoryCopyTextKind.AiRequest);
    }

    private void HistoryListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        RefreshSelectedHistoryDetails();
    }

    private void PlaybackRateComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        ApplyPlaybackRate();
    }

    private async Task RefreshHistoryAsync(Guid? selectId = null)
    {
        await LoadHistoryPageAsync(reset: true, selectId);
    }

    private async Task LoadMoreHistoryAsync()
    {
        if (!hasMoreHistory || nextHistoryCursor is null)
        {
            SetStatus("All loaded history is visible");
            return;
        }

        await LoadHistoryPageAsync(reset: false, selectId: null);
    }

    private async Task LoadHistoryPageAsync(bool reset, Guid? selectId)
    {
        var selectedIds = selectId is null
            ? SelectedHistoryIds().ToHashSet()
            : [selectId.Value];

        if (reset)
        {
            currentHistoryQuery = SearchTextBox.Text.Trim();
            nextHistoryCursor = null;
            hasMoreHistory = false;
        }

        SetBusy(true, reset ? "Refreshing history" : "Loading more history");
        try
        {
            var page = await historyStore.ListPageAsync(
                currentHistoryQuery,
                reset ? null : nextHistoryCursor,
                HistoryPageSize,
                lifetime.Token);
            historyItems = reset
                ? page.Items
                : historyItems.Concat(page.Items).ToArray();
            nextHistoryCursor = page.NextCursor;
            hasMoreHistory = page.HasMore;
            historyRows = historyItems
                .Select(item => new HistoryWindowListRow(item.Id, HistoryListItem(item)))
                .ToArray();
            HistoryListView.ItemsSource = historyRows;
            RefreshHistoryListHeader();

            HistoryListView.SelectedItems.Clear();
            foreach (var row in historyRows.Where(row => selectedIds.Contains(row.Id)))
            {
                HistoryListView.SelectedItems.Add(row);
            }

            RefreshSelectedHistoryDetails();
            SetStatus(HistoryPageStatus(reset, page.Items.Count));
        }
        catch (OperationCanceledException) when (lifetime.IsCancellationRequested)
        {
            SetStatus("Closing");
        }
        catch (Exception ex)
        {
            SetStatus($"History refresh failed: {ex.Message}");
        }
        finally
        {
            SetBusy(false);
        }
    }

    private async Task ExportHistoryAsync()
    {
        if (historyItems.Count == 0)
        {
            await RefreshHistoryAsync();
        }

        if (historyItems.Count == 0)
        {
            SetStatus("No history to export");
            return;
        }

        try
        {
            var picker = new FileSavePicker
            {
                SuggestedStartLocation = PickerLocationId.DocumentsLibrary,
                SuggestedFileName = $"VoiceInk-history-{DateTimeOffset.Now:yyyyMMdd-HHmmss}"
            };
            picker.FileTypeChoices.Add("CSV file", [".csv"]);
            InitializeWithWindow.Initialize(picker, WindowNative.GetWindowHandle(this));

            var file = await picker.PickSaveFileAsync();
            if (file is null)
            {
                SetStatus("History export canceled");
                return;
            }

            await FileIO.WriteTextAsync(file, HistoryCsvExporter.Export(historyItems));
            SetStatus($"History exported: {file.Name}");
        }
        catch (OperationCanceledException) when (lifetime.IsCancellationRequested)
        {
            SetStatus("Closing");
        }
        catch (Exception ex)
        {
            SetStatus($"History export failed: {ex.Message}");
        }
    }

    private async Task ExportSelectedHistoryAsync()
    {
        var selected = SelectedHistoryItems();
        if (selected.Count == 0)
        {
            SetStatus("Select transcriptions to export");
            return;
        }

        try
        {
            var picker = new FileSavePicker
            {
                SuggestedStartLocation = PickerLocationId.DocumentsLibrary,
                SuggestedFileName = $"VoiceInk-history-selected-{DateTimeOffset.Now:yyyyMMdd-HHmmss}"
            };
            picker.FileTypeChoices.Add("CSV file", [".csv"]);
            InitializeWithWindow.Initialize(picker, WindowNative.GetWindowHandle(this));

            var file = await picker.PickSaveFileAsync();
            if (file is null)
            {
                SetStatus("Selected history export canceled");
                return;
            }

            await FileIO.WriteTextAsync(file, HistoryCsvExporter.Export(selected));
            SetStatus($"Selected history exported: {file.Name}");
        }
        catch (OperationCanceledException) when (lifetime.IsCancellationRequested)
        {
            SetStatus("Closing");
        }
        catch (Exception ex)
        {
            SetStatus($"Selected history export failed: {ex.Message}");
        }
    }

    private async Task RetrySelectedHistoryAsync()
    {
        var item = SelectedHistoryItem();
        if (item is null)
        {
            SetStatus("Select a transcription to retry");
            return;
        }

        if (!HistoryWindowCommandPresenter.Present(item).CanRetry)
        {
            SetStatus("Audio file not found");
            return;
        }

        SetBusy(true, "Retrying transcription");
        try
        {
            var result = await historyRetryService.RetryAsync(item, lifetime.Token);
            await RefreshHistoryAsync(result.Item?.Id);
            SetStatus(result.Message);
        }
        catch (OperationCanceledException) when (lifetime.IsCancellationRequested)
        {
            SetStatus("Closing");
        }
        catch (Exception ex)
        {
            SetStatus($"Retry failed: {ex.Message}");
        }
        finally
        {
            SetBusy(false);
        }
    }

    private async Task ReenhanceSelectedHistoryAsync()
    {
        var item = SelectedHistoryItem();
        if (item is null)
        {
            SetStatus("Select a transcription to re-enhance");
            return;
        }

        if (!HistoryWindowCommandPresenter.Present(item).CanReenhance)
        {
            SetStatus("Only completed transcriptions can be re-enhanced");
            return;
        }

        SetBusy(true, "Re-enhancing transcription");
        try
        {
            var result = await historyReenhancementService.ReenhanceAsync(item, lifetime.Token);
            await RefreshHistoryAsync(result.Item?.Id);
            SetStatus(result.Message);
        }
        catch (OperationCanceledException) when (lifetime.IsCancellationRequested)
        {
            SetStatus("Closing");
        }
        catch (Exception ex)
        {
            SetStatus($"Re-enhance failed: {ex.Message}");
        }
        finally
        {
            SetBusy(false);
        }
    }

    private async Task CopySelectedHistoryTextAsync(HistoryCopyTextKind kind)
    {
        var item = SelectedHistoryItem();
        if (item is null)
        {
            SetStatus("Select a transcription to copy");
            return;
        }

        var result = HistoryCopyTextSelector.Select(item, kind);
        if (!result.Success)
        {
            SetStatus(result.Message);
            return;
        }

        SetBusy(true, "Copying history text");
        try
        {
            await textInjectionService.CopyAsync(result.Text, lifetime.Token);
            SetStatus(result.Message);
        }
        catch (OperationCanceledException) when (lifetime.IsCancellationRequested)
        {
            SetStatus("Closing");
        }
        catch (Exception ex)
        {
            SetStatus($"Copy failed: {ex.Message}");
        }
        finally
        {
            SetBusy(false);
        }
    }

    private async Task DeleteActiveHistoryAsync()
    {
        var item = SelectedHistoryItem();
        if (item is null)
        {
            SetStatus("Select a transcription to delete");
            return;
        }

        var dialog = new ContentDialog
        {
            XamlRoot = RootGrid.XamlRoot,
            Title = "Delete transcription?",
            Content = "This action cannot be undone.",
            PrimaryButtonText = "Delete",
            SecondaryButtonText = "Cancel",
            DefaultButton = ContentDialogButton.Secondary
        };

        var result = await dialog.ShowAsync();
        if (result != ContentDialogResult.Primary)
        {
            SetStatus("Delete canceled");
            return;
        }

        SetBusy(true, "Deleting transcription");
        try
        {
            ClearAudioPlayer();
            var deleted = await historyStore.DeleteAsync(item.Id, lifetime.Token);
            TryDeleteAudioFile(item);
            await RefreshHistoryAsync();
            SetStatus(deleted ? "Transcription deleted" : "Transcription was already deleted");
        }
        catch (OperationCanceledException) when (lifetime.IsCancellationRequested)
        {
            SetStatus("Closing");
        }
        catch (Exception ex)
        {
            SetStatus($"History delete failed: {ex.Message}");
        }
        finally
        {
            SetBusy(false);
        }
    }

    private async Task DeleteSelectedHistoryAsync()
    {
        var selected = SelectedHistoryItems();
        if (selected.Count == 0)
        {
            SetStatus("Select transcriptions to delete");
            return;
        }

        var dialog = new ContentDialog
        {
            XamlRoot = RootGrid.XamlRoot,
            Title = "Delete selected transcriptions?",
            Content = $"This deletes {selected.Count.ToString("N0", CultureInfo.CurrentCulture)} history item(s). This action cannot be undone.",
            PrimaryButtonText = "Delete",
            SecondaryButtonText = "Cancel",
            DefaultButton = ContentDialogButton.Secondary
        };

        var result = await dialog.ShowAsync();
        if (result != ContentDialogResult.Primary)
        {
            SetStatus("Delete selected canceled");
            return;
        }

        SetBusy(true, "Deleting selected transcriptions");
        try
        {
            ClearAudioPlayer();
            var deletedCount = 0;
            foreach (var item in selected)
            {
                if (await historyStore.DeleteAsync(item.Id, lifetime.Token))
                {
                    deletedCount++;
                }

                TryDeleteAudioFile(item);
            }

            await RefreshHistoryAsync();
            SetStatus($"Deleted {deletedCount.ToString("N0", CultureInfo.CurrentCulture)} transcription(s)");
        }
        catch (OperationCanceledException) when (lifetime.IsCancellationRequested)
        {
            SetStatus("Closing");
        }
        catch (Exception ex)
        {
            SetStatus($"Delete selected failed: {ex.Message}");
        }
        finally
        {
            SetBusy(false);
        }
    }

    private void OpenSelectedHistoryAudio()
    {
        var audioPath = SelectedHistoryAudioPath();
        if (audioPath is null)
        {
            SetStatus("Audio file not found");
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
            SetStatus("Audio file opened");
        }
        catch (Exception ex)
        {
            SetStatus($"Open audio failed: {ex.Message}");
        }
    }

    private void RefreshSelectedHistoryDetails()
    {
        var item = SelectedHistoryItem();
        var state = HistoryWindowCommandPresenter.Present(item);
        var selectedIds = SelectedHistoryIds();
        var batchState = HistoryWindowBatchCommandPresenter.Present(historyRows.Count, selectedIds.Count);

        SelectionTextBlock.Text = state.SelectionStatus;
        BatchSelectionTextBlock.Text = batchState.SelectionLabel;
        SelectAllButton.IsEnabled = !isBusy && batchState.CanSelectAll;
        ClearSelectionButton.IsEnabled = !isBusy && batchState.CanClearSelection;
        ExportSelectedButton.IsEnabled = !isBusy && batchState.CanExportSelected;
        DeleteSelectedButton.IsEnabled = !isBusy && batchState.CanDeleteSelected;
        RetryButton.IsEnabled = !isBusy && state.CanRetry;
        ReEnhanceButton.IsEnabled = !isBusy && state.CanReenhance;
        OpenAudioButton.IsEnabled = !isBusy && state.CanOpenAudio;
        DeleteButton.IsEnabled = !isBusy && state.CanDelete;
        CopyOriginalButton.IsEnabled = !isBusy && state.CanCopyOriginal;
        CopyFinalButton.IsEnabled = !isBusy && state.CanCopyFinal;
        CopyEnhancedButton.IsEnabled = !isBusy && state.CanCopyEnhanced;
        CopyAiRequestButton.IsEnabled = !isBusy && state.CanCopyAiRequest;

        if (item is null)
        {
            OriginalTextBox.Text = string.Empty;
            FinalTextBox.Text = string.Empty;
            EnhancedTextBox.Text = string.Empty;
            MetadataTextBlock.Text = historyItems.Count == 0 ? "No transcriptions" : "No Metadata";
            SystemPromptTextBox.Text = string.Empty;
            UserMessageTextBox.Text = string.Empty;
            ClearAudioPlayer();
            return;
        }

        OriginalTextBox.Text = item.OriginalText;
        FinalTextBox.Text = item.Text;
        EnhancedTextBox.Text = item.EnhancedText ?? string.Empty;
        SystemPromptTextBox.Text = item.AiRequestSystemMessage ?? string.Empty;
        UserMessageTextBox.Text = item.AiRequestUserMessage ?? string.Empty;

        var audioPath = SelectedHistoryAudioPath();
        var audioStatus = string.IsNullOrWhiteSpace(item.AudioFilePath)
            ? "Not recorded"
            : audioPath is null
                ? $"Missing: {item.AudioFilePath}"
                : audioPath;
        MetadataTextBlock.Text = string.Join(
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
        RefreshAudioPlayer(audioPath);
    }

    private TranscriptionHistoryItem? SelectedHistoryItem() =>
        SelectedHistoryItems().FirstOrDefault();

    private IReadOnlyList<TranscriptionHistoryItem> SelectedHistoryItems()
    {
        var selectedIds = SelectedHistoryIds().ToHashSet();
        return selectedIds.Count == 0
            ? []
            : historyItems.Where(item => selectedIds.Contains(item.Id)).ToArray();
    }

    private IReadOnlyList<Guid> SelectedHistoryIds() =>
        HistoryListView.SelectedItems
            .OfType<HistoryWindowListRow>()
            .Select(row => row.Id)
            .ToArray();

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

    private void RefreshAudioPlayer(string? audioPath)
    {
        if (audioPath is null)
        {
            ClearAudioPlayer();
            return;
        }

        AudioPlayer.Source = MediaSource.CreateFromUri(new Uri(audioPath, UriKind.Absolute));
        RefreshWaveform(audioPath);
        PlaybackRateComboBox.SelectedIndex = HistoryPlaybackRatePresenter.SelectedIndexFor(
            HistoryPlaybackRatePresenter.DefaultChoice.Value);
        ApplyPlaybackRate();
        AudioPanel.Visibility = Visibility.Visible;
    }

    private void ClearAudioPlayer()
    {
        AudioPlayer.Source = null;
        WaveformItemsControl.ItemsSource = null;
        WaveformItemsControl.Visibility = Visibility.Collapsed;
        AudioPanel.Visibility = Visibility.Collapsed;
    }

    private void ApplyPlaybackRate()
    {
        var rate = HistoryPlaybackRatePresenter.ChoiceAtOrDefault(PlaybackRateComboBox.SelectedIndex).Value;
        AudioPlayer.MediaPlayer.PlaybackSession.PlaybackRate = rate;
    }

    private void RefreshWaveform(string audioPath)
    {
        try
        {
            var peaks = HistoryWaveformPeakExtractor.ExtractPeaks(
                File.ReadAllBytes(audioPath),
                WaveformPeakCount);
            WaveformItemsControl.ItemsSource = peaks;
            WaveformItemsControl.Visibility = peaks.Count > 0
                ? Visibility.Visible
                : Visibility.Collapsed;
        }
        catch
        {
            WaveformItemsControl.ItemsSource = null;
            WaveformItemsControl.Visibility = Visibility.Collapsed;
        }
    }

    private void SetBusy(bool busy, string? status = null)
    {
        isBusy = busy;
        if (status is not null)
        {
            SetStatus(status);
        }

        SearchButton.IsEnabled = !busy;
        ClearSearchButton.IsEnabled = !busy;
        RefreshButton.IsEnabled = !busy;
        ExportButton.IsEnabled = !busy;
        HistoryListView.IsEnabled = !busy;
        LoadMoreHistoryButton.IsEnabled = !busy && hasMoreHistory;
        LoadMoreHistoryButton.Visibility = hasMoreHistory ? Visibility.Visible : Visibility.Collapsed;
        RefreshSelectedHistoryDetails();
    }

    private void SetStatus(string status)
    {
        StatusTextBlock.Text = status;
    }

    private void RefreshHistoryListHeader()
    {
        var loaded = historyItems.Count.ToString("N0", CultureInfo.CurrentCulture);
        ListHeaderTextBlock.Text = hasMoreHistory
            ? $"History ({loaded} loaded, more available)"
            : $"History ({loaded} loaded)";
    }

    private string HistoryPageStatus(bool reset, int loadedCount)
    {
        if (historyItems.Count == 0)
        {
            return "No transcriptions";
        }

        if (!reset)
        {
            return $"Loaded {loadedCount.ToString("N0", CultureInfo.CurrentCulture)} more transcription(s)";
        }

        return hasMoreHistory
            ? "History refreshed - more transcriptions available"
            : "History refreshed";
    }

    private void TryDeleteAudioFile(TranscriptionHistoryItem item)
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

    private static string Seconds(TimeSpan duration) =>
        duration.TotalSeconds.ToString("0.000", CultureInfo.CurrentCulture);

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

    private sealed record HistoryWindowListRow(Guid Id, string DisplayText)
    {
        public override string ToString() => DisplayText;
    }
}
