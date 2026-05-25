using VoiceInk.Windows.Core.History;
using Xunit;

namespace VoiceInk.Windows.Core.Tests.History;

public sealed class HistoryWindowBatchCommandPresenterTests
{
    [Fact]
    public void Present_DisablesActionsWhenThereAreNoRows()
    {
        var state = HistoryWindowBatchCommandPresenter.Present(totalCount: 0, selectedCount: 0);

        Assert.False(state.CanSelectAll);
        Assert.False(state.CanClearSelection);
        Assert.False(state.CanExportSelected);
        Assert.False(state.CanDeleteSelected);
        Assert.False(state.AllSelected);
        Assert.Equal("No transcriptions", state.SelectionLabel);
    }

    [Fact]
    public void Present_EnablesSelectAllWhenRowsExistButNothingIsSelected()
    {
        var state = HistoryWindowBatchCommandPresenter.Present(totalCount: 3, selectedCount: 0);

        Assert.True(state.CanSelectAll);
        Assert.False(state.CanClearSelection);
        Assert.False(state.CanExportSelected);
        Assert.False(state.CanDeleteSelected);
        Assert.False(state.AllSelected);
        Assert.Equal("0 selected", state.SelectionLabel);
    }

    [Fact]
    public void Present_EnablesBatchActionsForPartialSelection()
    {
        var state = HistoryWindowBatchCommandPresenter.Present(totalCount: 5, selectedCount: 2);

        Assert.True(state.CanSelectAll);
        Assert.True(state.CanClearSelection);
        Assert.True(state.CanExportSelected);
        Assert.True(state.CanDeleteSelected);
        Assert.False(state.AllSelected);
        Assert.Equal("2 selected", state.SelectionLabel);
    }

    [Fact]
    public void Present_ReportsAllSelected()
    {
        var state = HistoryWindowBatchCommandPresenter.Present(totalCount: 4, selectedCount: 4);

        Assert.True(state.CanSelectAll);
        Assert.True(state.CanClearSelection);
        Assert.True(state.CanExportSelected);
        Assert.True(state.CanDeleteSelected);
        Assert.True(state.AllSelected);
        Assert.Equal("4 selected", state.SelectionLabel);
    }
}
