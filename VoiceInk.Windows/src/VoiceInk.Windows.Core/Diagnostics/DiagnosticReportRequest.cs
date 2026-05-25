namespace VoiceInk.Windows.Core.Diagnostics;

public sealed record DiagnosticReportRequest
{
    public DateTimeOffset ExportedAtUtc { get; init; }
    public string AppVersion { get; init; } = "source build";
    public string OsDescription { get; init; } = string.Empty;
    public string RuntimeDescription { get; init; } = string.Empty;
    public string ProcessArchitecture { get; init; } = string.Empty;
    public string AppBaseDirectory { get; init; } = string.Empty;
    public string AppDataDirectory { get; init; } = string.Empty;
    public string RecordingsDirectory { get; init; } = string.Empty;
    public string ActiveSection { get; init; } = string.Empty;
    public string DictationState { get; init; } = string.Empty;
    public string SelectedModelPath { get; init; } = string.Empty;
    public IReadOnlyList<DiagnosticFileEntry> Files { get; init; } = [];
    public IReadOnlyList<string> RecentEvents { get; init; } = [];
}
