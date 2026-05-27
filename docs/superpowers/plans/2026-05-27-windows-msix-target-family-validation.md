# Windows MSIX Target Family Validation Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Make non-installing MSIX artifact validation catch missing or incorrect Windows Desktop target-device metadata.

**Architecture:** Extend the existing packaging asset test and `test-msix-package.ps1` manifest checks. Keep all behavior read-only and artifact-root bounded.

**Tech Stack:** .NET 10, xUnit, PowerShell.

---

### Task 1: Red Test Packaging Coverage

**Files:**
- Modify: `VoiceInk.Windows/tests/VoiceInk.Windows.Core.Tests/Packaging/WindowsPackagingAssetsTests.cs`

- [x] **Step 1: Add source manifest assertions**

Assert `Package.appxmanifest` contains `TargetDeviceFamily` with `Windows.Desktop`, `10.0.19041.0`, and `10.0.26100.0`.

- [x] **Step 2: Add artifact validator assertions**

Assert `test-msix-package.ps1` contains those same compatibility strings so packaged artifacts are checked after build.

- [x] **Step 3: Run focused RED**

Run:

```powershell
& '..\.dotnet-sdk-10\dotnet.exe' test VoiceInk.Windows\tests\VoiceInk.Windows.Core.Tests\VoiceInk.Windows.Core.Tests.csproj --filter FullyQualifiedName~WindowsPackagingAssetsTests.MsixPackageSmokeScript_ValidatesPackageContentsWithoutInstalling -nr:false -p:UseSharedCompilation=false
```

Expected RED: `TargetDeviceFamily` is not found in `test-msix-package.ps1`.

### Task 2: MSIX Validator

**Files:**
- Modify: `VoiceInk.Windows/scripts/test-msix-package.ps1`

- [x] **Step 1: Parse and validate TargetDeviceFamily**

After identity validation, select `/appx:Package/appx:Dependencies/appx:TargetDeviceFamily` and require:

- `Name = Windows.Desktop`
- `MinVersion = 10.0.19041.0`
- `MaxVersionTested = 10.0.26100.0`

- [x] **Step 2: Run focused GREEN**

Run the focused packaging test again.

Expected GREEN: one focused packaging test passes.

- [x] **Step 3: Run all packaging asset tests**

Run:

```powershell
& '..\.dotnet-sdk-10\dotnet.exe' test VoiceInk.Windows\tests\VoiceInk.Windows.Core.Tests\VoiceInk.Windows.Core.Tests.csproj --filter FullyQualifiedName~WindowsPackagingAssetsTests -nr:false -p:UseSharedCompilation=false
```

Expected GREEN: all packaging asset tests pass.

### Task 3: Docs, Verification, Commit

**Files:**
- Create: `docs/superpowers/specs/2026-05-27-windows-msix-target-family-validation.md`
- Create: `docs/superpowers/plans/2026-05-27-windows-msix-target-family-validation.md`
- Modify: `docs/superpowers/specs/2026-05-24-windows-open-source-parity-design.md`
- Modify: `docs/superpowers/project-completion.md`

- [x] **Step 1: Update parity docs**

Document TargetDeviceFamily validation and move Packaging from 97% to 98%.

- [x] **Step 2: Run full verification**

Run:

```powershell
.\VoiceInk.Windows\scripts\test-release-readiness.ps1
& '..\.dotnet-sdk-10\dotnet.exe' test VoiceInk.Windows\VoiceInk.Windows.sln -nr:false -p:UseSharedCompilation=false
& '..\.dotnet-sdk-10\dotnet.exe' build VoiceInk.Windows\VoiceInk.Windows.sln -c Debug -p:Platform=x64 -nr:false -p:UseSharedCompilation=false
git diff --check
```

- [x] **Step 3: Commit**

Commit with:

```powershell
git add VoiceInk.Windows/scripts/test-msix-package.ps1 VoiceInk.Windows/tests/VoiceInk.Windows.Core.Tests/Packaging/WindowsPackagingAssetsTests.cs docs/superpowers/specs/2026-05-27-windows-msix-target-family-validation.md docs/superpowers/plans/2026-05-27-windows-msix-target-family-validation.md docs/superpowers/specs/2026-05-24-windows-open-source-parity-design.md docs/superpowers/project-completion.md
git commit -m "test(windows): validate msix target family"
```
