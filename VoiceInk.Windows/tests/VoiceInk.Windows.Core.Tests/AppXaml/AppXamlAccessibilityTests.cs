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
    public void MainWindow_UpdateGuidanceRows_BindAccessibleNameAndOpenReleases()
    {
        var xaml = File.ReadAllText(SourcePath("VoiceInk.Windows", "src", "VoiceInk.Windows.App", "MainWindow.xaml"));
        var code = File.ReadAllText(SourcePath("VoiceInk.Windows", "src", "VoiceInk.Windows.App", "MainWindow.xaml.cs"));
        var listStart = xaml.IndexOf("x:Name=\"UpdateGuidanceListView\"", StringComparison.Ordinal);
        Assert.True(listStart >= 0);
        var templateStart = xaml.IndexOf("<DataTemplate>", listStart, StringComparison.Ordinal);
        var templateEnd = xaml.IndexOf("</DataTemplate>", templateStart, StringComparison.Ordinal);
        Assert.True(templateStart >= 0);
        Assert.True(templateEnd > templateStart);

        var template = xaml[templateStart..templateEnd];
        Assert.Contains("AutomationProperties.Name=\"{Binding AccessibleName}\"", template);
        Assert.Contains("OpenReleasesButton", xaml);
        Assert.Contains("Check for Updates", xaml);
        Assert.Contains("OpenReleasesButton_Click", code);
        Assert.Contains("https://github.com/Beingpax/VoiceInk/releases", code);
    }

    [Fact]
    public void MainWindow_MetricsSummaryLine_HasAutomationName()
    {
        var xaml = File.ReadAllText(SourcePath("VoiceInk.Windows", "src", "VoiceInk.Windows.App", "MainWindow.xaml"));
        var code = File.ReadAllText(SourcePath("VoiceInk.Windows", "src", "VoiceInk.Windows.App", "MainWindow.xaml.cs"));

        Assert.Contains("x:Name=\"MetricsSummaryLineTextBlock\"", xaml);
        Assert.Contains("AutomationProperties.Name=\"Metrics selected filter summary\"", xaml);
        Assert.Contains("MetricsSummaryLineTextBlock.Text = presentation.SummaryLine", code);
    }

    [Fact]
    public void MainWindow_EnhancementAssistantSummaryLine_HasAutomationName()
    {
        var xaml = File.ReadAllText(SourcePath("VoiceInk.Windows", "src", "VoiceInk.Windows.App", "MainWindow.xaml"));
        var code = File.ReadAllText(SourcePath("VoiceInk.Windows", "src", "VoiceInk.Windows.App", "MainWindow.xaml.cs"));

        Assert.Contains("x:Name=\"EnhancementAssistantSummaryTextBlock\"", xaml);
        Assert.Contains("AutomationProperties.Name=\"Enhancement assistant mode summary\"", xaml);
        Assert.Contains("EnhancementAssistantSummaryTextBlock.Text = presentation.AssistantModeSummary", code);
    }

    [Fact]
    public void MainWindow_ShellNavigation_UsesFooterMenuItemsForSettingsAndAbout()
    {
        var code = File.ReadAllText(SourcePath("VoiceInk.Windows", "src", "VoiceInk.Windows.App", "MainWindow.xaml.cs"));

        Assert.Contains("RootNavigationView.FooterMenuItems.Clear()", code);
        Assert.Contains("item.IsFooter", code);
        Assert.Contains("RootNavigationView.FooterMenuItems.Add(navigationItem)", code);
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
        Assert.Contains("AutomationProperties.SetName(onboardingCurrentStageTextBlock", code);
        Assert.Contains("presentation.CurrentStageAccessibleName", code);
    }

    [Fact]
    public void MainWindow_EnhancementPromptEditor_ExposesMoveControls()
    {
        var xaml = File.ReadAllText(SourcePath("VoiceInk.Windows", "src", "VoiceInk.Windows.App", "MainWindow.xaml"));
        var code = File.ReadAllText(SourcePath("VoiceInk.Windows", "src", "VoiceInk.Windows.App", "MainWindow.xaml.cs"));

        Assert.Contains("MovePromptUpButton", xaml);
        Assert.Contains("MovePromptDownButton", xaml);
        Assert.Contains("Move Up", xaml);
        Assert.Contains("Move Down", xaml);
        Assert.Contains("MovePromptUpButton_Click", code);
        Assert.Contains("MovePromptDownButton_Click", code);
        Assert.Contains("MovePromptAsync", code);
    }

    [Theory]
    [InlineData("SettingsSectionDirectoryListView")]
    [InlineData("SettingsActionSummaryListView")]
    [InlineData("SettingsPreferenceSummaryListView")]
    [InlineData("SettingsBackupGuidanceListView")]
    [InlineData("PowerModeSetupListView")]
    [InlineData("MetricsDashboardCardsListView")]
    [InlineData("MetricsDataGuidanceListView")]
    [InlineData("MetricsActionListView")]
    [InlineData("MetricsDiagnosticsListView")]
    public void MainWindow_SummaryRows_BindAccessibleName(string listViewName)
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
    [InlineData("EnhancementContextReadinessListView")]
    [InlineData("EnhancementContextPrivacyListView")]
    [InlineData("EnhancementContextActionsListView")]
    public void MainWindow_EnhancementContextRows_BindAccessibleName(string listViewName)
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
    [InlineData("DictionaryWorkflowListView")]
    [InlineData("DictionarySummaryListView")]
    [InlineData("DictionaryRuleGuidanceListView")]
    [InlineData("VocabularyListView")]
    [InlineData("ReplacementListView")]
    public void MainWindow_DictionaryRows_BindAccessibleName(string listViewName)
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
    public void MainWindow_PermissionsChecklistRows_BindAccessibleName()
    {
        var xaml = File.ReadAllText(SourcePath("VoiceInk.Windows", "src", "VoiceInk.Windows.App", "MainWindow.xaml"));
        var listStart = xaml.IndexOf("x:Name=\"PermissionsChecklistListView\"", StringComparison.Ordinal);
        Assert.True(listStart >= 0);
        var templateStart = xaml.IndexOf("<DataTemplate>", listStart, StringComparison.Ordinal);
        var templateEnd = xaml.IndexOf("</DataTemplate>", templateStart, StringComparison.Ordinal);
        Assert.True(templateStart >= 0);
        Assert.True(templateEnd > templateStart);

        var template = xaml[templateStart..templateEnd];
        Assert.Contains("AutomationProperties.Name=\"{Binding AccessibleName}\"", template);
    }

    [Fact]
    public void MainWindow_PowerModeRules_BindAccessibleName()
    {
        var xaml = File.ReadAllText(SourcePath("VoiceInk.Windows", "src", "VoiceInk.Windows.App", "MainWindow.xaml"));
        var listStart = xaml.IndexOf("x:Name=\"PowerModeRulesListView\"", StringComparison.Ordinal);
        Assert.True(listStart >= 0);
        var templateStart = xaml.IndexOf("<DataTemplate>", listStart, StringComparison.Ordinal);
        var templateEnd = xaml.IndexOf("</DataTemplate>", templateStart, StringComparison.Ordinal);
        Assert.True(templateStart >= 0);
        Assert.True(templateEnd > templateStart);

        var template = xaml[templateStart..templateEnd];
        Assert.Contains("AutomationProperties.Name=\"{Binding AccessibleName}\"", template);
    }

    [Fact]
    public void MainWindow_AudioFileQueueRows_BindAccessibleName()
    {
        var xaml = File.ReadAllText(SourcePath("VoiceInk.Windows", "src", "VoiceInk.Windows.App", "MainWindow.xaml"));
        var listStart = xaml.IndexOf("x:Name=\"AudioFileQueueListView\"", StringComparison.Ordinal);
        Assert.True(listStart >= 0);
        var templateStart = xaml.IndexOf("<DataTemplate>", listStart, StringComparison.Ordinal);
        var templateEnd = xaml.IndexOf("</DataTemplate>", templateStart, StringComparison.Ordinal);
        Assert.True(templateStart >= 0);
        Assert.True(templateEnd > templateStart);

        var template = xaml[templateStart..templateEnd];
        Assert.Contains("AutomationProperties.Name=\"{Binding AccessibleName}\"", template);
    }

    [Fact]
    public void MainWindow_AudioFileQueueRows_UseAccessibleNameModel()
    {
        var code = File.ReadAllText(SourcePath("VoiceInk.Windows", "src", "VoiceInk.Windows.App", "MainWindow.xaml.cs"));

        Assert.Contains("AudioFileQueueListRow(string DisplayText, string AccessibleName)", code);
        Assert.Contains("AudioFileQueueAccessibleName", code);
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

    [Theory]
    [InlineData("HistoryListView")]
    [InlineData("HistoryAnalysisListView")]
    public void MainWindow_HistoryRows_BindAccessibleName(string listViewName)
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
    public void MainWindow_HistoryRows_UseAccessibleNameModel()
    {
        var code = File.ReadAllText(SourcePath("VoiceInk.Windows", "src", "VoiceInk.Windows.App", "MainWindow.xaml.cs"));

        Assert.Contains("MainHistoryListRow(string DisplayText, string AccessibleName)", code);
        Assert.Contains("HistoryListAccessibleName", code);
    }

    [Fact]
    public void FloatingRecorderWindow_AppliesRecorderHelpText()
    {
        var code = File.ReadAllText(SourcePath("VoiceInk.Windows", "src", "VoiceInk.Windows.App", "FloatingRecorderWindow.xaml.cs"));

        Assert.Contains("AutomationProperties.SetName(RecorderChrome, state.AccessibleName)", code);
        Assert.Contains("AutomationProperties.SetHelpText(RecorderChrome, state.AccessibleHelpText)", code);
    }

    [Fact]
    public void TrayIcon_ExposesTaskbarSettingsAction()
    {
        var trayCode = File.ReadAllText(SourcePath("VoiceInk.Windows", "src", "VoiceInk.Windows.Native", "Tray", "TrayIconService.cs"));
        var mainWindowCode = File.ReadAllText(SourcePath("VoiceInk.Windows", "src", "VoiceInk.Windows.App", "MainWindow.xaml.cs"));

        Assert.Contains("OpenTaskbarSettingsRequested", trayCode);
        Assert.Contains("visibilityGuidanceItem = new ToolStripMenuItem", trayCode);
        Assert.Contains("Enabled = true", trayCode);
        Assert.Contains("ms-settings:taskbar", mainWindowCode);
        Assert.Contains("UseShellExecute = true", mainWindowCode);
        Assert.Contains("TrayIconService_OpenTaskbarSettingsRequested", mainWindowCode);
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
