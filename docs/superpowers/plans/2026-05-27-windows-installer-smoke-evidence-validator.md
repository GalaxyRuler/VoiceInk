# Windows Installer Smoke Evidence Validator Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add a non-mutating script that validates uploaded installer-smoke evidence from the disposable Windows runner lane.

**Architecture:** Keep validation in a standalone PowerShell script under `VoiceInk.Windows\scripts`; keep tests as static packaging asset tests.

**Tech Stack:** PowerShell, xUnit static asset tests, project release-readiness script.

---

### Task 1: Evidence Validator

**Files:**
- Create: `VoiceInk.Windows/scripts/test-installer-smoke-evidence.ps1`
- Modify: `VoiceInk.Windows/tests/VoiceInk.Windows.Core.Tests/Packaging/WindowsPackagingAssetsTests.cs`
- Modify: `VoiceInk.Windows/scripts/test-release-readiness.ps1`
- Modify: `README.md`
- Modify: `docs/superpowers/project-completion.md`

- [x] **Step 1: Write failing static asset test**

Add a test that expects `test-installer-smoke-evidence.ps1` to contain:

```text
installer-smoke-summary.txt
RequireInstallSmoke
RequireWackReport
Install smoke executed: true
Windows App Certification Kit requested: true
wack-report.xml
[xml]
Installer smoke evidence validation passed
This script does not install
```

- [x] **Step 2: Run RED**

Run:

```powershell
& '..\.dotnet-sdk-10\dotnet.exe' test VoiceInk.Windows\tests\VoiceInk.Windows.Core.Tests\VoiceInk.Windows.Core.Tests.csproj --filter WindowsPackagingAssetsTests.InstallerSmokeEvidenceScript_ValidatesUploadedRunnerEvidenceWithoutMutation -nr:false -p:UseSharedCompilation=false
```

Result: FAIL because the script did not exist yet.

- [x] **Step 3: Implement script and docs references**

Create the script with read-only summary and optional WACK XML checks. Add it to release-readiness required assets and README packaging notes.

- [x] **Step 4: Run GREEN and script smoke**

Run:

```powershell
& '..\.dotnet-sdk-10\dotnet.exe' test VoiceInk.Windows\tests\VoiceInk.Windows.Core.Tests\VoiceInk.Windows.Core.Tests.csproj --filter FullyQualifiedName~WindowsPackagingAssetsTests -nr:false -p:UseSharedCompilation=false
.\VoiceInk.Windows\scripts\test-release-readiness.ps1
```

Create a temporary evidence folder outside the repo, run the validator with both requirements, then remove the temporary folder.

Result: Focused evidence-script test passed 1/1, temporary evidence validation passed for default and custom WACK report filenames, packaging asset tests passed 21/21, and release-readiness passed.

- [x] **Step 5: Full verification and commit**

Run full solution tests/build, `git diff --check`, then commit:

```powershell
git add VoiceInk.Windows\scripts\test-installer-smoke-evidence.ps1 VoiceInk.Windows\tests\VoiceInk.Windows.Core.Tests\Packaging\WindowsPackagingAssetsTests.cs VoiceInk.Windows\scripts\test-release-readiness.ps1 README.md docs\superpowers\specs\2026-05-27-windows-installer-smoke-evidence-validator.md docs\superpowers\plans\2026-05-27-windows-installer-smoke-evidence-validator.md docs\superpowers\project-completion.md
git commit -m "test(windows): validate installer smoke evidence"
```

Result: Full solution tests passed Core 825/825 and Infrastructure 267/267, and Debug x64 build passed with 0 warnings and 0 errors.
