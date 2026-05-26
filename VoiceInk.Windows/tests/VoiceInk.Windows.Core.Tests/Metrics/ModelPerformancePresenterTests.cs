using System.Globalization;
using VoiceInk.Windows.Core.Metrics;
using Xunit;

namespace VoiceInk.Windows.Core.Tests.Metrics;

public sealed class ModelPerformancePresenterTests
{
    [Fact]
    public void PresentTranscription_WithStats_BuildsLocalDiagnosticsRows()
    {
        WithCulture("en-US", () =>
        {
            var stat = new ModelPerformanceStat(
                "ggml-base.en",
                SessionCount: 3,
                TotalProcessingDuration: TimeSpan.FromSeconds(9),
                AverageProcessingDuration: TimeSpan.FromSeconds(3),
                AverageAudioDuration: TimeSpan.FromSeconds(12),
                SpeedFactor: 4.2);

            var rows = ModelPerformancePresenter.PresentTranscription([stat]);
            var row = Assert.Single(rows);

            Assert.Equal("ggml-base.en", row.Title);
            Assert.Equal("3 sessions - 4.2x realtime", row.Subtitle);
            Assert.Equal("3s avg processing; 12s avg audio", row.Detail);
        });
    }

    [Fact]
    public void PresentEnhancement_WithStats_BuildsEnhancementRows()
    {
        WithCulture("en-US", () =>
        {
            var stat = new ModelPerformanceStat(
                "gpt-4o-mini",
                SessionCount: 2,
                TotalProcessingDuration: TimeSpan.FromSeconds(5),
                AverageProcessingDuration: TimeSpan.FromSeconds(2.5),
                AverageAudioDuration: TimeSpan.Zero,
                SpeedFactor: 0);

            var rows = ModelPerformancePresenter.PresentEnhancement([stat]);
            var row = Assert.Single(rows);

            Assert.Equal("gpt-4o-mini", row.Title);
            Assert.Equal("2 sessions", row.Subtitle);
            Assert.Equal("2.5s avg enhancement processing", row.Detail);
        });
    }

    [Fact]
    public void PresentTranscription_EmptyStats_ShowsEmptyGuidance()
    {
        var row = Assert.Single(ModelPerformancePresenter.PresentTranscription([]));

        Assert.Equal("No transcription model metrics yet", row.Title);
        Assert.Equal("Complete a local or cloud transcription to compare model speed.", row.Detail);
        Assert.True(row.IsEmpty);
    }

    [Fact]
    public void PresentEnhancement_EmptyStats_ShowsEmptyGuidance()
    {
        var row = Assert.Single(ModelPerformancePresenter.PresentEnhancement([]));

        Assert.Equal("No enhancement model metrics yet", row.Title);
        Assert.Equal("Enable enhancement and complete a session to compare enhancement latency.", row.Detail);
        Assert.True(row.IsEmpty);
    }

    private static void WithCulture(string cultureName, Action action)
    {
        var originalCulture = CultureInfo.CurrentCulture;
        CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(cultureName);
        try
        {
            action();
        }
        finally
        {
            CultureInfo.CurrentCulture = originalCulture;
        }
    }
}
