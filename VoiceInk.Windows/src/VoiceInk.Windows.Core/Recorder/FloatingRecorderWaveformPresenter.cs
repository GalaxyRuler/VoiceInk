namespace VoiceInk.Windows.Core.Recorder;

public static class FloatingRecorderWaveformPresenter
{
    public const int BarCount = 15;
    public const double MinimumHeight = 4;
    public const double MaximumHeight = 28;

    public static IReadOnlyList<FloatingRecorderWaveformBar> Present(
        double inputLevel,
        int animationStep)
    {
        var safeInputLevel = double.IsFinite(inputLevel) ? Math.Clamp(inputLevel, 0, 1) : 0;
        var amplitude = safeInputLevel > 0.01
            ? Math.Pow(safeInputLevel, 0.7)
            : 0.2;
        var bars = new List<FloatingRecorderWaveformBar>(BarCount);
        for (var index = 0; index < BarCount; index++)
        {
            var phase = index * 0.4;
            var wave = Math.Sin(animationStep * 0.75 + phase) * 0.5 + 0.5;
            var centerDistance = Math.Abs(index - ((BarCount - 1) / 2.0)) / ((BarCount - 1) / 2.0);
            var centerBoost = 1.0 - centerDistance * 0.4;
            var normalized = Math.Clamp(amplitude * centerBoost * (0.35 + wave * 0.65), 0, 1);
            var height = MinimumHeight + normalized * (MaximumHeight - MinimumHeight);
            var opacity = 0.35 + normalized * 0.65;
            bars.Add(new FloatingRecorderWaveformBar(height, opacity));
        }

        return bars;
    }
}
