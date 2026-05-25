namespace VoiceInk.Windows.Core.Models;

public sealed record WhisperModelCatalogEntry(
    string Name,
    string DisplayName,
    string Size,
    bool IsMultilingual,
    string Description,
    double Speed,
    double Accuracy,
    double RamUsage)
{
    private const string WhisperCppModelBaseUrl = "https://huggingface.co/ggerganov/whisper.cpp/resolve/main/";

    public string LanguageDisplay => IsMultilingual ? "Multilingual" : "English-only";

    public string FileName => $"{Name}.bin";

    public Uri DownloadUri => new($"{WhisperCppModelBaseUrl}{FileName}");

    public double SpeedScore => Math.Round(Speed * 10, 1);

    public double AccuracyScore => Math.Round(Accuracy * 10, 1);

    public override string ToString() => DisplayName;
}
