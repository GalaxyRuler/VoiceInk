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

    [Fact]
    public void TryAdd_NormalizesNewWord()
    {
        var added = FillerWordSettings.TryAdd(["um"], " Like ", out var words);

        Assert.True(added);
        Assert.Equal(["um", "like"], words);
    }

    [Fact]
    public void TryAdd_RejectsDuplicateWord()
    {
        var added = FillerWordSettings.TryAdd(["um"], "UM", out var words);

        Assert.False(added);
        Assert.Equal(["um"], words);
    }

    [Fact]
    public void Remove_RemovesWordCaseInsensitively()
    {
        var words = FillerWordSettings.Remove(["um", "like"], "LIKE");

        Assert.Equal(["um"], words);
    }
}
