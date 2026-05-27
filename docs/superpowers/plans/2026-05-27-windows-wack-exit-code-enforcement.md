# Windows WACK Exit Code Enforcement Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Fail the manual installer-smoke workflow when WACK reset or test commands return nonzero exit codes.

**Architecture:** Keep enforcement in `.github/workflows/windows-installer-smoke.yml`; keep tests as static packaging asset tests in `WindowsPackagingAssetsTests`.

**Tech Stack:** GitHub Actions, PowerShell, xUnit static asset tests.

---

### Task 1: WACK Exit Enforcement

**Files:**
- Modify: `VoiceInk.Windows/tests/VoiceInk.Windows.Core.Tests/Packaging/WindowsPackagingAssetsTests.cs`
- Modify: `.github/workflows/windows-installer-smoke.yml`
- Modify: `docs/superpowers/project-completion.md`

- [x] **Step 1: Write failing test**

Add `InstallerSmokeWorkflow_FailsWhenWackCommandsReturnNonZero` and assert the workflow contains:

```text
& $appCert reset
if ($LASTEXITCODE -ne 0)
& $appCert test -appxpackagepath
Windows App Certification Kit reset failed
Windows App Certification Kit test failed
```

- [x] **Step 2: Run RED**

Run:

```powershell
& '..\.dotnet-sdk-10\dotnet.exe' test VoiceInk.Windows\tests\VoiceInk.Windows.Core.Tests\VoiceInk.Windows.Core.Tests.csproj --filter WindowsPackagingAssetsTests.InstallerSmokeWorkflow_FailsWhenWackCommandsReturnNonZero -nr:false -p:UseSharedCompilation=false
```

Result: FAIL because the workflow called `appcert.exe` without explicit exit-code checks.

- [x] **Step 3: Update workflow**

Use the resolved `$appCert` path with the call operator, then throw if `$LASTEXITCODE` is nonzero after both `reset` and `test`.

- [x] **Step 4: Run GREEN and verification**

Run:

```powershell
& '..\.dotnet-sdk-10\dotnet.exe' test VoiceInk.Windows\tests\VoiceInk.Windows.Core.Tests\VoiceInk.Windows.Core.Tests.csproj --filter FullyQualifiedName~WindowsPackagingAssetsTests -nr:false -p:UseSharedCompilation=false
.\VoiceInk.Windows\scripts\test-release-readiness.ps1
& '..\.dotnet-sdk-10\dotnet.exe' test VoiceInk.Windows\VoiceInk.Windows.sln -nr:false -p:UseSharedCompilation=false
& '..\.dotnet-sdk-10\dotnet.exe' build VoiceInk.Windows\VoiceInk.Windows.sln -c Debug -p:Platform=x64 -nr:false -p:UseSharedCompilation=false
```

Result: WACK exit-code test passed 1/1, packaging asset tests passed 20/20, release-readiness report passed, full solution tests passed Core 824/824 and Infrastructure 267/267, and Debug x64 build passed with 0 warnings and 0 errors.

- [x] **Step 5: Commit**

Update the tracker current slice, run `git diff --check`, then commit:

```powershell
git add .github\workflows\windows-installer-smoke.yml VoiceInk.Windows\tests\VoiceInk.Windows.Core.Tests\Packaging\WindowsPackagingAssetsTests.cs docs\superpowers\specs\2026-05-27-windows-wack-exit-code-enforcement.md docs\superpowers\plans\2026-05-27-windows-wack-exit-code-enforcement.md docs\superpowers\project-completion.md
git commit -m "ci(windows): fail installer smoke on wack errors"
```
