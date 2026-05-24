namespace VoiceInk.Windows.Core.Services;

public interface ISecretStore
{
    Task<string?> ReadSecretAsync(string name, CancellationToken cancellationToken);

    Task SaveSecretAsync(string name, string secret, CancellationToken cancellationToken);

    Task DeleteSecretAsync(string name, CancellationToken cancellationToken);

    Task<bool> HasSecretAsync(string name, CancellationToken cancellationToken);
}
