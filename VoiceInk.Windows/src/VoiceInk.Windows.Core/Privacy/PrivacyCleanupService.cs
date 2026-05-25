using VoiceInk.Windows.Core.History;
using VoiceInk.Windows.Core.Services;

namespace VoiceInk.Windows.Core.Privacy;

public sealed class PrivacyCleanupService(
    ISettingsStore settingsStore,
    IHistoryStore historyStore,
    TimeProvider timeProvider,
    string recordingsDirectory)
{
    private readonly string recordingsRoot = NormalizeDirectory(recordingsDirectory);

    public async Task<PrivacyCleanupResult> RunTranscriptCleanupAsync(CancellationToken cancellationToken)
    {
        var settings = await settingsStore.LoadAsync(cancellationToken);
        if (!settings.IsTranscriptionCleanupEnabled)
        {
            return new PrivacyCleanupResult(false, 0, 0, 0, 0, 0);
        }

        var cutoff = RetentionCutoff(TimeSpan.FromMinutes(Math.Max(0, settings.TranscriptionRetentionMinutes)));
        var items = await historyStore.ListOlderThanAsync(cutoff, cancellationToken);
        var audioResult = TryDeleteAudioFiles(items);
        var failedAudioIds = audioResult.FailedIds.ToHashSet();

        var deletedTranscriptions = 0;
        foreach (var item in items)
        {
            if (failedAudioIds.Contains(item.Id))
            {
                continue;
            }

            if (await historyStore.DeleteAsync(item.Id, cancellationToken))
            {
                deletedTranscriptions++;
            }
        }

        return new PrivacyCleanupResult(
            true,
            deletedTranscriptions,
            audioResult.DeletedCount,
            audioResult.FailedCount,
            0,
            audioResult.DeletedBytes);
    }

    public async Task<PrivacyCleanupPreview> PreviewAudioCleanupAsync(CancellationToken cancellationToken)
    {
        var settings = await settingsStore.LoadAsync(cancellationToken);
        if (!settings.IsAudioCleanupEnabled || settings.IsTranscriptionCleanupEnabled)
        {
            return new PrivacyCleanupPreview(false, 0, 0);
        }

        var cutoff = RetentionCutoff(TimeSpan.FromDays(Math.Max(0, settings.AudioRetentionPeriod)));
        var items = await historyStore.ListOlderThanAsync(cutoff, cancellationToken);
        var files = EligibleAudioFiles(items).ToArray();

        return new PrivacyCleanupPreview(
            true,
            files.Length,
            files.Sum(file => file.Length));
    }

    public async Task<PrivacyCleanupResult> RunAudioCleanupAsync(CancellationToken cancellationToken)
    {
        var settings = await settingsStore.LoadAsync(cancellationToken);
        if (!settings.IsAudioCleanupEnabled || settings.IsTranscriptionCleanupEnabled)
        {
            return new PrivacyCleanupResult(false, 0, 0, 0, 0, 0);
        }

        var cutoff = RetentionCutoff(TimeSpan.FromDays(Math.Max(0, settings.AudioRetentionPeriod)));
        var items = await historyStore.ListOlderThanAsync(cutoff, cancellationToken);
        var audioResult = TryDeleteAudioFiles(items);
        var cleared = audioResult.DeletedIds.Count == 0
            ? 0
            : await historyStore.ClearAudioFilePathAsync(audioResult.DeletedIds, cancellationToken);

        return new PrivacyCleanupResult(
            true,
            0,
            audioResult.DeletedCount,
            audioResult.FailedCount,
            cleared,
            audioResult.DeletedBytes);
    }

    private DateTimeOffset RetentionCutoff(TimeSpan retention) =>
        timeProvider.GetUtcNow().Subtract(retention);

    private AudioDeletionResult TryDeleteAudioFiles(IReadOnlyList<TranscriptionHistoryItem> items)
    {
        var deletedIds = new List<Guid>();
        var failedIds = new List<Guid>();
        var deletedCount = 0;
        var failedCount = 0;
        long deletedBytes = 0;

        foreach (var item in items)
        {
            if (string.IsNullOrWhiteSpace(item.AudioFilePath)
                || !TryGetSafeAudioFileInfo(item.AudioFilePath, out var file))
            {
                continue;
            }

            try
            {
                var length = file.Length;
                file.Delete();
                deletedIds.Add(item.Id);
                deletedCount++;
                deletedBytes += length;
            }
            catch (IOException)
            {
                failedIds.Add(item.Id);
                failedCount++;
            }
            catch (UnauthorizedAccessException)
            {
                failedIds.Add(item.Id);
                failedCount++;
            }
        }

        return new AudioDeletionResult(deletedIds, failedIds, deletedCount, failedCount, deletedBytes);
    }

    private IEnumerable<FileInfo> EligibleAudioFiles(IReadOnlyList<TranscriptionHistoryItem> items)
    {
        foreach (var item in items)
        {
            if (!string.IsNullOrWhiteSpace(item.AudioFilePath)
                && TryGetSafeAudioFileInfo(item.AudioFilePath, out var file))
            {
                yield return file;
            }
        }
    }

    private bool TryGetSafeAudioFileInfo(string path, out FileInfo file)
    {
        file = null!;
        try
        {
            var fullPath = Path.GetFullPath(path);
            if (DirectoryHasReparsePoint(recordingsRoot))
            {
                return false;
            }

            var rootWithSeparator = recordingsRoot.EndsWith(Path.DirectorySeparatorChar)
                ? recordingsRoot
                : recordingsRoot + Path.DirectorySeparatorChar;
            if (!fullPath.StartsWith(rootWithSeparator, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            if (!IsDirectAppRecordingFile(fullPath))
            {
                return false;
            }

            var candidate = new FileInfo(fullPath);
            if (!candidate.Exists)
            {
                return false;
            }

            if ((candidate.Attributes & FileAttributes.ReparsePoint) != 0)
            {
                return false;
            }

            file = candidate;
            return true;
        }
        catch (ArgumentException)
        {
            return false;
        }
        catch (NotSupportedException)
        {
            return false;
        }
        catch (PathTooLongException)
        {
            return false;
        }
        catch (UnauthorizedAccessException)
        {
            return false;
        }
    }

    private bool IsDirectAppRecordingFile(string fullPath)
    {
        var parent = Directory.GetParent(fullPath)?.FullName;
        if (!string.Equals(parent, recordingsRoot, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        var fileName = Path.GetFileName(fullPath);
        var stem = Path.GetFileNameWithoutExtension(fileName);
        return string.Equals(Path.GetExtension(fileName), ".wav", StringComparison.OrdinalIgnoreCase)
            && (IsHexGuidStem(stem)
                || (stem.StartsWith("transcribed_", StringComparison.OrdinalIgnoreCase)
                    && IsHexGuidStem(stem["transcribed_".Length..])));
    }

    private static bool IsHexGuidStem(string value) =>
        value.Length == 32 && value.All(IsLowerHexDigit);

    private static bool IsLowerHexDigit(char value) =>
        value is >= '0' and <= '9' or >= 'a' and <= 'f';

    private static string NormalizeDirectory(string directory) =>
        Path.GetFullPath(directory).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

    private static bool DirectoryHasReparsePoint(string directory)
    {
        try
        {
            return (File.GetAttributes(directory) & FileAttributes.ReparsePoint) != 0;
        }
        catch (ArgumentException)
        {
            return true;
        }
        catch (IOException)
        {
            return true;
        }
        catch (UnauthorizedAccessException)
        {
            return true;
        }
    }

    private sealed record AudioDeletionResult(
        IReadOnlyCollection<Guid> DeletedIds,
        IReadOnlyCollection<Guid> FailedIds,
        int DeletedCount,
        int FailedCount,
        long DeletedBytes);
}
