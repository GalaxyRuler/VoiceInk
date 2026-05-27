# Windows WinGet GPL License Metadata

## Goal

Correct the Windows WinGet manifest generator and validator so generated release metadata reflects the repository GPL license instead of the incorrect MIT label.

## Source Of Truth

- The repository root `LICENSE` contains the GNU GPL license text.
- The README presents VoiceInk as GPL v3 and the Windows fork as free/open-source.
- Microsoft's WinGet manifest documentation includes `License` and `LicenseUrl` as default-locale manifest fields. The Windows fork should use those fields to identify the GPL license and point to the repository license file.

## Requirements

- `write-winget-manifest.ps1` writes `License: GPL-3.0`.
- `test-winget-manifest.ps1` validates `License: GPL-3.0`.
- Packaging asset tests reject the old `License: MIT` metadata.
- Keep WinGet generation non-mutating: no `winget`, install, uninstall, signing, certificate creation, or certificate trust changes.

## Acceptance

- Focused WinGet packaging tests fail before implementation because `License: GPL-3.0` is absent.
- Focused tests pass after implementation.
- Generated WinGet manifest smoke validation passes.
- Full solution tests and Debug x64 build pass.
