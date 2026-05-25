# Windows Deepgram Live Preview Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add Deepgram as the first real Windows streaming partial transcript source for the floating recorder live transcript preview.

**Architecture:** Add small Core contracts for audio chunk publication and live preview sessions. Keep `DictationController` in charge of recording lifecycle and partial transcript state, keep NAudio as the native PCM chunk source, and keep Deepgram HTTP/WebSocket details in Infrastructure behind testable interfaces. Final transcription remains a normal stopped-recording service result.

**Tech Stack:** .NET 10, WinUI 3, NAudio, `ClientWebSocket`, xUnit, JSON settings, Windows Credential Manager through `ISecretStore`.

---

### Task 1: Core Live Preview Contracts And Controller Lifecycle

**Files:**
- Create: `VoiceInk.Windows/src/VoiceInk.Windows.Core/Audio/AudioChunk.cs`
- Create: `VoiceInk.Windows/src/VoiceInk.Windows.Core/Services/IAudioChunkPublisher.cs`
- Create: `VoiceInk.Windows/src/VoiceInk.Windows.Core/Services/ILiveTranscriptionPreviewService.cs`
- Create: `VoiceInk.Windows/src/VoiceInk.Windows.Core/Services/ILiveTranscriptionPreviewSession.cs`
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.Core/Dictation/DictationController.cs`
- Modify: `VoiceInk.Windows/tests/VoiceInk.Windows.Core.Tests/Dictation/DictationControllerTests.cs`

- [x] **Step 1: Add failing controller tests**

Add tests proving a preview session starts when settings enable live preview, provider callbacks update `PartialTranscript` while recording, audio chunks are enqueued into the session, and the session is completed/disposed on stop/cancel before late partials can update UI state.

- [x] **Step 2: Run focused controller tests and verify red**

```powershell
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" test VoiceInk.Windows\tests\VoiceInk.Windows.Core.Tests\VoiceInk.Windows.Core.Tests.csproj --filter "FullyQualifiedName~DictationControllerTests"
```

Expected: fails because live preview contracts and constructor plumbing do not exist.

- [x] **Step 3: Implement Core contracts and lifecycle**

Add immutable `AudioChunk`, chunk publisher/session/service interfaces, optional live preview service constructor dependency, best-effort start, event subscription, serialized session enqueue calls, and cleanup on stop/cancel/failure.

- [x] **Step 4: Re-run focused controller tests**

Expected: controller tests pass.

### Task 2: Deepgram Provider Preset And Batch Transcription

**Files:**
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.Core/Transcription/TranscriptionProviderPresetCatalog.cs`
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.Core/Transcription/TranscriptionConfiguration.cs`
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.Infrastructure/Transcription/TranscriptionServiceRouter.cs`
- Create: `VoiceInk.Windows/src/VoiceInk.Windows.Infrastructure/Transcription/DeepgramCloudTranscriptionService.cs`
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.App/MainWindow.xaml.cs`
- Modify: `VoiceInk.Windows/tests/VoiceInk.Windows.Core.Tests/Transcription/TranscriptionProviderPresetCatalogTests.cs`
- Modify: `VoiceInk.Windows/tests/VoiceInk.Windows.Infrastructure.Tests/Transcription/TranscriptionServiceRouterTests.cs`
- Create: `VoiceInk.Windows/tests/VoiceInk.Windows.Infrastructure.Tests/Transcription/DeepgramCloudTranscriptionServiceTests.cs`

- [x] **Step 1: Add failing preset/router/batch tests**

Assert Deepgram preset metadata, provider-specific secret name, router dispatch by cloud provider id, direct Deepgram request URI/header/body, transcript extraction from Deepgram JSON, and sanitized HTTP/JSON errors.

- [x] **Step 2: Run focused transcription tests and verify red**

```powershell
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" test VoiceInk.Windows\VoiceInk.Windows.sln --filter "FullyQualifiedName~TranscriptionProviderPresetCatalogTests|FullyQualifiedName~TranscriptionServiceRouterTests|FullyQualifiedName~DeepgramCloudTranscriptionServiceTests"
```

Expected: fails because Deepgram provider/service does not exist.

- [x] **Step 3: Implement Deepgram batch service and routing**

Add Deepgram preset, secret mapping, direct Deepgram batch HTTP service, router dispatch for `CloudProviderId == "deepgram"`, and app construction wiring.

- [x] **Step 4: Re-run focused transcription tests**

Expected: focused transcription tests pass.

### Task 3: Deepgram Streaming Preview Infrastructure

**Files:**
- Create: `VoiceInk.Windows/src/VoiceInk.Windows.Infrastructure/Transcription/DeepgramLiveTranscriptionPreviewService.cs`
- Create: `VoiceInk.Windows/src/VoiceInk.Windows.Infrastructure/Transcription/DeepgramStreamingMessageParser.cs`
- Create: `VoiceInk.Windows/src/VoiceInk.Windows.Infrastructure/Transcription/DeepgramStreamingUriFactory.cs`
- Create: `VoiceInk.Windows/src/VoiceInk.Windows.Infrastructure/Transcription/IStreamingWebSocket.cs`
- Create: `VoiceInk.Windows/src/VoiceInk.Windows.Infrastructure/Transcription/ClientStreamingWebSocket.cs`
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.App/MainWindow.xaml.cs`
- Create: `VoiceInk.Windows/tests/VoiceInk.Windows.Infrastructure.Tests/Transcription/DeepgramStreamingMessageParserTests.cs`
- Create: `VoiceInk.Windows/tests/VoiceInk.Windows.Infrastructure.Tests/Transcription/DeepgramStreamingUriFactoryTests.cs`

- [x] **Step 1: Add failing parser/URI tests**

Assert streaming URI includes Deepgram model, language when not auto, `encoding=linear16`, `channels=1`, `sample_rate=16000`, and `interim_results=true`. Assert parser returns transcript/final status from Deepgram interim/final JSON and ignores empty/unexpected messages.

- [x] **Step 2: Run focused streaming tests and verify red**

```powershell
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" test VoiceInk.Windows\tests\VoiceInk.Windows.Infrastructure.Tests\VoiceInk.Windows.Infrastructure.Tests.csproj --filter "FullyQualifiedName~DeepgramStreaming"
```

Expected: fails because streaming parser/URI factory do not exist.

- [x] **Step 3: Implement streaming preview service**

Add websocket wrapper, URI factory, parser, send queue, receive loop, committed+partial aggregation, best-effort error handling, and graceful close. Use exactly one send loop and one receive loop per `ClientWebSocket`.

- [x] **Step 4: Re-run focused streaming tests and build**

Expected: focused streaming tests pass and app builds.

### Task 4: Native Chunk Publisher And App Wiring

**Files:**
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.Native/Audio/NAudioCaptureService.cs`
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.App/MainWindow.xaml.cs`

- [x] **Step 1: Publish copied PCM chunks from NAudio**

Implement `IAudioChunkPublisher.AudioChunkAvailable`, copy `WaveInEventArgs.Buffer` bytes, include sample rate/channels from `WaveFormat`, and isolate subscriber failures just like level publishing.

- [x] **Step 2: Wire Deepgram live preview into controller**

Construct `DeepgramLiveTranscriptionPreviewService` with the existing secret store and a websocket factory, pass it to `DictationController`, and ensure selecting Deepgram plus enabling live preview can produce partial updates.

- [x] **Step 3: Run Debug build**

```powershell
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" build VoiceInk.Windows\VoiceInk.Windows.sln -c Debug -p:Platform=x64
```

Expected: app builds with no errors.

### Task 5: Docs, Review, Verification, Commit

**Files:**
- Modify: `README.md`
- Modify: `docs/superpowers/project-completion.md`
- Modify: `docs/superpowers/specs/2026-05-24-windows-open-source-parity-design.md`
- Modify: this plan file

- [x] **Step 1: Update docs and completion tracker**

Document Deepgram batch + live preview support, Deepgram key storage, and the manual smoke path. Keep other streaming providers listed as remaining gaps.

- [x] **Step 2: Run full verification**

```powershell
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" test VoiceInk.Windows\VoiceInk.Windows.sln
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" build VoiceInk.Windows\VoiceInk.Windows.sln -c Debug -p:Platform=x64
git diff --check
```

- [x] **Step 3: Request code review and fix Critical/Important findings**

Review focus: no fake partials, Deepgram request correctness, websocket send serialization, session cleanup on stop/cancel/failure, no key leaks, final transcription path still works.

- [ ] **Step 4: Commit slice**

```powershell
git add VoiceInk.Windows README.md docs\superpowers
git diff --cached --check
git commit -m "feat(windows): add deepgram live transcript preview"
```
