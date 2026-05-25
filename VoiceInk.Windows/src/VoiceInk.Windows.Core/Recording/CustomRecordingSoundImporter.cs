namespace VoiceInk.Windows.Core.Recording;

public sealed class CustomRecordingSoundImporter(
    IRecordingSoundFileSystem fileSystem,
    IRecordingSoundFileProbe fileProbe,
    string soundsDirectory)
{
    public static readonly TimeSpan MaximumDuration = TimeSpan.FromSeconds(3);

    private static readonly HashSet<string> SupportedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".wav",
        ".mp3",
        ".aiff",
        ".aif"
    };

    public CustomRecordingSoundImportResult Import(string sourcePath, RecordingSoundKind kind)
    {
        if (string.IsNullOrWhiteSpace(sourcePath))
        {
            return CustomRecordingSoundImportResult.Failure("Choose a sound file to import.");
        }

        var extension = Path.GetExtension(sourcePath);
        if (!SupportedExtensions.Contains(extension))
        {
            return CustomRecordingSoundImportResult.Failure(
                "Unsupported sound file. Choose a .wav, .mp3, .aiff, or .aif file.");
        }

        if (!fileSystem.FileExists(sourcePath))
        {
            return CustomRecordingSoundImportResult.Failure("The selected sound file does not exist.");
        }

        var probe = fileProbe.Probe(sourcePath);
        if (!probe.IsSuccess)
        {
            return CustomRecordingSoundImportResult.Failure(probe.ErrorMessage);
        }

        if (!double.IsFinite(probe.Duration.TotalSeconds) || probe.Duration <= TimeSpan.Zero)
        {
            return CustomRecordingSoundImportResult.Failure("The selected sound file has no valid duration.");
        }

        if (probe.Duration > MaximumDuration)
        {
            return CustomRecordingSoundImportResult.Failure("The selected sound file must be 3 seconds or shorter.");
        }

        fileSystem.CreateDirectory(soundsDirectory);
        var destinationPath = Path.Combine(
            soundsDirectory,
            $"{FileNamePrefix(kind)}{extension.ToLowerInvariant()}");

        DeleteExistingKindFiles(kind, exceptPath: sourcePath);
        if (!string.Equals(sourcePath, destinationPath, StringComparison.OrdinalIgnoreCase))
        {
            fileSystem.CopyFile(sourcePath, destinationPath, overwrite: true);
        }

        return CustomRecordingSoundImportResult.Success(destinationPath, Path.GetFileName(destinationPath));
    }

    public void Reset(RecordingSoundKind kind)
    {
        DeleteExistingKindFiles(kind, exceptPath: null);
    }

    private void DeleteExistingKindFiles(RecordingSoundKind kind, string? exceptPath)
    {
        foreach (var filePath in fileSystem.EnumerateFiles(soundsDirectory, $"{FileNamePrefix(kind)}.*"))
        {
            if (string.Equals(filePath, exceptPath, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            fileSystem.DeleteFileIfExists(filePath);
        }
    }

    private static string FileNamePrefix(RecordingSoundKind kind) =>
        kind == RecordingSoundKind.Start
            ? "CustomStartSound"
            : "CustomStopSound";
}

public sealed record CustomRecordingSoundImportResult
{
    private CustomRecordingSoundImportResult(
        bool isSuccess,
        string customSoundPath,
        string fileName,
        string errorMessage)
    {
        IsSuccess = isSuccess;
        CustomSoundPath = customSoundPath;
        FileName = fileName;
        ErrorMessage = errorMessage;
    }

    public bool IsSuccess { get; }
    public string CustomSoundPath { get; }
    public string FileName { get; }
    public string ErrorMessage { get; }

    public static CustomRecordingSoundImportResult Success(string customSoundPath, string fileName) =>
        new(true, customSoundPath, fileName, string.Empty);

    public static CustomRecordingSoundImportResult Failure(string errorMessage) =>
        new(false, string.Empty, string.Empty, errorMessage);
}
