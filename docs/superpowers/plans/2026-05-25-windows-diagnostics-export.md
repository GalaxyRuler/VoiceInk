# Windows Diagnostics Export Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add a macOS-aligned, local-only `Export Diagnostic Logs` action for the Windows fork.

**Architecture:** Keep diagnostic report formatting and secret redaction in Core so it is testable without WinUI. Keep file-picker export and app-session event collection in the WinUI app. Export plain UTF-8 logs, not telemetry.

**Tech Stack:** .NET 10, WinUI 3, Windows App SDK file picker, xUnit.

---

## File Structure

- Create `VoiceInk.Windows/src/VoiceInk.Windows.Core/Diagnostics/DiagnosticFileEntry.cs`: safe file metadata.
- Create `VoiceInk.Windows/src/VoiceInk.Windows.Core/Diagnostics/DiagnosticReportRequest.cs`: report input model.
- Create `VoiceInk.Windows/src/VoiceInk.Windows.Core/Diagnostics/DiagnosticReportBuilder.cs`: report formatting and redaction.
- Create `VoiceInk.Windows/tests/VoiceInk.Windows.Core.Tests/Diagnostics/DiagnosticReportBuilderTests.cs`: report and redaction tests.
- Create `VoiceInk.Windows/tests/VoiceInk.Windows.Core.Tests/Diagnostics/DiagnosticEventSanitizerTests.cs`: generic recent-event label tests.
- Modify `VoiceInk.Windows/src/VoiceInk.Windows.App/MainWindow.xaml`: add `Export Diagnostic Logs` button.
- Modify `VoiceInk.Windows/src/VoiceInk.Windows.App/MainWindow.xaml.cs`: keep a recent generic in-app status log, build diagnostic report inputs, and export through a save picker.
- Modify `README.md`, `docs/superpowers/project-completion.md`, and `docs/superpowers/specs/2026-05-24-windows-open-source-parity-design.md`: document behavior and update the completion bar.

## Task 1: Core Diagnostic Report Builder

**Files:**
- Create: `VoiceInk.Windows/src/VoiceInk.Windows.Core/Diagnostics/DiagnosticFileEntry.cs`
- Create: `VoiceInk.Windows/src/VoiceInk.Windows.Core/Diagnostics/DiagnosticReportRequest.cs`
- Create: `VoiceInk.Windows/src/VoiceInk.Windows.Core/Diagnostics/DiagnosticReportBuilder.cs`
- Create: `VoiceInk.Windows/tests/VoiceInk.Windows.Core.Tests/Diagnostics/DiagnosticReportBuilderTests.cs`

- [x] **Step 1: Write failing report tests**

Test that the report includes export time, app version, OS/runtime, app data paths, file inventory, active section, dictation state, selected model path, and recent generic status labels.

- [x] **Step 2: Write failing redaction tests**

Test that `api_key`, `token`, `secret`, `credential`, authorization/bearer/key aliases, and leading key-value diagnostics are redacted from diagnostic lines, while non-sensitive path text remains.

- [x] **Step 3: Verify RED**

Run:

```powershell
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" test VoiceInk.Windows\tests\VoiceInk.Windows.Core.Tests\VoiceInk.Windows.Core.Tests.csproj --filter "DiagnosticReportBuilderTests"
```

Expected: compile failures because diagnostics types do not exist yet.

- [x] **Step 4: Implement minimal Core support**

Implement report formatting with these section headers:

- `=== VoiceInk for Windows Diagnostic Logs ===`
- `System`
- `Paths`
- `Files`
- `State`
- `Recent Events`
- `Privacy Notice`

Do not include file contents or settings JSON contents.

- [x] **Step 5: Verify GREEN**

Run the same focused test command. Expected: all selected tests pass.

## Task 2: WinUI Export Action

**Files:**
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.App/MainWindow.xaml`
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.App/MainWindow.xaml.cs`

- [x] **Step 1: Add About button**

Add an `Export Diagnostic Logs` button next to the existing diagnostics actions in About / Open Source.

- [x] **Step 2: Track recent generic status labels**

Maintain an in-memory list of the latest 200 generic status labels from `RefreshUiFromControllerState`, timestamped in UTC. Do not log clipboard contents, transcripts, history row text, settings JSON, environment variables, user prompt titles, dictionary words, file names, or exception payloads.

- [x] **Step 3: Build report inputs**

Collect app version, OS version, runtime description, process architecture, app data paths, file existence/sizes for settings/history/metrics/dictionary, active section, dictation state, selected model path, and recent generic status labels.

- [x] **Step 4: Export through save picker**

Use the existing WinRT `FileSavePicker` pattern with `InitializeWithWindow.Initialize(...)`, suggested file name `VoiceInk_Diagnostic_Logs_yyyyMMdd-HHmmss`, and `.log` file type. Write the Core report text with `FileIO.WriteTextAsync`.

- [x] **Step 5: Build**

Run:

```powershell
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" build VoiceInk.Windows\VoiceInk.Windows.sln -c Debug -p:Platform=x64
```

Expected: build succeeds.

## Task 3: Docs, Review, Commit

**Files:**
- Modify: `README.md`
- Modify: `docs/superpowers/project-completion.md`
- Modify: `docs/superpowers/specs/2026-05-24-windows-open-source-parity-design.md`
- Modify: `docs/superpowers/plans/2026-05-25-windows-diagnostics-export.md`

- [x] **Step 1: Document behavior**

Update docs to describe local diagnostic log export, safe contents, and excluded sensitive data.

- [x] **Step 2: Full verification**

Run:

```powershell
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" test VoiceInk.Windows\VoiceInk.Windows.sln
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" build VoiceInk.Windows\VoiceInk.Windows.sln -c Debug -p:Platform=x64
git diff --check
```

- [x] **Step 3: Request review**

Ask for review focused on privacy exclusions, redaction, useful diagnostic coverage, file-picker behavior, and no commercial telemetry/support surfaces. Fix all Critical/Important findings.

- [x] **Step 4: Commit**

Commit with:

```powershell
git commit -m "feat(windows): export diagnostic logs"
```
