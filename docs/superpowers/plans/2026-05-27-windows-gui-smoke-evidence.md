# Windows GUI Smoke Evidence Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add a default-off disposable-runner GUI smoke lane that launches the signed MSIX app and uploads logs, window evidence, and a screenshot.

**Architecture:** Keep mutation inside the existing manual self-hosted Windows workflow. Add one project-owned PowerShell script for install/launch/evidence/uninstall, extend the evidence validator to recognize GUI evidence, and cover the behavior with static packaging tests.

**Tech Stack:** GitHub Actions workflow YAML, PowerShell, xUnit packaging asset tests, WinUI packaged app launch through `shell:AppsFolder`.

---

### Task 1: GUI Smoke Workflow and Evidence

**Files:**
- Create: `VoiceInk.Windows/scripts/smoke-msix-gui.ps1`
- Modify: `.github/workflows/windows-installer-smoke.yml`
- Modify: `VoiceInk.Windows/scripts/test-installer-smoke-evidence.ps1`
- Modify: `VoiceInk.Windows/tests/VoiceInk.Windows.Core.Tests/Packaging/WindowsPackagingAssetsTests.cs`
- Modify: `qa/vm/README.md`
- Modify: `docs/superpowers/project-completion.md`

- [x] **Step 1: Write failing packaging tests**

Add assertions that the workflow exposes `run_gui_smoke`, that GUI execution is gated by both `execute_install_smoke` and `run_gui_smoke`, that `smoke-msix-gui.ps1` captures `gui-smoke-log.txt`, `gui-smoke-window.json`, and `gui-smoke-screenshot.png`, and that the evidence validator supports `-RequireGuiSmoke`.

- [x] **Step 2: Run the focused test and verify RED**

Run:

```powershell
& '..\.dotnet-sdk-10\dotnet.exe' test VoiceInk.Windows\tests\VoiceInk.Windows.Core.Tests\VoiceInk.Windows.Core.Tests.csproj --filter FullyQualifiedName~WindowsPackagingAssetsTests
```

Expected: fails because `run_gui_smoke`, `smoke-msix-gui.ps1`, and `RequireGuiSmoke` do not exist yet.

- [x] **Step 3: Implement the GUI smoke script**

Create `smoke-msix-gui.ps1` with explicit package path validation, Authenticode `Valid` enforcement, `Add-AppxPackage`, `shell:AppsFolder` launch, process/window polling, screenshot capture, JSON evidence output, and guaranteed uninstall cleanup in `finally`.

- [x] **Step 4: Wire the workflow and evidence validator**

Add `run_gui_smoke` to workflow inputs, write `GUI smoke requested:` in `installer-smoke-summary.txt`, skip the old install/uninstall-only step when GUI smoke is requested, run `smoke-msix-gui.ps1` only when both booleans are true, and extend `test-installer-smoke-evidence.ps1 -RequireGuiSmoke` to require the GUI log, window JSON, and screenshot.

- [x] **Step 5: Update docs and completion tracker**

Update `qa/vm/README.md` and `docs/superpowers/project-completion.md` so the remaining gate is actual maintainer execution on a disposable runner, not missing source-level GUI evidence plumbing.

- [x] **Step 6: Run focused verification and commit**

Run:

```powershell
& '..\.dotnet-sdk-10\dotnet.exe' test VoiceInk.Windows\tests\VoiceInk.Windows.Core.Tests\VoiceInk.Windows.Core.Tests.csproj --filter FullyQualifiedName~WindowsPackagingAssetsTests
git diff --check
```

Expected: packaging asset tests pass and whitespace check passes.
