# VoiceInk Windows Agents

<!-- BEGIN CODEX HOMELAB RUNNER INTEGRATION -->
## Homelab Runner Integration

This project may use the central Homelab runner for isolated, noisy, GUI, installer, VM, container, or long-running validation.

Project runner config:
- .codex/homelab-runner.json

Before running noisy tests, GUI automation, installers, VM jobs, or long-running validation:
1. Read .codex/homelab-runner.json.
2. Use the configured Homelab runner route.
3. Prefer containers for CLI, API, unit, integration, and headless checks.
4. Do not run GUI automation, MSI install/uninstall, or destructive validation on the active WHITEDRAGON desktop unless explicitly approved.
5. Keep project-specific runner profiles inside this repo, not in Homelab core.

<!-- END CODEX HOMELAB RUNNER INTEGRATION -->

## VoiceInk Windows Runner Boundary

Windows GUI/MSIX/App Installer install smoke is intentionally project-owned and must run through a disposable Windows VM or self-hosted GitHub Actions workflow, not through local active-desktop automation. The Homelab project config currently allows only `container` and `headless` metadata/routes because the central Homelab VM lanes have moved to project-owned CI workflows.

