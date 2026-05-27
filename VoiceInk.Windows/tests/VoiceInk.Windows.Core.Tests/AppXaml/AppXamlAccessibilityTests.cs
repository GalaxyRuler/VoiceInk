using Xunit;

namespace VoiceInk.Windows.Core.Tests.AppXaml;

public sealed class AppXamlAccessibilityTests
{
    [Fact]
    public void MainWindow_DiagnosticsGuidanceRows_BindAccessibleName()
    {
        var xaml = File.ReadAllText(SourcePath("VoiceInk.Windows", "src", "VoiceInk.Windows.App", "MainWindow.xaml"));
        var listStart = xaml.IndexOf("x:Name=\"DiagnosticsGuidanceListView\"", StringComparison.Ordinal);
        Assert.True(listStart >= 0);
        var templateStart = xaml.IndexOf("<DataTemplate>", listStart, StringComparison.Ordinal);
        var templateEnd = xaml.IndexOf("</DataTemplate>", templateStart, StringComparison.Ordinal);
        Assert.True(templateStart >= 0);
        Assert.True(templateEnd > templateStart);

        var template = xaml[templateStart..templateEnd];
        Assert.Contains("AutomationProperties.Name=\"{Binding AccessibleName}\"", template);
    }

    [Fact]
    public void MainWindow_OnboardingDialogControls_SetAutomationNames()
    {
        var code = File.ReadAllText(SourcePath("VoiceInk.Windows", "src", "VoiceInk.Windows.App", "MainWindow.xaml.cs"));

        Assert.Contains("AutomationProperties.SetName(modelPathTextBox", code);
        Assert.Contains("AutomationProperties.SetName(browseModelButton", code);
        Assert.Contains("AutomationProperties.SetName(recommendedModelComboBox", code);
        Assert.Contains("AutomationProperties.SetName(downloadRecommendedModelButton", code);
        Assert.Contains("AutomationProperties.SetName(audioInputComboBox", code);
        Assert.Contains("AutomationProperties.SetName(microphoneSettingsButton", code);
        Assert.Contains("AutomationProperties.SetName(refreshMicrophonesButton", code);
        Assert.Contains("AutomationProperties.SetName(shortcutTextBox", code);
        Assert.Contains("AutomationProperties.SetName(onboardingActionListView", code);
        Assert.Contains("AutomationProperties.SetName(onboardingSummaryListView", code);
        Assert.Contains("AutomationProperties.SetName(onboardingStagesListView", code);
        Assert.Contains("AutomationProperties.SetName(onboardingTutorialListView", code);
    }

    [Theory]
    [InlineData("MetricsDashboardCardsListView")]
    [InlineData("MetricsDataGuidanceListView")]
    [InlineData("MetricsActionListView")]
    [InlineData("MetricsDiagnosticsListView")]
    public void MainWindow_MetricsRows_BindAccessibleName(string listViewName)
    {
        var xaml = File.ReadAllText(SourcePath("VoiceInk.Windows", "src", "VoiceInk.Windows.App", "MainWindow.xaml"));
        var listStart = xaml.IndexOf($"x:Name=\"{listViewName}\"", StringComparison.Ordinal);
        Assert.True(listStart >= 0);
        var templateStart = xaml.IndexOf("<DataTemplate>", listStart, StringComparison.Ordinal);
        var templateEnd = xaml.IndexOf("</DataTemplate>", templateStart, StringComparison.Ordinal);
        Assert.True(templateStart >= 0);
        Assert.True(templateEnd > templateStart);

        var template = xaml[templateStart..templateEnd];
        Assert.Contains("AutomationProperties.Name=\"{Binding AccessibleName}\"", template);
    }

    [Theory]
    [InlineData("TranscriptionModelPerformanceListView")]
    [InlineData("EnhancementModelPerformanceListView")]
    public void MainWindow_ModelPerformanceRows_BindAccessibleName(string listViewName)
    {
        var xaml = File.ReadAllText(SourcePath("VoiceInk.Windows", "src", "VoiceInk.Windows.App", "MainWindow.xaml"));
        var listStart = xaml.IndexOf($"x:Name=\"{listViewName}\"", StringComparison.Ordinal);
        Assert.True(listStart >= 0);
        var templateStart = xaml.IndexOf("<DataTemplate>", listStart, StringComparison.Ordinal);
        var templateEnd = xaml.IndexOf("</DataTemplate>", templateStart, StringComparison.Ordinal);
        Assert.True(templateStart >= 0);
        Assert.True(templateEnd > templateStart);

        var template = xaml[templateStart..templateEnd];
        Assert.Contains("AutomationProperties.Name=\"{Binding AccessibleName}\"", template);
    }

    [Theory]
    [InlineData("ModelLibraryActionListView")]
    [InlineData("ModelLibraryStorageGuidanceListView")]
    [InlineData("LocalModelCatalogListView")]
    public void MainWindow_ModelLibraryRows_BindAccessibleName(string listViewName)
    {
        var xaml = File.ReadAllText(SourcePath("VoiceInk.Windows", "src", "VoiceInk.Windows.App", "MainWindow.xaml"));
        var listStart = xaml.IndexOf($"x:Name=\"{listViewName}\"", StringComparison.Ordinal);
        Assert.True(listStart >= 0);
        var templateStart = xaml.IndexOf("<DataTemplate>", listStart, StringComparison.Ordinal);
        var templateEnd = xaml.IndexOf("</DataTemplate>", templateStart, StringComparison.Ordinal);
        Assert.True(templateStart >= 0);
        Assert.True(templateEnd > templateStart);

        var template = xaml[templateStart..templateEnd];
        Assert.Contains("AutomationProperties.Name=\"{Binding AccessibleName}\"", template);
    }

    [Fact]
    public void HistoryWindow_HistoryRows_BindAccessibleName()
    {
        var xaml = File.ReadAllText(SourcePath("VoiceInk.Windows", "src", "VoiceInk.Windows.App", "HistoryWindow.xaml"));
        var code = File.ReadAllText(SourcePath("VoiceInk.Windows", "src", "VoiceInk.Windows.App", "HistoryWindow.xaml.cs"));
        var listStart = xaml.IndexOf("x:Name=\"HistoryListView\"", StringComparison.Ordinal);
        Assert.True(listStart >= 0);
        var templateStart = xaml.IndexOf("<DataTemplate>", listStart, StringComparison.Ordinal);
        var templateEnd = xaml.IndexOf("</DataTemplate>", templateStart, StringComparison.Ordinal);
        Assert.True(templateStart >= 0);
        Assert.True(templateEnd > templateStart);

        var template = xaml[templateStart..templateEnd];
        Assert.Contains("AutomationProperties.Name=\"{Binding AccessibleName}\"", template);
        Assert.Contains("HistoryWindowListRow(Guid Id, string DisplayText, string AccessibleName)", code);
        Assert.Contains("HistoryListAccessibleName", code);
    }

    private static string SourcePath(params string[] parts)
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            var candidate = Path.Combine(new[] { directory.FullName }.Concat(parts).ToArray());
            if (File.Exists(candidate))
            {
                return candidate;
            }

            directory = directory.Parent;
        }

        return Path.Combine(parts);
    }
}
