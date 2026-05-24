namespace VoiceInk.Windows.Core.Enhancement;

public sealed record TextEnhancementRequest(
    string Endpoint,
    string Model,
    string SystemMessage,
    string UserMessage,
    TimeSpan Timeout,
    double Temperature,
    bool RetryOnTimeout,
    int MaxRetries);
