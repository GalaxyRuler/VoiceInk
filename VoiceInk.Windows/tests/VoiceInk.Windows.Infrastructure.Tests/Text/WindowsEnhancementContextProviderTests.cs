using VoiceInk.Windows.Core.Enhancement;
using VoiceInk.Windows.Native.Text;
using Xunit;

namespace VoiceInk.Windows.Infrastructure.Tests.Text;

public sealed class WindowsEnhancementContextProviderTests
{
    [Fact]
    public async Task GetContextAsync_FallsBackToClipboardSelectedTextWhenUiaSelectionIsEmpty()
    {
        var selectedText = new FakeSelectedTextReader(string.Empty);
        var fallback = new FakeSelectedTextClipboardFallbackReader("fallback selection");
        var provider = new WindowsEnhancementContextProvider(
            new FakeClipboardTextReader(string.Empty),
            selectedText,
            fallback);

        var context = await provider.GetContextAsync(
            new EnhancementContextRequest(IncludeClipboard: false, IncludeSelectedText: true),
            CancellationToken.None);

        Assert.Equal("fallback selection", context.SelectedText);
        Assert.Equal(1, selectedText.CallCount);
        Assert.Equal(1, fallback.CallCount);
    }

    [Fact]
    public async Task GetContextAsync_SkipsClipboardFallbackWhenUiaSelectionExists()
    {
        var fallback = new FakeSelectedTextClipboardFallbackReader("fallback selection");
        var provider = new WindowsEnhancementContextProvider(
            new FakeClipboardTextReader(string.Empty),
            new FakeSelectedTextReader("uia selection"),
            fallback);

        var context = await provider.GetContextAsync(
            new EnhancementContextRequest(IncludeClipboard: false, IncludeSelectedText: true),
            CancellationToken.None);

        Assert.Equal("uia selection", context.SelectedText);
        Assert.Equal(0, fallback.CallCount);
    }

    [Fact]
    public async Task GetContextAsync_ReadsClipboardContextAfterSelectedTextFallbackCompletes()
    {
        var calls = new List<string>();
        var provider = new WindowsEnhancementContextProvider(
            new FakeClipboardTextReader("original clipboard", calls),
            new FakeSelectedTextReader(string.Empty, calls),
            new FakeSelectedTextClipboardFallbackReader("fallback selection", calls));

        var context = await provider.GetContextAsync(
            new EnhancementContextRequest(IncludeClipboard: true, IncludeSelectedText: true),
            CancellationToken.None);

        Assert.Equal("fallback selection", context.SelectedText);
        Assert.Equal("original clipboard", context.ClipboardText);
        Assert.Equal(["selected", "fallback", "clipboard"], calls);
    }

    private sealed class FakeClipboardTextReader(string text, List<string>? calls = null) : IClipboardTextReader
    {
        public Task<string> GetClipboardTextAsync(CancellationToken cancellationToken)
        {
            calls?.Add("clipboard");
            return Task.FromResult(text);
        }
    }

    private sealed class FakeSelectedTextReader(string text, List<string>? calls = null) : ISelectedTextReader
    {
        public int CallCount { get; private set; }

        public Task<string> GetSelectedTextAsync(CancellationToken cancellationToken)
        {
            CallCount++;
            calls?.Add("selected");
            return Task.FromResult(text);
        }
    }

    private sealed class FakeSelectedTextClipboardFallbackReader(
        string text,
        List<string>? calls = null) : ISelectedTextClipboardFallbackReader
    {
        public int CallCount { get; private set; }

        public Task<string> GetSelectedTextAsync(CancellationToken cancellationToken)
        {
            CallCount++;
            calls?.Add("fallback");
            return Task.FromResult(text);
        }
    }
}
