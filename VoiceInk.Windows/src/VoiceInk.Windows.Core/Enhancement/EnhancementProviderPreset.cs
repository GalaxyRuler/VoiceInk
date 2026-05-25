namespace VoiceInk.Windows.Core.Enhancement;

public sealed record EnhancementProviderPreset(
    string Id,
    string DisplayName,
    string Endpoint,
    string DefaultModel,
    IReadOnlyList<string> ModelIds,
    bool RequiresApiKey);
