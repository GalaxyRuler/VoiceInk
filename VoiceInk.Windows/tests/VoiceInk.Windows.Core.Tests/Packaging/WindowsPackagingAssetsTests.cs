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
    public void MsixPackagingScript_ProvidesCertificateFreePreflightWithoutMachineMutation()
    {
        var scriptPath = SourcePath("VoiceInk.Windows", "scripts", "package-msix.ps1");
        var script = File.ReadAllText(scriptPath);

        Assert.Contains("Preflight", script);
        Assert.Contains("PackageCertificateKeyFile is required for signed packaging", script);
        Assert.Contains("MSIX packaging preflight passed", script);
        Assert.Contains("test-msix-package.ps1", script);
        Assert.Contains("Add-AppxPackage -Path", script);
        Assert.Contains("Remove-AppxPackage -Package", script);
        Assert.Contains("dotnet publish", script);

        Assert.DoesNotContain("& Add-AppxPackage", script, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("& Remove-AppxPackage", script, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("New-SelfSignedCertificate", script, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Import-PfxCertificate", script, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Import-Certificate", script, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("cert:\\", script, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void MsixPackagingScript_CanRunArtifactValidationAfterSignedBuild()
    {
        var scriptPath = SourcePath("VoiceInk.Windows", "scripts", "package-msix.ps1");
        var script = File.ReadAllText(scriptPath);

        Assert.Contains("ValidateAfterBuild", script);
        Assert.Contains("Find-BuiltMsixPackage", script);
        Assert.Contains("test-msix-package.ps1", script);
        Assert.Contains("Validating signed MSIX artifact", script);
        Assert.Contains("& $msixValidator -PackagePath $builtPackagePath", script);
        Assert.Contains("Validated signed MSIX artifact", script);

        Assert.DoesNotContain("& Add-AppxPackage", script, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("& Remove-AppxPackage", script, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("New-SelfSignedCertificate", script, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Import-PfxCertificate", script, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Import-Certificate", script, StringComparison.OrdinalIgnoreCase);
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

    [Fact]
    public void DevZipInstallScripts_ArePerUserAndRequireExplicitExecute()
    {
        var installScriptPath = SourcePath("VoiceInk.Windows", "scripts", "install-dev-zip.ps1");
        var uninstallScriptPath = SourcePath("VoiceInk.Windows", "scripts", "uninstall-dev-zip.ps1");
        var installScript = File.ReadAllText(installScriptPath);
        var uninstallScript = File.ReadAllText(uninstallScriptPath);

        Assert.Contains("Execute", installScript);
        Assert.Contains("Execute", uninstallScript);
        Assert.Contains("Start Menu", installScript);
        Assert.Contains("CreateShortcut", installScript);
        Assert.Contains("LocalAppData", installScript);
        Assert.Contains("VoiceInk.Windows.App.exe", installScript);
        Assert.Contains("Dev ZIP install plan", installScript);
        Assert.Contains("Dev ZIP uninstall plan", uninstallScript);
        Assert.Contains("Refusing to install from a package outside artifact root", installScript);
        Assert.Contains("Refusing to uninstall path outside LocalAppData", uninstallScript);

        Assert.DoesNotContain("Add-AppxPackage", installScript, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Remove-AppxPackage", uninstallScript, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("ProgramData", installScript, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Program Files", installScript, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("HKLM", installScript, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("cert:\\", installScript, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Import-Certificate", installScript, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void MsixPackageSmokeScript_ValidatesPackageContentsWithoutInstalling()
    {
        var scriptPath = SourcePath("VoiceInk.Windows", "scripts", "test-msix-package.ps1");
        var script = File.ReadAllText(scriptPath);

        Assert.Contains("AppxManifest.xml", script);
        Assert.Contains("AppxBlockMap.xml", script);
        Assert.Contains("AppxSignature.p7x", script);
        Assert.Contains("VoiceInk.Windows.App.exe", script);
        Assert.Contains("VoiceInk.Windows", script);
        Assert.Contains("CN=VoiceInkOpenSource", script);
        Assert.Contains("runFullTrust", script);
        Assert.Contains("microphone", script);
        Assert.Contains("Refusing to inspect path outside artifact root", script);
        Assert.Contains("Refusing to use a package inside the extraction cleanup root", script);
        Assert.Contains("Assert-NoReparsePointInPath", script);
        Assert.Contains("Refusing to use reparse-point path for MSIX smoke validation", script);
        Assert.Contains("MSIX artifact validation passed", script);
        Assert.Contains("Manual signed MSIX smoke commands", script);
        Assert.Contains("This script does not cryptographically verify the package signature", script);
        Assert.Contains("PackagePath is required unless -Help is used", script);
        Assert.Contains("Add-AppxPackage -Path", script);
        Assert.Contains("Remove-AppxPackage -Package", script);

        Assert.DoesNotContain("[Parameter(Mandatory = $true)]", script, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("& Add-AppxPackage", script, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("& Remove-AppxPackage", script, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Start-Process", script, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("signtool", script, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("New-SelfSignedCertificate", script, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Import-PfxCertificate", script, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("cert:\\", script, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void MsixInstallSmokeScript_PrintsPlanByDefaultAndRequiresExecuteForMutation()
    {
        var scriptPath = SourcePath("VoiceInk.Windows", "scripts", "smoke-msix-install.ps1");
        var script = File.ReadAllText(scriptPath);

        Assert.Contains("Execute", script);
        Assert.Contains("MSIX install smoke plan", script);
        Assert.Contains("Add-AppxPackage -Path", script);
        Assert.Contains("Get-AppxPackage -Name", script);
        Assert.Contains("Remove-AppxPackage -Package", script);
        Assert.Contains("PackagePath is required unless -Help is used", script);
        Assert.Contains("Refusing to smoke package outside artifact root", script);
        Assert.Contains("Assert-NoReparsePointInPath", script);
        Assert.Contains("This script does not create or import certificates", script);
        Assert.Contains("Expected one installed package named", script);
        Assert.Contains("Refusing to choose a package to remove", script);

        Assert.DoesNotContain("New-SelfSignedCertificate", script, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Import-PfxCertificate", script, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Import-Certificate", script, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("cert:\\", script, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Start-Process", script, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ReleaseReadinessScript_PrintsNonMutatingPackagingChecklist()
    {
        var scriptPath = SourcePath("VoiceInk.Windows", "scripts", "test-release-readiness.ps1");
        var script = File.ReadAllText(scriptPath);

        Assert.Contains("Release readiness report", script);
        Assert.Contains("package-msix.ps1 -Preflight", script);
        Assert.Contains("package-msix.ps1 -ValidateAfterBuild", script);
        Assert.Contains("test-msix-package.ps1", script);
        Assert.Contains("smoke-msix-install.ps1", script);
        Assert.Contains("package-dev-zip.ps1", script);
        Assert.Contains("test-dev-zip.ps1", script);
        Assert.Contains("This script does not create or import certificates", script);
        Assert.Contains("Release readiness report passed", script);

        Assert.DoesNotContain("& Add-AppxPackage", script, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("& Remove-AppxPackage", script, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("& dotnet publish", script, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("New-SelfSignedCertificate", script, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Import-PfxCertificate", script, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Import-Certificate", script, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("cert:\\", script, StringComparison.OrdinalIgnoreCase);
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
