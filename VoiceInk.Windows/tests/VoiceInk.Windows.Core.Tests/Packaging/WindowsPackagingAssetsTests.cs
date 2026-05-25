using System.Xml.Linq;
using Xunit;

namespace VoiceInk.Windows.Core.Tests.Packaging;

public sealed class WindowsPackagingAssetsTests
{
    [Fact]
    public void PackageManifest_DeclaresOpenSourceDesktopAppIdentityAndCapabilities()
    {
        var manifestPath = SourcePath("VoiceInk.Windows", "src", "VoiceInk.Windows.App", "Package.appxmanifest");

        var document = XDocument.Load(manifestPath);
        XNamespace appx = "http://schemas.microsoft.com/appx/manifest/foundation/windows10";
        XNamespace uap = "http://schemas.microsoft.com/appx/manifest/uap/windows10";
        XNamespace rescap = "http://schemas.microsoft.com/appx/manifest/foundation/windows10/restrictedcapabilities";

        var identity = document.Root?.Element(appx + "Identity");
        Assert.NotNull(identity);
        Assert.Equal("VoiceInk.Windows", identity.Attribute("Name")?.Value);
        Assert.Equal("CN=VoiceInkOpenSource", identity.Attribute("Publisher")?.Value);

        var displayName = document.Root?
            .Element(appx + "Properties")?
            .Element(appx + "DisplayName")?
            .Value;
        Assert.Equal("VoiceInk for Windows", displayName);

        var application = document.Descendants(appx + "Application").Single();
        Assert.Equal("VoiceInk.Windows.App", application.Attribute("Id")?.Value);
        Assert.Equal("VoiceInk.Windows.App.exe", application.Attribute("Executable")?.Value);

        var visualElements = application.Element(uap + "VisualElements");
        Assert.NotNull(visualElements);
        Assert.Equal("VoiceInk for Windows", visualElements.Attribute("DisplayName")?.Value);

        var capabilities = document.Root?.Element(appx + "Capabilities");
        Assert.NotNull(capabilities);
        Assert.Contains(
            capabilities.Elements(appx + "DeviceCapability"),
            item => item.Attribute("Name")?.Value == "microphone");
        Assert.Contains(
            capabilities.Elements(rescap + "Capability"),
            item => item.Attribute("Name")?.Value == "runFullTrust");

        var manifestText = File.ReadAllText(manifestPath);
        Assert.DoesNotContain("trial", manifestText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("purchase", manifestText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("store", manifestText, StringComparison.OrdinalIgnoreCase);

        Assert.True(File.Exists(SourcePath("VoiceInk.Windows", "src", "VoiceInk.Windows.App", "Assets", "Square44x44Logo.png")));
        Assert.True(File.Exists(SourcePath("VoiceInk.Windows", "src", "VoiceInk.Windows.App", "Assets", "Square150x150Logo.png")));
    }

    [Fact]
    public void MsixPackagingScript_RequiresExternalCertificateAndUsesArtifactSafety()
    {
        var scriptPath = SourcePath("VoiceInk.Windows", "scripts", "package-msix.ps1");
        var script = File.ReadAllText(scriptPath);

        Assert.Contains("PackageCertificateKeyFile", script);
        Assert.Contains("OutputRoot must be inside VoiceInk.Windows\\artifacts", script);
        Assert.Contains("WindowsPackageType=MSIX", script);
        Assert.Contains("GenerateAppxPackageOnBuild=true", script);
        Assert.Contains("AppxBundle=Never", script);
        Assert.Contains("PackageCertificatePassword", script);
        Assert.Contains("Add-AppxPackage", script);
        Assert.Contains("Remove-AppxPackage", script);

        Assert.DoesNotContain("New-SelfSignedCertificate", script, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Import-PfxCertificate", script, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("cert:\\", script, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void DevZipSmokeScript_ValidatesExpectedPackageContentsWithoutInstalling()
    {
        var scriptPath = SourcePath("VoiceInk.Windows", "scripts", "test-dev-zip.ps1");
        var script = File.ReadAllText(scriptPath);

        Assert.Contains("VoiceInk.Windows.App.exe", script);
        Assert.Contains("VoiceInk.Windows.App.deps.json", script);
        Assert.Contains("VoiceInk.Windows.App.runtimeconfig.json", script);
        Assert.Contains("VOICEINK-WINDOWS-README.txt", script);
        Assert.Contains("Microsoft.WindowsAppRuntime.Bootstrap.dll", script);
        Assert.Contains("Expand-Archive", script);
        Assert.Contains("Refusing to inspect path outside artifact root", script);
        Assert.Contains("Refusing to use a package inside the extraction cleanup root", script);
        Assert.Contains("Assert-PathOutside -CandidatePath $resolvedPackagePath -RootPath $extractRoot", script);
        Assert.Contains("Dev ZIP smoke validation passed", script);

        Assert.DoesNotContain("Add-AppxPackage", script, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Start-Process", script, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Remove-Item -LiteralPath $PackagePath", script, StringComparison.OrdinalIgnoreCase);
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
