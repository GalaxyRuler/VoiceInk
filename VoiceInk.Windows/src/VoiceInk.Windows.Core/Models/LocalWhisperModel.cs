namespace VoiceInk.Windows.Core.Models;

public sealed record LocalWhisperModel(
    string Path,
    string DisplayName,
    DateTimeOffset ImportedAt)
{
    public override string ToString() => DisplayName;
}
