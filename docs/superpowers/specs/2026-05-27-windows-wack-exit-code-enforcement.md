# Windows WACK Exit Code Enforcement Spec

## Goal

Make the manual Windows installer-smoke workflow fail when Windows App Certification Kit commands fail.

## Source Of Truth

- Microsoft documents the Windows App Certification Kit command-line flow with `appcert.exe reset` followed by `appcert.exe test ...`.
- PowerShell external process failures are safest when the script checks `$LASTEXITCODE` after each external command.
- The workflow is already manually dispatched, self-hosted, and default-off for WACK, so the remaining source-level gap is preserving failure evidence rather than silently uploading a failing report as a green workflow.

## Requirements

- The WACK workflow step must invoke the resolved `appcert.exe` path.
- The workflow must check `$LASTEXITCODE` after `reset`.
- The workflow must check `$LASTEXITCODE` after `test`.
- Any nonzero WACK exit must throw so the workflow fails while the existing `always()` evidence upload still runs.
- The workflow must remain manual and default-off for WACK.
- Do not run WACK locally, install packages locally, import certificates, or mutate trust stores.

## Non-Goals

- No automatic certificate generation or import.
- No local active-desktop WACK execution.
- No change to package signing or install behavior.
