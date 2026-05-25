using VoiceInk.Windows.Core.Models;
using VoiceInk.Windows.Core.PowerMode;
using VoiceInk.Windows.Core.Enhancement;
using VoiceInk.Windows.Core.Recording;
using VoiceInk.Windows.Core.Recorder;
using VoiceInk.Windows.Core.Text;

namespace VoiceInk.Windows.Core.Settings;

public sealed record AppSettings
{
    public bool HasCompletedOnboarding { get; init; }
    public string ModelPath { get; init; } = string.Empty;
    public string Language { get; init; } = "auto";
    public bool AppendTrailingSpace { get; init; }
    public bool RestoreClipboard { get; init; } = true;
    public double ClipboardRestoreDelaySeconds { get; init; } = 2.0;
    public string PasteMethod { get; init; } = PasteMethodSettings.Default;
    public bool LaunchAtLogin { get; init; }
    public bool PrewarmModelOnWake { get; init; } = true;
    public bool ShowLiveTranscriptPreview { get; init; }
    public string RecorderStyle { get; init; } = RecorderStyleSettings.Mini;
    public bool IsSoundFeedbackEnabled { get; init; } = true;
    public string StartSoundMode { get; init; } = RecordingSoundModeSettings.SystemDefault;
    public string StopSoundMode { get; init; } = RecordingSoundModeSettings.SystemDefault;
    public string CustomStartSoundPath { get; init; } = string.Empty;
    public string CustomStopSoundPath { get; init; } = string.Empty;
    public bool IsSystemMuteEnabled { get; init; } = true;
    public bool IsPauseMediaEnabled { get; init; }
    public double AudioResumptionDelaySeconds { get; init; }
    public bool IsTranscriptionCleanupEnabled { get; init; }
    public int TranscriptionRetentionMinutes { get; init; } = 24 * 60;
    public bool IsAudioCleanupEnabled { get; init; }
    public int AudioRetentionPeriod { get; init; } = 7;
    public string Hotkey { get; init; } = "Ctrl+Alt+Space";
    public string SecondaryRecordingHotkey { get; init; } = string.Empty;
    public string PasteLastTranscriptionHotkey { get; init; } = string.Empty;
    public string PasteLastEnhancementHotkey { get; init; } = string.Empty;
    public string RetryLastTranscriptionHotkey { get; init; } = string.Empty;
    public string CancelRecordingHotkey { get; init; } = string.Empty;
    public string OpenHistoryHotkey { get; init; } = string.Empty;
    public string QuickAddDictionaryHotkey { get; init; } = string.Empty;
    public string ToggleEnhancementHotkey { get; init; } = string.Empty;
    public string CyclePowerModeHotkey { get; init; } = string.Empty;
    public int? AudioInputDeviceNumber { get; init; }
    public string AudioInputDeviceName { get; init; } = string.Empty;
    public LocalWhisperModel[] ImportedWhisperModels { get; init; } = [];
    public TranscriptionProviderKind TranscriptionProvider { get; init; } = TranscriptionProviderKind.LocalWhisper;
    public string CloudTranscriptionProviderId { get; init; } = "custom";
    public string CloudTranscriptionEndpoint { get; init; } = string.Empty;
    public string CloudTranscriptionModel { get; init; } = string.Empty;
    public bool IsEnhancementEnabled { get; init; }
    public string EnhancementProviderId { get; init; } = "custom";
    public string EnhancementEndpoint { get; init; } = string.Empty;
    public string EnhancementModel { get; init; } = string.Empty;
    public EnhancementPrompt[] CustomEnhancementPrompts { get; init; } = [];
    public Guid? SelectedEnhancementPromptId { get; init; }
    public int EnhancementTimeoutSeconds { get; init; } = 7;
    public bool EnhancementRetryOnTimeout { get; init; } = true;
    public bool SkipShortEnhancement { get; init; } = true;
    public int ShortEnhancementWordThreshold { get; init; } = 3;
    public bool UseClipboardContext { get; init; }
    public bool UseOcrContext { get; init; }
    public bool UseOcrCaptureRegion { get; init; }
    public int OcrCaptureRegionLeft { get; init; }
    public int OcrCaptureRegionTop { get; init; }
    public int OcrCaptureRegionWidth { get; init; }
    public int OcrCaptureRegionHeight { get; init; }
    public bool RemoveFillerWords { get; init; } = true;
    public PunctuationCleanupMode PunctuationCleanupMode { get; init; } = PunctuationCleanupMode.Keep;
    public bool LowercaseTranscription { get; init; }
    public Guid? SelectedPowerModeRuleId { get; init; }
    public PowerModeRule[] PowerModeRules { get; init; } = [];

    public bool Equals(AppSettings? other)
    {
        return other is not null &&
            HasCompletedOnboarding == other.HasCompletedOnboarding &&
            ModelPath == other.ModelPath &&
            Language == other.Language &&
            AppendTrailingSpace == other.AppendTrailingSpace &&
            RestoreClipboard == other.RestoreClipboard &&
            ClipboardRestoreDelaySeconds.Equals(other.ClipboardRestoreDelaySeconds) &&
            PasteMethod == other.PasteMethod &&
            LaunchAtLogin == other.LaunchAtLogin &&
            PrewarmModelOnWake == other.PrewarmModelOnWake &&
            ShowLiveTranscriptPreview == other.ShowLiveTranscriptPreview &&
            RecorderStyle == other.RecorderStyle &&
            IsSoundFeedbackEnabled == other.IsSoundFeedbackEnabled &&
            StartSoundMode == other.StartSoundMode &&
            StopSoundMode == other.StopSoundMode &&
            CustomStartSoundPath == other.CustomStartSoundPath &&
            CustomStopSoundPath == other.CustomStopSoundPath &&
            IsSystemMuteEnabled == other.IsSystemMuteEnabled &&
            IsPauseMediaEnabled == other.IsPauseMediaEnabled &&
            AudioResumptionDelaySeconds.Equals(other.AudioResumptionDelaySeconds) &&
            IsTranscriptionCleanupEnabled == other.IsTranscriptionCleanupEnabled &&
            TranscriptionRetentionMinutes == other.TranscriptionRetentionMinutes &&
            IsAudioCleanupEnabled == other.IsAudioCleanupEnabled &&
            AudioRetentionPeriod == other.AudioRetentionPeriod &&
            Hotkey == other.Hotkey &&
            SecondaryRecordingHotkey == other.SecondaryRecordingHotkey &&
            PasteLastTranscriptionHotkey == other.PasteLastTranscriptionHotkey &&
            PasteLastEnhancementHotkey == other.PasteLastEnhancementHotkey &&
            RetryLastTranscriptionHotkey == other.RetryLastTranscriptionHotkey &&
            CancelRecordingHotkey == other.CancelRecordingHotkey &&
            OpenHistoryHotkey == other.OpenHistoryHotkey &&
            QuickAddDictionaryHotkey == other.QuickAddDictionaryHotkey &&
            ToggleEnhancementHotkey == other.ToggleEnhancementHotkey &&
            CyclePowerModeHotkey == other.CyclePowerModeHotkey &&
            AudioInputDeviceNumber == other.AudioInputDeviceNumber &&
            AudioInputDeviceName == other.AudioInputDeviceName &&
            ImportedWhisperModels.SequenceEqual(other.ImportedWhisperModels) &&
            TranscriptionProvider == other.TranscriptionProvider &&
            CloudTranscriptionProviderId == other.CloudTranscriptionProviderId &&
            CloudTranscriptionEndpoint == other.CloudTranscriptionEndpoint &&
            CloudTranscriptionModel == other.CloudTranscriptionModel &&
            IsEnhancementEnabled == other.IsEnhancementEnabled &&
            EnhancementProviderId == other.EnhancementProviderId &&
            EnhancementEndpoint == other.EnhancementEndpoint &&
            EnhancementModel == other.EnhancementModel &&
            EnhancementPromptsEqual(CustomEnhancementPrompts, other.CustomEnhancementPrompts) &&
            SelectedEnhancementPromptId == other.SelectedEnhancementPromptId &&
            EnhancementTimeoutSeconds == other.EnhancementTimeoutSeconds &&
            EnhancementRetryOnTimeout == other.EnhancementRetryOnTimeout &&
            SkipShortEnhancement == other.SkipShortEnhancement &&
            ShortEnhancementWordThreshold == other.ShortEnhancementWordThreshold &&
            UseClipboardContext == other.UseClipboardContext &&
            UseOcrContext == other.UseOcrContext &&
            UseOcrCaptureRegion == other.UseOcrCaptureRegion &&
            OcrCaptureRegionLeft == other.OcrCaptureRegionLeft &&
            OcrCaptureRegionTop == other.OcrCaptureRegionTop &&
            OcrCaptureRegionWidth == other.OcrCaptureRegionWidth &&
            OcrCaptureRegionHeight == other.OcrCaptureRegionHeight &&
            RemoveFillerWords == other.RemoveFillerWords &&
            PunctuationCleanupMode == other.PunctuationCleanupMode &&
            LowercaseTranscription == other.LowercaseTranscription &&
            SelectedPowerModeRuleId == other.SelectedPowerModeRuleId &&
            PowerModeRules.SequenceEqual(other.PowerModeRules);
    }

    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(HasCompletedOnboarding);
        hash.Add(ModelPath);
        hash.Add(Language);
        hash.Add(AppendTrailingSpace);
        hash.Add(RestoreClipboard);
        hash.Add(ClipboardRestoreDelaySeconds);
        hash.Add(PasteMethod);
        hash.Add(LaunchAtLogin);
        hash.Add(PrewarmModelOnWake);
        hash.Add(ShowLiveTranscriptPreview);
        hash.Add(RecorderStyle);
        hash.Add(IsSoundFeedbackEnabled);
        hash.Add(StartSoundMode);
        hash.Add(StopSoundMode);
        hash.Add(CustomStartSoundPath);
        hash.Add(CustomStopSoundPath);
        hash.Add(IsSystemMuteEnabled);
        hash.Add(IsPauseMediaEnabled);
        hash.Add(AudioResumptionDelaySeconds);
        hash.Add(IsTranscriptionCleanupEnabled);
        hash.Add(TranscriptionRetentionMinutes);
        hash.Add(IsAudioCleanupEnabled);
        hash.Add(AudioRetentionPeriod);
        hash.Add(Hotkey);
        hash.Add(SecondaryRecordingHotkey);
        hash.Add(PasteLastTranscriptionHotkey);
        hash.Add(PasteLastEnhancementHotkey);
        hash.Add(RetryLastTranscriptionHotkey);
        hash.Add(CancelRecordingHotkey);
        hash.Add(OpenHistoryHotkey);
        hash.Add(QuickAddDictionaryHotkey);
        hash.Add(ToggleEnhancementHotkey);
        hash.Add(CyclePowerModeHotkey);
        hash.Add(AudioInputDeviceNumber);
        hash.Add(AudioInputDeviceName);
        foreach (var model in ImportedWhisperModels)
        {
            hash.Add(model);
        }

        hash.Add(TranscriptionProvider);
        hash.Add(CloudTranscriptionProviderId);
        hash.Add(CloudTranscriptionEndpoint);
        hash.Add(CloudTranscriptionModel);
        hash.Add(IsEnhancementEnabled);
        hash.Add(EnhancementProviderId);
        hash.Add(EnhancementEndpoint);
        hash.Add(EnhancementModel);
        foreach (var prompt in CustomEnhancementPrompts)
        {
            hash.Add(prompt.Id);
            hash.Add(prompt.Title);
            hash.Add(prompt.PromptText);
            hash.Add(prompt.Icon);
            hash.Add(prompt.Description);
            hash.Add(prompt.IsPredefined);
            hash.Add(prompt.UseSystemInstructions);
            foreach (var triggerWord in prompt.TriggerWords)
            {
                hash.Add(triggerWord);
            }
        }

        hash.Add(SelectedEnhancementPromptId);
        hash.Add(EnhancementTimeoutSeconds);
        hash.Add(EnhancementRetryOnTimeout);
        hash.Add(SkipShortEnhancement);
        hash.Add(ShortEnhancementWordThreshold);
        hash.Add(UseClipboardContext);
        hash.Add(UseOcrContext);
        hash.Add(UseOcrCaptureRegion);
        hash.Add(OcrCaptureRegionLeft);
        hash.Add(OcrCaptureRegionTop);
        hash.Add(OcrCaptureRegionWidth);
        hash.Add(OcrCaptureRegionHeight);
        hash.Add(RemoveFillerWords);
        hash.Add(PunctuationCleanupMode);
        hash.Add(LowercaseTranscription);
        hash.Add(SelectedPowerModeRuleId);
        foreach (var rule in PowerModeRules)
        {
            hash.Add(rule);
        }

        return hash.ToHashCode();
    }

    private static bool EnhancementPromptsEqual(
        IReadOnlyList<EnhancementPrompt> first,
        IReadOnlyList<EnhancementPrompt> second)
    {
        if (first.Count != second.Count)
        {
            return false;
        }

        for (var i = 0; i < first.Count; i++)
        {
            if (!EnhancementPromptEquals(first[i], second[i]))
            {
                return false;
            }
        }

        return true;
    }

    private static bool EnhancementPromptEquals(
        EnhancementPrompt first,
        EnhancementPrompt second) =>
        first.Id == second.Id
        && first.Title == second.Title
        && first.PromptText == second.PromptText
        && first.Icon == second.Icon
        && first.Description == second.Description
        && first.IsPredefined == second.IsPredefined
        && first.UseSystemInstructions == second.UseSystemInstructions
        && first.TriggerWords.SequenceEqual(second.TriggerWords);
}
