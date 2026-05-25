# Windows Custom Enhancement Prompts Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add source-runnable, local-only custom AI enhancement prompt persistence with trigger words, matching the macOS prompt-template workflow without any commercial prompt marketplace, account sync, telemetry, or upsell surface.

**Architecture:** Core owns prompt-library merging, validation, trigger normalization, persistence filtering, and dynamic prompt access for the enhancement pipeline. JSON settings persist prompt records and selected prompt ID. WinUI adds an Enhancement-section editor for prompt title, instructions, trigger words, system-instruction wrapping, and add/save/delete actions.

**Tech Stack:** .NET 10, WinUI 3, xUnit, JSON settings via `System.Text.Json`, existing `TextEnhancementPipeline`.

**Grounding:**

- macOS source of truth: `VoiceInk/Models/CustomPrompt.swift`, `VoiceInk/Models/PromptTemplates.swift`, `VoiceInk/Models/PredefinedPrompts.swift`, `VoiceInk/Services/AIEnhancement/AIEnhancementService.swift`, and `VoiceInk/Views/PromptEditorView.swift`.
- Microsoft docs checked on 2026-05-25: `System.Text.Json` supports records and init-only properties; WinUI ComboBox supports object `ItemsSource` with `DisplayMemberPath`; WinUI TextBox supports multiline input with `AcceptsReturn="True"` and wrapping; WinUI CheckBox is the native Boolean control.
- Open-source adaptation: templates are edited locally and stored in the user JSON settings file. No hosted template catalog, marketplace, paid prompt pack, account requirement, telemetry, or commercial sync flow.

---

## Files

- Create `VoiceInk.Windows/src/VoiceInk.Windows.Core/Enhancement/EnhancementPromptLibrary.cs`.
- Modify `VoiceInk.Windows/src/VoiceInk.Windows.Core/Settings/AppSettings.cs`.
- Modify `VoiceInk.Windows/src/VoiceInk.Windows.Core/Enhancement/TextEnhancementPipeline.cs`.
- Modify `VoiceInk.Windows/src/VoiceInk.Windows.App/MainWindow.xaml`.
- Modify `VoiceInk.Windows/src/VoiceInk.Windows.App/MainWindow.xaml.cs`.
- Modify `VoiceInk.Windows/tests/VoiceInk.Windows.Core.Tests/Enhancement/EnhancementPromptTests.cs`.
- Modify `VoiceInk.Windows/tests/VoiceInk.Windows.Core.Tests/Enhancement/TextEnhancementPipelineTests.cs`.
- Modify `VoiceInk.Windows/tests/VoiceInk.Windows.Infrastructure.Tests/Settings/JsonSettingsStoreTests.cs`.
- Modify `README.md`.
- Modify `docs/superpowers/specs/2026-05-24-windows-open-source-parity-design.md`.

## Task 1: Planning Docs

- [x] **Step 1: Update parity spec**

Record the Windows target: persist custom prompts, preserve predefined prompt source text, allow predefined trigger-word overrides, add local editor controls, and keep the enhancement pipeline dynamic.

- [x] **Step 2: Commit planning docs**

Run:

```powershell
git add docs\superpowers\plans\2026-05-25-windows-custom-enhancement-prompts.md docs\superpowers\specs\2026-05-24-windows-open-source-parity-design.md
git commit -m "docs(windows): plan custom enhancement prompts"
```

Expected: docs-only commit.

## Task 2: Core Prompt Library Red/Green

- [x] **Step 1: Add failing Core tests**

Extend `EnhancementPromptTests` to assert:

- `EnhancementPromptLibrary.BuildPrompts([])` returns the default prompt catalog.
- A persisted non-predefined prompt is appended after predefined prompts.
- A persisted predefined prompt with the default prompt ID preserves only trigger words while keeping the current source-controlled title and prompt text.
- `CreateCustomPrompt` trims title/instructions, normalizes comma-separated trigger words, removes blank/duplicate triggers case-insensitively, and sets `IsPredefined = false`.
- `DeletePrompt` removes custom prompts but keeps predefined prompts.
- `PersistentPrompts` returns custom prompts and predefined prompts with trigger words, but omits untouched predefined prompts.

- [x] **Step 2: Run red Core tests**

Run:

```powershell
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" test VoiceInk.Windows\tests\VoiceInk.Windows.Core.Tests\VoiceInk.Windows.Core.Tests.csproj --filter "FullyQualifiedName~EnhancementPromptTests"
```

Expected: compile failure because `EnhancementPromptLibrary` does not exist.

- [x] **Step 3: Implement Core prompt library**

Add `EnhancementPromptLibrary` with:

- `BuildPrompts(IReadOnlyList<EnhancementPrompt> persistedPrompts)`.
- `CreateCustomPrompt(Guid id, string title, string promptText, string icon, string? description, string triggerWordsText, bool useSystemInstructions)`.
- `UpdatePrompt(IReadOnlyList<EnhancementPrompt> prompts, EnhancementPrompt updatedPrompt)`.
- `DeletePrompt(IReadOnlyList<EnhancementPrompt> prompts, Guid promptId)`.
- `PersistentPrompts(IReadOnlyList<EnhancementPrompt> prompts)`.
- `ResolveSelectedPromptId(Guid? selectedPromptId, IReadOnlyList<EnhancementPrompt> prompts)`.

- [x] **Step 4: Verify Core tests pass**

Run the same focused Core command. Expected: tests pass.

## Task 3: Dynamic Pipeline Prompt Source Red/Green

- [x] **Step 1: Add failing pipeline test**

Add a `TextEnhancementPipelineTests` case where a mutable prompt list is changed after pipeline construction and the next enhancement uses the new prompt title/text.

- [x] **Step 2: Run red pipeline test**

Run:

```powershell
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" test VoiceInk.Windows\tests\VoiceInk.Windows.Core.Tests\VoiceInk.Windows.Core.Tests.csproj --filter "FullyQualifiedName~TextEnhancementPipelineTests"
```

Expected: failing assertion because the pipeline currently captures a static prompt list.

- [x] **Step 3: Implement dynamic prompt source**

Add a `Func<IReadOnlyList<EnhancementPrompt>>` prompt-source constructor path to `TextEnhancementPipeline`, while preserving the existing static-list constructor for tests and simple callers.

- [x] **Step 4: Verify pipeline tests pass**

Run the same focused command. Expected: tests pass.

## Task 4: Settings Persistence Red/Green

- [x] **Step 1: Add failing settings test**

Update `JsonSettingsStoreTests.SaveAsync_PersistsSettings` with:

```csharp
CustomEnhancementPrompts =
[
    new EnhancementPrompt(
        Guid.Parse("11111111-1111-1111-1111-111111111111"),
        "Standup",
        "Format as a terse standup update.",
        "list.bullet",
        "Daily update",
        IsPredefined: false,
        TriggerWords: ["standup mode"],
        UseSystemInstructions: true)
]
```

- [x] **Step 2: Run red settings tests**

Run:

```powershell
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" test VoiceInk.Windows\tests\VoiceInk.Windows.Infrastructure.Tests\VoiceInk.Windows.Infrastructure.Tests.csproj --filter "FullyQualifiedName~JsonSettingsStoreTests"
```

Expected: compile failure because `AppSettings.CustomEnhancementPrompts` does not exist.

- [x] **Step 3: Implement settings property**

Add `EnhancementPrompt[] CustomEnhancementPrompts { get; init; } = [];` to `AppSettings`, including equality and hash-code participation.

- [x] **Step 4: Verify settings tests pass**

Run the same focused Infrastructure command. Expected: tests pass.

## Task 5: WinUI Prompt Editor

- [x] **Step 1: Add editor controls**

In `MainWindow.xaml`, under `EnhancementPromptComboBox`, add:

- `PromptTitleTextBox`.
- `PromptInstructionsTextBox` with `AcceptsReturn="True"` and wrapping.
- `PromptTriggerWordsTextBox`.
- `PromptUseSystemInstructionsCheckBox`.
- `NewPromptButton`, `SavePromptButton`, and `DeletePromptButton`.

- [x] **Step 2: Load prompt library from settings**

In `InitializeAsync`, build `enhancementPrompts = EnhancementPromptLibrary.BuildPrompts(settings.CustomEnhancementPrompts)` before `RefreshEnhancementPromptChoices`.

- [x] **Step 3: Wire prompt selection and editor state**

Add helpers:

- `SelectedEnhancementPrompt()`.
- `RefreshPromptEditorFields()`.
- `SetPromptEditorForNewPrompt()`.
- `ParseTriggerWordsText()` through `EnhancementPromptLibrary.CreateCustomPrompt`.

Predefined prompts keep title/instructions/system-instruction fields disabled and only allow trigger-word edits. Delete is enabled only for custom prompts.

- [x] **Step 4: Save and delete prompt changes**

Add async handlers that update the in-memory prompt list, persist `CustomEnhancementPrompts = EnhancementPromptLibrary.PersistentPrompts(enhancementPrompts)` through settings, refresh combo/editor state, and keep `SelectedEnhancementPromptId` valid.

- [x] **Step 5: Make the enhancement pipeline use the current prompt library**

Construct `TextEnhancementPipeline` with a prompt-source lambda so dictation, Transcribe Audio, and retry use prompt edits without app restart.

- [x] **Step 6: Build for compile coverage**

Run:

```powershell
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" build VoiceInk.Windows\VoiceInk.Windows.sln -c Debug -p:Platform=x64
```

Expected: build succeeds.

## Task 6: Docs, Review, Commit

- [x] **Step 1: Update README and spec completion notes**

Document local custom prompt editor controls, trigger-word persistence, predefined prompt trigger overrides, and local-only storage.

- [x] **Step 2: Run focused and full verification**

Run:

```powershell
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" test VoiceInk.Windows\VoiceInk.Windows.sln --filter "FullyQualifiedName~EnhancementPromptTests|FullyQualifiedName~TextEnhancementPipelineTests|FullyQualifiedName~JsonSettingsStoreTests"
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" test VoiceInk.Windows\VoiceInk.Windows.sln
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" build VoiceInk.Windows\VoiceInk.Windows.sln -c Debug -p:Platform=x64
```

Expected: focused tests pass, full tests pass, and Debug x64 build succeeds.

- [x] **Step 3: Request review and fix Critical/Important findings**

Ask a review subagent to inspect prompt persistence, predefined prompt safety, selected prompt validity, dynamic pipeline prompt access, UI state, and docs. Fix all Critical and Important findings before committing.

- [ ] **Step 4: Commit implementation**

Run:

```powershell
git add VoiceInk.Windows README.md docs\superpowers\specs\2026-05-24-windows-open-source-parity-design.md docs\superpowers\plans\2026-05-25-windows-custom-enhancement-prompts.md
git commit -m "feat(windows): add custom enhancement prompts"
```

Expected: source/docs commit with `.superpowers/` left untracked.

## Self-Review

- Spec coverage: The plan covers prompt persistence, predefined trigger overrides, custom prompts, trigger words, JSON settings, dynamic pipeline access, WinUI controls, docs, review, and verification.
- Placeholder scan: No placeholder markers remain in implementation tasks.
- Type consistency: `CustomEnhancementPrompts`, `EnhancementPromptLibrary`, and existing `EnhancementPrompt`/`TextEnhancementPipeline` names are used consistently.
