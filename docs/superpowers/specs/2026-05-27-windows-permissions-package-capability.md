# Windows Permissions Package Capability

## Context

The macOS Permissions page separates user-controlled permissions from app readiness. On Windows, microphone access depends on both runtime privacy settings and the packaged app manifest declaring microphone capability for MSIX builds. The Windows Permissions page already showed runtime microphone/device readiness but did not expose the package capability check as a visible readiness row.

## Requirements

- Add a Permissions readiness row named `App Microphone Capability`.
- Mark it ready when represented by source-controlled manifest readiness.
- Explain that `Package.appxmanifest` declares `DeviceCapability Name="microphone"`.
- Keep the row informational and non-mutating.
- Keep runtime microphone privacy/device readiness as a separate row.
- The action button for this row must not open a broken navigation target.

## Verification

- Focused Permissions presenter tests cover row order, status, guidance, and accessible name.
- App build verifies the row action target handling compiles.
