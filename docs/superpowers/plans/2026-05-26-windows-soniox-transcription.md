# Windows Soniox Transcription Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add Soniox V4 as a selectable Windows cloud transcription provider with a working async batch adapter.

**Architecture:** Keep Soniox-specific HTTP orchestration in Infrastructure behind `ITranscriptionService`. Add a catalog preset and route `CloudProviderId=soniox` through the adapter from `TranscriptionServiceRouter`.

**Tech Stack:** .NET 10, xUnit, HttpClient, multipart upload, JSON polling.

---

### Task 1: Save Spec And Plan

**Files:**
- Create: `docs/superpowers/specs/2026-05-26-windows-soniox-transcription-design.md`
- Create: `docs/superpowers/plans/2026-05-26-windows-soniox-transcription.md`

- [x] **Step 1: Capture design and plan**

Document endpoint, async request flow, cleanup, routing, non-goals, and verification.

### Task 2: Add Failing Tests

**Files:**
- Modify: `VoiceInk.Windows/tests/VoiceInk.Windows.Core.Tests/Transcription/TranscriptionProviderPresetCatalogTests.cs`
- Create: `VoiceInk.Windows/tests/VoiceInk.Windows.Infrastructure.Tests/Transcription/SonioxCloudTranscriptionServiceTests.cs`
- Modify: `VoiceInk.Windows/tests/VoiceInk.Windows.Infrastructure.Tests/Transcription/TranscriptionServiceRouterTests.cs`

- [x] **Step 1: Write failing tests**

Assert catalog and secret-name support, bearer auth, upload/create/poll/transcript sequence, token rendering, best-effort cleanup, sanitized errors, and router selection.

- [x] **Step 2: Run focused tests and confirm failure**

Run catalog and Soniox/router tests. Expected: fail until implementation exists.

### Task 3: Implement Soniox Adapter

**Files:**
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.Core/Transcription/TranscriptionProviderPresetCatalog.cs`
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.Core/Transcription/TranscriptionConfiguration.cs`
- Create: `VoiceInk.Windows/src/VoiceInk.Windows.Infrastructure/Transcription/SonioxCloudTranscriptionService.cs`
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.Infrastructure/Transcription/TranscriptionServiceRouter.cs`
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.App/MainWindow.xaml.cs`

- [x] **Step 1: Add preset and secret segment**

Add Soniox to the catalog and credential naming.

- [x] **Step 2: Add provider-specific adapter**

Implement upload, create, poll, transcript fetch, token rendering, and best-effort cleanup.

- [x] **Step 3: Route and wire app**

Route `soniox` through the new adapter and instantiate it in the app.

### Task 4: Verify And Commit

**Files:**
- Modify: `docs/superpowers/project-completion.md`

- [x] **Step 1: Run focused tests**

Run catalog, Soniox adapter, and router tests.

- [x] **Step 2: Run full tests and build**

Run full solution tests and Debug x64 build.

- [x] **Step 3: Update completion tracker**

Raise cloud transcription slightly and note Soniox async batch support.

- [x] **Step 4: Review and commit**

Commit with:

```powershell
git commit -m "feat(windows): add soniox transcription"
```
