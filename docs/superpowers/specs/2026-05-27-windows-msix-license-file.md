# Windows MSIX License File

## Goal

Include the repository GPL license text in signed MSIX release artifacts, matching the dev ZIP license inclusion and keeping binary distribution metadata open-source compliant.

## Source Of Truth

- The repository root `LICENSE` contains the GNU GPL license text.
- Binary recipients should receive a copy of the license text with the application artifact.
- The MSIX package should include this as a neutral `LICENSE.txt` payload file, not as a commercial licensing gate or app feature.

## Requirements

- The Windows app project includes the root `LICENSE` as `LICENSE.txt` in build/package output.
- `test-msix-package.ps1` validates `LICENSE.txt` exists in an extracted MSIX and contains GPL text.
- Existing MSIX manifest checks continue to reject commercial wording in `AppxManifest.xml`.
- No installer execution, signing, certificate creation/import, trust-store mutation, or global machine changes are added.

## Acceptance

- Focused packaging tests fail before implementation because project/package license inclusion is absent.
- Focused tests pass after implementation.
- Full solution tests and Debug x64 build pass.
