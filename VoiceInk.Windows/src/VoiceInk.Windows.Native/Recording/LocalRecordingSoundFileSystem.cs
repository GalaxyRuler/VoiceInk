using VoiceInk.Windows.Core.Recording;

namespace VoiceInk.Windows.Native.Recording;

public sealed class LocalRecordingSoundFileSystem : IRecordingSoundFileSystem
{
    public bool FileExists(string filePath) => File.Exists(filePath);

    public void CreateDirectory(string directoryPath)
    {
        Directory.CreateDirectory(directoryPath);
    }

    public void CopyFile(string sourcePath, string destinationPath, bool overwrite)
    {
        File.Copy(sourcePath, destinationPath, overwrite);
    }

    public void DeleteFileIfExists(string filePath)
    {
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }
    }

    public IEnumerable<string> EnumerateFiles(string directoryPath, string searchPattern) =>
        Directory.Exists(directoryPath)
            ? Directory.EnumerateFiles(directoryPath, searchPattern, SearchOption.TopDirectoryOnly)
            : [];
}
