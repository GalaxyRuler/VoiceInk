# Windows WinApp Signing Reference Design

## Context

VoiceInk for Windows already has MSIX packaging scripts, artifact validation, a signed install smoke helper, and a release readiness report. Actual signed MSIX installation remains gated on a trusted maintainer/test-machine signing setup.

Current Microsoft MSIX guidance documents WinApp CLI as the recommended local development path for self-signed testing certificates, while still requiring explicit certificate trust before install. The repository should point maintainers toward that route without mutating the machine or creating certificates during readiness checks.

## Goal

Extend the read-only release readiness report with a WinApp CLI local signing reference:

- mention `winapp cert generate` for optional disposable-machine local development certificates;
- mention `winapp sign` for local development signing after manifest/certificate subject checks;
- restate that certificate trust remains external and this script never creates/imports certificates.

## Non-Goals

- No certificate generation.
- No certificate import/trust step.
- No package signing.
- No package install/uninstall.
- No dependency on WinApp CLI being installed.

## Testability

Packaging asset tests assert the readiness report contains the reference text and still avoids certificate mutation commands. The script itself is run as a read-only verification command.
