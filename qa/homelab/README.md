# VoiceInk Windows Homelab Runner Notes

VoiceInk Windows uses project-owned Homelab metadata for route discovery and dry-run validation planning.

## Runner Classes

- `container`: release metadata and script readiness planning.
- `headless`: Windows .NET CLI command planning.

The central Homelab runner currently supports only `container` and `headless` project classes. GUI, MSIX install/uninstall, certificate trust, and App Installer launch validation must not run on the active WHITEDRAGON desktop through Homelab.

## Profiles

- `qa/profiles/windows-dotnet-cli.json`: plans restore, test, and Debug x64 build commands with the repo-local .NET SDK.
- `qa/profiles/release-metadata.json`: plans non-mutating release readiness and packaging asset checks.

Both profiles set `dryRunOnly = true`, `liveExecution.allowed = false`, and `networkMode = none`.

## Safety Boundary

These profiles do not:

- install or uninstall MSIX/MSI packages;
- launch the WinUI app;
- import signing certificates;
- mutate certificate trust stores;
- publish `.appinstaller` files;
- access secrets.

Before opening live execution, a project owner must review runner image, runtime paths, network mode, command list, and artifact expectations.
