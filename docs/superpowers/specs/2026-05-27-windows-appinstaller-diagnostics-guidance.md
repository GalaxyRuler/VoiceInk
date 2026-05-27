# Windows App Installer Diagnostics Guidance

VoiceInk for Windows already has read-only release readiness and gated signed MSIX install smoke scripts. The remaining maintainer-owned signed install path can fail in two distinct places: App Installer may fail before package deployment starts, or `Add-AppxPackage`/`Remove-AppxPackage` may fail during deployment and return an ActivityID.

Microsoft's MSIX troubleshooting guidance points maintainers to AppxDeployment/AppxPackaging operational logs for package deployment and to `Get-AppxLog -ActivityID` when PowerShell package cmdlets return an ActivityID. App Installer troubleshooting also separates `.appinstaller` launch/update issues from package deployment. The Windows fork should expose those diagnostics in the repo-local smoke plan without creating certificates, importing certificates, signing packages, installing packages, or changing trust stores.

## Requirements

- `smoke-msix-install.ps1` default plan prints:
  - existing signature status, signer subject/thumbprint, trust-store, and deployment-log guidance;
  - `Get-AppxLog -ActivityID <activity-id>` guidance for package cmdlet failures;
  - `Microsoft-Windows-AppInstaller/Operational` guidance for `.appinstaller` failures.
- `test-release-readiness.ps1` prints the same diagnostics as release checklist items.
- Packaging asset tests assert the diagnostics and continue to guard against certificate creation/import, trust-store mutation, direct install/uninstall execution in plan scripts, and secret handling.

## Non-Goals

- No certificate generation or import.
- No trust-store mutation.
- No automatic signed install smoke.
- No `.appinstaller` generation or publishing.
