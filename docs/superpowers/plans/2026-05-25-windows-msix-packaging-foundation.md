# Windows MSIX Packaging Foundation Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add a safe, documented MSIX packaging foundation for the open-source Windows fork.

**Architecture:** Keep default builds unpackaged. Add manifest and script assets under `VoiceInk.Windows`, with tests that validate packaging metadata and script safety.

**Tech Stack:** .NET 10, WinUI 3, Windows App SDK single-project packaging metadata, PowerShell, xUnit XML/file tests.

---

### Task 1: Packaging Tests

**Files:**
- Create: `VoiceInk.Windows/tests/VoiceInk.Windows.Core.Tests/Packaging/WindowsPackagingAssetsTests.cs`

- [ ] **Step 1: Write failing tests**

Test that `Package.appxmanifest` exists and has expected identity/capabilities/application metadata. Test that `package-msix.ps1` exists and contains certificate-path requirement, artifact-root path safety, package build properties, and no certificate import/generation commands.

- [ ] **Step 2: Run focused tests and verify red**

Run:

```powershell
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" test VoiceInk.Windows\tests\VoiceInk.Windows.Core.Tests\VoiceInk.Windows.Core.Tests.csproj --filter "FullyQualifiedName~WindowsPackagingAssetsTests"
```

Expected: fail because the manifest and script do not exist.

### Task 2: Manifest and Script

**Files:**
- Create: `VoiceInk.Windows/src/VoiceInk.Windows.App/Package.appxmanifest`
- Create: `VoiceInk.Windows/scripts/package-msix.ps1`

- [ ] **Step 1: Add package manifest**

Create a Windows App SDK MSIX manifest for `VoiceInk.Windows.App.exe`, display name `VoiceInk for Windows`, identity `VoiceInk.Windows`, publisher `CN=VoiceInkOpenSource`, full trust restricted capability, and microphone capability.

- [ ] **Step 2: Add package script**

Create a PowerShell script that validates output-root safety, requires `-PackageCertificateKeyFile`, runs `dotnet publish` with package properties, and prints manual smoke commands.

- [ ] **Step 3: Run focused tests and verify green**

Run the focused packaging asset tests.

### Task 3: Docs, Verification, Commit

**Files:**
- Modify: `README.md`
- Modify: `docs/superpowers/project-completion.md`

- [ ] **Step 1: Document MSIX packaging**

Add README instructions for signing requirement and manual smoke commands.

- [ ] **Step 2: Verify**

Run focused packaging tests, full solution tests, Debug x64 build, `package-msix.ps1 -Help`, and `git diff --check`.

- [ ] **Step 3: Commit**

Commit only intentional source/docs/test/script changes.
