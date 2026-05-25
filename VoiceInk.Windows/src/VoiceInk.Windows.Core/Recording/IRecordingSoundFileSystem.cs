namespace VoiceInk.Windows.Core.Recording;

public interface IRecordingSoundFileSystem
{
    bool FileExists(string filePath);
    void CreateDirectory(string directoryPath);
    void CopyFile(string sourcePath, string destinationPath, bool overwrite);
    void DeleteFileIfExists(string filePath);
    IEnumerable<string> EnumerateFiles(string directoryPath, string searchPattern);
}
