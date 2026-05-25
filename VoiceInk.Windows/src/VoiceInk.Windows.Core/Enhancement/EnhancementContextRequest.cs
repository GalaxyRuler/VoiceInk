namespace VoiceInk.Windows.Core.Enhancement;

public sealed record EnhancementContextRequest(
    bool IncludeClipboard,
    bool IncludeSelectedText,
    bool IncludeActiveWindow = true,
    bool IncludeBrowserUrl = true);
