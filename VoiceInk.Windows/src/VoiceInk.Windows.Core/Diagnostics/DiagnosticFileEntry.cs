namespace VoiceInk.Windows.Core.Diagnostics;

public sealed record DiagnosticFileEntry(
    string Label,
    string Path,
    bool Exists,
    long? SizeBytes);
