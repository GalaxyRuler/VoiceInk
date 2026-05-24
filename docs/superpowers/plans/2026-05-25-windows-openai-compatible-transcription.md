# Windows OpenAI-Compatible Transcription Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add a source-runnable, default-off OpenAI-compatible cloud transcription provider that works across dictation, Transcribe Audio, and history retry without adding commercial gates or provider-specific paid flows.

**Architecture:** Core carries provider selection and provider-neutral transcription options. Infrastructure owns the OpenAI-compatible multipart HTTP adapter and a simple router between local Whisper and cloud transcription. Native continues to own Windows Credential Manager secret storage. WinUI owns endpoint/model/provider/key settings.

**Tech Stack:** .NET 10, WinUI 3, xUnit, `HttpClient`, `MultipartFormDataContent`, JSON settings, Windows Credential Manager.

**Grounding:**

- macOS source of truth: `VoiceInk/Transcription/Cloud/CloudTranscriptionService.swift`, `VoiceInk/Transcription/Cloud/OpenAICompatibleTranscriptionService.swift`, `VoiceInk/Transcription/Cloud/CloudProvider.swift`, `VoiceInk/Transcription/Cloud/GroqProvider.swift`, and `VoiceInk/Transcription/Engine/TranscriptionServiceRegistry.swift`.
- OpenAI official docs: request-based speech-to-text uses `POST /v1/audio/transcriptions` with multipart `file`, required `model`, optional `language`, optional `prompt`, and JSON `text` responses. The docs list supported request audio formats and note request-based APIs are simpler for completed file uploads while realtime is for live low-latency sessions.
- Open-source adaptation: no bundled paid default, no purchase prompts, no telemetry, no API key in JSON. Users configure an endpoint/model/key manually.

---

## Files

- Create `VoiceInk.Windows/src/VoiceInk.Windows.Infrastructure/Transcription/OpenAICompatibleCloudTranscriptionService.cs`.
- Create `VoiceInk.Windows/src/VoiceInk.Windows.Infrastructure/Transcription/TranscriptionServiceRouter.cs`.
- Create `VoiceInk.Windows/tests/VoiceInk.Windows.Infrastructure.Tests/Transcription/OpenAICompatibleCloudTranscriptionServiceTests.cs`.
- Create `VoiceInk.Windows/tests/VoiceInk.Windows.Infrastructure.Tests/Transcription/TranscriptionServiceRouterTests.cs`.
- Modify `VoiceInk.Windows/src/VoiceInk.Windows.Core/Settings/TranscriptionProviderKind.cs`.
- Modify `VoiceInk.Windows/src/VoiceInk.Windows.Core/Settings/AppSettings.cs`.
- Modify `VoiceInk.Windows/src/VoiceInk.Windows.Core/Transcription/TranscriptionOptions.cs`.
- Modify `VoiceInk.Windows/src/VoiceInk.Windows.Core/Dictation/DictationController.cs`.
- Modify `VoiceInk.Windows/src/VoiceInk.Windows.Core/AudioFiles/AudioFileTranscriptionService.cs`.
- Modify `VoiceInk.Windows/src/VoiceInk.Windows.Core/History/HistoryRetryService.cs`.
- Modify `VoiceInk.Windows/tests/VoiceInk.Windows.Core.Tests/Dictation/DictationControllerTests.cs`.
- Modify `VoiceInk.Windows/tests/VoiceInk.Windows.Core.Tests/AudioFiles/AudioFileTranscriptionServiceTests.cs`.
- Modify `VoiceInk.Windows/tests/VoiceInk.Windows.Core.Tests/History/HistoryRetryServiceTests.cs`.
- Modify `VoiceInk.Windows/tests/VoiceInk.Windows.Infrastructure.Tests/Settings/JsonSettingsStoreTests.cs`.
- Modify `VoiceInk.Windows/src/VoiceInk.Windows.App/MainWindow.xaml`.
- Modify `VoiceInk.Windows/src/VoiceInk.Windows.App/MainWindow.xaml.cs`.
- Modify `README.md`.
- Modify `docs/superpowers/specs/2026-05-24-windows-open-source-parity-design.md`.

## Task 1: Planning Docs

- [x] **Step 1: Update parity spec**

Record the Windows cloud transcription MVP contract:

- Add OpenAI-compatible custom transcription provider.
- Endpoint/model/key are user supplied.
- Key lives in Windows Credential Manager.
- Send multipart `file`, `model`, `response_format=json`, optional `language`, and optional prompt/vocabulary.
- Parse JSON `text`.
- Keep named cards and streaming later.

- [x] **Step 2: Commit planning docs**

Run:

```powershell
git add docs\superpowers\plans\2026-05-25-windows-openai-compatible-transcription.md docs\superpowers\specs\2026-05-24-windows-open-source-parity-design.md
git commit -m "docs(windows): plan cloud transcription"
```

Expected: docs-only commit.

## Task 2: Core Option And Pipeline Red/Green

- [x] **Step 1: Add failing Core tests**

Cover:

- `DictationController.StartAsync` accepts missing local model path when provider is OpenAI-compatible and cloud endpoint/model are present.
- dictation passes provider, endpoint, model, language, and vocabulary prompt into `TranscriptionOptions`.
- Transcribe Audio and History Retry use the same provider validation and options.
- local provider still requires model path.

- [x] **Step 2: Run red Core tests**

Run:

```powershell
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" test VoiceInk.Windows\tests\VoiceInk.Windows.Core.Tests\VoiceInk.Windows.Core.Tests.csproj --filter "FullyQualifiedName~DictationControllerTests|FullyQualifiedName~AudioFileTranscriptionServiceTests|FullyQualifiedName~HistoryRetryServiceTests"
```

Expected: compile/test failures until provider fields and validation exist.

- [x] **Step 3: Implement Core settings/options validation**

Add:

- `TranscriptionProviderKind.OpenAICompatible`.
- `AppSettings.CloudTranscriptionEndpoint` and `AppSettings.CloudTranscriptionModel`.
- `TranscriptionOptions.Provider`, `CloudEndpoint`, and `CloudModel`.
- provider-aware required-configuration validation in dictation, Transcribe Audio, and history retry.

- [x] **Step 4: Verify Core tests pass**

Run the same focused Core command. Expected: tests pass.

## Task 3: Infrastructure Provider Red/Green

- [x] **Step 1: Add failing HTTP adapter/router tests**

Cover:

- provider builds multipart request with authorization, file, model, response format, language, and prompt.
- provider parses `{ "text": "..." }`.
- missing endpoint/model/key produce sanitized configuration errors.
- non-success HTTP status returns sanitized status-only errors.
- router dispatches local vs OpenAI-compatible provider by `TranscriptionOptions.Provider`.

- [x] **Step 2: Run red Infrastructure tests**

Run:

```powershell
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" test VoiceInk.Windows\tests\VoiceInk.Windows.Infrastructure.Tests\VoiceInk.Windows.Infrastructure.Tests.csproj --filter "FullyQualifiedName~OpenAICompatibleCloudTranscriptionServiceTests|FullyQualifiedName~TranscriptionServiceRouterTests"
```

Expected: compile failure until provider/router exist.

- [x] **Step 3: Implement provider and router**

Implement:

- `OpenAICompatibleCloudTranscriptionService` as an `ITranscriptionService`.
- Secret name `VoiceInk.Windows.Transcription.OpenAICompatible.ApiKey`.
- `TranscriptionServiceRouter` with local and cloud services.
- sanitized errors and no response-body logging.

- [x] **Step 4: Verify Infrastructure tests pass**

Run the same focused Infrastructure command. Expected: tests pass.

## Task 4: WinUI Wiring

- [x] **Step 1: Add AI Models controls**

Add provider ComboBox, cloud endpoint/model fields, cloud API key PasswordBox, save/clear key buttons, key status, and apply provider settings.

- [x] **Step 2: Persist and load settings**

Load `TranscriptionProvider`, `CloudTranscriptionEndpoint`, and `CloudTranscriptionModel`, save them through `SaveSettingsAsync`, and wire key save/clear through `WindowsCredentialSecretStore`.

- [x] **Step 3: Use router in dictation, Transcribe Audio, and retry**

Replace direct `WhisperNetTranscriptionService` injections with `TranscriptionServiceRouter(new WhisperNetTranscriptionService(), new OpenAICompatibleCloudTranscriptionService(...))`.

- [x] **Step 4: Verify build**

Run:

```powershell
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" build VoiceInk.Windows\VoiceInk.Windows.sln -c Debug -p:Platform=x64
```

Expected: build succeeds with zero errors.

## Task 5: Docs, Review, And Commit

- [x] **Step 1: Update README and parity spec**

Document the source-runnable OpenAI-compatible transcription path and remaining named-provider/streaming gaps.

- [x] **Step 2: Run focused and full verification**

Run:

```powershell
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" test VoiceInk.Windows\VoiceInk.Windows.sln --filter "FullyQualifiedName~OpenAICompatibleCloudTranscriptionServiceTests|FullyQualifiedName~TranscriptionServiceRouterTests|FullyQualifiedName~DictationControllerTests|FullyQualifiedName~AudioFileTranscriptionServiceTests|FullyQualifiedName~HistoryRetryServiceTests"
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" test VoiceInk.Windows\VoiceInk.Windows.sln
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" build VoiceInk.Windows\VoiceInk.Windows.sln -c Debug -p:Platform=x64
```

Expected: focused tests pass, full tests pass, build succeeds.

- [x] **Step 3: Request review and fix Critical/Important findings**

Ask a subagent to review provider request construction, secret safety, settings persistence, and pipeline integration.

- [x] **Step 4: Commit implementation**

Run:

```powershell
git add VoiceInk.Windows README.md docs\superpowers\specs\2026-05-24-windows-open-source-parity-design.md docs\superpowers\plans\2026-05-25-windows-openai-compatible-transcription.md
git diff --check --cached
git commit -m "feat(windows): add openai-compatible transcription"
```

Expected: implementation commit with no whitespace errors.
