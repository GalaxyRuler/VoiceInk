# Windows MSIX Target Family Validation Spec

## Goal

Harden non-installing MSIX artifact validation so release packages must preserve the Windows Desktop target-device compatibility metadata from `Package.appxmanifest`.

## Source Of Truth

Microsoft MSIX deployment troubleshooting identifies manifest target-family, architecture, and dependency metadata as install and launch compatibility factors. VoiceInk for Windows is a WinUI desktop app and its manifest declares:

- `TargetDeviceFamily Name="Windows.Desktop"`
- `MinVersion="10.0.19041.0"`
- `MaxVersionTested="10.0.26100.0"`

## Windows Behavior

`test-msix-package.ps1` must parse the extracted `AppxManifest.xml` and fail if the target-device family is missing or differs from the expected Windows Desktop compatibility range.

This remains a non-installing validation path. It must not run `Add-AppxPackage`, mutate certificate stores, sign packages, launch the app, or trust certificates.

## Verification

- Add packaging asset tests that assert the source manifest and MSIX artifact validator cover `TargetDeviceFamily`.
- Run the focused packaging test red before implementation.
- Run focused packaging tests, release readiness, full tests, Debug x64 build, and `git diff --check` before committing.
