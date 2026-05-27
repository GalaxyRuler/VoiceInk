# Windows Installer Smoke Workflow

VoiceInk for Windows now has MSIX/App Installer scripts and Homelab metadata, but the actual signed install smoke must run on a disposable Windows machine rather than the active desktop. The project needs a manually triggered GitHub Actions workflow that owns the installer smoke lane.

GitHub Actions supports `workflow_dispatch` inputs for manually triggered workflows and self-hosted runner labels for routing jobs to project-owned Windows runners. This workflow should validate artifacts by default and run install/uninstall only when explicitly requested.

## Requirements

- Add `.github/workflows/windows-installer-smoke.yml`.
- The workflow must:
  - use `workflow_dispatch`;
  - run on `self-hosted`, `windows`, and a caller-provided runner label;
  - require a signed `.msix` package path;
  - run `test-msix-package.ps1`;
  - generate and validate optional `.appinstaller` metadata;
  - print the signed install smoke plan;
  - execute `smoke-msix-install.ps1 -Execute` only when `execute_install_smoke` is true;
  - state that it does not create/import signing certificates.

## Non-Goals

- No automatic trigger on push or pull request.
- No certificate generation/import.
- No signing-secret storage in source.
- No active WHITEDRAGON install smoke.
