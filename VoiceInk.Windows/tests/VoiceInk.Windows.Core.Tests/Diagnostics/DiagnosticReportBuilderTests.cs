using VoiceInk.Windows.Core.Diagnostics;
using Xunit;

namespace VoiceInk.Windows.Core.Tests.Diagnostics;

public sealed class DiagnosticReportBuilderTests
{
    [Fact]
    public void Build_IncludesSafeSystemPathsFilesStateAndRecentEvents()
    {
        var request = new DiagnosticReportRequest
        {
            ExportedAtUtc = DateTimeOffset.Parse("2026-05-25T12:34:56Z"),
            AppVersion = "1.2.3",
            OsDescription = "Windows 11",
            RuntimeDescription = ".NET 10",
            ProcessArchitecture = "X64",
            AppBaseDirectory = @"C:\VoiceInk\App",
            AppDataDirectory = @"C:\Users\Admin\AppData\Local\VoiceInk.Windows",
            RecordingsDirectory = @"C:\Users\Admin\AppData\Local\VoiceInk.Windows\Recordings",
            ActiveSection = "Dashboard",
            DictationState = "Idle",
            SelectedModelPath = @"C:\Models\ggml-base.en.bin",
            Files =
            [
                new DiagnosticFileEntry("Settings", @"C:\VoiceInk\settings.json", Exists: true, SizeBytes: 2048),
                new DiagnosticFileEntry("History", @"C:\VoiceInk\history.db", Exists: false, SizeBytes: null)
            ],
            RecentEvents =
            [
                "2026-05-25T12:00:00Z Loading settings",
                "2026-05-25T12:01:00Z Idle"
            ]
        };

        var report = DiagnosticReportBuilder.Build(request);

        Assert.Contains("=== VoiceInk for Windows Diagnostic Logs ===", report);
        Assert.Contains("Exported At UTC: 2026-05-25T12:34:56.0000000+00:00", report);
        Assert.Contains("App Version: 1.2.3", report);
        Assert.Contains("OS: Windows 11", report);
        Assert.Contains("Runtime: .NET 10", report);
        Assert.Contains("Process Architecture: X64", report);
        Assert.Contains(@"App Data: C:\Users\Admin\AppData\Local\VoiceInk.Windows", report);
        Assert.Contains(@"Recordings: C:\Users\Admin\AppData\Local\VoiceInk.Windows\Recordings", report);
        Assert.Contains("Settings: exists, 2048 bytes", report);
        Assert.Contains("History: missing", report);
        Assert.Contains("Active Section: Dashboard", report);
        Assert.Contains("Dictation State: Idle", report);
        Assert.Contains(@"Selected Model Path: C:\Models\ggml-base.en.bin", report);
        Assert.Contains("2026-05-25T12:00:00Z Loading settings", report);
        Assert.Contains("Privacy Notice", report);
    }

    [Fact]
    public void Build_RedactsSensitiveQueryValuesFromDiagnosticLines()
    {
        var request = new DiagnosticReportRequest
        {
            ExportedAtUtc = DateTimeOffset.Parse("2026-05-25T12:34:56Z"),
            AppVersion = "source build",
            OsDescription = "Windows",
            RuntimeDescription = ".NET",
            ProcessArchitecture = "X64",
            RecentEvents =
            [
                "Provider failed: https://api.example.test/v1?api_key=sk-secret&refresh_token=token-value-123&x-secret=hidden&x-credential=credential-value-123&key=plain-key&access_key=access-key&auth-key=auth-value-123&authorization=bearer-value&bearer=direct-bearer&model=whisper",
                "key=leading-secret model=whisper",
                @"Path remained: C:\Models\ggml-base.en.bin"
            ]
        };

        var report = DiagnosticReportBuilder.Build(request);

        Assert.DoesNotContain("sk-secret", report, StringComparison.Ordinal);
        Assert.DoesNotContain("token-value-123", report, StringComparison.Ordinal);
        Assert.DoesNotContain("hidden", report, StringComparison.Ordinal);
        Assert.DoesNotContain("credential-value-123", report, StringComparison.Ordinal);
        Assert.DoesNotContain("plain-key", report, StringComparison.Ordinal);
        Assert.DoesNotContain("access-key", report, StringComparison.Ordinal);
        Assert.DoesNotContain("auth-value-123", report, StringComparison.Ordinal);
        Assert.DoesNotContain("bearer-value", report, StringComparison.Ordinal);
        Assert.DoesNotContain("direct-bearer", report, StringComparison.Ordinal);
        Assert.DoesNotContain("leading-secret", report, StringComparison.Ordinal);
        Assert.Contains("api_key=[redacted]", report);
        Assert.Contains("refresh_token=[redacted]", report);
        Assert.Contains("x-secret=[redacted]", report);
        Assert.Contains("x-credential=[redacted]", report);
        Assert.Contains("key=[redacted]", report);
        Assert.Contains("access_key=[redacted]", report);
        Assert.Contains("auth-key=[redacted]", report);
        Assert.Contains("authorization=[redacted]", report);
        Assert.Contains("bearer=[redacted]", report);
        Assert.Contains("model=whisper", report);
        Assert.Contains(@"C:\Models\ggml-base.en.bin", report);
    }
}
