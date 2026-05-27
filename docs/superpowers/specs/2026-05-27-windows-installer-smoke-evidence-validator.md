# Windows Installer Smoke Evidence Validator Spec

## Goal

Validate the evidence artifact produced by the manual Windows installer-smoke workflow without installing, uninstalling, signing, trusting certificates, or running WACK locally.

## Source Of Truth

- Microsoft WACK documentation says the command-line kit creates report files after `appcert.exe test`, including XML report output at the requested path.
- The project workflow already writes `installer-smoke-summary.txt` and optionally uploads `wack-report.xml` under `VoiceInk.Windows\artifacts\gha-installer-smoke`.
- The repo needs a source-controlled way to verify that an uploaded evidence folder is complete enough to support a release decision.

## Requirements

- Add `VoiceInk.Windows\scripts\test-installer-smoke-evidence.ps1`.
- The script must accept `-EvidenceRoot`, plus optional `-RequireInstallSmoke` and `-RequireWackReport`.
- The script must require `installer-smoke-summary.txt`.
- The summary must mention the signed package path, install-smoke execution flag, WACK request flag, and WACK report path.
- When `-RequireInstallSmoke` is passed, the summary must show `Install smoke executed: true`.
- When `-RequireWackReport` is passed, the summary must show `Windows App Certification Kit requested: true`, the report file must exist, must be non-empty, and must parse as XML.
- The script must print a success message and remain read-only.
- Add static asset tests and release-readiness references.

## Non-Goals

- Do not run WACK.
- Do not install, uninstall, launch, sign, create/import certificates, or mutate trust stores.
- Do not infer that the WACK report passed; this script validates evidence presence and parseability, not certification semantics.
