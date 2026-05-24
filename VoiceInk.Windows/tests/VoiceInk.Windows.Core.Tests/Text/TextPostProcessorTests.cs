using VoiceInk.Windows.Core.Text;
using Xunit;

namespace VoiceInk.Windows.Core.Tests.Text;

public sealed class TextPostProcessorTests
{
    [Fact]
    public void Process_TrimsWhitespace()
    {
        var result = TextPostProcessor.Process("  hello from voice ink \r\n", new TextPostProcessingOptions());

        Assert.Equal("hello from voice ink", result);
    }

    [Fact]
    public void Process_AppendsTrailingSpaceWhenEnabled()
    {
        var result = TextPostProcessor.Process("hello", new TextPostProcessingOptions(AppendTrailingSpace: true));

        Assert.Equal("hello ", result);
    }

    [Fact]
    public void Process_ReturnsEmptyStringForWhitespaceOnlyInput()
    {
        var result = TextPostProcessor.Process(" \r\n\t ", new TextPostProcessingOptions(AppendTrailingSpace: true));

        Assert.Equal(string.Empty, result);
    }
}
