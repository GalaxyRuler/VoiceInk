# Windows App Installer Manifest Validator

VoiceInk for Windows can now generate a `.appinstaller` file from `Package.appxmanifest`. The next release-safety gap is validating that generated or maintainer-provided `.appinstaller` file before it is published or opened with App Installer.

Microsoft's App Installer schema requires the `MainPackage` identity values to match the referenced package identity. The Windows fork should provide a non-installing validator that compares `.appinstaller` identity against the repo manifest and checks absolute URIs before any manual publish/install step.

## Requirements

- Add `VoiceInk.Windows/scripts/test-appinstaller.ps1`.
- The script must:
  - accept `-AppInstallerPath`;
  - require the file to stay under `VoiceInk.Windows/artifacts`;
  - require `.appinstaller` extension;
  - parse the App Installer `2017/2` schema;
  - require an `AppInstaller` root and `MainPackage`;
  - compare `MainPackage` `Name`, `Publisher`, and `Version` against `Package.appxmanifest`;
  - require `ProcessorArchitecture`;
  - require absolute `AppInstaller` and `MainPackage` URIs;
  - avoid publishing, launching, installing, uninstalling, signing, certificate creation, certificate import, and trust-store mutation.
- The release readiness report must list the validator in required assets and non-mutating commands.
- Packaging asset tests must cover the validator and its safety boundary.

## Non-Goals

- No network fetch of the MSIX URI.
- No App Installer launch.
- No package install/uninstall.
- No certificate or signature validation.
