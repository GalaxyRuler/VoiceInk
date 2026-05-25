# Windows ElevenLabs Transcription Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add ElevenLabs Scribe as a selectable Windows cloud transcription provider with a working batch transcription adapter.

**Architecture:** Keep provider-specific HTTP behavior in Infrastructure behind `ITranscriptionService`. Add a catalog preset and route `CloudProviderId=elevenlabs` through the adapter from `TranscriptionServiceRouter`.

**Tech Stack:** .NET 10, xUnit, HttpClient, multipart form upload.

---

### Task 1: Save Spec And Plan

**Files:**
- Create: `docs/superpowers/specs/2026-05-26-windows-elevenlabs-transcription-design.md`
- Create: `docs/superpowers/plans/2026-05-26-windows-elevenlabs-transcription.md`

- [x] **Step 1: Capture design and plan**

Document endpoint, headers, model fields, routing, non-goals, and verification.

### Task 2: Add Failing Tests

**Files:**
- Modify: `VoiceInk.Windows/tests/VoiceInk.Windows.Core.Tests/Transcription/TranscriptionProviderPresetCatalogTests.cs`
- Create: `VoiceInk.Windows/tests/VoiceInk.Windows.Infrastructure.Tests/Transcription/ElevenLabsCloudTranscriptionServiceTests.cs`
- Modify: `VoiceInk.Windows/tests/VoiceInk.Windows.Infrastructure.Tests/Transcription/TranscriptionServiceRouterTests.cs`

- [x] **Step 1: Write failing tests**

Assert catalog and secret-name support, `xi-api-key` header, multipart `model_id`, optional language, response text parsing, sanitized errors, and router selection.

- [x] **Step 2: Run focused tests and confirm failure**

Run catalog and Infrastructure focused tests. Expected: fail until implementation exists.

### Task 3: Implement ElevenLabs Adapter

**Files:**
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.Core/Transcription/TranscriptionProviderPresetCatalog.cs`
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.Core/Transcription/TranscriptionConfiguration.cs`
- Create: `VoiceInk.Windows/src/VoiceInk.Windows.Infrastructure/Transcription/ElevenLabsCloudTranscriptionService.cs`
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.Infrastructure/Transcription/TranscriptionServiceRouter.cs`
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.App/MainWindow.xaml.cs`

- [x] **Step 1: Add preset and secret segment**

Add ElevenLabs to the catalog and credential naming.

- [x] **Step 2: Add provider-specific adapter**

Post multipart audio to ElevenLabs with `xi-api-key`, `model_id`, optional `language_code`, and parse `text`.

- [x] **Step 3: Route and wire app**

Route `elevenlabs` through the new adapter and instantiate it in the app.

### Task 4: Verify And Commit

**Files:**
- Modify: `docs/superpowers/project-completion.md`

- [x] **Step 1: Run focused tests**

Run catalog, ElevenLabs adapter, and router tests.

- [x] **Step 2: Run full tests and build**

Run full solution tests and Debug x64 build.

- [x] **Step 3: Update completion tracker**

Raise cloud transcription slightly and note ElevenLabs batch support.

- [x] **Step 4: Review and commit**

Commit with:

```powershell
git commit -m "feat(windows): add elevenlabs transcription"
```
