# Windows App Installer Readiness Reference

## Goal

Add a read-only packaging readiness reference for optional `.appinstaller` distribution so maintainers can prepare an open-source Windows release without guessing how App Installer identity matching relates to the MSIX manifest.

## Source Of Truth

Microsoft's App Installer documentation requires the package identity referenced by `.appinstaller` package entries to match the package identity in the app package manifest. For VoiceInk for Windows, that means the optional `.appinstaller` `MainPackage` entry must align with `VoiceInk.Windows.App\Package.appxmanifest` and the signed MSIX artifact.

## Requirements

- The release readiness report must mention `.appinstaller` as an optional distribution path, not a mandatory packaging target.
- The report must name the `MainPackage` entry and the `Name/Publisher/Version` identity match requirement.
- The report must point maintainers back to `Package.appxmanifest` as the package identity source.
- The report must stay non-mutating: no package generation, publishing, installing, uninstalling, certificate creation, certificate import, or trust-store mutation.
- Tests must guard the readiness text and the non-mutating guarantees.

## Open-Source Boundary

This slice does not add a paid updater, private update channel, account flow, telemetry, or commercial release gate. Any future `.appinstaller` file should be generated only from maintainer-owned release artifacts and published through an open-source friendly distribution path.
