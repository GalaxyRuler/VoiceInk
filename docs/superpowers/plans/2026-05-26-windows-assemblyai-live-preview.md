# Windows AssemblyAI Live Preview Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add AssemblyAI as a Windows live transcript preview provider and route preview startup through provider-aware services.

**Architecture:** Keep live preview behind `ILiveTranscriptionPreviewService`. Add AssemblyAI-specific URI, parser, and session code in Infrastructure, then compose Deepgram and AssemblyAI behind a small router used by the WinUI app.

**Tech Stack:** .NET 10, xUnit, WinUI 3, PowerShell verification, WebSocket abstraction.

---

### Task 1: Save Spec And Plan

**Files:**
- Create: `docs/superpowers/specs/2026-05-26-windows-assemblyai-live-preview-design.md`
- Create: `docs/superpowers/plans/2026-05-26-windows-assemblyai-live-preview.md`

- [x] **Step 1: Capture design and execution plan**

Document external grounding, behavior, non-goals, and verification.

### Task 2: Add Failing Tests

**Files:**
- Modify: `VoiceInk.Windows/tests/VoiceInk.Windows.Core.Tests/Transcription/TranscriptionProviderPresetCatalogTests.cs`
- Create: `VoiceInk.Windows/tests/VoiceInk.Windows.Infrastructure.Tests/Transcription/AssemblyAIStreamingUriFactoryTests.cs`
- Create: `VoiceInk.Windows/tests/VoiceInk.Windows.Infrastructure.Tests/Transcription/AssemblyAIStreamingMessageParserTests.cs`
- Create: `VoiceInk.Windows/tests/VoiceInk.Windows.Infrastructure.Tests/Transcription/AssemblyAILiveTranscriptionPreviewServiceTests.cs`
- Create: `VoiceInk.Windows/tests/VoiceInk.Windows.Infrastructure.Tests/Transcription/CompositeLiveTranscriptionPreviewServiceTests.cs`

- [x] **Step 1: Write failing preset, URI, parser, session, and router tests**

Assert AssemblyAI appears in the provider catalog, builds a `wss://streaming.assemblyai.com/v3/ws` URI with model/sample rate, parses partial/final text, sends queued audio, emits transcript updates, and is selected by the composite preview router.

- [x] **Step 2: Run focused tests and confirm failure**

Run:

```powershell
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" test VoiceInk.Windows\tests\VoiceInk.Windows.Infrastructure.Tests\VoiceInk.Windows.Infrastructure.Tests.csproj --filter "AssemblyAI|CompositeLive"
```

Expected: fail because AssemblyAI streaming classes do not exist yet.

### Task 3: Implement AssemblyAI Preview

**Files:**
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.Core/Transcription/TranscriptionProviderPresetCatalog.cs`
- Create: `VoiceInk.Windows/src/VoiceInk.Windows.Infrastructure/Transcription/AssemblyAIStreamingUriFactory.cs`
- Create: `VoiceInk.Windows/src/VoiceInk.Windows.Infrastructure/Transcription/AssemblyAIStreamingMessageParser.cs`
- Create: `VoiceInk.Windows/src/VoiceInk.Windows.Infrastructure/Transcription/AssemblyAILiveTranscriptionPreviewService.cs`
- Create: `VoiceInk.Windows/src/VoiceInk.Windows.Infrastructure/Transcription/CompositeLiveTranscriptionPreviewService.cs`
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.App/MainWindow.xaml.cs`

- [x] **Step 1: Add the AssemblyAI preset**

Add the preset and include it in `TranscriptionProviderPresetCatalog.All`.

- [x] **Step 2: Add URI factory and parser**

Build the WebSocket endpoint and parse common AssemblyAI streaming transcript response shapes without throwing on unknown JSON.

- [x] **Step 3: Add live preview service and composite router**

Implement best-effort audio send/receive loops and provider routing while preserving Deepgram behavior.

- [x] **Step 4: Wire the app to the composite service**

Instantiate Deepgram and AssemblyAI preview services in `MainWindow.xaml.cs` and pass a composite to `DictationController`.

### Task 4: Verify And Document

**Files:**
- Modify: `docs/superpowers/project-completion.md`

- [x] **Step 1: Run focused tests**

Run new focused tests and existing Deepgram preview tests.

- [x] **Step 2: Run full tests and build**

Run full solution tests and Debug x64 build.

- [x] **Step 3: Update completion tracker**

Raise floating recorder and cloud transcription progress modestly, add current slice completion, and note real-key manual smoke status.

- [x] **Step 4: Review and commit**

Commit with:

```powershell
git commit -m "feat(windows): add assemblyai live preview"
```
