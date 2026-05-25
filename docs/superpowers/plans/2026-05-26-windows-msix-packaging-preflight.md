# Windows MSIX Packaging Preflight Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add a safe `package-msix.ps1 -Preflight` mode that validates packaging readiness without requiring certificates or changing machine state.

**Architecture:** Keep packaging in repo-local PowerShell scripts under `VoiceInk.Windows/scripts`. Extend the existing static packaging tests so safety and release-flow wording remain covered. Documentation stays in the root README and completion tracker.

**Tech Stack:** PowerShell, .NET 10 xUnit, WinUI Windows App SDK MSIX publish properties.

---

### Task 1: Document the preflight contract

**Files:**
- Create: `docs/superpowers/specs/2026-05-26-windows-msix-packaging-preflight-design.md`
- Create: `docs/superpowers/plans/2026-05-26-windows-msix-packaging-preflight.md`

- [x] **Step 1: Save the design spec**

Document `-Preflight` inputs, outputs, explicit non-goals, and verification commands.

- [x] **Step 2: Save this execution plan**

Record the TDD steps, touched files, and expected verification path.

### Task 2: Add failing preflight test

**Files:**
- Modify: `VoiceInk.Windows/tests/VoiceInk.Windows.Core.Tests/Packaging/WindowsPackagingAssetsTests.cs`

- [x] **Step 1: Add `MsixPackagingScript_ProvidesCertificateFreePreflightWithoutMachineMutation`**

Assert the script contains `Preflight`, `PackageCertificateKeyFile is required for signed packaging`, `MSIX packaging preflight passed`, `test-msix-package.ps1`, `Add-AppxPackage -Path`, `Remove-AppxPackage -Package`, and `dotnet publish`.

Assert the script does not contain `New-SelfSignedCertificate`, `Import-PfxCertificate`, `cert:\`, `Import-Certificate`, or command-invoked `Add-AppxPackage` / `Remove-AppxPackage`.

- [x] **Step 2: Run focused packaging tests and confirm failure**

Run:

```powershell
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" test VoiceInk.Windows\tests\VoiceInk.Windows.Core.Tests\VoiceInk.Windows.Core.Tests.csproj --filter WindowsPackagingAssetsTests
```

Expected: fail because `-Preflight` is not implemented yet.

### Task 3: Implement script preflight

**Files:**
- Modify: `VoiceInk.Windows/scripts/package-msix.ps1`

- [x] **Step 1: Add `-Preflight` parameter and usage text**

Describe that preflight does not require or read a certificate and does not publish.

- [x] **Step 2: Add preflight validation**

Validate app project, manifest, dotnet path resolution, artifact root containment, and publish root containment.

- [x] **Step 3: Add preflight output**

Print resolved paths, publish properties, signed build command shape, validator command, and manual install/query/uninstall smoke commands.

- [x] **Step 4: Keep signed packaging behavior unchanged**

Only require `PackageCertificateKeyFile` when `-Preflight` is not set.

- [x] **Step 5: Run focused packaging tests and confirm pass**

Run the focused packaging test command above.

### Task 4: Document user workflow

**Files:**
- Modify: `README.md`
- Modify: `docs/superpowers/project-completion.md`

- [x] **Step 1: Add README preflight command**

Document:

```powershell
.\VoiceInk.Windows\scripts\package-msix.ps1 -Preflight -DotNetPath "..\.dotnet-sdk-10\dotnet.exe"
```

- [x] **Step 2: Update completion tracker**

Raise packaging progress modestly and set current slice to MSIX packaging preflight.

### Task 5: Verify and commit

**Files:**
- All changed files

- [x] **Step 1: Run script checks**

Run:

```powershell
.\VoiceInk.Windows\scripts\package-msix.ps1 -Help
.\VoiceInk.Windows\scripts\package-msix.ps1 -Preflight -DotNetPath "..\.dotnet-sdk-10\dotnet.exe"
```

- [x] **Step 2: Run focused and full verification**

Run focused packaging tests, full solution tests, and Debug x64 build.

- [x] **Step 3: Review diff for safety**

Check that no certificate generation/import, package install/uninstall execution, or secret material was added.

- [x] **Step 4: Commit**

Commit with:

```powershell
git commit -m "feat(windows): add msix packaging preflight"
```
