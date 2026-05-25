using VoiceInk.Windows.Core.Recording;
using Xunit;

namespace VoiceInk.Windows.Core.Tests.Recording;

public sealed class CustomRecordingSoundImporterTests
{
    [Theory]
    [InlineData(null, "systemDefault")]
    [InlineData("", "systemDefault")]
    [InlineData("  ", "systemDefault")]
    [InlineData("SYSTEMDEFAULT", "systemDefault")]
    [InlineData("custom", "custom")]
    [InlineData("Custom", "custom")]
    [InlineData("unknown", "systemDefault")]
    public void Normalize_ReturnsSupportedSoundModes(string? mode, string expected)
    {
        Assert.Equal(expected, RecordingSoundModeSettings.Normalize(mode));
    }

    [Fact]
    public void Import_RejectsUnsupportedExtension()
    {
        var fileSystem = new FakeRecordingSoundFileSystem();
        fileSystem.AddFile(@"C:\Downloads\start.flac");
        var importer = CreateImporter(fileSystem);

        var result = importer.Import(@"C:\Downloads\start.flac", RecordingSoundKind.Start);

        Assert.False(result.IsSuccess);
        Assert.Equal(string.Empty, result.CustomSoundPath);
        Assert.Contains("Unsupported", result.ErrorMessage);
        Assert.Empty(fileSystem.CopiedFiles);
    }

    [Fact]
    public void Import_RejectsMissingSourceFile()
    {
        var importer = CreateImporter();

        var result = importer.Import(@"C:\Downloads\missing.wav", RecordingSoundKind.Start);

        Assert.False(result.IsSuccess);
        Assert.Contains("does not exist", result.ErrorMessage);
    }

    [Fact]
    public void Import_RejectsUnreadableAudioFile()
    {
        var fileSystem = new FakeRecordingSoundFileSystem();
        fileSystem.AddFile(@"C:\Downloads\start.wav");
        var probe = new FakeRecordingSoundFileProbe();
        probe.SetFailure(@"C:\Downloads\start.wav", "Cannot decode audio file.");
        var importer = CreateImporter(fileSystem, probe);

        var result = importer.Import(@"C:\Downloads\start.wav", RecordingSoundKind.Start);

        Assert.False(result.IsSuccess);
        Assert.Contains("Cannot decode", result.ErrorMessage);
        Assert.Empty(fileSystem.CopiedFiles);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(3.1)]
    public void Import_RejectsInvalidDuration(double seconds)
    {
        var fileSystem = new FakeRecordingSoundFileSystem();
        fileSystem.AddFile(@"C:\Downloads\stop.mp3");
        var probe = new FakeRecordingSoundFileProbe();
        probe.SetDuration(@"C:\Downloads\stop.mp3", TimeSpan.FromSeconds(seconds));
        var importer = CreateImporter(fileSystem, probe);

        var result = importer.Import(@"C:\Downloads\stop.mp3", RecordingSoundKind.Stop);

        Assert.False(result.IsSuccess);
        Assert.NotEmpty(result.ErrorMessage);
        Assert.Empty(fileSystem.CopiedFiles);
    }

    [Fact]
    public void Import_CopiesToStableDestinationAndReplacesPreviousKindFile()
    {
        var fileSystem = new FakeRecordingSoundFileSystem();
        fileSystem.AddFile(@"C:\Downloads\start.wav");
        fileSystem.AddFile(@"C:\VoiceInk\Sounds\CustomStartSound.mp3");
        fileSystem.AddFile(@"C:\VoiceInk\Sounds\CustomStopSound.mp3");
        var probe = new FakeRecordingSoundFileProbe();
        probe.SetDuration(@"C:\Downloads\start.wav", TimeSpan.FromSeconds(2.5));
        var importer = CreateImporter(fileSystem, probe);

        var result = importer.Import(@"C:\Downloads\start.wav", RecordingSoundKind.Start);

        Assert.True(result.IsSuccess);
        Assert.Equal(@"C:\VoiceInk\Sounds\CustomStartSound.wav", result.CustomSoundPath);
        Assert.Equal("CustomStartSound.wav", result.FileName);
        Assert.Contains(@"C:\VoiceInk\Sounds", fileSystem.CreatedDirectories);
        Assert.Equal([(@"C:\Downloads\start.wav", @"C:\VoiceInk\Sounds\CustomStartSound.wav")], fileSystem.CopiedFiles);
        Assert.Contains(@"C:\VoiceInk\Sounds\CustomStartSound.mp3", fileSystem.DeletedFiles);
        Assert.DoesNotContain(@"C:\VoiceInk\Sounds\CustomStopSound.mp3", fileSystem.DeletedFiles);
    }

    [Fact]
    public void Reset_DeletesOnlyFilesForRequestedKind()
    {
        var fileSystem = new FakeRecordingSoundFileSystem();
        fileSystem.AddFile(@"C:\VoiceInk\Sounds\CustomStartSound.wav");
        fileSystem.AddFile(@"C:\VoiceInk\Sounds\CustomStartSound.mp3");
        fileSystem.AddFile(@"C:\VoiceInk\Sounds\CustomStopSound.wav");
        var importer = CreateImporter(fileSystem);

        importer.Reset(RecordingSoundKind.Start);

        Assert.Contains(@"C:\VoiceInk\Sounds\CustomStartSound.wav", fileSystem.DeletedFiles);
        Assert.Contains(@"C:\VoiceInk\Sounds\CustomStartSound.mp3", fileSystem.DeletedFiles);
        Assert.DoesNotContain(@"C:\VoiceInk\Sounds\CustomStopSound.wav", fileSystem.DeletedFiles);
    }

    private static CustomRecordingSoundImporter CreateImporter(
        FakeRecordingSoundFileSystem? fileSystem = null,
        FakeRecordingSoundFileProbe? probe = null) =>
        new(
            fileSystem ?? new FakeRecordingSoundFileSystem(),
            probe ?? new FakeRecordingSoundFileProbe(),
            @"C:\VoiceInk\Sounds");

    private sealed class FakeRecordingSoundFileProbe : IRecordingSoundFileProbe
    {
        private readonly Dictionary<string, RecordingSoundFileProbeResult> results = new(StringComparer.OrdinalIgnoreCase);

        public void SetDuration(string filePath, TimeSpan duration)
        {
            results[filePath] = RecordingSoundFileProbeResult.Success(duration);
        }

        public void SetFailure(string filePath, string errorMessage)
        {
            results[filePath] = RecordingSoundFileProbeResult.Failure(errorMessage);
        }

        public RecordingSoundFileProbeResult Probe(string filePath) =>
            results.TryGetValue(filePath, out var result)
                ? result
                : RecordingSoundFileProbeResult.Success(TimeSpan.FromSeconds(1));
    }

    private sealed class FakeRecordingSoundFileSystem : IRecordingSoundFileSystem
    {
        private readonly HashSet<string> files = new(StringComparer.OrdinalIgnoreCase);

        public List<string> CreatedDirectories { get; } = [];
        public List<(string SourcePath, string DestinationPath)> CopiedFiles { get; } = [];
        public List<string> DeletedFiles { get; } = [];

        public void AddFile(string filePath)
        {
            files.Add(filePath);
        }

        public bool FileExists(string filePath) => files.Contains(filePath);

        public void CreateDirectory(string directoryPath)
        {
            CreatedDirectories.Add(directoryPath);
        }

        public void CopyFile(string sourcePath, string destinationPath, bool overwrite)
        {
            CopiedFiles.Add((sourcePath, destinationPath));
            files.Add(destinationPath);
        }

        public void DeleteFileIfExists(string filePath)
        {
            DeletedFiles.Add(filePath);
            files.Remove(filePath);
        }

        public IEnumerable<string> EnumerateFiles(string directoryPath, string searchPattern)
        {
            var prefix = searchPattern.TrimEnd('*');
            return files
                .Where(file => Path.GetDirectoryName(file)?.Equals(directoryPath, StringComparison.OrdinalIgnoreCase) == true)
                .Where(file => Path.GetFileName(file).StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                .ToArray();
        }
    }
}
