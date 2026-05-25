namespace VoiceInk.Windows.Infrastructure.Transcription;

public interface IStreamingWebSocket : IAsyncDisposable
{
    Task ConnectAsync(
        Uri uri,
        string authorizationHeader,
        CancellationToken cancellationToken);

    Task SendBinaryAsync(ReadOnlyMemory<byte> bytes, CancellationToken cancellationToken);

    IAsyncEnumerable<string> ReceiveTextMessagesAsync(CancellationToken cancellationToken);

    Task CloseAsync(CancellationToken cancellationToken);

    void Abort();
}
