# Windows Release Evidence Operator Lane Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add the non-mutating operator materials needed to prepare the disposable Windows runner, dispatch the installer/WACK/GUI workflow, and validate downloaded release evidence.

**Architecture:** Keep release execution external and explicit. Repository-owned assets document the runner setup and evidence workflow, while a read-only PowerShell helper checks GitHub workflow visibility and runner labels before printing the dispatch command.

**Tech Stack:** PowerShell, GitHub CLI, GitHub Actions self-hosted runners, Windows App Certification Kit, xUnit static packaging tests.

---

### Task 1: Operator Docs And Dispatch Readiness

**Files:**
- Create: `docs/superpowers/windows-disposable-runner-setup-packet.md`
- Create: `docs/superpowers/windows-release-evidence-operator-runbook.md`
- Create: `VoiceInk.Windows/scripts/test-installer-dispatch-readiness.ps1`
- Modify: `VoiceInk.Windows/scripts/test-release-readiness.ps1`
- Modify: `VoiceInk.Windows/tests/VoiceInk.Windows.Core.Tests/Packaging/WindowsPackagingAssetsTests.cs`

- [x] **Step 1: Write failing static packaging tests**

Add tests that require the runner setup packet, release evidence runbook, dispatch readiness script, and release-readiness references.

- [x] **Step 2: Run focused tests and verify RED**

Run:

```powershell
& '..\.dotnet-sdk-10\dotnet.exe' test VoiceInk.Windows\tests\VoiceInk.Windows.Core.Tests\VoiceInk.Windows.Core.Tests.csproj --filter FullyQualifiedName~WindowsPackagingAssetsTests
```

Expected: failure because the docs/script/readiness references do not exist yet.

- [x] **Step 3: Add docs and script**

Add the two operator docs and the read-only GitHub dispatch readiness script. The script must not run `gh workflow run`, install packages, sign packages, trust certificates, run WACK, or launch VoiceInk.

- [x] **Step 4: Run focused verification**

Run:

```powershell
& '..\.dotnet-sdk-10\dotnet.exe' test VoiceInk.Windows\tests\VoiceInk.Windows.Core.Tests\VoiceInk.Windows.Core.Tests.csproj --filter FullyQualifiedName~WindowsPackagingAssetsTests
.\VoiceInk.Windows\scripts\test-installer-dispatch-readiness.ps1 -Help
.\VoiceInk.Windows\scripts\test-release-readiness.ps1
git diff --check
```

Expected: all commands pass.
