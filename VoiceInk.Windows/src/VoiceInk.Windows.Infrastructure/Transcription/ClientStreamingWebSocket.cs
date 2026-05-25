using System.Net.WebSockets;
using System.Runtime.CompilerServices;
using System.Text;

namespace VoiceInk.Windows.Infrastructure.Transcription;

public sealed class ClientStreamingWebSocket : IStreamingWebSocket
{
    private readonly ClientWebSocket webSocket = new();

    public async Task ConnectAsync(
        Uri uri,
        string authorizationHeader,
        CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(authorizationHeader))
        {
            webSocket.Options.SetRequestHeader("Authorization", authorizationHeader);
        }

        await webSocket.ConnectAsync(uri, cancellationToken).ConfigureAwait(false);
    }

    public Task SendBinaryAsync(ReadOnlyMemory<byte> bytes, CancellationToken cancellationToken) =>
        webSocket.SendAsync(bytes, WebSocketMessageType.Binary, endOfMessage: true, cancellationToken).AsTask();

    public async IAsyncEnumerable<string> ReceiveTextMessagesAsync(
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var buffer = new byte[8192];
        using var message = new MemoryStream();

        while (webSocket.State is WebSocketState.Open or WebSocketState.CloseSent)
        {
            var result = await webSocket.ReceiveAsync(buffer.AsMemory(), cancellationToken)
                .ConfigureAwait(false);
            if (result.MessageType == WebSocketMessageType.Close)
            {
                yield break;
            }

            if (result.MessageType != WebSocketMessageType.Text)
            {
                if (result.EndOfMessage)
                {
                    message.SetLength(0);
                }

                continue;
            }

            message.Write(buffer, 0, result.Count);
            if (!result.EndOfMessage)
            {
                continue;
            }

            yield return Encoding.UTF8.GetString(message.ToArray());
            message.SetLength(0);
        }
    }

    public async Task CloseAsync(CancellationToken cancellationToken)
    {
        if (webSocket.State is WebSocketState.Open or WebSocketState.CloseReceived)
        {
            await webSocket.CloseAsync(
                    WebSocketCloseStatus.NormalClosure,
                    "VoiceInk live preview ended",
                    cancellationToken)
                .ConfigureAwait(false);
        }

        webSocket.Dispose();
    }

    public void Abort()
    {
        webSocket.Abort();
    }

    public ValueTask DisposeAsync()
    {
        webSocket.Dispose();
        return ValueTask.CompletedTask;
    }
}
