namespace VoiceInk.Windows.Core.History;

public sealed record TranscriptionHistoryItem
{
    public const string CanceledTranscriptionText = "The transcription was canceled.";

    public Guid Id { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public string Text { get; init; }
    public string ProviderName { get; init; }
    public TimeSpan AudioDuration { get; init; }
    public TimeSpan TranscriptionDuration { get; init; }
    public string OriginalText { get; init; }
    public string? EnhancedText { get; init; }
    public TranscriptionHistoryStatus Status { get; init; }
    public string Language { get; init; }
    public string? ModelPath { get; init; }
    public string? PromptName { get; init; }
    public TimeSpan? EnhancementDuration { get; init; }
    public string? ErrorMessage { get; init; }
    public string? AudioFilePath { get; init; }
    public string? EnhancementProviderName { get; init; }
    public string? EnhancementModelName { get; init; }
    public string? AiRequestSystemMessage { get; init; }
    public string? AiRequestUserMessage { get; init; }

    public TranscriptionHistoryItem(
        Guid id,
        DateTimeOffset createdAt,
        string text,
        string providerName,
        TimeSpan audioDuration,
        TimeSpan transcriptionDuration,
        string? originalText = null,
        string? enhancedText = null,
        TranscriptionHistoryStatus status = TranscriptionHistoryStatus.Completed,
        string language = "auto",
        string? modelPath = null,
        string? promptName = null,
        TimeSpan? enhancementDuration = null,
        string? errorMessage = null,
        string? audioFilePath = null,
        string? enhancementProviderName = null,
        string? enhancementModelName = null,
        string? aiRequestSystemMessage = null,
        string? aiRequestUserMessage = null)
    {
        Id = id;
        CreatedAt = createdAt;
        Text = text;
        ProviderName = providerName;
        AudioDuration = audioDuration;
        TranscriptionDuration = transcriptionDuration;
        OriginalText = string.IsNullOrEmpty(originalText) ? text : originalText;
        EnhancedText = enhancedText;
        Status = status;
        Language = string.IsNullOrWhiteSpace(language) ? "auto" : language;
        ModelPath = modelPath;
        PromptName = promptName;
        EnhancementDuration = enhancementDuration;
        ErrorMessage = errorMessage;
        AudioFilePath = string.IsNullOrWhiteSpace(audioFilePath) ? null : audioFilePath;
        EnhancementProviderName = string.IsNullOrWhiteSpace(enhancementProviderName) ? null : enhancementProviderName;
        EnhancementModelName = string.IsNullOrWhiteSpace(enhancementModelName) ? null : enhancementModelName;
        AiRequestSystemMessage = string.IsNullOrWhiteSpace(aiRequestSystemMessage) ? null : aiRequestSystemMessage;
        AiRequestUserMessage = string.IsNullOrWhiteSpace(aiRequestUserMessage) ? null : aiRequestUserMessage;
    }
}
