# Windows Signed MSIX Build Validation Implementation Plan

**Goal:** Add a gated signed MSIX build validation path that runs the existing non-installing artifact validator after a real signed package build.

**Tech Stack:** PowerShell, .NET 10 xUnit packaging tests, Windows App SDK single-project MSIX publish.

## Task 1: Add Test Coverage

Add a focused packaging asset test asserting `package-msix.ps1` exposes `ValidateAfterBuild`, finds a built `.msix`, invokes `test-msix-package.ps1`, and does not add certificate-store or install/uninstall mutation.

Status: completed.

## Task 2: Implement Post-Build Validation

Update `package-msix.ps1` to:

- accept `-ValidateAfterBuild`;
- document it in usage and preflight output;
- locate exactly one `.msix` under the publish root after `dotnet publish`;
- invoke `test-msix-package.ps1 -PackagePath <built-msix>` when requested;
- fail clearly when package discovery or validation fails.

Status: completed.

## Task 3: Document Packaging Flow

Update README and completion tracker to mention the optional signed-build validation step while preserving the no-secrets/no-machine-mutation packaging posture.

Status: completed.

## Verification Plan

- Run focused packaging tests.
- Run `package-msix.ps1 -Help`.
- Run `package-msix.ps1 -Preflight`.
- Run full solution tests.
- Run Debug x64 build.
- Run `git diff --check`.
