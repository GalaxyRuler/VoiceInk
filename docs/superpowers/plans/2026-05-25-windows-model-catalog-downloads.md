# Windows Model Catalog Downloads Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add a source-runnable Windows AI Models catalog with macOS-aligned local Whisper cards and direct GGML downloads.

**Architecture:** Keep model metadata and selection helpers in Core, put HTTP/filesystem download plumbing in Infrastructure, and keep WinUI code-behind responsible only for user interaction, progress updates, and settings persistence. Downloaded models are copied into `%LocalAppData%\VoiceInk.Windows\Models`, added to the existing imported-local-model list, and selected as the default model path.

**Tech Stack:** .NET 10, WinUI 3, `HttpClient`, existing JSON settings, existing Core/Infrastructure/App project split, xUnit.

---

## Context

macOS references:

- `VoiceInk/Models/TranscriptionModel.swift` defines `WhisperModel` fields: `name`, `displayName`, `size`, `description`, `speed`, `accuracy`, `ramUsage`, `downloadURL`, and `filename`.
- `VoiceInk/Models/TranscriptionModelRegistry.swift` defines local Whisper catalog entries: `ggml-tiny`, `ggml-tiny.en`, `ggml-base`, `ggml-base.en`, `ggml-large-v2`, `ggml-large-v3`, `ggml-large-v3-turbo`, and `ggml-large-v3-turbo-q5_0`.
- `VoiceInk/Views/AI Models/ModelManagementView.swift` filters models into `Recommended`, `Local`, `Cloud`, and `Custom`, with recommended local Whisper names `ggml-base.en` and `ggml-large-v3-turbo-q5_0`.
- `VoiceInk/Views/AI Models/WhisperModelCardView.swift` shows language, size, speed, accuracy, description, download/default/delete/show actions, and progress.
- `VoiceInk/Transcription/Whisper/WhisperModelManager.swift` downloads `https://huggingface.co/ggerganov/whisper.cpp/resolve/main/{model}.bin`, imports `.bin` files into a models directory, and scans that directory.

Online grounding:

- whisper.cpp documents converted GGML model files and manual downloads from the Hugging Face `ggerganov/whisper.cpp` repository.
- The Hugging Face model card lists the same GGML files and sizes for common models.
- WinUI ListView and ProgressBar are native controls suited to model-card lists and download progress.

## Files

- Create: `VoiceInk.Windows/src/VoiceInk.Windows.Core/Models/WhisperModelCatalogEntry.cs`
- Create: `VoiceInk.Windows/src/VoiceInk.Windows.Core/Models/WhisperModelCatalog.cs`
- Create: `VoiceInk.Windows/src/VoiceInk.Windows.Core/Models/WhisperModelDownloadProgress.cs`
- Create: `VoiceInk.Windows/src/VoiceInk.Windows.Core/Models/IWhisperModelDownloader.cs`
- Create: `VoiceInk.Windows/src/VoiceInk.Windows.Infrastructure/Models/HttpWhisperModelDownloader.cs`
- Create: `VoiceInk.Windows/tests/VoiceInk.Windows.Infrastructure.Tests/Models/HttpWhisperModelDownloaderTests.cs`
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.Core/Models/LocalWhisperModelService.cs`
- Modify: `VoiceInk.Windows/tests/VoiceInk.Windows.Core.Tests/Models/LocalWhisperModelServiceTests.cs`
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.App/MainWindow.xaml`
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.App/MainWindow.xaml.cs`
- Modify: `README.md`
- Modify: `docs/superpowers/specs/2026-05-24-windows-open-source-parity-design.md`
- Modify: `docs/superpowers/project-completion.md`

### Task 1: Core Catalog And Selection Helpers

- [ ] **Step 1: Write failing Core catalog tests**

Add tests to `VoiceInk.Windows/tests/VoiceInk.Windows.Core.Tests/Models/LocalWhisperModelServiceTests.cs` that assert:

```csharp
Assert.Equal(
    ["ggml-tiny", "ggml-tiny.en", "ggml-base", "ggml-base.en", "ggml-large-v2", "ggml-large-v3", "ggml-large-v3-turbo", "ggml-large-v3-turbo-q5_0"],
    WhisperModelCatalog.All.Select(model => model.Name));

var baseEnglish = WhisperModelCatalog.All.Single(model => model.Name == "ggml-base.en");
Assert.Equal("Base (English)", baseEnglish.DisplayName);
Assert.Equal("English-only", baseEnglish.LanguageDisplay);
Assert.Equal("142 MB", baseEnglish.Size);
Assert.Equal("https://huggingface.co/ggerganov/whisper.cpp/resolve/main/ggml-base.en.bin", baseEnglish.DownloadUri.AbsoluteUri);

Assert.Equal(
    ["ggml-base.en", "ggml-large-v3-turbo-q5_0"],
    WhisperModelCatalog.Recommended.Select(model => model.Name));
```

- [ ] **Step 2: Run red test**

Run:

```powershell
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" test VoiceInk.Windows\tests\VoiceInk.Windows.Core.Tests\VoiceInk.Windows.Core.Tests.csproj --filter "FullyQualifiedName~LocalWhisperModelServiceTests"
```

Expected: compile failure because the catalog types do not exist.

- [ ] **Step 3: Add Core catalog records**

Implement `WhisperModelCatalogEntry`, `WhisperModelCatalog`, and `WhisperModelDownloadProgress` with the macOS registry metadata. Keep `DownloadUri` generated from `https://huggingface.co/ggerganov/whisper.cpp/resolve/main/{FileName}` and keep `FileName` as `{Name}.bin`.

- [ ] **Step 4: Extend local model import helpers**

Update `LocalWhisperModelService.Import` so downloaded app-local paths and user-imported paths share the same duplicate detection. Add no file-copy behavior to Core.

- [ ] **Step 5: Run green Core test**

Run the same filtered Core command. Expected: all `LocalWhisperModelServiceTests` pass.

### Task 2: HTTP Download Service

- [ ] **Step 1: Write failing Infrastructure tests**

Create `VoiceInk.Windows/tests/VoiceInk.Windows.Infrastructure.Tests/Models/HttpWhisperModelDownloaderTests.cs` with fake `HttpMessageHandler` tests that assert:

```csharp
var model = WhisperModelCatalog.All.Single(entry => entry.Name == "ggml-base.en");
var result = await downloader.DownloadAsync(model, modelsDirectory, progress, CancellationToken.None);

Assert.Equal(Path.Combine(modelsDirectory, "ggml-base.en.bin"), result.Path);
Assert.Equal("ggml-base.en", result.DisplayName);
Assert.True(File.Exists(result.Path));
Assert.Equal("model-bytes", await File.ReadAllTextAsync(result.Path));
Assert.Contains(progressReports, item => item.FractionComplete == 1);
```

Also assert a non-success status throws `HttpRequestException` and does not leave a `.download` file.

- [ ] **Step 2: Run red Infrastructure test**

Run:

```powershell
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" test VoiceInk.Windows\tests\VoiceInk.Windows.Infrastructure.Tests\VoiceInk.Windows.Infrastructure.Tests.csproj --filter "FullyQualifiedName~HttpWhisperModelDownloaderTests"
```

Expected: compile failure because the downloader does not exist.

- [ ] **Step 3: Implement downloader**

Add `IWhisperModelDownloader` in Core and `HttpWhisperModelDownloader` in Infrastructure. The downloader must create the destination directory, stream bytes through a `.download` temp file, report progress by bytes when `Content-Length` is available, move atomically to `{model.FileName}`, delete temp files on failure/cancellation, and return a `LocalWhisperModel`.

- [ ] **Step 4: Run green Infrastructure test**

Run the same filtered Infrastructure command. Expected: downloader tests pass.

### Task 3: WinUI AI Models Catalog

- [ ] **Step 1: Update XAML with catalog list and progress controls**

In `MainWindow.xaml`, add a local catalog list under `AI Models` with:

- `TextBlock x:Name="DefaultModelStatusTextBlock"`
- `ListView x:Name="LocalModelCatalogListView"` with bindings for display name, language, size, speed score, accuracy score, description, and status.
- Buttons named `DownloadCatalogModelButton`, `UseCatalogModelButton`, and `ShowCatalogModelButton`.
- `ProgressBar x:Name="ModelDownloadProgressBar"` and `TextBlock x:Name="ModelDownloadStatusTextBlock"`.

- [ ] **Step 2: Wire code-behind behavior**

In `MainWindow.xaml.cs`, add a `modelsDirectory`, instantiate `HttpWhisperModelDownloader`, maintain catalog view models, handle selection, download selected model, set selected downloaded model as default, and show downloaded file in Explorer. Include `isDownloadingModel` in operation gating and cancel the download on window close.

- [ ] **Step 3: Run app build**

Run:

```powershell
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" build VoiceInk.Windows\VoiceInk.Windows.sln -c Debug -p:Platform=x64
```

Expected: build succeeds.

### Task 4: Docs, Completion Bar, Review, And Commit

- [ ] **Step 1: Update docs**

Update README, the parity spec, and project completion tracker to describe:

- AI Models local Whisper catalog cards.
- Direct downloads to `%LocalAppData%\VoiceInk.Windows\Models`.
- Recommended model defaults.
- Windows omits Core ML encoder downloads because they are macOS-specific.

- [ ] **Step 2: Run targeted and full verification**

Run:

```powershell
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" test VoiceInk.Windows\tests\VoiceInk.Windows.Core.Tests\VoiceInk.Windows.Core.Tests.csproj --filter "FullyQualifiedName~LocalWhisperModelServiceTests"
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" test VoiceInk.Windows\tests\VoiceInk.Windows.Infrastructure.Tests\VoiceInk.Windows.Infrastructure.Tests.csproj --filter "FullyQualifiedName~HttpWhisperModelDownloaderTests"
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" test VoiceInk.Windows\VoiceInk.Windows.sln
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" build VoiceInk.Windows\VoiceInk.Windows.sln -c Debug -p:Platform=x64
git diff --check
```

- [ ] **Step 3: Request review and fix findings**

Review the slice for download safety, UI gating, app-local file handling, settings persistence, and fidelity to macOS model behavior. Fix Critical and Important findings before committing.

- [ ] **Step 4: Commit**

Stage only intentional files, leaving `.superpowers/` untracked, then commit:

```powershell
git add README.md docs/superpowers/project-completion.md docs/superpowers/specs/2026-05-24-windows-open-source-parity-design.md docs/superpowers/plans/2026-05-25-windows-model-catalog-downloads.md VoiceInk.Windows/src VoiceInk.Windows/tests
git commit -m "feat(windows): add local model catalog downloads"
```
