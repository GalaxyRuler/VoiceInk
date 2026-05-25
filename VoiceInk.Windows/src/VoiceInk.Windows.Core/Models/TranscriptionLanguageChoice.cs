namespace VoiceInk.Windows.Core.Models;

public sealed record TranscriptionLanguageChoice(string Code, string DisplayName)
{
    public override string ToString() => DisplayName;
}
