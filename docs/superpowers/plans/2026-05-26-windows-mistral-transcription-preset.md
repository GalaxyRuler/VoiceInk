# Windows Mistral Transcription Preset Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add Mistral Voxtral as a selectable Windows cloud transcription preset backed by the existing OpenAI-compatible adapter.

**Architecture:** This is a Core catalog and credential-naming slice. No new Infrastructure adapter is needed because Mistral's transcription endpoint accepts the existing multipart request style.

**Tech Stack:** .NET 10, xUnit, existing OpenAI-compatible transcription service.

---

### Task 1: Save Spec And Plan

**Files:**
- Create: `docs/superpowers/specs/2026-05-26-windows-mistral-transcription-preset-design.md`
- Create: `docs/superpowers/plans/2026-05-26-windows-mistral-transcription-preset.md`

- [x] **Step 1: Capture design and plan**

Document endpoint, model, credential naming, and verification.

### Task 2: Add Failing Tests

**Files:**
- Modify: `VoiceInk.Windows/tests/VoiceInk.Windows.Core.Tests/Transcription/TranscriptionProviderPresetCatalogTests.cs`
- Modify: `VoiceInk.Windows/tests/VoiceInk.Windows.Infrastructure.Tests/Transcription/OpenAICompatibleCloudTranscriptionServiceTests.cs`

- [x] **Step 1: Add catalog and adapter tests**

Assert the catalog contains Mistral, resolves `mistral`, uses the Mistral secret name, and the OpenAI-compatible adapter reads that key and posts to Mistral's endpoint.

- [x] **Step 2: Run focused tests and confirm failure**

Run:

```powershell
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" test VoiceInk.Windows\tests\VoiceInk.Windows.Core.Tests\VoiceInk.Windows.Core.Tests.csproj --filter TranscriptionProviderPresetCatalogTests
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" test VoiceInk.Windows\tests\VoiceInk.Windows.Infrastructure.Tests\VoiceInk.Windows.Infrastructure.Tests.csproj --filter OpenAICompatibleCloudTranscriptionServiceTests
```

Expected: fail until the preset and secret segment exist.

### Task 3: Implement Preset

**Files:**
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.Core/Transcription/TranscriptionProviderPresetCatalog.cs`
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.Core/Transcription/TranscriptionConfiguration.cs`

- [x] **Step 1: Add Mistral preset and secret segment**

Add the preset to `All` and map `mistral` to `Mistral` in credential naming.

### Task 4: Verify And Commit

**Files:**
- Modify: `docs/superpowers/project-completion.md`

- [x] **Step 1: Run focused tests**

Run catalog and OpenAI-compatible provider tests.

- [x] **Step 2: Run full tests and build**

Run full solution tests and Debug x64 build.

- [x] **Step 3: Update completion tracker**

Raise cloud transcription slightly and note Mistral batch preset support.

- [x] **Step 4: Review and commit**

Commit with:

```powershell
git commit -m "feat(windows): add mistral transcription preset"
```
