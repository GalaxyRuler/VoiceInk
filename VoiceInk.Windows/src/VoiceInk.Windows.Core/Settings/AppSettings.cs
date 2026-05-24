using VoiceInk.Windows.Core.Models;
using VoiceInk.Windows.Core.Text;

namespace VoiceInk.Windows.Core.Settings;

public sealed record AppSettings
{
    public bool HasCompletedOnboarding { get; init; }
    public string ModelPath { get; init; } = string.Empty;
    public string Language { get; init; } = "auto";
    public bool AppendTrailingSpace { get; init; }
    public bool RestoreClipboard { get; init; } = true;
    public string Hotkey { get; init; } = "Ctrl+Alt+Space";
    public string SecondaryRecordingHotkey { get; init; } = string.Empty;
    public string PasteLastTranscriptionHotkey { get; init; } = string.Empty;
    public string PasteLastEnhancementHotkey { get; init; } = string.Empty;
    public string RetryLastTranscriptionHotkey { get; init; } = string.Empty;
    public string CancelRecordingHotkey { get; init; } = string.Empty;
    public string OpenHistoryHotkey { get; init; } = string.Empty;
    public string QuickAddDictionaryHotkey { get; init; } = string.Empty;
    public int? AudioInputDeviceNumber { get; init; }
    public string AudioInputDeviceName { get; init; } = string.Empty;
    public LocalWhisperModel[] ImportedWhisperModels { get; init; } = [];
    public TranscriptionProviderKind TranscriptionProvider { get; init; } = TranscriptionProviderKind.LocalWhisper;
    public bool RemoveFillerWords { get; init; } = true;
    public PunctuationCleanupMode PunctuationCleanupMode { get; init; } = PunctuationCleanupMode.Keep;
    public bool LowercaseTranscription { get; init; }

    public bool Equals(AppSettings? other)
    {
        return other is not null &&
            HasCompletedOnboarding == other.HasCompletedOnboarding &&
            ModelPath == other.ModelPath &&
            Language == other.Language &&
            AppendTrailingSpace == other.AppendTrailingSpace &&
            RestoreClipboard == other.RestoreClipboard &&
            Hotkey == other.Hotkey &&
            SecondaryRecordingHotkey == other.SecondaryRecordingHotkey &&
            PasteLastTranscriptionHotkey == other.PasteLastTranscriptionHotkey &&
            PasteLastEnhancementHotkey == other.PasteLastEnhancementHotkey &&
            RetryLastTranscriptionHotkey == other.RetryLastTranscriptionHotkey &&
            CancelRecordingHotkey == other.CancelRecordingHotkey &&
            OpenHistoryHotkey == other.OpenHistoryHotkey &&
            QuickAddDictionaryHotkey == other.QuickAddDictionaryHotkey &&
            AudioInputDeviceNumber == other.AudioInputDeviceNumber &&
            AudioInputDeviceName == other.AudioInputDeviceName &&
            ImportedWhisperModels.SequenceEqual(other.ImportedWhisperModels) &&
            TranscriptionProvider == other.TranscriptionProvider &&
            RemoveFillerWords == other.RemoveFillerWords &&
            PunctuationCleanupMode == other.PunctuationCleanupMode &&
            LowercaseTranscription == other.LowercaseTranscription;
    }

    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(HasCompletedOnboarding);
        hash.Add(ModelPath);
        hash.Add(Language);
        hash.Add(AppendTrailingSpace);
        hash.Add(RestoreClipboard);
        hash.Add(Hotkey);
        hash.Add(SecondaryRecordingHotkey);
        hash.Add(PasteLastTranscriptionHotkey);
        hash.Add(PasteLastEnhancementHotkey);
        hash.Add(RetryLastTranscriptionHotkey);
        hash.Add(CancelRecordingHotkey);
        hash.Add(OpenHistoryHotkey);
        hash.Add(QuickAddDictionaryHotkey);
        hash.Add(AudioInputDeviceNumber);
        hash.Add(AudioInputDeviceName);
        foreach (var model in ImportedWhisperModels)
        {
            hash.Add(model);
        }

        hash.Add(TranscriptionProvider);
        hash.Add(RemoveFillerWords);
        hash.Add(PunctuationCleanupMode);
        hash.Add(LowercaseTranscription);
        return hash.ToHashCode();
    }
}
