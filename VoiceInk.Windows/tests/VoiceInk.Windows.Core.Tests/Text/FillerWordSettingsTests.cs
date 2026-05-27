using VoiceInk.Windows.Core.Text;
using Xunit;

namespace VoiceInk.Windows.Core.Tests.Text;

public sealed class FillerWordSettingsTests
{
    [Fact]
    public void ParseList_NormalizesAndDeduplicatesWords()
    {
        var words = FillerWordSettings.ParseList(" Um, like\r\nLIKE\n you know , ");

        Assert.Equal(["um", "like", "you know"], words);
    }

    [Fact]
    public void ToEditableText_UsesOneWordPerLine()
    {
        var text = FillerWordSettings.ToEditableText(["um", "like", "you know"]);

        Assert.Equal("um\r\nlike\r\nyou know", text);
    }
}
