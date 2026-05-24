# Windows Local Model Library Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add source-runnable local Whisper model management to Windows: import `.bin` model references, persist them, and select the default model path from the shell.

**Architecture:** Core owns the local model reference record and import/choice rules. Settings persists imported local models alongside the existing default `ModelPath`. WinUI exposes an imported-model combo box plus Import/Use/Open Downloads actions, while keeping actual model download cards and prewarm for later model-management slices.

**Tech Stack:** .NET 10, WinUI 3 `ComboBox`/`FileOpenPicker`, JSON settings persistence, xUnit.

---

## Files

- Create `VoiceInk.Windows/src/VoiceInk.Windows.Core/Models/LocalWhisperModel.cs`.
- Create `VoiceInk.Windows/src/VoiceInk.Windows.Core/Models/LocalWhisperModelService.cs`.
- Modify `VoiceInk.Windows/src/VoiceInk.Windows.Core/Settings/AppSettings.cs` to persist `ImportedWhisperModels`.
- Create `VoiceInk.Windows/tests/VoiceInk.Windows.Core.Tests/Models/LocalWhisperModelServiceTests.cs`.
- Modify `VoiceInk.Windows/tests/VoiceInk.Windows.Infrastructure.Tests/Settings/JsonSettingsStoreTests.cs`.
- Modify `VoiceInk.Windows/src/VoiceInk.Windows.App/MainWindow.xaml`.
- Modify `VoiceInk.Windows/src/VoiceInk.Windows.App/MainWindow.xaml.cs`.
- Modify `README.md` and `docs/superpowers/specs/2026-05-24-windows-open-source-parity-design.md`.

## Task 1: Planning Docs

- [x] **Step 1: Mark local model library active**

Update the model-management section to record the macOS source behavior: available local models, imported local models, import panel for Whisper ggml `.bin`, default model selection, and download cards. Record the Windows adaptation: imported local model references and default path selector now; model catalog/download/prewarm remains later.

- [x] **Step 2: Commit the plan**

Run:

```powershell
git add docs\superpowers\plans\2026-05-24-windows-local-model-library.md docs\superpowers\specs\2026-05-24-windows-open-source-parity-design.md
git commit -m "docs(windows): plan local model library"
```

Expected: docs-only commit.

## Task 2: Red Tests

- [x] **Step 1: Add Core service tests**

Create tests proving:

- Importing `C:\Models\ggml-base.en.bin` creates a model with display name `ggml-base.en`.
- Import rejects non-`.bin` paths.
- Import rejects duplicate paths case-insensitively.
- Build choices includes the current `ModelPath` when it is not already imported.

- [x] **Step 2: Add settings persistence test**

Update `JsonSettingsStoreTests.SaveAsync_PersistsSettings` to include one imported model and assert round-trip equality.

- [x] **Step 3: Run red tests**

Run:

```powershell
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" test VoiceInk.Windows\tests\VoiceInk.Windows.Core.Tests\VoiceInk.Windows.Core.Tests.csproj --filter FullyQualifiedName~LocalWhisperModelServiceTests
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" test VoiceInk.Windows\tests\VoiceInk.Windows.Infrastructure.Tests\VoiceInk.Windows.Infrastructure.Tests.csproj --filter FullyQualifiedName~JsonSettingsStoreTests
```

Expected: compile failures because the local model types and settings property do not exist yet.

## Task 3: Core And Settings

- [x] **Step 1: Add model record**

`LocalWhisperModel` contains `Path`, `DisplayName`, and `ImportedAt`. `ToString()` returns `DisplayName`.

- [x] **Step 2: Add import/choice service**

`LocalWhisperModelService.Import(string path, IEnumerable<LocalWhisperModel> existing, DateTimeOffset importedAt, out string? error)` trims the path, requires a `.bin` extension, rejects duplicate paths case-insensitively with `Model is already imported.`, and appends a new model with file-name-without-extension display name.

`LocalWhisperModelService.BuildChoices(AppSettings settings)` returns imported models plus a synthetic model for `settings.ModelPath` when nonblank and not already imported.

- [x] **Step 3: Persist imported models**

Add `public LocalWhisperModel[] ImportedWhisperModels { get; init; } = [];` to `AppSettings`.

- [x] **Step 4: Verify focused tests**

Run the same focused Core and Infrastructure test commands from Task 2. Expected: selected tests pass.

## Task 4: WinUI Wiring

- [x] **Step 1: Add shell controls**

Under the model path text box, add:

- `ModelComboBox` with header `Imported Local Models`.
- `ImportModelButton` with content `Import Model`.
- `UseSelectedModelButton` with content `Use Selected Model`.
- `OpenModelDownloadsButton` with content `Open GGML Model Downloads`.

- [x] **Step 2: Load and refresh choices**

Keep a `localWhisperModels` field. During initialization, set it from settings and refresh the combo box with `LocalWhisperModelService.BuildChoices(settings)`. Preserve selection by path when possible.

- [x] **Step 3: Import and select behavior**

`ImportModelButton` opens a `.bin` `FileOpenPicker`, imports the path through Core, sets the imported model as `ModelPath`, persists settings, and refreshes choices. `UseSelectedModelButton` sets the selected model path as the default and persists. `OpenModelDownloadsButton` opens `https://huggingface.co/ggerganov/whisper.cpp/tree/main` with `UseShellExecute = true`.

- [x] **Step 4: UI state**

Disable import/select/download controls while recording, transcribing, inserting, onboarding, quick-add, retry, paste, or cancel operations are active. Keep the raw model path text box for manual source-runnable paths.

- [x] **Step 5: Verify app build**

Run:

```powershell
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" build VoiceInk.Windows\VoiceInk.Windows.sln -c Debug -p:Platform=x64
```

Expected: build passes with 0 warnings/errors.

## Task 5: Docs, Review, Commit

- [x] **Step 1: Update README/spec**

Document imported local model references and default model selection as implemented. Keep download cards, catalog metadata, language capability UI, warmup/preload, and cloud provider cards as remaining gaps.

- [x] **Step 2: Full verification**

Run:

```powershell
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" test VoiceInk.Windows\VoiceInk.Windows.sln
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" build VoiceInk.Windows\VoiceInk.Windows.sln -c Debug -p:Platform=x64
```

Expected: all tests and Debug x64 build pass.

- [x] **Step 3: Request review and fix findings**

Request subagent review against this plan and the macOS AI Models source. Fix all Critical and Important findings before committing.

- [x] **Step 4: Commit**

Run:

```powershell
git add README.md `
  VoiceInk.Windows\src\VoiceInk.Windows.Core\Models\LocalWhisperModel.cs `
  VoiceInk.Windows\src\VoiceInk.Windows.Core\Models\LocalWhisperModelService.cs `
  VoiceInk.Windows\src\VoiceInk.Windows.Core\Settings\AppSettings.cs `
  VoiceInk.Windows\tests\VoiceInk.Windows.Core.Tests\Models\LocalWhisperModelServiceTests.cs `
  VoiceInk.Windows\tests\VoiceInk.Windows.Infrastructure.Tests\Settings\JsonSettingsStoreTests.cs `
  VoiceInk.Windows\src\VoiceInk.Windows.App\MainWindow.xaml `
  VoiceInk.Windows\src\VoiceInk.Windows.App\MainWindow.xaml.cs `
  docs\superpowers\plans\2026-05-24-windows-local-model-library.md `
  docs\superpowers\specs\2026-05-24-windows-open-source-parity-design.md
git commit -m "feat(windows): add local model library"
```
