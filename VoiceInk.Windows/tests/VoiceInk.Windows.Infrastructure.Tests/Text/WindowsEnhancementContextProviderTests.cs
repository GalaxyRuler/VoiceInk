using VoiceInk.Windows.Core.Enhancement;
using VoiceInk.Windows.Core.PowerMode;
using VoiceInk.Windows.Core.Services;
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

    [Fact]
    public async Task GetContextAsync_IncludesActiveWindowContextWhenRequested()
    {
        var provider = new WindowsEnhancementContextProvider(
            new FakeClipboardTextReader(string.Empty),
            new FakeSelectedTextReader(string.Empty),
            new FakeSelectedTextClipboardFallbackReader(string.Empty),
            new FakePowerModeTargetProvider(new PowerModeTarget("WINWORD", "Quarterly Planning", 10)),
            new FakeBrowserUrlReader(string.Empty));

        var context = await provider.GetContextAsync(
            new EnhancementContextRequest(
                IncludeClipboard: false,
                IncludeSelectedText: false,
                IncludeActiveWindow: true),
            CancellationToken.None);

        Assert.Equal("WINWORD", context.ActiveWindowProcessName);
        Assert.Equal("Quarterly Planning", context.ActiveWindowTitle);
    }

    [Fact]
    public async Task GetContextAsync_IncludesBrowserUrlWhenRequested()
    {
        var browserReader = new FakeBrowserUrlReader("https://example.com/docs?token=secret#part");
        var provider = new WindowsEnhancementContextProvider(
            new FakeClipboardTextReader(string.Empty),
            new FakeSelectedTextReader(string.Empty),
            new FakeSelectedTextClipboardFallbackReader(string.Empty),
            new FakePowerModeTargetProvider(null),
            browserReader);

        var context = await provider.GetContextAsync(
            new EnhancementContextRequest(
                IncludeClipboard: false,
                IncludeSelectedText: false,
                IncludeBrowserUrl: true),
            CancellationToken.None);

        Assert.Equal("https://example.com/docs", context.BrowserUrl);
        Assert.Equal(1, browserReader.CallCount);
    }

    [Fact]
    public async Task GetContextAsync_SkipsBrowserUrlWhenNotRequested()
    {
        var browserReader = new FakeBrowserUrlReader("https://example.com/docs");
        var provider = new WindowsEnhancementContextProvider(
            new FakeClipboardTextReader(string.Empty),
            new FakeSelectedTextReader(string.Empty),
            new FakeSelectedTextClipboardFallbackReader(string.Empty),
            new FakePowerModeTargetProvider(null),
            browserReader);

        var context = await provider.GetContextAsync(
            new EnhancementContextRequest(
                IncludeClipboard: false,
                IncludeSelectedText: false,
                IncludeBrowserUrl: false),
            CancellationToken.None);

        Assert.Equal(string.Empty, context.BrowserUrl);
        Assert.Equal(0, browserReader.CallCount);
    }

    [Fact]
    public async Task GetContextAsync_IgnoresBrowserUrlReaderFailures()
    {
        var provider = new WindowsEnhancementContextProvider(
            new FakeClipboardTextReader(string.Empty),
            new FakeSelectedTextReader(string.Empty),
            new FakeSelectedTextClipboardFallbackReader(string.Empty),
            new FakePowerModeTargetProvider(null),
            new FakeBrowserUrlReader("ignored")
            {
                Exception = new InvalidOperationException("UIA unavailable")
            });

        var context = await provider.GetContextAsync(
            new EnhancementContextRequest(
                IncludeClipboard: false,
                IncludeSelectedText: false,
                IncludeBrowserUrl: true),
            CancellationToken.None);

        Assert.Equal(string.Empty, context.BrowserUrl);
    }

    [Fact]
    public async Task GetContextAsync_IncludesOcrContextWhenRequested()
    {
        var ocrReader = new FakeOcrTextReader("Invoice total forty two dollars");
        var provider = new WindowsEnhancementContextProvider(
            new FakeClipboardTextReader(string.Empty),
            new FakeSelectedTextReader(string.Empty),
            new FakeSelectedTextClipboardFallbackReader(string.Empty),
            new FakePowerModeTargetProvider(null),
            new FakeBrowserUrlReader(string.Empty),
            ocrReader);

        var context = await provider.GetContextAsync(
            new EnhancementContextRequest(
                IncludeClipboard: false,
                IncludeSelectedText: false,
                IncludeOcr: true),
            CancellationToken.None);

        Assert.Equal("Invoice total forty two dollars", context.OcrText);
        Assert.Equal(1, ocrReader.CallCount);
    }

    [Fact]
    public async Task GetContextAsync_SkipsOcrContextWhenNotRequested()
    {
        var ocrReader = new FakeOcrTextReader("ignored");
        var provider = new WindowsEnhancementContextProvider(
            new FakeClipboardTextReader(string.Empty),
            new FakeSelectedTextReader(string.Empty),
            new FakeSelectedTextClipboardFallbackReader(string.Empty),
            new FakePowerModeTargetProvider(null),
            new FakeBrowserUrlReader(string.Empty),
            ocrReader);

        var context = await provider.GetContextAsync(
            new EnhancementContextRequest(
                IncludeClipboard: false,
                IncludeSelectedText: false,
                IncludeOcr: false),
            CancellationToken.None);

        Assert.Equal(string.Empty, context.OcrText);
        Assert.Equal(0, ocrReader.CallCount);
    }

    [Fact]
    public async Task GetContextAsync_IgnoresOcrReaderFailures()
    {
        var provider = new WindowsEnhancementContextProvider(
            new FakeClipboardTextReader(string.Empty),
            new FakeSelectedTextReader(string.Empty),
            new FakeSelectedTextClipboardFallbackReader(string.Empty),
            new FakePowerModeTargetProvider(null),
            new FakeBrowserUrlReader(string.Empty),
            new FakeOcrTextReader("ignored")
            {
                Exception = new InvalidOperationException("OCR unavailable")
            });

        var context = await provider.GetContextAsync(
            new EnhancementContextRequest(
                IncludeClipboard: false,
                IncludeSelectedText: false,
                IncludeOcr: true),
            CancellationToken.None);

        Assert.Equal(string.Empty, context.OcrText);
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

    private sealed class FakePowerModeTargetProvider(PowerModeTarget? target) : IPowerModeTargetProvider
    {
        public Task<PowerModeTarget?> GetCurrentTargetAsync(CancellationToken cancellationToken) =>
            Task.FromResult(target);
    }

    private sealed class FakeBrowserUrlReader(string url) : IBrowserUrlReader
    {
        public int CallCount { get; private set; }
        public Exception? Exception { get; init; }

        public Task<string> GetBrowserUrlAsync(CancellationToken cancellationToken)
        {
            CallCount++;
            if (Exception is not null)
            {
                throw Exception;
            }

            return Task.FromResult(url);
        }
    }

    private sealed class FakeOcrTextReader(string text) : IOcrTextReader
    {
        public int CallCount { get; private set; }
        public Exception? Exception { get; init; }

        public Task<string> GetOcrTextAsync(CancellationToken cancellationToken)
        {
            CallCount++;
            if (Exception is not null)
            {
                throw Exception;
            }

            return Task.FromResult(text);
        }
    }
}
