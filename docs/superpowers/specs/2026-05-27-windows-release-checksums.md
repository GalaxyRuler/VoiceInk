# Windows Release Checksums

## Goal

Close a packaging evidence gap by adding a non-mutating SHA256 checksum manifest helper for release artifacts.

## Source Of Truth

- Open-source release artifacts should be reviewable and reproducible without commercial channels.
- Packaging helpers must not install packages, trust certificates, create certificates, publish artifacts, or mutate machine state.

## Online Grounding

- Microsoft documents `Get-FileHash` as the PowerShell cmdlet for calculating file hashes; SHA256 is the default and appropriate for release integrity evidence: https://learn.microsoft.com/powershell/module/microsoft.powershell.utility/get-filehash
- Microsoft documents `Resolve-Path -LiteralPath` for resolving paths exactly as typed, which fits artifact paths that may contain special characters: https://learn.microsoft.com/powershell/module/microsoft.powershell.management/resolve-path

## Requirements

- Add a `write-release-checksums.ps1` script that writes a deterministic `SHA256SUMS.txt` file under `VoiceInk.Windows\artifacts`.
- Accept one or more artifact paths, resolve them with literal path handling, and require each artifact to stay inside `VoiceInk.Windows\artifacts`.
- Write uppercase SHA256 values with repository-artifact-relative paths.
- Add packaging tests and release readiness wiring.
- Keep the helper non-mutating beyond writing its own output file inside artifacts.

## Acceptance

- Focused packaging tests fail before the script and readiness wiring exist.
- Script help and a smoke checksum run over a local artifact fixture pass after implementation.
- Full solution tests and Debug x64 build pass.
