# Windows Release Signing Checklist Design

## Context

VoiceInk for Windows has non-mutating packaging preflight, MSIX artifact validation, signed build validation, and a gated signed install smoke helper. The remaining real signed install smoke is correctly blocked on maintainer-owned certificate and test-machine trust setup.

Microsoft documents that MSIX packages must be signed, certificate subject must match the manifest publisher, timestamping helps signatures remain valid after certificate expiry, and self-signed/test packages require certificate trust on the target device. Release maintainers need these checks visible in the repo-local readiness report.

## Goal

Extend `test-release-readiness.ps1` with a non-mutating signing/trust checklist covering:

- manifest publisher and certificate subject must match;
- signed packages should be timestamped;
- test machines must trust the signing certificate before install smoke;
- AppxDeployment/AppxPackaging event logs are the first troubleshooting destination.

## Non-Goals

- No certificate generation.
- No certificate import or trust-store mutation.
- No signing provider integration.
- No package install/uninstall.

## Testability

Packaging tests assert the readiness script prints the signing checklist while continuing to avoid mutating commands and certificate-store access.
