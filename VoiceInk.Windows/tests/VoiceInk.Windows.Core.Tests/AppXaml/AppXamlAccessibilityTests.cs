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
