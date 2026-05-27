# Windows Release SBOM Readiness

## Goal

Close a packaging transparency gap by adding non-mutating SBOM readiness guidance to the Windows release checklist.

## Source Of Truth

- VoiceInk for Windows is an open-source fork and release artifacts should be transparent and auditable.
- Microsoft documents open-source SBOM tooling that creates SPDX 2.2-compatible SBOMs for release artifacts.

## Online Grounding

- Microsoft SBOM Tool is open source and creates SPDX 2.2-compatible SBOMs for varied artifacts: https://github.com/microsoft/sbom-tool
- Microsoft describes SBOMs as release transparency artifacts that list software components used in a build: https://devblogs.microsoft.com/engineering-at-microsoft/generating-software-bills-of-materials-sboms-with-spdx-at-microsoft/

## Requirements

- Extend `test-release-readiness.ps1` with a read-only SBOM readiness section.
- Mention SPDX 2.2, Microsoft SBOM Tool, staged artifacts, and release-side publication of the generated SBOM.
- Keep the readiness script non-mutating: no package publish, install, signing, certificate, or trust operations.
- Update README and project completion tracker.

## Acceptance

- Focused packaging asset test fails before SBOM readiness text exists.
- Focused packaging test and release readiness script pass after implementation.
- Full solution tests and Debug x64 build pass.
