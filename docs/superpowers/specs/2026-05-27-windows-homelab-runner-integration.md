# Windows Homelab Runner Integration

VoiceInk for Windows needs a clear route for noisy release validation without running GUI, installer, certificate, or registry-mutating checks on the active WHITEDRAGON desktop. The central Homelab runner currently supports `container` and `headless` project classes; Windows VM/MSIX/App Installer install smoke has moved to project-owned GitHub Actions workflows backed by disposable Windows runners.

## Requirements

- Add project-owned Homelab metadata under `.codex/homelab-runner.json`.
- Allow only `container` and `headless` classes.
- Add dry-run-only project profiles:
  - `windows-dotnet-cli` for .NET restore/test/build command planning;
  - `release-metadata` for release readiness and packaging asset planning.
- Keep `liveExecution.allowed = false` and `networkMode = none`.
- Add repo docs explaining:
  - Homelab is used for route discovery and dry-run planning;
  - GUI/MSIX/App Installer install smoke belongs in a disposable VM or self-hosted GitHub Actions workflow;
  - no certificates, trust stores, installers, publishing, or secrets are handled by Homelab metadata.
- Register the project with the central Homelab registry when the registration script exists.

## Non-Goals

- No live Homelab container execution.
- No active-desktop GUI automation.
- No VM mutation from this session.
- No certificate, API key, or secret handling.
