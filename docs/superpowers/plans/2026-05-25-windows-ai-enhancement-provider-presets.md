# Windows AI Enhancement Provider Presets Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add macOS-aligned AI enhancement provider presets/cards for OpenAI-compatible chat-completions providers without adding commercial gates, bundled keys, account flows, paid prompts, telemetry, or provider-specific purchase surfaces.

**Architecture:** Core owns provider preset metadata, provider ID persistence, credential-name routing, provider metadata names, and endpoint validation. Infrastructure keeps the existing OpenAI-compatible chat-completions adapter but reads the selected provider's key and reports the selected provider name. WinUI adds provider and model pickers that fill endpoint/model defaults while keeping custom endpoint/model entry available.

**Tech Stack:** .NET 10, WinUI 3, xUnit, JSON settings, Windows Credential Manager, existing `OpenAICompatibleTextEnhancementService`.

**Grounding:**

- macOS source of truth: `VoiceInk/Services/AIEnhancement/AIService.swift` defines AI providers, base URLs, default models, and static model lists; `VoiceInk/Views/AI Models/APIKeyManagementView.swift` exposes a Provider picker, model picker, custom endpoint/model fields, Ollama server/model controls, and key status.
- Official provider docs checked on 2026-05-25: OpenAI Chat Completions, Groq OpenAI-compatible Chat Completions, Google Gemini OpenAI compatibility, OpenRouter Chat Completions, Mistral Chat Completions, Cerebras OpenAI compatibility, and Ollama OpenAI compatibility all support chat-completions-style requests compatible with the current Windows adapter.
- Open-source adaptation: provider presets only populate local configuration and credential names. Users supply their own endpoint, model, local Ollama server, and keys. Anthropic, Local CLI, and speech-provider enhancement paths remain separate slices because they do not fit the current OpenAI-compatible chat-completions adapter.

---

## Files

- Create `VoiceInk.Windows/src/VoiceInk.Windows.Core/Enhancement/EnhancementProviderPreset.cs`.
- Create `VoiceInk.Windows/src/VoiceInk.Windows.Core/Enhancement/EnhancementProviderPresetCatalog.cs`.
- Create `VoiceInk.Windows/src/VoiceInk.Windows.Core/Enhancement/EnhancementConfiguration.cs`.
- Modify `VoiceInk.Windows/src/VoiceInk.Windows.Core/Settings/AppSettings.cs`.
- Modify `VoiceInk.Windows/src/VoiceInk.Windows.Core/Enhancement/TextEnhancementRequest.cs`.
- Modify `VoiceInk.Windows/src/VoiceInk.Windows.Core/Enhancement/TextEnhancementPipeline.cs`.
- Modify `VoiceInk.Windows/src/VoiceInk.Windows.Infrastructure/Enhancement/OpenAICompatibleTextEnhancementService.cs`.
- Modify `VoiceInk.Windows/src/VoiceInk.Windows.App/MainWindow.xaml`.
- Modify `VoiceInk.Windows/src/VoiceInk.Windows.App/MainWindow.xaml.cs`.
- Modify Core and Infrastructure tests under `VoiceInk.Windows/tests`.
- Modify `README.md` and `docs/superpowers/specs/2026-05-24-windows-open-source-parity-design.md`.

## Task 1: Planning Docs

- [x] **Step 1: Update parity spec**

Record the provider-preset target under AI Enhancement: custom, Cerebras, Groq, Gemini, OpenAI, OpenRouter, Mistral, and Ollama through the current OpenAI-compatible chat-completions adapter; defer Anthropic, Local CLI, and speech/transcription-only providers.

- [ ] **Step 2: Commit planning docs**

Run:

```powershell
git add docs\superpowers\plans\2026-05-25-windows-ai-enhancement-provider-presets.md docs\superpowers\specs\2026-05-24-windows-open-source-parity-design.md
git commit -m "docs(windows): plan enhancement provider presets"
```

Expected: docs-only commit after the plan has been self-reviewed.

## Task 2: Core Preset Catalog Red/Green

- [ ] **Step 1: Add failing Core tests**

Create `VoiceInk.Windows/tests/VoiceInk.Windows.Core.Tests/Enhancement/EnhancementProviderPresetCatalogTests.cs` with tests that assert:

- `All` contains `custom`, `cerebras`, `groq`, `gemini`, `openai`, `openrouter`, `mistral`, and `ollama`.
- `custom` displays as `Custom OpenAI-compatible` with blank endpoint/model.
- `gemini` uses `https://generativelanguage.googleapis.com/v1beta/openai/chat/completions` and default model `gemini-2.5-flash-lite`.
- `ollama` uses `http://localhost:11434/v1/chat/completions`, requires no API key, and defaults to `mistral`.
- `Resolve` falls back to custom for missing/unknown IDs.
- `EnhancementConfiguration.SecretNameForProvider` returns provider-specific names such as `VoiceInk.Windows.Enhancement.OpenAICompatible.Groq.ApiKey`.
- `EnhancementConfiguration.SecretNamesForProvider("custom")` includes the legacy fallback `VoiceInk.Windows.Enhancement.OpenAICompatible.ApiKey`.
- `EnhancementConfiguration.ProviderNameFor("custom")` returns `openai-compatible`; named providers return their IDs.
- Endpoint validation rejects plain HTTP remote endpoints, embedded credentials, and key/token query parameters while allowing HTTP loopback for Ollama/local development.

- [ ] **Step 2: Run red Core tests**

Run:

```powershell
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" test VoiceInk.Windows\tests\VoiceInk.Windows.Core.Tests\VoiceInk.Windows.Core.Tests.csproj --filter "FullyQualifiedName~EnhancementProviderPresetCatalogTests"
```

Expected: compile failure because the preset catalog/configuration types do not exist.

- [ ] **Step 3: Implement Core catalog and configuration**

Add:

- `EnhancementProviderPreset` record with `Id`, `DisplayName`, `Endpoint`, `DefaultModel`, `ModelIds`, and `RequiresApiKey`.
- `EnhancementProviderPresetCatalog.All`, named preset properties, and `Resolve(string?)`.
- `AppSettings.EnhancementProviderId` defaulting to `custom`.
- `TextEnhancementRequest.ProviderId` defaulting to `custom`.
- `EnhancementConfiguration` constants and methods for `ValidateEndpoint`, `TryCreateEndpoint`, `SecretNameForProvider`, `SecretNamesForProvider`, and `ProviderNameFor`.

- [ ] **Step 4: Verify Core tests pass**

Run the same focused Core command. Expected: tests pass.

## Task 3: Pipeline Provider ID Red/Green

- [ ] **Step 1: Add failing pipeline tests**

Extend `TextEnhancementPipelineTests` to assert:

- When settings use `EnhancementProviderId = "groq"`, the provider request carries `ProviderId == "groq"`.
- When the fake provider returns provider name from `EnhancementConfiguration.ProviderNameFor(request.ProviderId)`, the pipeline returns enhancement metadata `groq`.
- Missing endpoint/model still returns original text and does not read context.

- [ ] **Step 2: Run red pipeline tests**

Run:

```powershell
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" test VoiceInk.Windows\tests\VoiceInk.Windows.Core.Tests\VoiceInk.Windows.Core.Tests.csproj --filter "FullyQualifiedName~TextEnhancementPipelineTests"
```

Expected: compile or assertion failure until provider ID flows into the request and metadata.

- [ ] **Step 3: Implement pipeline provider ID flow**

Update `TextEnhancementPipeline` to pass `EnhancementProviderPresetCatalog.Resolve(settings.EnhancementProviderId).Id` into `TextEnhancementRequest`.

- [ ] **Step 4: Verify pipeline tests pass**

Run the same focused command. Expected: tests pass.

## Task 4: Infrastructure Secret Routing And Validation Red/Green

- [ ] **Step 1: Add failing Infrastructure tests**

Extend `OpenAICompatibleTextEnhancementServiceTests` to assert:

- Named provider requests read only the provider-specific credential and return provider metadata such as `groq`.
- Custom provider falls back to the legacy secret if the new custom secret is missing.
- Ollama/local provider requests do not require an API key and do not send an Authorization header.
- Remote HTTP, embedded credentials, and key/token query endpoint strings fail before HTTP without leaking secrets.
- HTTP loopback endpoint is allowed for local development.

- [ ] **Step 2: Run red Infrastructure tests**

Run:

```powershell
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" test VoiceInk.Windows\tests\VoiceInk.Windows.Infrastructure.Tests\VoiceInk.Windows.Infrastructure.Tests.csproj --filter "FullyQualifiedName~OpenAICompatibleTextEnhancementServiceTests"
```

Expected: failures because the adapter still reads the old shared secret, always sends bearer auth, and lacks endpoint hardening.

- [ ] **Step 3: Implement adapter changes**

Change `OpenAICompatibleTextEnhancementService` to:

- Validate endpoint through `EnhancementConfiguration.TryCreateEndpoint`.
- Resolve provider preset from `request.ProviderId`.
- Read secret names from `EnhancementConfiguration.SecretNamesForProvider(request.ProviderId)`.
- Skip API-key lookup and Authorization header for presets with `RequiresApiKey == false`.
- Return `EnhancementConfiguration.ProviderNameFor(request.ProviderId)` in `TextEnhancementResult`.
- Keep `SecretName` as the legacy custom fallback constant.

- [ ] **Step 4: Verify Infrastructure tests pass**

Run the same focused Infrastructure command. Expected: tests pass.

## Task 5: WinUI Preset Wiring

- [ ] **Step 1: Add Enhancement provider controls**

In `MainWindow.xaml`, add:

- `EnhancementProviderPresetComboBox` above endpoint/model fields with `DisplayMemberPath="DisplayName"`.
- `EnhancementModelComboBox` below endpoint/model fields for static preset model choices.

- [ ] **Step 2: Load and save selected preset**

In `InitializeAsync`, set the provider combo from `settings.EnhancementProviderId`, then load endpoint/model and model choices. In `CurrentSettingsAsync`, save `EnhancementProviderId = SelectedEnhancementProviderId()`.

- [ ] **Step 3: Wire preset selection and key status**

Add helpers mirroring cloud transcription:

- `SelectedEnhancementProviderId()`.
- `SelectedEnhancementPreset()`.
- `SelectEnhancementPreset(string?)`.
- `ApplySelectedEnhancementPreset(bool fillConfiguration)`.
- `RefreshEnhancementModelChoices(string selectedModel)`.
- `HasEnhancementApiKeyAsync(string providerId, CancellationToken cancellationToken)`.

Update save/clear/status to use provider-specific secret names and display the selected provider name. Skip save/clear/key-required status for non-key providers such as Ollama.

- [ ] **Step 4: Enable and disable controls**

Update `RefreshUiFromControllerState` so provider/model picker controls follow enhancement control state and the API-key controls are disabled for providers that do not require a key.

- [ ] **Step 5: Build for compile coverage**

Run:

```powershell
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" build VoiceInk.Windows\VoiceInk.Windows.sln -c Debug -p:Platform=x64
```

Expected: build succeeds.

## Task 6: Settings Persistence And Docs

- [ ] **Step 1: Extend JSON settings test**

Update `JsonSettingsStoreTests.SaveAsync_PersistsSettings` to set and assert `EnhancementProviderId = "gemini"`.

- [ ] **Step 2: Verify settings tests**

Run:

```powershell
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" test VoiceInk.Windows\tests\VoiceInk.Windows.Infrastructure.Tests\VoiceInk.Windows.Infrastructure.Tests.csproj --filter "FullyQualifiedName~JsonSettingsStoreTests"
```

Expected: tests pass.

- [ ] **Step 3: Update README**

Document the Enhancement provider picker, static model presets, per-provider Credential Manager keys, Ollama local endpoint behavior, and endpoint safety validation.

- [ ] **Step 4: Run focused and full verification**

Run:

```powershell
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" test VoiceInk.Windows\VoiceInk.Windows.sln --filter "FullyQualifiedName~EnhancementProviderPresetCatalogTests|FullyQualifiedName~TextEnhancementPipelineTests|FullyQualifiedName~OpenAICompatibleTextEnhancementServiceTests|FullyQualifiedName~JsonSettingsStoreTests"
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" test VoiceInk.Windows\VoiceInk.Windows.sln
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" build VoiceInk.Windows\VoiceInk.Windows.sln -c Debug -p:Platform=x64
```

Expected: focused tests pass, full tests pass, and Debug x64 build succeeds.

- [ ] **Step 5: Request review and fix Critical/Important findings**

Ask a review subagent to inspect provider preset parity, credential routing, endpoint safety, WinUI wiring, and docs. Fix all Critical and Important findings before committing implementation.

- [ ] **Step 6: Commit implementation**

Run:

```powershell
git add VoiceInk.Windows README.md docs\superpowers\specs\2026-05-24-windows-open-source-parity-design.md docs\superpowers\plans\2026-05-25-windows-ai-enhancement-provider-presets.md
git commit -m "feat(windows): add enhancement provider presets"
```

Expected: source/docs commit with `.superpowers/` left untracked.

## Self-Review

- Spec coverage: The plan covers provider metadata, settings persistence, per-provider secrets, endpoint validation, WinUI controls, docs, verification, and review. Provider-specific Anthropic, Local CLI, OpenRouter dynamic model loading, and richer key verification are intentionally deferred because they need separate adapters or external requests.
- Placeholder scan: No placeholder markers remain in implementation tasks.
- Type consistency: `EnhancementProviderPreset`, `EnhancementProviderPresetCatalog`, and `EnhancementConfiguration` are consistently named across Core, Infrastructure, App, and tests.
