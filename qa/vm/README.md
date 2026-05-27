# VoiceInk Windows VM and Installer QA Lane

Windows GUI, MSIX install/uninstall, App Installer launch, registry, certificate trust, and upgrade validation belong in a project-owned Windows VM or self-hosted GitHub Actions workflow.

## Current Boundary

The central Homelab runner no longer orchestrates `vm-gui`, `desktop-msi`, or VM-mutating lanes. Homelab can provision and reset VM infrastructure, but project repos own the workflow YAML, installer steps, logs, artifacts, and manual approval gates.

## Manual Workflow Shape

When a maintainer-owned signed package and trusted test certificate path exist, run the manually triggered `.github/workflows/windows-installer-smoke.yml` workflow. It:

1. runs on a disposable self-hosted Windows runner label;
2. restores/builds/tests the solution with the repo-local SDK or an equivalent pinned SDK;
3. builds a signed MSIX with a maintainer-owned certificate;
4. runs `test-msix-package.ps1`;
5. runs `write-appinstaller.ps1` and `test-appinstaller.ps1` if App Installer distribution is being tested;
6. installs with `Add-AppxPackage` only inside the disposable runner when `execute_install_smoke` is true;
7. runs `smoke-msix-gui.ps1` only when both `execute_install_smoke` and `run_gui_smoke` are true;
8. launches VoiceInk for Windows through `shell:AppsFolder`, verifies the window appears, and captures `gui-smoke-log.txt`, `gui-smoke-window.json`, and `gui-smoke-screenshot.png`;
9. uninstalls with `Remove-AppxPackage`;
10. uploads logs and artifacts.

The workflow must not store certificate material in source. Use the CI platform's secret store or a VM-local secured certificate setup, and keep the signed install smoke gated by manual dispatch.

Do not run GUI smoke, MSIX install/uninstall, WACK, or certificate trust mutation on active WHITEDRAGON. Those checks require a disposable/self-hosted Windows runner with an active user session.
