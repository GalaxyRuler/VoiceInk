namespace VoiceInk.Windows.Core.Models;

public sealed record WhisperModelCatalogItem(
    WhisperModelCatalogEntry Model,
    string? LocalPath,
    bool IsRecommended,
    bool IsDefault)
{
    public string Name => Model.Name;

    public string DisplayName => Model.DisplayName;

    public string LanguageDisplay => Model.LanguageDisplay;

    public string Size => Model.Size;

    public string Description => Model.Description;

    public double SpeedScore => Model.SpeedScore;

    public double AccuracyScore => Model.AccuracyScore;

    public bool IsDownloaded => !string.IsNullOrWhiteSpace(LocalPath);

    public string Status =>
        IsDefault
            ? "Default Model"
            : IsDownloaded
                ? "Downloaded"
                : IsRecommended
                    ? "Recommended"
                    : "Available";

    public string PrimaryActionLabel => IsDownloaded ? "Set as Default" : "Download";

    public string AccessibleName => string.Join(
        ", ",
        new[]
        {
            DisplayName,
            Status,
            LanguageDisplay,
            Size,
            $"speed {SpeedScore}",
            $"accuracy {AccuracyScore}",
            Description
        }.Where(part => !string.IsNullOrWhiteSpace(part)).Select(part => part.Trim()));
}
