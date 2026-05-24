namespace VoiceInk.Windows.Core.Enhancement;

public sealed record TextEnhancementResult(
    string Text,
    string ProviderName,
    string ModelName,
    TimeSpan Duration);
