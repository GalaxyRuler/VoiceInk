# Windows App Installer Manifest Generator

VoiceInk for Windows has signed MSIX packaging, artifact validation, install smoke guidance, and release readiness diagnostics. The remaining App Installer distribution gap was that the `.appinstaller` file itself was only described in prose. Maintainers still had to hand-write identity values, which is error-prone because App Installer validates package identity against the referenced MSIX.

Microsoft's App Installer schema requires `MainPackage` `Name`, `Publisher`, `Version`, `ProcessorArchitecture`, and `Uri`, and documents that the identity values must match the app package manifest and referenced package. The Windows fork should generate the `.appinstaller` manifest from `Package.appxmanifest` so identity stays aligned.

## Requirements

- Add `VoiceInk.Windows/scripts/write-appinstaller.ps1`.
- The script must:
  - read `VoiceInk.Windows/src/VoiceInk.Windows.App/Package.appxmanifest`;
  - use the official `http://schemas.microsoft.com/appx/appinstaller/2017/2` namespace;
  - write an `AppInstaller` root and `MainPackage` entry;
  - copy `Name`, `Publisher`, and `Version` from `Package.appxmanifest`;
  - accept a maintainer-provided absolute MSIX URI;
  - keep output under `VoiceInk.Windows/artifacts`;
  - refuse package identity drift away from `VoiceInk.Windows` / `CN=VoiceInkOpenSource`;
  - avoid publishing, installing, uninstalling, signing, certificate generation, certificate import, and trust-store mutation.
- The release readiness report must list the generator in required assets and non-mutating commands.
- Packaging asset tests must cover the generator and its safety boundary.

## Non-Goals

- No `.appinstaller` publishing.
- No App Installer launch.
- No signed MSIX install smoke.
- No certificate or trust-store handling.
