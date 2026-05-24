using VoiceInk.Windows.Core.Text;

namespace VoiceInk.Windows.Core.Settings;

public sealed record AppSettings
{
    public string ModelPath { get; init; } = string.Empty;
    public string Language { get; init; } = "auto";
    public bool AppendTrailingSpace { get; init; }
    public bool RestoreClipboard { get; init; } = true;
    public string Hotkey { get; init; } = "Ctrl+Alt+Space";
    public string PasteLastTranscriptionHotkey { get; init; } = string.Empty;
    public string PasteLastEnhancementHotkey { get; init; } = string.Empty;
    public string RetryLastTranscriptionHotkey { get; init; } = string.Empty;
    public string CancelRecordingHotkey { get; init; } = string.Empty;
    public string OpenHistoryHotkey { get; init; } = string.Empty;
    public string QuickAddDictionaryHotkey { get; init; } = string.Empty;
    public int? AudioInputDeviceNumber { get; init; }
    public string AudioInputDeviceName { get; init; } = string.Empty;
    public TranscriptionProviderKind TranscriptionProvider { get; init; } = TranscriptionProviderKind.LocalWhisper;
    public bool RemoveFillerWords { get; init; } = true;
    public PunctuationCleanupMode PunctuationCleanupMode { get; init; } = PunctuationCleanupMode.Keep;
    public bool LowercaseTranscription { get; init; }
}
