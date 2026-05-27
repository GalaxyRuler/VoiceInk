# VoiceInk Windows VM and Installer QA Lane

Windows GUI, MSIX install/uninstall, App Installer launch, registry, certificate trust, and upgrade validation belong in a project-owned Windows VM or self-hosted GitHub Actions workflow.

## Current Boundary

The central Homelab runner no longer orchestrates `vm-gui`, `desktop-msi`, or VM-mutating lanes. Homelab can provision and reset VM infrastructure, but project repos own the workflow YAML, installer steps, logs, artifacts, and manual approval gates.

## Future Workflow Shape

When a maintainer-owned signed package and trusted test certificate path exist, add a manually triggered Windows workflow that:

1. runs on a disposable self-hosted Windows runner label;
2. restores/builds/tests the solution with the repo-local SDK or an equivalent pinned SDK;
3. builds a signed MSIX with a maintainer-owned certificate;
4. runs `test-msix-package.ps1`;
5. runs `write-appinstaller.ps1` and `test-appinstaller.ps1` if App Installer distribution is being tested;
6. installs with `Add-AppxPackage` only inside the disposable runner;
7. launches VoiceInk for Windows, verifies the window appears, and captures logs/screenshots;
8. uninstalls with `Remove-AppxPackage`;
9. uploads logs and artifacts.

The workflow must not store certificate material in source. Use the CI platform's secret store or a VM-local secured certificate setup, and keep the signed install smoke gated by manual dispatch.
