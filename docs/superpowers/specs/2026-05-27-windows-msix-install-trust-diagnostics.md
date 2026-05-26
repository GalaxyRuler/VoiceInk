# Windows MSIX Install Trust Diagnostics Design

## Context

VoiceInk for Windows already has MSIX packaging, artifact validation, and an install-smoke helper that mutates the machine only when `-Execute` is passed. The remaining signed-install path is still maintainer-gated because MSIX install success depends on a trusted signer certificate on the test machine.

Microsoft's MSIX guidance calls out certificate trust and deployment logs as the key troubleshooting path for local signed-package testing. The Windows fork should expose those details in the smoke plan before any install attempt, while preserving the open-source rule and avoiding certificate creation, import, or paid release services.

## Goal

Improve `smoke-msix-install.ps1` so the default plan prints read-only trust diagnostics:

- Authenticode signature status;
- signer certificate subject and thumbprint when present;
- `0x800B0109` trust-failure guidance;
- `TrustedPeople` trust-store reference;
- `Microsoft-Windows-AppxDeployment-Server` deployment-log reference.

## Non-Goals

- No certificate creation, import, trust-store mutation, signing, package build, install, or uninstall unless the existing explicit `-Execute` path is used.
- No paid signing provider integration.
- No automatic selection of a certificate.

## Testability

`WindowsPackagingAssetsTests` asserts that the MSIX install smoke helper contains the read-only signature and trust diagnostics and still avoids certificate mutation commands.
