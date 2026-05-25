namespace VoiceInk.Windows.Core.Enhancement;

public static class BrowserUrlContextSanitizer
{
    public static string Sanitize(string url)
    {
        var trimmed = url.Trim();
        if (trimmed.Length == 0
            || !Uri.TryCreate(trimmed, UriKind.Absolute, out var uri)
            || (uri.Scheme != Uri.UriSchemeHttps && uri.Scheme != Uri.UriSchemeHttp))
        {
            return string.Empty;
        }

        var builder = new UriBuilder(uri)
        {
            UserName = string.Empty,
            Password = string.Empty,
            Query = string.Empty,
            Fragment = string.Empty
        };

        return builder.Uri.AbsoluteUri;
    }
}
