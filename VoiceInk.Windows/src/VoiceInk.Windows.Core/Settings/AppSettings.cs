namespace VoiceInk.Windows.Core.Settings;

public sealed record AppSettings
{
    public string ModelPath { get; init; } = string.Empty;
    public string Language { get; init; } = "auto";
    public bool AppendTrailingSpace { get; init; }
    public bool RestoreClipboard { get; init; } = true;
    public string Hotkey { get; init; } = "Ctrl+Alt+Space";
    public TranscriptionProviderKind TranscriptionProvider { get; init; } = TranscriptionProviderKind.LocalWhisper;
}
