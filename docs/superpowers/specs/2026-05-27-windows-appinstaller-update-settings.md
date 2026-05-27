# Windows App Installer Update Settings

VoiceInk for Windows can generate and validate `.appinstaller` manifests. The optional App Installer release path also needs explicit update-check control so maintainers can decide whether a generated file should check for updates on launch.

Microsoft's App Installer schema supports `UpdateSettings` with `OnLaunch` and `HoursBetweenUpdateChecks`, where the hour value is constrained to 0 through 255. VoiceInk should expose that as an opt-in generator flag and validate it when present.

## Requirements

- `write-appinstaller.ps1` accepts:
  - `-EnableOnLaunchUpdateCheck`;
  - `-HoursBetweenUpdateChecks`, defaulting to 24;
  - range validation from 0 through 255.
- When enabled, the generated `.appinstaller` includes `UpdateSettings/OnLaunch` with `HoursBetweenUpdateChecks`.
- `test-appinstaller.ps1` validates optional `UpdateSettings`:
  - when `UpdateSettings` exists, `OnLaunch` must exist;
  - `HoursBetweenUpdateChecks` must exist and parse as an integer between 0 and 255.
- The scripts remain non-mutating and must not publish, launch, install, uninstall, sign, create/import certificates, or modify trust stores.

## Non-Goals

- No automatic updates for source-run or dev ZIP installs.
- No `.appinstaller` publishing.
- No App Installer launch smoke.
