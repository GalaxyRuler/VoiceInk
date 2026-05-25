namespace VoiceInk.Windows.Core.History;

public sealed record HistoryWindowBatchCommandState(
    bool CanSelectAll,
    bool CanClearSelection,
    bool CanExportSelected,
    bool CanDeleteSelected,
    bool AllSelected,
    string SelectionLabel);
