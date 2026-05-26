# Windows Dev ZIP Per-User Install Design

VoiceInk for Windows needs an open-source friendly install path while signed MSIX release signing remains gated on maintainer-owned certificates.

## Constraints

- Microsoft requires MSIX packages to be signed, and self-signed packages require the test machine to trust the certificate.
- The repo must not create certificates, import certificates, read signing secrets, or mutate global trust stores.
- The install path must remain per-user and reversible.

## Design

- Keep signed MSIX packaging as the release path when a maintainer-owned certificate is available.
- Add a per-user Dev ZIP install helper for source-built testing:
  - Default behavior prints an install plan only.
  - `-Execute` extracts a validated dev ZIP under `%LocalAppData%`.
  - `-Execute` creates a current-user Start Menu shortcut with `WScript.Shell`.
  - Package input must stay under `VoiceInk.Windows\artifacts`.
  - Install output must stay under `%LocalAppData%`.
- Add a matching uninstall helper:
  - Default behavior prints an uninstall plan only.
  - `-Execute` removes the current-user Start Menu shortcut and the bounded `%LocalAppData%` install folder.
- Do not install MSIX packages, create certificates, import certificates, modify machine-wide folders, or write uninstall registry entries in this source-built helper.

## Completion

Completed on 2026-05-26:

- Added `install-dev-zip.ps1`.
- Added `uninstall-dev-zip.ps1`.
- Added packaging asset tests for per-user safety and explicit `-Execute` mutation.
- Updated README and completion tracking.
