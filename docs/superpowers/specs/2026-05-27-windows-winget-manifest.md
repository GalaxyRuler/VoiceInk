# Windows WinGet Manifest Packaging

## Goal

Close a packaging parity gap by adding a non-mutating WinGet manifest generation and validation path for the open-source Windows release artifacts.

## Source Of Truth

- Windows packaging must stay open-source friendly and avoid commercial Store, purchase, trial, or account flows.
- Maintainer-owned signing and disposable install smoke remain external gates; repository scripts should still prepare reviewable distribution metadata without mutating the machine.

## Online Grounding

- Microsoft documents WinGet manifests as YAML package metadata for Windows Package Manager community distribution, with `InstallerType: msix` supported for MSIX/MSIXBundle installers: https://learn.microsoft.com/en-us/windows/package-manager/package/manifest
- Microsoft documents WinGet as the Windows Package Manager command-line client for install, upgrade, remove, and validation workflows on supported Windows versions: https://learn.microsoft.com/windows/package-manager/winget

## Requirements

- Add a `write-winget-manifest.ps1` script that writes WinGet multi-file YAML manifests under `VoiceInk.Windows\artifacts`.
- Read package identity and version from `Package.appxmanifest`.
- Require maintainer-supplied installer URL and SHA256, or compute SHA256 from a local package path without downloading or installing.
- Emit package, default-locale, and installer manifests with open-source metadata and `InstallerType: msix`.
- Add a `test-winget-manifest.ps1` script that validates the generated files without invoking `winget`, installing packages, publishing manifests, downloading installers, or mutating certificates.
- Wire the scripts into release readiness docs and tests.

## Acceptance

- Focused packaging tests fail before the scripts and readiness wiring exist.
- Script help and a generated fake-manifest validation pass after implementation.
- Full solution tests and Debug x64 build pass.
