using System.Text;
using System.Text.RegularExpressions;

namespace VoiceInk.Windows.Core.Diagnostics;

public static partial class DiagnosticReportBuilder
{
    public static string Build(DiagnosticReportRequest request)
    {
        var builder = new StringBuilder();
        builder.AppendLine("=== VoiceInk for Windows Diagnostic Logs ===");
        builder.AppendLine($"Exported At UTC: {request.ExportedAtUtc.ToUniversalTime():O}");
        builder.AppendLine();

        builder.AppendLine("System");
        builder.AppendLine($"App Version: {Sanitize(request.AppVersion)}");
        builder.AppendLine($"OS: {Sanitize(request.OsDescription)}");
        builder.AppendLine($"Runtime: {Sanitize(request.RuntimeDescription)}");
        builder.AppendLine($"Process Architecture: {Sanitize(request.ProcessArchitecture)}");
        builder.AppendLine();

        builder.AppendLine("Paths");
        builder.AppendLine($"App Base: {Sanitize(request.AppBaseDirectory)}");
        builder.AppendLine($"App Data: {Sanitize(request.AppDataDirectory)}");
        builder.AppendLine($"Recordings: {Sanitize(request.RecordingsDirectory)}");
        builder.AppendLine();

        builder.AppendLine("Files");
        foreach (var file in request.Files)
        {
            builder.AppendLine(file.Exists
                ? $"{Sanitize(file.Label)}: exists, {file.SizeBytes ?? 0} bytes ({Sanitize(file.Path)})"
                : $"{Sanitize(file.Label)}: missing ({Sanitize(file.Path)})");
        }

        if (request.Files.Count == 0)
        {
            builder.AppendLine("No known files were reported.");
        }

        builder.AppendLine();
        builder.AppendLine("State");
        builder.AppendLine($"Active Section: {Sanitize(request.ActiveSection)}");
        builder.AppendLine($"Dictation State: {Sanitize(request.DictationState)}");
        builder.AppendLine($"Selected Model Path: {Sanitize(request.SelectedModelPath)}");
        builder.AppendLine();

        builder.AppendLine("Recent Events");
        foreach (var recentEvent in request.RecentEvents.TakeLast(200))
        {
            builder.AppendLine(Sanitize(recentEvent));
        }

        if (request.RecentEvents.Count == 0)
        {
            builder.AppendLine("No recent in-app status events were recorded.");
        }

        builder.AppendLine();
        builder.AppendLine("Privacy Notice");
        builder.AppendLine("This local diagnostic export excludes API keys, Credential Manager values, environment variables, clipboard contents, transcript/history text, rendered AI prompt contents, and settings file contents.");

        return builder.ToString();
    }

    private static string Sanitize(string? text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return string.Empty;
        }

        return QueryParameterRegex().Replace(
            text,
            match => IsSensitiveQueryKey(match.Groups["key"].Value)
                ? $"{match.Groups["prefix"].Value}{match.Groups["key"].Value}=[redacted]"
                : match.Value);
    }

    private static bool IsSensitiveQueryKey(string key)
    {
        var normalized = Uri.UnescapeDataString(key)
            .Trim()
            .Replace("_", string.Empty, StringComparison.Ordinal)
            .Replace("-", string.Empty, StringComparison.Ordinal)
            .Replace(".", string.Empty, StringComparison.Ordinal)
            .ToLowerInvariant();
        return normalized.Contains("token", StringComparison.Ordinal)
            || normalized.Contains("secret", StringComparison.Ordinal)
            || normalized.Contains("credential", StringComparison.Ordinal)
            || normalized is "apikey"
            or "apitoken"
            or "accesskey"
            or "accesstoken"
            or "authkey"
            or "authtoken"
            or "authorization"
            or "bearer"
            or "bearertoken"
            or "clientsecret"
            or "credential"
            or "key"
            or "secret"
            or "token";
    }

    [GeneratedRegex(
        @"(?<prefix>^|[?&\s])(?<key>[\w.-]+)=[^&\s]+",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex QueryParameterRegex();
}
