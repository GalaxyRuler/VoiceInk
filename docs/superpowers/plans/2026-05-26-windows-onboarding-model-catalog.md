# Windows Onboarding Model Catalog Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add recommended model selection and download to first-run onboarding.

**Architecture:** Keep recommended model choice logic in Core, reuse the existing `WhisperModelCatalog` and `IWhisperModelDownloader`, and wire the WinUI onboarding dialog to fill the existing model path field.

**Tech Stack:** .NET 10, xUnit, WinUI 3, existing HTTP GGML downloader.

---

### Task 1: Document model onboarding behavior

**Files:**
- Create: `docs/superpowers/specs/2026-05-26-windows-onboarding-model-catalog-design.md`
- Create: `docs/superpowers/plans/2026-05-26-windows-onboarding-model-catalog.md`

- [x] **Step 1: Save design and plan**

Capture behavior, non-goals, and verification.

### Task 2: Add failing catalog tests

**Files:**
- Create: `VoiceInk.Windows/tests/VoiceInk.Windows.Core.Tests/Onboarding/OnboardingModelCatalogServiceTests.cs`

- [x] **Step 1: Add recommended/default tests**

Assert recommended choices include `ggml-base.en` and `ggml-large-v3-turbo-q5_0`, and default is `ggml-base.en`.

- [x] **Step 2: Run focused onboarding catalog tests and confirm failure**

Run:

```powershell
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" test VoiceInk.Windows\tests\VoiceInk.Windows.Core.Tests\VoiceInk.Windows.Core.Tests.csproj --filter OnboardingModelCatalogServiceTests
```

Expected: compile failure because `OnboardingModelCatalogService` does not exist yet.

### Task 3: Implement catalog helper

**Files:**
- Create: `VoiceInk.Windows/src/VoiceInk.Windows.Core/Onboarding/OnboardingModelCatalogService.cs`

- [x] **Step 1: Add helper**

Expose `DefaultRecommendedModelName` and `BuildRecommendedChoices()`.

- [x] **Step 2: Run focused tests and confirm pass**

Run the focused onboarding catalog test command.

### Task 4: Wire onboarding download

**Files:**
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.App/MainWindow.xaml.cs`

- [x] **Step 1: Add recommended model ComboBox and download button**

Add the controls to the first-run dialog near the model path.

- [x] **Step 2: Add download handler**

Download the selected model with `modelDownloader`, update local model list, fill model path text boxes, refresh model choices, and update dialog status.

- [x] **Step 3: Build app**

Run Debug x64 app build.

### Task 5: Verify, document, and commit

**Files:**
- Modify: `README.md`
- Modify: `docs/superpowers/project-completion.md`

- [x] **Step 1: Update docs and completion tracker**

Mention onboarding recommended model download.

- [x] **Step 2: Run verification**

Run focused onboarding catalog tests, full solution tests, Debug x64 build, `git diff --check`, and review.

- [x] **Step 3: Commit**

Commit with:

```powershell
git commit -m "feat(windows): add onboarding model catalog download"
```
