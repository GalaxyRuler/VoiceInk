using System.Text.Json.Nodes;
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
    public void AppInstallerScript_GeneratesSchemaManifestWithoutPublishingOrInstalling()
    {
        var scriptPath = SourcePath("VoiceInk.Windows", "scripts", "write-appinstaller.ps1");
        var script = File.ReadAllText(scriptPath);

        Assert.Contains("AppInstaller", script);
        Assert.Contains("MainPackage", script);
        Assert.Contains("http://schemas.microsoft.com/appx/appinstaller/2017/2", script);
        Assert.Contains("Package.appxmanifest", script);
        Assert.Contains("Name", script);
        Assert.Contains("Publisher", script);
        Assert.Contains("Version", script);
        Assert.Contains("ProcessorArchitecture", script);
        Assert.Contains("Uri", script);
        Assert.Contains("EnableOnLaunchUpdateCheck", script);
        Assert.Contains("HoursBetweenUpdateChecks", script);
        Assert.Contains("0 and 255", script);
        Assert.Contains("must point to a .msix or .msixbundle file", script);
        Assert.Contains("Assert-AppPackageUri -Value $MainPackageUri -Name \"MainPackageUri\"", script);
        Assert.Contains("VoiceInk.Windows", script);
        Assert.Contains("CN=VoiceInkOpenSource", script);
        Assert.Contains("OutputPath must stay inside VoiceInk.Windows\\artifacts", script);
        Assert.Contains("App Installer manifest written", script);

        Assert.DoesNotContain("& Add-AppxPackage", script, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("& Remove-AppxPackage", script, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("& dotnet publish", script, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("New-SelfSignedCertificate", script, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Import-PfxCertificate", script, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Import-Certificate", script, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("cert:\\", script, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Start-Process", script, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void AppInstallerSmokeScript_ValidatesSchemaIdentityWithoutInstalling()
    {
        var scriptPath = SourcePath("VoiceInk.Windows", "scripts", "test-appinstaller.ps1");
        var script = File.ReadAllText(scriptPath);

        Assert.Contains("AppInstaller", script);
        Assert.Contains("MainPackage", script);
        Assert.Contains("http://schemas.microsoft.com/appx/appinstaller/2017/2", script);
        Assert.Contains("Package.appxmanifest", script);
        Assert.Contains("Name", script);
        Assert.Contains("Publisher", script);
        Assert.Contains("Version", script);
        Assert.Contains("ProcessorArchitecture", script);
        Assert.Contains("Uri", script);
        Assert.Contains("UpdateSettings", script);
        Assert.Contains("OnLaunch", script);
        Assert.Contains("HoursBetweenUpdateChecks", script);
        Assert.Contains("0 and 255", script);
        Assert.Contains("must point to a .msix or .msixbundle file", script);
        Assert.Contains("Assert-AppPackageUri -Value $mainPackage.Uri -Name \"MainPackage Uri\"", script);
        Assert.Contains("Refusing to inspect path outside artifact root", script);
        Assert.Contains("App Installer manifest validation passed", script);

        Assert.DoesNotContain("& Add-AppxPackage", script, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("& Remove-AppxPackage", script, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("& dotnet publish", script, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("New-SelfSignedCertificate", script, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Import-PfxCertificate", script, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Import-Certificate", script, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("cert:\\", script, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Start-Process", script, StringComparison.OrdinalIgnoreCase);
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
        Assert.Contains("Get-AuthenticodeSignature -FilePath", script);
        Assert.Contains("Signer certificate subject", script);
        Assert.Contains("Signer certificate thumbprint", script);
        Assert.Contains("Assert-SignatureReadyForExecute", script);
        Assert.Contains("Refusing to execute install smoke because Authenticode signature status is", script);
        Assert.Contains("0x800B0109", script);
        Assert.Contains("TrustedPeople", script);
        Assert.Contains("Microsoft-Windows-AppxDeployment-Server", script);
        Assert.Contains("Get-AppxLog -ActivityID", script);
        Assert.Contains("Microsoft-Windows-AppInstaller/Operational", script);
        Assert.Contains("Expected one installed package named", script);
        Assert.Contains("Refusing to choose a package to remove", script);
        Assert.Contains("Get-AppxPackageManifest -Package", script);
        Assert.Contains("VoiceInk.Windows.App", script);
        Assert.Contains("PackageFamilyName", script);
        Assert.Contains("shell:AppsFolder", script);

        Assert.DoesNotContain("New-SelfSignedCertificate", script, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Import-PfxCertificate", script, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Import-Certificate", script, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("cert:\\", script, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Start-Process", script, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void MsixInstallSmokeScript_VerifiesUninstallRemovesPackage()
    {
        var scriptPath = SourcePath("VoiceInk.Windows", "scripts", "smoke-msix-install.ps1");
        var script = File.ReadAllText(scriptPath);

        Assert.Contains("Verifying uninstall cleanup", script);
        Assert.Contains("remainingPackages", script);
        Assert.Contains("Package still resolves after Remove-AppxPackage", script);
        Assert.Contains("Uninstall cleanup verified", script);
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
        Assert.Contains("write-appinstaller.ps1", script);
        Assert.Contains("test-appinstaller.ps1", script);
        Assert.Contains("package-dev-zip.ps1", script);
        Assert.Contains("test-dev-zip.ps1", script);
        Assert.Contains("Publisher/certificate subject match", script);
        Assert.Contains("CN=VoiceInkOpenSource", script);
        Assert.Contains("Timestamp signed packages", script);
        Assert.Contains("Trusted People", script);
        Assert.Contains(@"Cert:\LocalMachine\TrustedPeople", script);
        Assert.Contains("0x800B0109", script);
        Assert.Contains("WinApp CLI local signing reference", script);
        Assert.Contains("winapp cert generate", script);
        Assert.Contains("winapp sign", script);
        Assert.Contains("App Installer readiness reference", script);
        Assert.Contains(".appinstaller", script);
        Assert.Contains("MainPackage", script);
        Assert.Contains("Name/Publisher/Version", script);
        Assert.Contains("Package.appxmanifest", script);
        Assert.Contains("AppxDeployment-Server", script);
        Assert.Contains("AppxPackaging", script);
        Assert.Contains("Get-AppxLog -ActivityID", script);
        Assert.Contains("Microsoft-Windows-AppInstaller/Operational", script);
        Assert.Contains("Get-AppxPackageManifest -Package", script);
        Assert.Contains("VoiceInk.Windows.App", script);
        Assert.Contains("shell:AppsFolder", script);
        Assert.Contains("This script does not create or import certificates", script);
        Assert.Contains("Release readiness report passed", script);

        Assert.DoesNotContain("& Add-AppxPackage", script, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("& Remove-AppxPackage", script, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("& dotnet publish", script, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("New-SelfSignedCertificate", script, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Import-PfxCertificate", script, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Import-Certificate", script, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void InstallerSmokeWorkflow_IsManualAndRequiresExplicitInstallExecution()
    {
        var workflowPath = SourcePath(".github", "workflows", "windows-installer-smoke.yml");
        var workflow = File.ReadAllText(workflowPath);

        Assert.Contains("workflow_dispatch", workflow);
        Assert.Contains("self-hosted", workflow);
        Assert.Contains("windows", workflow);
        Assert.Contains("runner_label", workflow);
        Assert.Contains("execute_install_smoke", workflow);
        Assert.Contains("default: false", workflow);
        Assert.Contains("test-msix-package.ps1", workflow);
        Assert.Contains("write-appinstaller.ps1", workflow);
        Assert.Contains("test-appinstaller.ps1", workflow);
        Assert.Contains("smoke-msix-install.ps1", workflow);
        Assert.Contains("if: ${{ inputs.execute_install_smoke }}", workflow);
        Assert.Contains("signed_package_path", workflow);
        Assert.Contains("main_package_uri", workflow);
        Assert.Contains("No signing certificates are created or imported by this workflow", workflow);
        Assert.Contains("VoiceInk.Windows\\artifacts\\gha-installer-smoke", workflow);
        Assert.Contains("installer-smoke-summary.txt", workflow);
        Assert.Contains("actions/upload-artifact@v4", workflow);
        Assert.Contains("installer-smoke-evidence", workflow);
        Assert.Contains("if: ${{ always() }}", workflow);
        Assert.Contains("include-hidden-files: false", workflow);

        Assert.DoesNotContain("New-SelfSignedCertificate", workflow, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Import-PfxCertificate", workflow, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Import-Certificate", workflow, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("cert:\\", workflow, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(".pfx", workflow, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void HomelabMetadata_DefinesDryRunContainerAndHeadlessRoutes()
    {
        var configPath = SourcePath(".codex", "homelab-runner.json");
        var config = JsonNode.Parse(File.ReadAllText(configPath))!.AsObject();

        Assert.Equal("VoiceInk Windows", (string?)config["projectName"]);
        Assert.Equal(
            ["container", "headless"],
            config["allowedClasses"]!.AsArray().Select(item => item!.GetValue<string>()).ToArray());

        var preferredRoutes = config["preferredRoutes"]!.AsObject();
        Assert.Equal("container", (string?)preferredRoutes["container"]);
        Assert.Equal("headless", (string?)preferredRoutes["headless"]);

        AssertProfile(
            config,
            "windows-dotnet-cli",
            expectedClass: "headless",
            expectedPath: SourcePath("qa", "profiles", "windows-dotnet-cli.json"));
        AssertProfile(
            config,
            "release-metadata",
            expectedClass: "container",
            expectedPath: SourcePath("qa", "profiles", "release-metadata.json"));
    }

    private static void AssertProfile(
        JsonObject config,
        string profileName,
        string expectedClass,
        string expectedPath)
    {
        var profile = config["profiles"]![profileName]!.AsObject();
        Assert.Equal(expectedClass, (string?)profile["class"]);
        Assert.False((bool?)profile["approvalRequired"]);
        Assert.Equal(
            expectedPath,
            SourcePath(((string?)profile["profilePath"])!.Split('/')));

        var profileConfig = JsonNode.Parse(File.ReadAllText(expectedPath))!.AsObject();
        Assert.Equal(expectedClass, (string?)profileConfig["class"]);
        Assert.True((bool?)profileConfig["dryRunOnly"]);
        Assert.Equal("none", (string?)profileConfig["networkMode"]);

        var liveExecution = profileConfig["liveExecution"]!.AsObject();
        Assert.False((bool?)liveExecution["allowed"]);
        Assert.False((bool?)liveExecution["requiresNetwork"]);
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
