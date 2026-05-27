# Windows MSIX Launch Identity Smoke

## Goal

Strengthen the signed MSIX install smoke helper so the maintainer-gated `-Execute` path verifies the installed VoiceInk app identity before uninstalling, while the default path remains a non-mutating plan.

## Source Of Truth

- Microsoft MSIX PowerShell guidance uses `Add-AppxPackage`, `Get-AppxPackage`, and `Remove-AppxPackage` for package lifecycle checks.
- Packaged desktop apps expose launch identity through the installed package family name and the application id from `AppxManifest.xml`.
- `VoiceInk.Windows/src/VoiceInk.Windows.App/Package.appxmanifest` declares `Application Id="VoiceInk.Windows.App"`.

## Requirements

- Keep `smoke-msix-install.ps1` non-mutating unless `-Execute` is supplied.
- Print a `shell:AppsFolder\<PackageFamilyName>!VoiceInk.Windows.App` launch reference in the smoke plan.
- In the `-Execute` path, inspect the installed package manifest with `Get-AppxPackageManifest`.
- Verify the installed package contains application id `VoiceInk.Windows.App`.
- Keep install, query, manifest verification, and uninstall tied to the exact installed package selected by `Get-AppxPackage -Name`.
- Do not create/import certificates, sign packages, trust certificates, publish packages, or launch the GUI app in this slice.

## Non-Goals

- No actual package install/uninstall is run during development verification.
- No GUI launch automation.
- No certificate-store mutation.
