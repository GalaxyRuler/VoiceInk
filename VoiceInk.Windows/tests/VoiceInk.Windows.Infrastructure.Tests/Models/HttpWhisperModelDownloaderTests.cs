using System.Net;
using System.Text;
using VoiceInk.Windows.Core.Models;
using VoiceInk.Windows.Infrastructure.Models;
using Xunit;

namespace VoiceInk.Windows.Infrastructure.Tests.Models;

public sealed class HttpWhisperModelDownloaderTests : IDisposable
{
    private readonly string modelsDirectory = Path.Combine(
        Path.GetTempPath(),
        "VoiceInk.Windows.Tests",
        Guid.NewGuid().ToString("N"));

    [Fact]
    public async Task DownloadAsync_WritesModelToDestinationAndReportsProgress()
    {
        var handler = new QueueHttpMessageHandler(
            _ => new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new ByteArrayContent(Encoding.UTF8.GetBytes("model-bytes"))
                {
                    Headers = { ContentLength = "model-bytes".Length }
                }
            });
        var downloader = new HttpWhisperModelDownloader(new HttpClient(handler));
        var model = WhisperModelCatalog.All.Single(entry => entry.Name == "ggml-base.en");
        var progressReports = new List<WhisperModelDownloadProgress>();

        var result = await downloader.DownloadAsync(
            model,
            modelsDirectory,
            new Progress<WhisperModelDownloadProgress>(progressReports.Add),
            CancellationToken.None);

        Assert.Equal(Path.Combine(modelsDirectory, "ggml-base.en.bin"), result.Path);
        Assert.Equal("ggml-base.en", result.DisplayName);
        Assert.Equal("model-bytes", await File.ReadAllTextAsync(result.Path));
        Assert.Contains(progressReports, item => item.FractionComplete == 1);
        Assert.Empty(Directory.GetFiles(modelsDirectory, "*.download"));
    }

    [Fact]
    public async Task DownloadAsync_FailedStatusDeletesTemporaryFile()
    {
        var handler = new QueueHttpMessageHandler(
            _ => new HttpResponseMessage(HttpStatusCode.InternalServerError)
            {
                Content = new StringContent("server failed")
            });
        var downloader = new HttpWhisperModelDownloader(new HttpClient(handler));
        var model = WhisperModelCatalog.All.Single(entry => entry.Name == "ggml-base.en");

        await Assert.ThrowsAsync<HttpRequestException>(() => downloader.DownloadAsync(
            model,
            modelsDirectory,
            progress: null,
            CancellationToken.None));

        Assert.True(Directory.Exists(modelsDirectory));
        Assert.Empty(Directory.GetFiles(modelsDirectory));
    }

    [Fact]
    public async Task DownloadAsync_OverwritesExistingCompleteFileOnlyAfterSuccess()
    {
        Directory.CreateDirectory(modelsDirectory);
        var modelPath = Path.Combine(modelsDirectory, "ggml-base.en.bin");
        await File.WriteAllTextAsync(modelPath, "previous-model");
        var handler = new QueueHttpMessageHandler(
            _ => new HttpResponseMessage(HttpStatusCode.ServiceUnavailable));
        var downloader = new HttpWhisperModelDownloader(new HttpClient(handler));
        var model = WhisperModelCatalog.All.Single(entry => entry.Name == "ggml-base.en");

        await Assert.ThrowsAsync<HttpRequestException>(() => downloader.DownloadAsync(
            model,
            modelsDirectory,
            progress: null,
            CancellationToken.None));

        Assert.Equal("previous-model", await File.ReadAllTextAsync(modelPath));
    }

    [Fact]
    public async Task DownloadAsync_CancellationDeletesTemporaryFile()
    {
        using var cancellation = new CancellationTokenSource();
        var handler = new QueueHttpMessageHandler(
            _ => new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StreamContent(new CancelAfterFirstReadStream(
                    Encoding.UTF8.GetBytes("partial-model"),
                    cancellation))
                {
                    Headers = { ContentLength = 128 }
                }
            });
        var downloader = new HttpWhisperModelDownloader(new HttpClient(handler));
        var model = WhisperModelCatalog.All.Single(entry => entry.Name == "ggml-base.en");

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => downloader.DownloadAsync(
            model,
            modelsDirectory,
            progress: null,
            cancellation.Token));

        Assert.True(Directory.Exists(modelsDirectory));
        Assert.Empty(Directory.GetFiles(modelsDirectory));
    }

    public void Dispose()
    {
        if (Directory.Exists(modelsDirectory))
        {
            Directory.Delete(modelsDirectory, recursive: true);
        }
    }

    private sealed class QueueHttpMessageHandler(params Func<HttpRequestMessage, HttpResponseMessage>[] responses)
        : HttpMessageHandler
    {
        private readonly Queue<Func<HttpRequestMessage, HttpResponseMessage>> responses = new(responses);

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            if (responses.Count == 0)
            {
                throw new InvalidOperationException("No HTTP responses queued.");
            }

            return Task.FromResult(responses.Dequeue()(request));
        }
    }

    private sealed class CancelAfterFirstReadStream(
        byte[] bytes,
        CancellationTokenSource cancellation) : Stream
    {
        private bool hasRead;

        public override bool CanRead => true;

        public override bool CanSeek => false;

        public override bool CanWrite => false;

        public override long Length => bytes.Length;

        public override long Position
        {
            get => hasRead ? bytes.Length : 0;
            set => throw new NotSupportedException();
        }

        public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (hasRead)
            {
                return ValueTask.FromResult(0);
            }

            bytes.CopyTo(buffer);
            hasRead = true;
            cancellation.Cancel();
            return ValueTask.FromResult(bytes.Length);
        }

        public override int Read(byte[] buffer, int offset, int count) =>
            throw new NotSupportedException();

        public override void Flush()
        {
        }

        public override long Seek(long offset, SeekOrigin origin) =>
            throw new NotSupportedException();

        public override void SetLength(long value) =>
            throw new NotSupportedException();

        public override void Write(byte[] buffer, int offset, int count) =>
            throw new NotSupportedException();
    }
}
