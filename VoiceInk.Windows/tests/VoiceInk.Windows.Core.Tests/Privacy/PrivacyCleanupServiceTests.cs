using VoiceInk.Windows.Core.History;
using VoiceInk.Windows.Core.Privacy;
using VoiceInk.Windows.Core.Services;
using VoiceInk.Windows.Core.Settings;
using Xunit;

namespace VoiceInk.Windows.Core.Tests.Privacy;

public sealed class PrivacyCleanupServiceTests
{
    [Fact]
    public async Task RunTranscriptCleanupAsync_DeletesOldRowsAndAssociatedAudioFiles()
    {
        using var temp = new TempDirectory();
        var oldAudioPath = temp.WriteFile($"{Guid.NewGuid():N}.wav", "older audio");
        var newAudioPath = temp.WriteFile($"{Guid.NewGuid():N}.wav", "new audio");
        var oldAudioBytes = new FileInfo(oldAudioPath).Length;
        var now = DateTimeOffset.Parse("2026-05-25T12:00:00Z");
        var oldItem = new TranscriptionHistoryItem(
            Guid.NewGuid(),
            now.AddMinutes(-90),
            "old",
            "local-whisper",
            TimeSpan.Zero,
            TimeSpan.Zero,
            audioFilePath: oldAudioPath);
        var newItem = oldItem with
        {
            Id = Guid.NewGuid(),
            CreatedAt = now.AddMinutes(-30),
            Text = "new",
            OriginalText = "new",
            AudioFilePath = newAudioPath
        };
        var historyStore = new FakeHistoryStore([oldItem, newItem]);
        var settingsStore = new FakeSettingsStore(new AppSettings
        {
            IsTranscriptionCleanupEnabled = true,
            TranscriptionRetentionMinutes = 60
        });
        var service = new PrivacyCleanupService(
            settingsStore,
            historyStore,
            new FixedTimeProvider(now),
            temp.Path);

        var result = await service.RunTranscriptCleanupAsync(CancellationToken.None);

        Assert.True(result.IsEnabled);
        Assert.Equal(1, result.DeletedTranscriptionCount);
        Assert.Equal(1, result.DeletedAudioFileCount);
        Assert.Equal(oldAudioBytes, result.DeletedAudioBytes);
        Assert.False(File.Exists(oldAudioPath));
        Assert.True(File.Exists(newAudioPath));
        Assert.Equal(newItem.Id, Assert.Single(historyStore.Items).Id);
    }

    [Fact]
    public async Task RunAudioCleanupAsync_DeletesOldAudioAndPreservesTranscript()
    {
        using var temp = new TempDirectory();
        var oldAudioPath = temp.WriteFile($"{Guid.NewGuid():N}.wav", "older audio");
        var newAudioPath = temp.WriteFile($"{Guid.NewGuid():N}.wav", "new audio");
        var now = DateTimeOffset.Parse("2026-05-25T12:00:00Z");
        var oldItem = new TranscriptionHistoryItem(
            Guid.NewGuid(),
            now.AddDays(-8),
            "old",
            "local-whisper",
            TimeSpan.Zero,
            TimeSpan.Zero,
            audioFilePath: oldAudioPath);
        var newItem = oldItem with
        {
            Id = Guid.NewGuid(),
            CreatedAt = now.AddDays(-2),
            Text = "new",
            OriginalText = "new",
            AudioFilePath = newAudioPath
        };
        var historyStore = new FakeHistoryStore([oldItem, newItem]);
        var settingsStore = new FakeSettingsStore(new AppSettings
        {
            IsAudioCleanupEnabled = true,
            AudioRetentionPeriod = 7
        });
        var service = new PrivacyCleanupService(
            settingsStore,
            historyStore,
            new FixedTimeProvider(now),
            temp.Path);

        var result = await service.RunAudioCleanupAsync(CancellationToken.None);

        Assert.True(result.IsEnabled);
        Assert.Equal(0, result.DeletedTranscriptionCount);
        Assert.Equal(1, result.DeletedAudioFileCount);
        Assert.Equal(1, result.ClearedAudioReferenceCount);
        Assert.False(File.Exists(oldAudioPath));
        Assert.True(File.Exists(newAudioPath));
        Assert.Equal(2, historyStore.Items.Count);
        Assert.Null(historyStore.Items.Single(item => item.Id == oldItem.Id).AudioFilePath);
        Assert.Equal(newAudioPath, historyStore.Items.Single(item => item.Id == newItem.Id).AudioFilePath);
    }

    [Fact]
    public async Task PreviewAudioCleanupAsync_ReturnsEligibleFileCountAndBytes()
    {
        using var temp = new TempDirectory();
        var oldAudioPath = temp.WriteFile($"transcribed_{Guid.NewGuid():N}.wav", "older audio");
        var missingAudioPath = Path.Combine(temp.Path, "missing.wav");
        var now = DateTimeOffset.Parse("2026-05-25T12:00:00Z");
        var oldItem = new TranscriptionHistoryItem(
            Guid.NewGuid(),
            now.AddDays(-8),
            "old",
            "local-whisper",
            TimeSpan.Zero,
            TimeSpan.Zero,
            audioFilePath: oldAudioPath);
        var missingItem = oldItem with
        {
            Id = Guid.NewGuid(),
            Text = "missing",
            OriginalText = "missing",
            AudioFilePath = missingAudioPath
        };
        var historyStore = new FakeHistoryStore([oldItem, missingItem]);
        var settingsStore = new FakeSettingsStore(new AppSettings
        {
            IsAudioCleanupEnabled = true,
            AudioRetentionPeriod = 7
        });
        var service = new PrivacyCleanupService(
            settingsStore,
            historyStore,
            new FixedTimeProvider(now),
            temp.Path);

        var preview = await service.PreviewAudioCleanupAsync(CancellationToken.None);

        Assert.True(preview.IsEnabled);
        Assert.Equal(1, preview.FileCount);
        Assert.Equal(new FileInfo(oldAudioPath).Length, preview.TotalBytes);
    }

    [Fact]
    public async Task RunTranscriptCleanupAsync_KeepsRowsWhenAppOwnedAudioDeleteFails()
    {
        using var temp = new TempDirectory();
        var oldAudioPath = temp.WriteFile($"{Guid.NewGuid():N}.wav", "locked audio");
        await using var lockedFile = new FileStream(
            oldAudioPath,
            FileMode.Open,
            FileAccess.Read,
            FileShare.None);
        var now = DateTimeOffset.Parse("2026-05-25T12:00:00Z");
        var oldItem = new TranscriptionHistoryItem(
            Guid.NewGuid(),
            now.AddMinutes(-90),
            "old",
            "local-whisper",
            TimeSpan.Zero,
            TimeSpan.Zero,
            audioFilePath: oldAudioPath);
        var historyStore = new FakeHistoryStore([oldItem]);
        var settingsStore = new FakeSettingsStore(new AppSettings
        {
            IsTranscriptionCleanupEnabled = true,
            TranscriptionRetentionMinutes = 60
        });
        var service = new PrivacyCleanupService(
            settingsStore,
            historyStore,
            new FixedTimeProvider(now),
            temp.Path);

        var result = await service.RunTranscriptCleanupAsync(CancellationToken.None);

        Assert.True(result.IsEnabled);
        Assert.Equal(0, result.DeletedTranscriptionCount);
        Assert.Equal(0, result.DeletedAudioFileCount);
        Assert.Equal(1, result.FailedAudioFileCount);
        Assert.True(File.Exists(oldAudioPath));
        Assert.Equal(oldItem, Assert.Single(historyStore.Items));
    }

    [Fact]
    public async Task RunAudioCleanupAsync_DoesNotDeleteFilesOutsideRecordingsDirectory()
    {
        using var recordings = new TempDirectory();
        using var outside = new TempDirectory();
        var externalAudioPath = outside.WriteFile("external.wav", "external audio");
        var now = DateTimeOffset.Parse("2026-05-25T12:00:00Z");
        var item = new TranscriptionHistoryItem(
            Guid.NewGuid(),
            now.AddDays(-8),
            "external",
            "local-whisper",
            TimeSpan.Zero,
            TimeSpan.Zero,
            audioFilePath: externalAudioPath);
        var historyStore = new FakeHistoryStore([item]);
        var settingsStore = new FakeSettingsStore(new AppSettings
        {
            IsAudioCleanupEnabled = true,
            AudioRetentionPeriod = 7
        });
        var service = new PrivacyCleanupService(
            settingsStore,
            historyStore,
            new FixedTimeProvider(now),
            recordings.Path);

        var result = await service.RunAudioCleanupAsync(CancellationToken.None);

        Assert.True(result.IsEnabled);
        Assert.Equal(0, result.DeletedAudioFileCount);
        Assert.Equal(0, result.ClearedAudioReferenceCount);
        Assert.True(File.Exists(externalAudioPath));
        Assert.Equal(externalAudioPath, Assert.Single(historyStore.Items).AudioFilePath);
    }

    [Fact]
    public async Task RunAudioCleanupAsync_DoesNotDeleteUnexpectedFilesInRecordingsDirectory()
    {
        using var temp = new TempDirectory();
        var manualAudioPath = temp.WriteFile("manual.wav", "manual audio");
        var now = DateTimeOffset.Parse("2026-05-25T12:00:00Z");
        var item = new TranscriptionHistoryItem(
            Guid.NewGuid(),
            now.AddDays(-8),
            "manual",
            "local-whisper",
            TimeSpan.Zero,
            TimeSpan.Zero,
            audioFilePath: manualAudioPath);
        var historyStore = new FakeHistoryStore([item]);
        var settingsStore = new FakeSettingsStore(new AppSettings
        {
            IsAudioCleanupEnabled = true,
            AudioRetentionPeriod = 7
        });
        var service = new PrivacyCleanupService(
            settingsStore,
            historyStore,
            new FixedTimeProvider(now),
            temp.Path);

        var result = await service.RunAudioCleanupAsync(CancellationToken.None);

        Assert.True(result.IsEnabled);
        Assert.Equal(0, result.DeletedAudioFileCount);
        Assert.Equal(0, result.ClearedAudioReferenceCount);
        Assert.True(File.Exists(manualAudioPath));
        Assert.Equal(manualAudioPath, Assert.Single(historyStore.Items).AudioFilePath);
    }

    private sealed class FakeSettingsStore(AppSettings settings) : ISettingsStore
    {
        public Task<AppSettings> LoadAsync(CancellationToken cancellationToken) =>
            Task.FromResult(settings);

        public Task SaveAsync(AppSettings settings, CancellationToken cancellationToken) =>
            Task.CompletedTask;
    }

    private sealed class FakeHistoryStore(IReadOnlyList<TranscriptionHistoryItem> items) : IHistoryStore
    {
        public List<TranscriptionHistoryItem> Items { get; } = items.ToList();

        public Task SaveAsync(TranscriptionHistoryItem item, CancellationToken cancellationToken)
        {
            Items.Add(item);
            return Task.CompletedTask;
        }

        public Task<IReadOnlyList<TranscriptionHistoryItem>> ListRecentAsync(
            int limit,
            CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyList<TranscriptionHistoryItem>>(
                Items
                    .OrderByDescending(item => item.CreatedAt.UtcDateTime.Ticks)
                    .Take(limit)
                    .ToArray());

        public Task<IReadOnlyList<TranscriptionHistoryItem>> SearchAsync(
            string query,
            int limit,
            CancellationToken cancellationToken) =>
            ListRecentAsync(limit, cancellationToken);

        public Task<TranscriptionHistoryItem?> GetLatestCompletedAsync(CancellationToken cancellationToken) =>
            Task.FromResult(Items.LastOrDefault(item => item.Status == TranscriptionHistoryStatus.Completed));

        public Task<TranscriptionHistoryItem?> GetLatestCompletedWithAudioAsync(CancellationToken cancellationToken) =>
            Task.FromResult(Items.LastOrDefault(item =>
                item.Status == TranscriptionHistoryStatus.Completed
                && !string.IsNullOrWhiteSpace(item.AudioFilePath)));

        public Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken)
        {
            var index = Items.FindIndex(item => item.Id == id);
            if (index < 0)
            {
                return Task.FromResult(false);
            }

            Items.RemoveAt(index);
            return Task.FromResult(true);
        }

        public Task<IReadOnlyList<TranscriptionHistoryItem>> ListOlderThanAsync(
            DateTimeOffset cutoff,
            CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyList<TranscriptionHistoryItem>>(
                Items
                    .Where(item => item.CreatedAt.UtcDateTime.Ticks < cutoff.UtcDateTime.Ticks)
                    .OrderByDescending(item => item.CreatedAt.UtcDateTime.Ticks)
                    .ToArray());

        public Task<int> ClearAudioFilePathAsync(
            IReadOnlyCollection<Guid> ids,
            CancellationToken cancellationToken)
        {
            var idSet = ids.ToHashSet();
            var count = 0;
            for (var index = 0; index < Items.Count; index++)
            {
                var item = Items[index];
                if (!idSet.Contains(item.Id) || item.AudioFilePath is null)
                {
                    continue;
                }

                Items[index] = item with { AudioFilePath = null };
                count++;
            }

            return Task.FromResult(count);
        }
    }

    private sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }

    private sealed class TempDirectory : IDisposable
    {
        public string Path { get; } = System.IO.Path.Combine(
            System.IO.Path.GetTempPath(),
            $"voiceink-{Guid.NewGuid():N}");

        public TempDirectory()
        {
            Directory.CreateDirectory(Path);
        }

        public string WriteFile(string fileName, string content)
        {
            var path = System.IO.Path.Combine(Path, fileName);
            File.WriteAllText(path, content);
            return path;
        }

        public void Dispose()
        {
            if (Directory.Exists(Path))
            {
                Directory.Delete(Path, recursive: true);
            }
        }
    }
}
