# Windows Release Readiness Report Design

## Context

VoiceInk for Windows now has source-run instructions, a self-contained dev ZIP path, MSIX manifest/script foundation, certificate-free MSIX preflight, non-installing MSIX artifact validation, signed-build validation, and a gated signed install smoke helper.

Microsoft documents MSIX as a signed package workflow: packages must be signed, and packages signed with development/self-signed certificates require the certificate to be trusted on the target machine before install. This slice must not create certificates, import certificates, trust certificates, install packages, uninstall packages, publish packages, or read signing secrets.

## Goal

Add a repo-local read-only release readiness report that gives maintainers one command to confirm the Windows release packaging assets are present and to see the exact next commands for:

- certificate-free MSIX preflight;
- dev ZIP packaging and validation;
- signed MSIX publishing with a maintainer-owned certificate;
- non-installing MSIX artifact validation;
- gated signed MSIX install/query/uninstall smoke on a trusted test machine.

## Non-Goals

- No certificate generation, import, trust-store mutation, or secret handling.
- No package build, publish, install, uninstall, launch, or signing.
- No commercial update channel, paid store flow, licensing gate, or telemetry.

## User Experience

Running `VoiceInk.Windows/scripts/test-release-readiness.ps1` prints a concise release readiness report:

- required repository assets and whether each is present;
- non-mutating validation commands;
- maintainer-gated signed release commands;
- reminder that certificate trust/install smoke must happen on an explicitly prepared test machine.

The script exits non-zero when a required packaging asset is missing.

## Safety

The script must avoid direct execution of `Add-AppxPackage`, `Remove-AppxPackage`, `dotnet publish`, signing tools, and certificate-store commands. It may print those commands as manual next steps only.
