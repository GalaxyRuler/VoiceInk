namespace VoiceInk.Windows.Core.Backup;

internal static class VoiceInkSettingsBackupEndpointSanitizer
{
    private static readonly string[] SecretNameFragments =
    [
        "apikey",
        "apitoken",
        "accesskey",
        "accesstoken",
        "authkey",
        "authtoken",
        "bearertoken",
        "clientsecret",
        "credential",
        "secret",
        "token"
    ];

    private static readonly HashSet<string> ExactSecretNames = new(StringComparer.Ordinal)
    {
        "authorization",
        "bearer",
        "key"
    };

    public static string Sanitize(string endpoint)
    {
        var trimmed = endpoint.Trim();
        if (trimmed.Length == 0)
        {
            return string.Empty;
        }

        if (ContainsCredentialLikeAuthority(trimmed)
            || ContainsSecretQueryParameter(ExtractRawQuery(trimmed)))
        {
            return string.Empty;
        }

        if (!Uri.TryCreate(trimmed, UriKind.Absolute, out var uri))
        {
            return trimmed;
        }

        return ContainsCredentials(uri) || ContainsSecretQueryParameter(uri)
            ? string.Empty
            : trimmed;
    }

    private static bool ContainsCredentials(Uri uri) =>
        !string.IsNullOrWhiteSpace(uri.UserInfo);

    private static bool ContainsCredentialLikeAuthority(string endpoint)
    {
        int authorityStartIndex;
        if (endpoint.StartsWith("//", StringComparison.Ordinal))
        {
            authorityStartIndex = 2;
        }
        else
        {
            var schemeSeparatorIndex = endpoint.IndexOf("://", StringComparison.Ordinal);
            if (schemeSeparatorIndex < 0)
            {
                return false;
            }

            authorityStartIndex = schemeSeparatorIndex + 3;
        }

        var authorityEndIndex = endpoint.IndexOfAny(['/', '?', '#'], authorityStartIndex);
        var authority = authorityEndIndex < 0
            ? endpoint[authorityStartIndex..]
            : endpoint[authorityStartIndex..authorityEndIndex];

        return authority.IndexOf('@', StringComparison.Ordinal) > 0;
    }

    private static string ExtractRawQuery(string endpoint)
    {
        var queryStartIndex = endpoint.IndexOf('?', StringComparison.Ordinal);
        if (queryStartIndex < 0)
        {
            return string.Empty;
        }

        var queryEndIndex = endpoint.IndexOf('#', queryStartIndex + 1);
        return queryEndIndex < 0
            ? endpoint[(queryStartIndex + 1)..]
            : endpoint[(queryStartIndex + 1)..queryEndIndex];
    }

    private static bool ContainsSecretQueryParameter(Uri uri) =>
        ContainsSecretQueryParameter(uri.Query.TrimStart('?'));

    private static bool ContainsSecretQueryParameter(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return false;
        }

        foreach (var pair in query.Split('&', StringSplitOptions.RemoveEmptyEntries))
        {
            var rawName = pair.Split('=', 2)[0];
            var normalized = Uri.UnescapeDataString(rawName)
                .Trim()
                .Replace("_", string.Empty)
                .Replace("-", string.Empty)
                .Replace(".", string.Empty)
                .ToLowerInvariant();
            if (ExactSecretNames.Contains(normalized)
                || SecretNameFragments.Any(fragment => normalized.Contains(fragment, StringComparison.Ordinal)))
            {
                return true;
            }
        }

        return false;
    }
}
