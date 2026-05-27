using VoiceInk.Windows.Core.PowerMode;
using Xunit;

namespace VoiceInk.Windows.Core.Tests.PowerMode;

public sealed class PowerModeTargetFieldComposerTests
{
    [Fact]
    public void AppendTarget_AddsCurrentTargetToExistingAlternatives()
    {
        var fields = PowerModeTargetFieldComposer.AppendTarget(
            new PowerModeTargetFields(
                ProcessNamePattern: "winword",
                WindowTitlePattern: "Planning",
                BrowserUrlPattern: "docs.example.com"),
            new PowerModeTarget(
                ProcessName: "notepad",
                WindowTitle: "Daily Note",
                ProcessId: 42,
                BrowserUrl: "notes.example.com"));

        Assert.Equal("winword; notepad", fields.ProcessNamePattern);
        Assert.Equal("Planning; Daily Note", fields.WindowTitlePattern);
        Assert.Equal("docs.example.com; notes.example.com", fields.BrowserUrlPattern);
    }

    [Fact]
    public void AppendTarget_DeduplicatesAlternativesCaseInsensitively()
    {
        var fields = PowerModeTargetFieldComposer.AppendTarget(
            new PowerModeTargetFields(
                ProcessNamePattern: "WINWORD; notepad",
                WindowTitlePattern: "Planning",
                BrowserUrlPattern: "https://docs.example.com/path"),
            new PowerModeTarget(
                ProcessName: "winword",
                WindowTitle: "planning",
                ProcessId: 42,
                BrowserUrl: "https://docs.example.com/path"));

        Assert.Equal("WINWORD; notepad", fields.ProcessNamePattern);
        Assert.Equal("Planning", fields.WindowTitlePattern);
        Assert.Equal("https://docs.example.com/path", fields.BrowserUrlPattern);
    }

    [Fact]
    public void AppendTarget_SkipsEmptyTargetValues()
    {
        var fields = PowerModeTargetFieldComposer.AppendTarget(
            new PowerModeTargetFields(
                ProcessNamePattern: "code",
                WindowTitlePattern: "",
                BrowserUrlPattern: ""),
            new PowerModeTarget(
                ProcessName: "",
                WindowTitle: "   ",
                ProcessId: null,
                BrowserUrl: ""));

        Assert.Equal("code", fields.ProcessNamePattern);
        Assert.Equal("", fields.WindowTitlePattern);
        Assert.Equal("", fields.BrowserUrlPattern);
    }
}
