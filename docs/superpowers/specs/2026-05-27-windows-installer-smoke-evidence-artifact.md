# Windows Installer Smoke Evidence Artifact

## Goal

Make the manual signed MSIX installer-smoke workflow preserve non-secret run evidence from disposable Windows runners, so release validation can be reviewed without rerunning installer steps.

## Source Of Truth

- GitHub Actions supports uploading workflow artifacts with `actions/upload-artifact`.
- The existing Windows installer-smoke workflow is manual and self-hosted-runner only.
- The project must not create/import certificates or run install smoke on the active desktop.

## Requirements

- Prepare an artifact-local evidence directory under `VoiceInk.Windows\artifacts\gha-installer-smoke`.
- Write a short README/summary file explaining the signed package path, generated App Installer path, and whether install smoke was executed.
- Upload the evidence directory with `actions/upload-artifact@v4`.
- Upload evidence with `if: ${{ always() }}` so validation failures still leave context.
- Do not upload certificates, `.pfx` files, private keys, secrets, or arbitrary runner folders.

## Non-Goals

- No certificate generation/import.
- No automatic workflow triggers.
- No change to the explicit `execute_install_smoke` gate.
