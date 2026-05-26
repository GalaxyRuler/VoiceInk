namespace VoiceInk.Windows.Infrastructure.Transcription;

public interface IStreamingWebSocket : IAsyncDisposable
{
    Task ConnectAsync(
        Uri uri,
        string authorizationHeader,
        CancellationToken cancellationToken);

    Task ConnectAsync(
        Uri uri,
        IReadOnlyDictionary<string, string> headers,
        CancellationToken cancellationToken) =>
        ConnectAsync(
            uri,
            headers.TryGetValue("Authorization", out var authorizationHeader)
                ? authorizationHeader
                : string.Empty,
            cancellationToken);

    Task SendBinaryAsync(ReadOnlyMemory<byte> bytes, CancellationToken cancellationToken);

    Task SendTextAsync(string text, CancellationToken cancellationToken);

    IAsyncEnumerable<string> ReceiveTextMessagesAsync(CancellationToken cancellationToken);

    Task CloseAsync(CancellationToken cancellationToken);

    void Abort();
}
