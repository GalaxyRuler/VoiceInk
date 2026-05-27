# Windows Homelab Route Hardening

## Goal

Harden the project-owned Homelab metadata so release/readiness route discovery remains non-mutating and the headless .NET profile resolves to a headless route instead of silently falling back to the container route.

## Grounding

- The existing Windows Homelab spec keeps central Homelab usage limited to `container` and `headless`.
- The project profile `qa/profiles/windows-dotnet-cli.json` is a headless .NET CLI planning profile.
- The project profile `qa/profiles/release-metadata.json` is a container metadata/planning profile.
- Microsoft MSIX guidance still requires signed, trusted packages for install smoke; Homelab metadata must not create certificates, trust certificates, install packages, uninstall packages, or publish App Installer files.

## Requirements

- Add test coverage for `.codex/homelab-runner.json`.
- Assert the allowed classes are exactly `container` and `headless`.
- Assert `preferredRoutes.container` is `container`.
- Assert `preferredRoutes.headless` is `headless`.
- Assert both project profiles reference files that exist in the repo.
- Assert profile metadata stays dry-run-only, no-network, and live execution disabled.

## Non-Goals

- Do not run Homelab jobs.
- Do not register the project globally.
- Do not modify Homelab core.
- Do not install/uninstall packages, import certificates, trust certificates, or touch secrets.
