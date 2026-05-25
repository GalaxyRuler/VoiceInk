# Windows MSIX Artifact Validator Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add a safe MSIX artifact validator that inspects package contents without installing, launching, signing, trusting certificates, or claiming cryptographic signature validation.

**Architecture:** Add a repo-local PowerShell validator under `VoiceInk.Windows/scripts` and extend the existing packaging asset tests to assert the script's safety contract. Document the validation flow in the root README and project completion tracker.

**Tech Stack:** PowerShell, .NET 10 xUnit packaging tests, MSIX ZIP container inspection, XML manifest parsing.

---

### Task 1: Script Contract Test

**Files:**
- Modify: `VoiceInk.Windows/tests/VoiceInk.Windows.Core.Tests/Packaging/WindowsPackagingAssetsTests.cs`

- [x] **Step 1: Write failing script test**

Assert `VoiceInk.Windows/scripts/test-msix-package.ps1` exists, checks the expected MSIX files, parses `AppxManifest.xml`, refuses paths outside artifacts, rejects package input inside the cleanup root, avoids install/remove/signing commands, and prints manual smoke commands.

- [x] **Step 2: Verify red**

Focused packaging tests failed because `scripts/test-msix-package.ps1` did not exist.

### Task 2: Validator Script

**Files:**
- Create: `VoiceInk.Windows/scripts/test-msix-package.ps1`

- [x] **Step 1: Implement validator**

Add artifact-root path safety, reparse-point rejection, MSIX extraction, cleanup-root input rejection, required-file checks, manifest XML checks, commercial-word checks, clear signature-file-only wording, and manual install/query/uninstall smoke output.

- [x] **Step 2: Verify focused tests**

Run:

```powershell
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" test VoiceInk.Windows\tests\VoiceInk.Windows.Core.Tests\VoiceInk.Windows.Core.Tests.csproj --filter WindowsPackagingAssetsTests
```

Focused packaging tests passed.

### Task 3: Synthetic Package Smoke And Docs

**Files:**
- Modify: `README.md`
- Modify: `docs/superpowers/project-completion.md`

- [x] **Step 1: Validate a synthetic package**

Created a minimal artifact-local `.msix` ZIP with `AppxManifest.xml`, `AppxBlockMap.xml`, `AppxSignature.p7x`, and `VoiceInk.Windows.App.exe`, then ran the validator successfully. This proves the script execution path without needing a private signing certificate.

- [x] **Step 2: Document MSIX validation**

Add README instructions for `test-msix-package.ps1` and update the completion bar to reflect MSIX artifact validation.

- [x] **Step 3: Full verification and commit**

Ran focused packaging tests, synthetic MSIX smoke, script help, full solution tests, Debug x64 build, and review. Fixed Important review findings around reparse-point scratch paths and signature wording before commit.
