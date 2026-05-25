namespace VoiceInk.Windows.Core.Audio;

public sealed class AudioInputLevel(double peak) : EventArgs, IEquatable<AudioInputLevel>
{
    public double Peak { get; } = double.IsFinite(peak)
        ? Math.Clamp(peak, 0, 1)
        : 0;

    public static AudioInputLevel Silent { get; } = new(0);

    public bool Equals(AudioInputLevel? other) =>
        other is not null && Peak.Equals(other.Peak);

    public override bool Equals(object? obj) =>
        obj is AudioInputLevel other && Equals(other);

    public override int GetHashCode() => Peak.GetHashCode();
}
