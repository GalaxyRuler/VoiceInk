# Windows Transcription Provider Presets Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add source-runnable transcription provider presets/cards for custom OpenAI-compatible and Groq without adding commercial gates, purchase prompts, telemetry, or provider-specific paywall flows.

**Architecture:** Core owns a small provider preset catalog, provider IDs, provider-specific secret naming, and history/provider metadata. Infrastructure keeps the existing OpenAI-compatible multipart adapter but reads the API key for the selected preset. WinUI adds a preset selector and model choices in AI Models, reusing the current endpoint/model/key controls rather than inventing a separate settings surface.

**Tech Stack:** .NET 10, WinUI 3, xUnit, JSON settings, Windows Credential Manager, existing `OpenAICompatibleCloudTranscriptionService`.

**Grounding:**

- macOS source of truth: `VoiceInk/Transcription/Cloud/CloudProvider.swift` lists named providers; `VoiceInk/Transcription/Cloud/GroqProvider.swift` defines provider key `Groq`, model `whisper-large-v3-turbo`, and an OpenAI-compatible base URL.
- Official Groq docs: Groq Speech to Text documents an OpenAI-compatible transcription endpoint at `https://api.groq.com/openai/v1/audio/transcriptions` with supported models `whisper-large-v3-turbo` and `whisper-large-v3`.
- Open-source adaptation: provider presets only fill local configuration and secret names. They do not include sign-up flows, purchase prompts, bundled keys, paid-plan hints, or telemetry.

---

## Files

- Create `VoiceInk.Windows/src/VoiceInk.Windows.Core/Transcription/TranscriptionProviderPreset.cs`.
- Create `VoiceInk.Windows/src/VoiceInk.Windows.Core/Transcription/TranscriptionProviderPresetCatalog.cs`.
- Modify `VoiceInk.Windows/src/VoiceInk.Windows.Core/Settings/AppSettings.cs`.
- Modify `VoiceInk.Windows/src/VoiceInk.Windows.Core/Transcription/TranscriptionOptions.cs`.
- Modify `VoiceInk.Windows/src/VoiceInk.Windows.Core/Transcription/TranscriptionConfiguration.cs`.
- Modify `VoiceInk.Windows/src/VoiceInk.Windows.Infrastructure/Transcription/OpenAICompatibleCloudTranscriptionService.cs`.
- Modify `VoiceInk.Windows/src/VoiceInk.Windows.App/MainWindow.xaml`.
- Modify `VoiceInk.Windows/src/VoiceInk.Windows.App/MainWindow.xaml.cs`.
- Modify Core and Infrastructure tests under `VoiceInk.Windows/tests`.
- Modify `README.md` and `docs/superpowers/specs/2026-05-24-windows-open-source-parity-design.md`.

## Task 1: Planning Docs

- [x] **Step 1: Update parity spec**

Add the provider preset target under Cloud Transcription: custom OpenAI-compatible plus Groq first, with other named providers deferred until their request shapes are implemented.

- [x] **Step 2: Commit planning docs**

Run:

```powershell
git add docs\superpowers\plans\2026-05-25-windows-transcription-provider-presets.md docs\superpowers\specs\2026-05-24-windows-open-source-parity-design.md
git commit -m "docs(windows): plan transcription provider presets"
```

Expected: docs-only commit.

## Task 2: Core Preset Catalog Red/Green

- [x] **Step 1: Add failing Core tests**

Add `VoiceInk.Windows/tests/VoiceInk.Windows.Core.Tests/Transcription/TranscriptionProviderPresetCatalogTests.cs` with tests:

```csharp
[Fact]
public void All_IncludesCustomAndGroqPresets()
{
    var presets = TranscriptionProviderPresetCatalog.All;

    Assert.Contains(presets, item => item.Id == "custom" && item.DisplayName == "Custom OpenAI-compatible");
    var groq = Assert.Single(presets.Where(item => item.Id == "groq"));
    Assert.Equal("Groq", groq.DisplayName);
    Assert.Equal("https://api.groq.com/openai/v1/audio/transcriptions", groq.Endpoint);
    Assert.Equal("whisper-large-v3-turbo", groq.DefaultModel);
    Assert.Contains("whisper-large-v3", groq.ModelIds);
}

[Theory]
[InlineData("", "custom")]
[InlineData("missing", "custom")]
[InlineData("groq", "groq")]
public void Resolve_ReturnsRequestedPresetOrCustomFallback(string id, string expectedId)
{
    Assert.Equal(expectedId, TranscriptionProviderPresetCatalog.Resolve(id).Id);
}

[Theory]
[InlineData("custom", "VoiceInk.Windows.Transcription.OpenAICompatible.Custom.ApiKey")]
[InlineData("groq", "VoiceInk.Windows.Transcription.OpenAICompatible.Groq.ApiKey")]
public void SecretNameFor_ReturnsProviderSpecificCredentialName(string providerId, string expected)
{
    Assert.Equal(expected, TranscriptionConfiguration.SecretNameForCloudProvider(providerId));
}
```

- [x] **Step 2: Run red Core tests**

Run:

```powershell
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" test VoiceInk.Windows\tests\VoiceInk.Windows.Core.Tests\VoiceInk.Windows.Core.Tests.csproj --filter "FullyQualifiedName~TranscriptionProviderPresetCatalogTests"
```

Expected: compile failure because preset catalog/types do not exist.

- [x] **Step 3: Implement Core catalog and provider id**

Add:

- `TranscriptionProviderPreset` record with `Id`, `DisplayName`, `Endpoint`, `DefaultModel`, and `ModelIds`.
- `TranscriptionProviderPresetCatalog.All`, `.Custom`, `.Groq`, and `.Resolve(string?)`.
- `AppSettings.CloudTranscriptionProviderId` defaulting to `"custom"`.
- `TranscriptionOptions.CloudProviderId` defaulting to `"custom"`.
- `TranscriptionConfiguration.SecretNameForCloudProvider(string?)`, normalizing unknown IDs to custom and known IDs to title-case credential segments.
- `TranscriptionConfiguration.ProviderName(AppSettings)` returns `"groq"` when the cloud provider ID is `groq`; custom remains `"openai-compatible"`.
- `TranscriptionConfiguration.BuildOptions` copies the cloud provider ID into options.

- [x] **Step 4: Verify Core tests pass**

Run the same focused Core command. Expected: tests pass.

## Task 3: Pipeline Metadata Red/Green

- [x] **Step 1: Add failing Core pipeline tests**

Extend existing dictation, Transcribe Audio, and History Retry tests with a Groq case that sets:

```csharp
TranscriptionProvider = TranscriptionProviderKind.OpenAICompatible,
CloudTranscriptionProviderId = "groq",
CloudTranscriptionEndpoint = "https://api.groq.com/openai/v1/audio/transcriptions",
CloudTranscriptionModel = "whisper-large-v3-turbo"
```

Assert `TranscriptionOptions.CloudProviderId == "groq"` and saved history `ProviderName == "groq"`.

- [x] **Step 2: Run red pipeline tests**

Run:

```powershell
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" test VoiceInk.Windows\tests\VoiceInk.Windows.Core.Tests\VoiceInk.Windows.Core.Tests.csproj --filter "FullyQualifiedName~DictationControllerTests|FullyQualifiedName~AudioFileTranscriptionServiceTests|FullyQualifiedName~HistoryRetryServiceTests"
```

Expected: compile or assertion failures until provider ID flows through options and metadata.

- [x] **Step 3: Implement minimal pipeline support**

Use `TranscriptionConfiguration.ProviderName(settings)` when saving completed history rows for cloud providers if the adapter returns generic `openai-compatible`. Do not change local Whisper behavior.

- [x] **Step 4: Verify pipeline tests pass**

Run the same focused command. Expected: tests pass.

## Task 4: Infrastructure Secret Routing Red/Green

- [x] **Step 1: Add failing Infrastructure tests**

Extend `OpenAICompatibleCloudTranscriptionServiceTests`:

```csharp
[Fact]
public async Task TranscribeAsync_ReadsProviderSpecificSecret()
{
    using var audioFile = new TempAudioFile();
    var handler = new QueueHttpMessageHandler(_ => JsonResponse(HttpStatusCode.OK, """{"text":"Groq text"}"""));
    var secrets = new FakeSecretStore { Secret = "gsk-test-secret" };
    var service = new OpenAICompatibleCloudTranscriptionService(new HttpClient(handler), secrets);

    await service.TranscribeAsync(
        Audio(audioFile.Path),
        Options(
            endpoint: "https://api.groq.com/openai/v1/audio/transcriptions",
            model: "whisper-large-v3-turbo",
            providerId: "groq"),
        CancellationToken.None);

    Assert.Equal("VoiceInk.Windows.Transcription.OpenAICompatible.Groq.ApiKey", secrets.LastReadName);
    Assert.Equal("gsk-test-secret", handler.Requests[0].Headers.Authorization?.Parameter);
}
```

- [x] **Step 2: Run red Infrastructure tests**

Run:

```powershell
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" test VoiceInk.Windows\tests\VoiceInk.Windows.Infrastructure.Tests\VoiceInk.Windows.Infrastructure.Tests.csproj --filter "FullyQualifiedName~OpenAICompatibleCloudTranscriptionServiceTests"
```

Expected: assertion failure because the adapter still reads the old shared secret name.

- [x] **Step 3: Implement provider-specific secret lookup**

Change `OpenAICompatibleCloudTranscriptionService` to call:

```csharp
var secretName = TranscriptionConfiguration.SecretNameForCloudProvider(options.CloudProviderId);
var apiKey = await secretStore.ReadSecretAsync(secretName, cancellationToken);
```

Keep the existing `SecretName` constant as an obsolete compatibility alias for custom provider UI references only if needed; new code should use `SecretNameForCloudProvider`.

- [x] **Step 4: Verify Infrastructure tests pass**

Run the same focused Infrastructure command. Expected: tests pass.

## Task 5: WinUI Preset Wiring

- [x] **Step 1: Add AI Models preset controls**

In `MainWindow.xaml`, add a `CloudTranscriptionPresetComboBox` above endpoint/model fields and a `CloudTranscriptionModelComboBox` near the model field. Use the existing section style. The custom preset leaves endpoint/model editable; Groq fills endpoint and exposes model choices.

- [x] **Step 2: Load/save selected preset**

In `InitializeAsync`, set the preset combo from `settings.CloudTranscriptionProviderId`. In `CurrentSettingsAsync`, save `CloudTranscriptionProviderId = SelectedCloudTranscriptionProviderId()`.

- [x] **Step 3: Wire preset selection**

Add helpers:

```csharp
private string SelectedCloudTranscriptionProviderId() =>
    SelectedCloudTranscriptionPreset()?.Id ?? TranscriptionProviderPresetCatalog.Custom.Id;

private TranscriptionProviderPreset? SelectedCloudTranscriptionPreset() =>
    CloudTranscriptionPresetComboBox.SelectedItem as TranscriptionProviderPreset;
```

On preset selection:

- If Groq is selected, fill endpoint with catalog endpoint.
- If model text is blank or not in preset model list, fill default model.
- Refresh key status using `TranscriptionConfiguration.SecretNameForCloudProvider(SelectedCloudTranscriptionProviderId())`.

- [x] **Step 4: Update key save/clear/status**

Change cloud transcription key save/clear/status methods to use provider-specific secret names. Update status text to include the selected preset display name.

- [x] **Step 5: Verify build**

Run:

```powershell
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" build VoiceInk.Windows\VoiceInk.Windows.sln -c Debug -p:Platform=x64
```

Expected: build succeeds with zero errors.

## Task 6: Docs, Review, And Commit

- [x] **Step 1: Update README and parity spec**

Document:

- Custom OpenAI-compatible and Groq presets.
- Groq endpoint/model defaults.
- Provider-specific Credential Manager entries.
- Deepgram, AssemblyAI, and other providers remain later because their request flows differ or need streaming/provider-specific adapters.

- [x] **Step 2: Run focused and full verification**

Run:

```powershell
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" test VoiceInk.Windows\VoiceInk.Windows.sln --filter "FullyQualifiedName~TranscriptionProviderPresetCatalogTests|FullyQualifiedName~OpenAICompatibleCloudTranscriptionServiceTests|FullyQualifiedName~DictationControllerTests|FullyQualifiedName~AudioFileTranscriptionServiceTests|FullyQualifiedName~HistoryRetryServiceTests"
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" test VoiceInk.Windows\VoiceInk.Windows.sln
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" build VoiceInk.Windows\VoiceInk.Windows.sln -c Debug -p:Platform=x64
```

Expected: focused tests pass, full tests pass, build succeeds.

- [x] **Step 3: Request review and fix Critical/Important findings**

Ask a subagent to review provider preset defaults, provider-specific secret handling, endpoint validation, and UI persistence.

- [x] **Step 4: Commit implementation**

Run:

```powershell
git add VoiceInk.Windows README.md docs\superpowers\specs\2026-05-24-windows-open-source-parity-design.md docs\superpowers\plans\2026-05-25-windows-transcription-provider-presets.md
git diff --check --cached
git commit -m "feat(windows): add transcription provider presets"
```

Expected: implementation commit with no whitespace errors.
