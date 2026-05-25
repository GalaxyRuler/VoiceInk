using VoiceInk.Windows.Core.Enhancement;
using Xunit;

namespace VoiceInk.Windows.Core.Tests.Enhancement;

public sealed class BrowserUrlContextSanitizerTests
{
    [Fact]
    public void Sanitize_RemovesQueryFragmentAndCredentials()
    {
        var sanitized = BrowserUrlContextSanitizer.Sanitize(
            "https://user:secret@example.com/docs/page?token=abc#section");

        Assert.Equal("https://example.com/docs/page", sanitized);
    }

    [Theory]
    [InlineData("http://example.com/")]
    [InlineData("https://example.com/path")]
    public void Sanitize_AllowsHttpAndHttpsUrls(string url)
    {
        Assert.Equal(url, BrowserUrlContextSanitizer.Sanitize(url));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("not a url")]
    [InlineData("file:///C:/Users/Admin/private.txt")]
    [InlineData("javascript:alert(1)")]
    public void Sanitize_RejectsEmptyInvalidAndNonWebUrls(string url)
    {
        Assert.Equal(string.Empty, BrowserUrlContextSanitizer.Sanitize(url));
    }
}
