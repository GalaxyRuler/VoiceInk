using VoiceInk.Windows.Core.Text;
using Xunit;

namespace VoiceInk.Windows.Core.Tests.Text;

public sealed class PasteMethodSettingsTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("appleScript")]
    [InlineData("unknown")]
    public void Normalize_ReturnsDefaultForUnsupportedMethods(string? method)
    {
        var normalized = PasteMethodSettings.Normalize(method);

        Assert.Equal(PasteMethodSettings.Default, normalized);
    }

    [Theory]
    [InlineData("default", PasteMethodSettings.Default)]
    [InlineData("directText", PasteMethodSettings.DirectText)]
    [InlineData("DirectText", PasteMethodSettings.DirectText)]
    public void Normalize_PreservesSupportedMethods(string method, string expected)
    {
        var normalized = PasteMethodSettings.Normalize(method);

        Assert.Equal(expected, normalized);
    }

    [Theory]
    [InlineData(-1.0, 0.25)]
    [InlineData(0.0, 0.25)]
    [InlineData(0.1, 0.25)]
    [InlineData(0.25, 0.25)]
    [InlineData(2.0, 2.0)]
    public void EffectiveClipboardRestoreDelaySeconds_ClampsToMinimum(
        double configured,
        double expected)
    {
        var actual = PasteMethodSettings.EffectiveClipboardRestoreDelaySeconds(configured);

        Assert.Equal(expected, actual);
    }
}
