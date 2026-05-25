using VoiceInk.Windows.Core.Diagnostics;
using Xunit;

namespace VoiceInk.Windows.Core.Tests.Diagnostics;

public sealed class DiagnosticEventSanitizerTests
{
    [Theory]
    [InlineData("Idle", "Idle")]
    [InlineData("Recording", "Recording")]
    [InlineData("Loading settings", "Loading settings")]
    [InlineData("My private prompt prompt saved", "Prompt saved")]
    [InlineData("Secret prompt prompt deleted", "Prompt deleted")]
    [InlineData("Default model: private-model-name", "Default model changed")]
    [InlineData("Dictionary exported: private-file-name.json", "Dictionary exported")]
    [InlineData("History export failed: C:\\Users\\Admin\\secret.csv", "History export failed")]
    [InlineData("Metrics reset canceled", "Metrics reset canceled")]
    [InlineData("Metrics reset failed: C:\\Users\\Admin\\metrics.db", "Metrics reset failed")]
    [InlineData("Duplicate vocabulary word: private-word", "Duplicate vocabulary word")]
    [InlineData("Duplicate word replacement: private phrase", "Duplicate word replacement")]
    public void Normalize_ReturnsGenericSafeEventLabel(string status, string expected)
    {
        var actual = DiagnosticEventSanitizer.Normalize(status);

        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("Private free-form status with user text")]
    public void Normalize_DropsUnknownStatusText(string status)
    {
        var actual = DiagnosticEventSanitizer.Normalize(status);

        Assert.Null(actual);
    }
}
