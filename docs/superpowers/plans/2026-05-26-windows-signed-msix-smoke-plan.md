# Windows Signed MSIX Smoke Plan Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add a safe signed-MSIX smoke helper whose default behavior is a non-mutating install/query/uninstall plan.

**Architecture:** Keep this as a repo-local PowerShell script under `VoiceInk.Windows/scripts`. Use static xUnit packaging tests to pin the safety contract and script wording.

**Tech Stack:** PowerShell, .NET 10 xUnit, MSIX Appx PowerShell cmdlets.

---

### Task 1: Document signed MSIX smoke helper

**Files:**
- Create: `docs/superpowers/specs/2026-05-26-windows-signed-msix-smoke-plan-design.md`
- Create: `docs/superpowers/plans/2026-05-26-windows-signed-msix-smoke-plan.md`

- [x] **Step 1: Save design and plan**

Capture safe default behavior, explicit execute mode, non-goals, and verification.

### Task 2: Add failing script safety test

**Files:**
- Modify: `VoiceInk.Windows/tests/VoiceInk.Windows.Core.Tests/Packaging/WindowsPackagingAssetsTests.cs`

- [x] **Step 1: Add `MsixInstallSmokeScript_PrintsPlanByDefaultAndRequiresExecuteForMutation`**

Assert `smoke-msix-install.ps1` contains `Execute`, `MSIX install smoke plan`, `Add-AppxPackage -Path`, `Get-AppxPackage -Name`, `Remove-AppxPackage -Package`, `PackagePath is required unless -Help is used`, `Refusing to smoke package outside artifact root`, `Assert-NoReparsePointInPath`, and `This script does not create or import certificates`.

Assert it does not contain `New-SelfSignedCertificate`, `Import-PfxCertificate`, `Import-Certificate`, `cert:\`, or `Start-Process`.

- [x] **Step 2: Run focused packaging tests and confirm failure**

Run:

```powershell
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" test VoiceInk.Windows\tests\VoiceInk.Windows.Core.Tests\VoiceInk.Windows.Core.Tests.csproj --filter WindowsPackagingAssetsTests
```

Expected: fail because the script does not exist yet.

### Task 3: Implement script

**Files:**
- Create: `VoiceInk.Windows/scripts/smoke-msix-install.ps1`

- [x] **Step 1: Add help and path validation**

Resolve repo root, artifacts root, package path, `.msix` extension, artifact containment, and reparse-point safety.

- [x] **Step 2: Add default plan output**

Print trust prerequisite and exact `Add-AppxPackage`, `Get-AppxPackage`, and `Remove-AppxPackage` commands without executing them.

- [x] **Step 3: Add explicit execute mode**

When `-Execute` is present, install, query, and uninstall the exact installed package full name.

- [x] **Step 4: Run focused packaging tests**

Confirm the static packaging tests pass.

### Task 4: Document workflow

**Files:**
- Modify: `README.md`
- Modify: `docs/superpowers/project-completion.md`

- [x] **Step 1: Add README smoke commands**

Document default plan mode and explicit `-Execute`.

- [x] **Step 2: Update completion tracker**

Raise packaging modestly and set current slice to signed MSIX smoke plan.

### Task 5: Verify and commit

**Files:**
- All changed files

- [x] **Step 1: Run script help and plan mode**

Run script `-Help` and plan mode against an artifact-local synthetic `.msix` file.

- [x] **Step 2: Run focused and full verification**

Run focused packaging tests, full solution tests, Debug x64 build, and `git diff --check`.

- [x] **Step 3: Review and commit**

Commit with:

```powershell
git commit -m "feat(windows): add signed msix smoke plan"
```
