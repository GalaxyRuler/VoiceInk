# Windows App Certification Kit Readiness Design

## Context

VoiceInk for Windows already has signed MSIX packaging scripts, App Installer metadata, WinGet metadata, checksum/SBOM guidance, and a manually gated self-hosted Windows installer-smoke workflow.

Microsoft's Windows App Certification Kit documentation says MSIX packages should be validated locally before Store/MSIX submission. The command-line path requires an active user session and supports both installed-package validation with `appcert.exe test -packagefullname` and package-file validation with `appcert.exe test -appxpackagepath`.

## Design

Add WACK readiness without changing the repository's safety boundary:

- Extend the read-only release readiness report with a Windows App Certification Kit section.
- Mention both installed package and package-path command shapes.
- Keep `appcert.exe` execution out of local readiness checks.
- Extend the manual self-hosted Windows installer-smoke workflow with a default-off `run_wack` input.
- Upload the requested `wack-report.xml` path with the existing installer-smoke evidence.

## Safety Boundary

- No local active-desktop WACK run.
- No certificate creation/import/trust.
- No signing, publishing, install, uninstall, or launch outside the existing manual workflow gate.
- WACK execution belongs on a disposable or prepared self-hosted Windows runner with an active user session.

## Verification

- Packaging asset tests assert the readiness report and workflow contain WACK guidance.
- The workflow remains manual and default-off for WACK.
- Full solution tests/build and whitespace checks remain required before committing the slice.
