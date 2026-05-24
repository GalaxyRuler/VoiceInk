# Windows AI Enhancement MVP Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add the first source-runnable Windows AI Enhancement workflow: prompt templates, OpenAI-compatible chat-completions provider, Windows-secured API key storage, optional dictation/file enhancement, original-text fallback, retry/timeout behavior, and history metadata.

**Architecture:** Core owns prompt catalog, prompt rendering, trigger detection, output filtering, and enhancement orchestration. Infrastructure owns HTTP request construction. Native owns Windows Credential Manager secret storage. WinUI owns settings and prompt selection.

**Grounding:**

- macOS source of truth: `VoiceInk/Services/AIEnhancement/*`, `VoiceInk/Models/PromptTemplates.swift`, `VoiceInk/Models/CustomPrompt.swift`, `VoiceInk/Services/PromptDetectionService.swift`, and `VoiceInk/Transcription/Engine/TranscriptionPipeline.swift`.
- OpenAI-compatible grounding: official Chat Completions endpoint accepts `model` and `messages` with bearer authorization.
- Windows secret-storage grounding: Win32 Credential Manager supports generic credentials through `CREDENTIALW`, `CredWriteW`, and `CredReadW`.

**Open-source adaptation:** No bundled paid provider, no license gates, no upgrade prompts, and no telemetry. Enhancement is off by default. Users supply an endpoint, model, and key. If enhancement fails or is unconfigured, VoiceInk inserts and saves the original cleaned transcription.

---

## Files

- Create `VoiceInk.Windows/src/VoiceInk.Windows.Core/Enhancement/EnhancementPrompt.cs`.
- Create `VoiceInk.Windows/src/VoiceInk.Windows.Core/Enhancement/EnhancementPromptCatalog.cs`.
- Create `VoiceInk.Windows/src/VoiceInk.Windows.Core/Enhancement/EnhancementPromptRenderer.cs`.
- Create `VoiceInk.Windows/src/VoiceInk.Windows.Core/Enhancement/EnhancementOutputFilter.cs`.
- Create `VoiceInk.Windows/src/VoiceInk.Windows.Core/Enhancement/PromptDetectionService.cs`.
- Create `VoiceInk.Windows/src/VoiceInk.Windows.Core/Enhancement/TextEnhancementRequest.cs`.
- Create `VoiceInk.Windows/src/VoiceInk.Windows.Core/Enhancement/TextEnhancementResult.cs`.
- Create `VoiceInk.Windows/src/VoiceInk.Windows.Core/Enhancement/ITextEnhancementService.cs`.
- Create `VoiceInk.Windows/src/VoiceInk.Windows.Core/Enhancement/TextEnhancementPipeline.cs`.
- Create `VoiceInk.Windows/src/VoiceInk.Windows.Core/Services/ISecretStore.cs`.
- Create `VoiceInk.Windows/src/VoiceInk.Windows.Infrastructure/Enhancement/OpenAICompatibleTextEnhancementService.cs`.
- Create `VoiceInk.Windows/src/VoiceInk.Windows.Native/Security/WindowsCredentialSecretStore.cs`.
- Create `VoiceInk.Windows/tests/VoiceInk.Windows.Core.Tests/Enhancement/*`.
- Create `VoiceInk.Windows/tests/VoiceInk.Windows.Infrastructure.Tests/Enhancement/*`.
- Modify `VoiceInk.Windows/src/VoiceInk.Windows.Core/Settings/AppSettings.cs`.
- Modify `VoiceInk.Windows/src/VoiceInk.Windows.Core/History/TranscriptionHistoryItem.cs`.
- Modify `VoiceInk.Windows/src/VoiceInk.Windows.Core/History/HistoryCsvExporter.cs`.
- Modify `VoiceInk.Windows/src/VoiceInk.Windows.Core/Dictation/DictationController.cs`.
- Modify `VoiceInk.Windows/src/VoiceInk.Windows.Core/AudioFiles/AudioFileTranscriptionService.cs`.
- Modify `VoiceInk.Windows/src/VoiceInk.Windows.Infrastructure/History/SqliteHistoryStore.cs`.
- Modify `VoiceInk.Windows/tests/VoiceInk.Windows.Core.Tests/Dictation/DictationControllerTests.cs`.
- Modify `VoiceInk.Windows/tests/VoiceInk.Windows.Core.Tests/AudioFiles/AudioFileTranscriptionServiceTests.cs`.
- Modify `VoiceInk.Windows/tests/VoiceInk.Windows.Infrastructure.Tests/History/SqliteHistoryStoreTests.cs`.
- Modify `VoiceInk.Windows/src/VoiceInk.Windows.Core/Shell/ShellNavigationPresenter.cs`.
- Modify `VoiceInk.Windows/tests/VoiceInk.Windows.Core.Tests/Shell/ShellNavigationPresenterTests.cs`.
- Modify `VoiceInk.Windows/src/VoiceInk.Windows.App/MainWindow.xaml`.
- Modify `VoiceInk.Windows/src/VoiceInk.Windows.App/MainWindow.xaml.cs`.
- Modify `README.md`.
- Modify `docs/superpowers/specs/2026-05-24-windows-open-source-parity-design.md`.

## Task 1: Planning Docs

- [x] **Step 1: Update parity spec**

Record the macOS enhancement behavior and this Windows MVP adaptation:

- Prompt catalog: Default, Assistant, and practical template prompts.
- Prompt rendering with `<TRANSCRIPT>` user message and optional vocabulary context.
- Trigger-word detection at the beginning or end of text, longest trigger first.
- Output filtering for `<think>`, `<thinking>`, and `<reasoning>` blocks.
- Enhancement after transcription cleanup and before insertion.
- Original cleaned transcription remains the saved `Text`; successful enhancement is saved as `EnhancedText` and inserted.
- Enhancement failure leaves `EnhancedText` empty, stores an error warning, and inserts original cleaned text.
- Default timeout is 7 seconds, retry-on-timeout is enabled, and transient provider failures retry up to 3 attempts with exponential backoff.
- History stores enhancement provider/model, prompt name, duration, and the rendered system/user request messages for local diagnostics.
- API key stored outside JSON settings in Windows Credential Manager.

- [ ] **Step 2: Commit planning docs**

Run:

```powershell
git add docs\superpowers\plans\2026-05-25-windows-ai-enhancement-mvp.md docs\superpowers\specs\2026-05-24-windows-open-source-parity-design.md
git commit -m "docs(windows): plan ai enhancement"
```

Expected: docs-only commit.

## Task 2: Core Prompt And Trigger Red/Green

- [ ] **Step 1: Add failing prompt/filter/detection tests**

Create focused tests covering:

- Default prompt catalog contains deterministic Default and Assistant prompts plus template prompts.
- Rendering wraps normal prompts with transcription-enhancer system instructions and `<TRANSCRIPT>` user content.
- Rendering appends vocabulary words inside `<CUSTOM_VOCABULARY>`.
- Assistant prompt bypasses transcription-enhancer wrapper.
- Output filter strips thinking/reasoning tags and trims output.
- Trigger detection strips leading/trailing trigger words, handles punctuation, and uses longest trigger first.

- [ ] **Step 2: Run red tests**

Run:

```powershell
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" test VoiceInk.Windows\tests\VoiceInk.Windows.Core.Tests\VoiceInk.Windows.Core.Tests.csproj --filter FullyQualifiedName~Enhancement
```

Expected: compile failure because Core enhancement types do not exist.

- [ ] **Step 3: Add Core prompt/filter/detection implementation**

Implement the files listed for prompt catalog, rendering, output filtering, and trigger detection.

- [ ] **Step 4: Verify prompt/filter/detection tests**

Run the focused Core enhancement test command. Expected: tests pass.

## Task 3: Core Enhancement Pipeline Red/Green

- [ ] **Step 1: Add failing pipeline tests**

Cover:

- Disabled enhancement returns original text and does not call provider.
- Enabled enhancement with selected prompt calls provider, inserts enhanced text, and returns prompt/model/duration metadata.
- Trigger words temporarily enable enhancement even when the global toggle is off.
- Short-text skip avoids enhancement unless a trigger word was detected.
- Enhancement failure returns original text, leaves enhanced text empty, and returns a warning/error message.
- Retry settings retry transient provider failures and timeout failures only when enabled.

- [ ] **Step 2: Implement `TextEnhancementPipeline`**

Implementation details:

- Resolve selected prompt from settings, falling back to Default.
- Detect and strip trigger words before provider request.
- Apply skip-short threshold.
- Render system/user messages with vocabulary.
- Call `ITextEnhancementService`.
- Filter provider output.
- Return original/enhanced/final-for-insertion metadata without throwing on provider failures.

- [ ] **Step 3: Verify pipeline tests**

Run the focused Core enhancement test command. Expected: tests pass.

## Task 4: OpenAI-Compatible Provider And Secret Store

- [ ] **Step 1: Add failing infrastructure provider tests**

Use a fake `HttpMessageHandler` and fake `ISecretStore` to assert:

- POST goes to the configured endpoint.
- Authorization header is `Bearer <secret>`.
- JSON body contains `model`, `messages[0].role = system`, `messages[1].role = user`, and `temperature`.
- Successful responses read `choices[0].message.content`.
- Non-success HTTP responses return sanitized provider errors without leaking the API key.
- Missing API key returns a not-configured error before HTTP.
- HTTP 429 and 5xx responses are retried up to the configured retry count.

- [ ] **Step 2: Implement provider**

Create `OpenAICompatibleTextEnhancementService` using `HttpClient`, `ISecretStore`, `System.Text.Json`, timeout handling, transient retry/backoff behavior, and sanitized errors.

- [ ] **Step 3: Add Windows Credential Manager store**

Create `WindowsCredentialSecretStore` with generic credential read/write/delete/exists. Do not add automated tests that write real user credentials; keep automated tests on the provider with a fake secret store.

- [ ] **Step 4: Verify provider tests**

Run:

```powershell
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" test VoiceInk.Windows\tests\VoiceInk.Windows.Infrastructure.Tests\VoiceInk.Windows.Infrastructure.Tests.csproj --filter FullyQualifiedName~Enhancement
```

Expected: provider tests pass.

## Task 5: Pipeline Integration

- [ ] **Step 1: Add failing dictation and file-transcription tests**

Cover:

- Dictation inserts enhanced text when enhancement succeeds, but saves original cleaned `Text`, `EnhancedText`, enhancement provider/model, prompt, duration, and request messages.
- Dictation inserts original text and records warning/error metadata when enhancement fails.
- File transcription saves `EnhancedText` and enhancement metadata when enhancement succeeds.
- Enhancement is not attempted when disabled/unconfigured.

- [ ] **Step 2: Integrate Core services**

Modify `DictationController` and `AudioFileTranscriptionService` to use optional `TextEnhancementPipeline`.

- Add migration-safe nullable history fields for enhancement provider/model and request messages.
- Keep API keys out of history, logs, diagnostics, and error strings.

- [ ] **Step 3: Verify focused tests**

Run focused Dictation/AudioFiles tests. Expected: tests pass.

## Task 6: WinUI Enhancement Settings

- [ ] **Step 1: Update navigation tests**

Add `Enhancement` after `AI Models` and before `Audio Input`.

- [ ] **Step 2: Add settings UI**

Add a WinUI `Enhancement` section with:

- Enable Enhancement toggle.
- Provider endpoint and model fields.
- API key password field with Save and Clear buttons.
- Prompt picker.
- Timeout and skip-short settings.
- Retry-on-timeout setting.
- Status text for whether a key is stored.

- [ ] **Step 3: Wire app services**

Instantiate `WindowsCredentialSecretStore`, `OpenAICompatibleTextEnhancementService`, and `TextEnhancementPipeline`. Persist non-secret settings to JSON. Store/clear only the key in Credential Manager.

- [ ] **Step 4: Verify navigation tests and build**

Run focused navigation tests and Debug x64 build.

## Task 7: Docs, Review, Commit

- [ ] **Step 1: Update README/spec/plan**

Document:

- Enhancement is off by default.
- API keys are stored in Windows Credential Manager, not JSON settings.
- Chat-completions-compatible providers can be used by changing endpoint/model.
- Manual API-key smoke requires a user-supplied key and network.
- Remaining gaps: provider cards, Ollama/local CLI, clipboard/selected-text/OCR context, custom prompt editing, dynamic provider model lists, and toggle-enhancement shortcut.

- [ ] **Step 2: Full verification**

Run:

```powershell
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" test VoiceInk.Windows\VoiceInk.Windows.sln
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" build VoiceInk.Windows\VoiceInk.Windows.sln -c Debug -p:Platform=x64
```

Expected: all tests pass and Debug x64 build passes with 0 warnings/errors.

- [ ] **Step 3: Request review and fix findings**

Request subagent review against this plan, the macOS enhancement source, and the OpenAI-compatible/secret-storage implementation. Fix all Critical and Important findings before committing.

- [ ] **Step 4: Commit**

Run:

```powershell
git add README.md `
  VoiceInk.Windows\src\VoiceInk.Windows.Core\Enhancement `
  VoiceInk.Windows\src\VoiceInk.Windows.Core\Services\ISecretStore.cs `
  VoiceInk.Windows\src\VoiceInk.Windows.Infrastructure\Enhancement `
  VoiceInk.Windows\src\VoiceInk.Windows.Native\Security `
  VoiceInk.Windows\tests\VoiceInk.Windows.Core.Tests\Enhancement `
  VoiceInk.Windows\tests\VoiceInk.Windows.Infrastructure.Tests\Enhancement `
  VoiceInk.Windows\src\VoiceInk.Windows.Core\Settings\AppSettings.cs `
  VoiceInk.Windows\src\VoiceInk.Windows.Core\History\TranscriptionHistoryItem.cs `
  VoiceInk.Windows\src\VoiceInk.Windows.Core\History\HistoryCsvExporter.cs `
  VoiceInk.Windows\src\VoiceInk.Windows.Core\Dictation\DictationController.cs `
  VoiceInk.Windows\src\VoiceInk.Windows.Core\AudioFiles\AudioFileTranscriptionService.cs `
  VoiceInk.Windows\src\VoiceInk.Windows.Infrastructure\History\SqliteHistoryStore.cs `
  VoiceInk.Windows\tests\VoiceInk.Windows.Core.Tests\Dictation\DictationControllerTests.cs `
  VoiceInk.Windows\tests\VoiceInk.Windows.Core.Tests\AudioFiles\AudioFileTranscriptionServiceTests.cs `
  VoiceInk.Windows\tests\VoiceInk.Windows.Infrastructure.Tests\History\SqliteHistoryStoreTests.cs `
  VoiceInk.Windows\src\VoiceInk.Windows.Core\Shell\ShellNavigationPresenter.cs `
  VoiceInk.Windows\tests\VoiceInk.Windows.Core.Tests\Shell\ShellNavigationPresenterTests.cs `
  VoiceInk.Windows\src\VoiceInk.Windows.App\MainWindow.xaml `
  VoiceInk.Windows\src\VoiceInk.Windows.App\MainWindow.xaml.cs `
  docs\superpowers\plans\2026-05-25-windows-ai-enhancement-mvp.md `
  docs\superpowers\specs\2026-05-24-windows-open-source-parity-design.md
git commit -m "feat(windows): add ai enhancement"
```
