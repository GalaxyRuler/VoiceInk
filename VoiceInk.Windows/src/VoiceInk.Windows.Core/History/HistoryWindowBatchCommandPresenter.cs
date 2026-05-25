namespace VoiceInk.Windows.Core.History;

public static class HistoryWindowBatchCommandPresenter
{
    public static HistoryWindowBatchCommandState Present(int totalCount, int selectedCount)
    {
        var safeTotal = Math.Max(0, totalCount);
        var safeSelected = Math.Clamp(selectedCount, 0, safeTotal);
        var hasRows = safeTotal > 0;
        var hasSelection = safeSelected > 0;

        return new HistoryWindowBatchCommandState(
            CanSelectAll: hasRows,
            CanClearSelection: hasSelection,
            CanExportSelected: hasSelection,
            CanDeleteSelected: hasSelection,
            AllSelected: hasRows && safeSelected == safeTotal,
            SelectionLabel: hasRows
                ? $"{safeSelected.ToString("N0")} selected"
                : "No transcriptions");
    }
}
