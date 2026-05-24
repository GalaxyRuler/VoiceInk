# Core Windows Dictation MVP Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Build the source-runnable Windows dictation MVP: WinUI 3 shell, global hotkey seam, microphone capture, local whisper.cpp transcription through .NET bindings, text insertion, settings, and history.

**Architecture:** Add a new Windows solution under `VoiceInk.Windows/` without modifying the existing macOS app. Keep orchestration and contracts in `VoiceInk.Windows.Core`, persistence in `VoiceInk.Windows.Infrastructure`, Windows/native adapters in `VoiceInk.Windows.Native`, and WinUI composition in `VoiceInk.Windows.App`.

**Tech Stack:** .NET 10, WinUI 3, Windows App SDK `1.8.260508005`, NAudio `2.2.1`, Whisper.net `1.9.0` over whisper.cpp, Microsoft.Data.Sqlite `10.0.8`, xUnit `2.9.3`.

---

## Scope Check

This plan covers only the approved source-built MVP. The approved spec also includes cloud transcription, AI text enhancement, and installer packaging. Those are separate follow-on plans because each is an independent subsystem that can ship after this MVP is working.

Follow-on plans to write after this one lands:

- `cloud-transcription-providers`: OpenAI-compatible custom provider, Groq, Deepgram, provider settings, API key storage.
- `ai-text-enhancement`: OpenAI-compatible enhancement provider, prompt templates, enhancement fallback.
- `windows-installer-packaging`: MSIX, zip/dev distribution, native binary placement, install/uninstall smoke tests.

## File Structure

Create this tree:

```text
VoiceInk.Windows/
  Directory.Build.props
  global.json
  VoiceInk.Windows.sln
  src/
    VoiceInk.Windows.App/
      VoiceInk.Windows.App.csproj
      App.xaml
      App.xaml.cs
      MainWindow.xaml
      MainWindow.xaml.cs
      app.manifest
    VoiceInk.Windows.Core/
      VoiceInk.Windows.Core.csproj
      Audio/AudioCaptureResult.cs
      Dictation/DictationController.cs
      Dictation/DictationState.cs
      History/TranscriptionHistoryItem.cs
      Settings/AppSettings.cs
      Settings/TranscriptionProviderKind.cs
      Text/TextPostProcessor.cs
      Text/TextPostProcessingOptions.cs
      Transcription/TranscriptionOptions.cs
      Transcription/TranscriptionResult.cs
      Transcription/ITranscriptionService.cs
      Services/IAudioCaptureService.cs
      Services/IHistoryStore.cs
      Services/ISettingsStore.cs
      Services/ITextInjectionService.cs
    VoiceInk.Windows.Infrastructure/
      VoiceInk.Windows.Infrastructure.csproj
      History/SqliteHistoryStore.cs
      Settings/JsonSettingsStore.cs
    VoiceInk.Windows.Native/
      VoiceInk.Windows.Native.csproj
      Audio/NAudioCaptureService.cs
      Hotkeys/GlobalHotkeyService.cs
      Text/ClipboardTextInjectionService.cs
      Transcription/WhisperNetTranscriptionService.cs
  tests/
    VoiceInk.Windows.Core.Tests/
      VoiceInk.Windows.Core.Tests.csproj
      Dictation/DictationControllerTests.cs
      Text/TextPostProcessorTests.cs
    VoiceInk.Windows.Infrastructure.Tests/
      VoiceInk.Windows.Infrastructure.Tests.csproj
      History/SqliteHistoryStoreTests.cs
      Settings/JsonSettingsStoreTests.cs
```

Responsibility map:

- `Core`: pure orchestration, records, interfaces, and post-processing. No WinUI, SQLite, NAudio, Whisper.net, or P/Invoke references.
- `Infrastructure`: filesystem and SQLite persistence.
- `Native`: Windows/whisper adapters behind `Core` interfaces.
- `App`: WinUI shell that composes services and presents basic status/settings.

## Task 1: Scaffold The Windows Solution

**Files:**
- Create: `VoiceInk.Windows/global.json`
- Create: `VoiceInk.Windows/Directory.Build.props`
- Create: `VoiceInk.Windows/src/VoiceInk.Windows.Core/VoiceInk.Windows.Core.csproj`
- Create: `VoiceInk.Windows/src/VoiceInk.Windows.Infrastructure/VoiceInk.Windows.Infrastructure.csproj`
- Create: `VoiceInk.Windows/src/VoiceInk.Windows.Native/VoiceInk.Windows.Native.csproj`
- Create: `VoiceInk.Windows/src/VoiceInk.Windows.App/VoiceInk.Windows.App.csproj`
- Create: `VoiceInk.Windows/tests/VoiceInk.Windows.Core.Tests/VoiceInk.Windows.Core.Tests.csproj`
- Create: `VoiceInk.Windows/tests/VoiceInk.Windows.Infrastructure.Tests/VoiceInk.Windows.Infrastructure.Tests.csproj`

- [ ] **Step 1: Create solution directories**

Run from repo root:

```powershell
New-Item -ItemType Directory -Force -Path VoiceInk.Windows | Out-Null
New-Item -ItemType Directory -Force -Path VoiceInk.Windows\src\VoiceInk.Windows.Core | Out-Null
New-Item -ItemType Directory -Force -Path VoiceInk.Windows\src\VoiceInk.Windows.Infrastructure | Out-Null
New-Item -ItemType Directory -Force -Path VoiceInk.Windows\src\VoiceInk.Windows.Native | Out-Null
New-Item -ItemType Directory -Force -Path VoiceInk.Windows\src\VoiceInk.Windows.App | Out-Null
New-Item -ItemType Directory -Force -Path VoiceInk.Windows\tests\VoiceInk.Windows.Core.Tests | Out-Null
New-Item -ItemType Directory -Force -Path VoiceInk.Windows\tests\VoiceInk.Windows.Infrastructure.Tests | Out-Null
```

Expected: directories exist.

- [ ] **Step 2: Create solution file**

Run:

```powershell
dotnet new sln -n VoiceInk.Windows -o VoiceInk.Windows
```

Expected: `VoiceInk.Windows/VoiceInk.Windows.sln` exists.

- [ ] **Step 3: Add SDK pin**

Create `VoiceInk.Windows/global.json`:

```json
{
  "sdk": {
    "version": "10.0.100",
    "rollForward": "latestFeature"
  }
}
```

- [ ] **Step 4: Add shared build properties**

Create `VoiceInk.Windows/Directory.Build.props`:

```xml
<Project>
  <PropertyGroup>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <TreatWarningsAsErrors>true</TreatWarningsAsErrors>
    <LangVersion>latest</LangVersion>
  </PropertyGroup>
</Project>
```

- [ ] **Step 5: Add Core project file**

Create `VoiceInk.Windows/src/VoiceInk.Windows.Core/VoiceInk.Windows.Core.csproj`:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
  </PropertyGroup>
</Project>
```

- [ ] **Step 6: Add Infrastructure project file**

Create `VoiceInk.Windows/src/VoiceInk.Windows.Infrastructure/VoiceInk.Windows.Infrastructure.csproj`:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0-windows10.0.19041.0</TargetFramework>
    <TargetPlatformMinVersion>10.0.19041.0</TargetPlatformMinVersion>
  </PropertyGroup>
  <ItemGroup>
    <ProjectReference Include="..\VoiceInk.Windows.Core\VoiceInk.Windows.Core.csproj" />
    <PackageReference Include="Microsoft.Data.Sqlite" Version="10.0.8" />
  </ItemGroup>
</Project>
```

- [ ] **Step 7: Add Native project file**

Create `VoiceInk.Windows/src/VoiceInk.Windows.Native/VoiceInk.Windows.Native.csproj`:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0-windows10.0.19041.0</TargetFramework>
    <TargetPlatformMinVersion>10.0.19041.0</TargetPlatformMinVersion>
    <AllowUnsafeBlocks>true</AllowUnsafeBlocks>
  </PropertyGroup>
  <ItemGroup>
    <ProjectReference Include="..\VoiceInk.Windows.Core\VoiceInk.Windows.Core.csproj" />
    <PackageReference Include="NAudio" Version="2.2.1" />
    <PackageReference Include="Whisper.net" Version="1.9.0" />
    <PackageReference Include="Whisper.net.Runtime" Version="1.9.0" />
  </ItemGroup>
</Project>
```

- [ ] **Step 8: Add WinUI app project file**

Create `VoiceInk.Windows/src/VoiceInk.Windows.App/VoiceInk.Windows.App.csproj`:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>WinExe</OutputType>
    <TargetFramework>net10.0-windows10.0.19041.0</TargetFramework>
    <TargetPlatformMinVersion>10.0.19041.0</TargetPlatformMinVersion>
    <RootNamespace>VoiceInk.Windows.App</RootNamespace>
    <ApplicationManifest>app.manifest</ApplicationManifest>
    <Platforms>x64</Platforms>
    <RuntimeIdentifiers>win-x64</RuntimeIdentifiers>
    <UseWinUI>true</UseWinUI>
    <WindowsPackageType>None</WindowsPackageType>
  </PropertyGroup>
  <ItemGroup>
    <ProjectReference Include="..\VoiceInk.Windows.Core\VoiceInk.Windows.Core.csproj" />
    <ProjectReference Include="..\VoiceInk.Windows.Infrastructure\VoiceInk.Windows.Infrastructure.csproj" />
    <ProjectReference Include="..\VoiceInk.Windows.Native\VoiceInk.Windows.Native.csproj" />
    <PackageReference Include="Microsoft.WindowsAppSDK" Version="1.8.260508005" />
    <PackageReference Include="Microsoft.Windows.SDK.BuildTools" Version="10.0.26100.7705" />
  </ItemGroup>
</Project>
```

- [ ] **Step 9: Add Core test project file**

Create `VoiceInk.Windows/tests/VoiceInk.Windows.Core.Tests/VoiceInk.Windows.Core.Tests.csproj`:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <IsPackable>false</IsPackable>
  </PropertyGroup>
  <ItemGroup>
    <ProjectReference Include="..\..\src\VoiceInk.Windows.Core\VoiceInk.Windows.Core.csproj" />
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="18.5.1" />
    <PackageReference Include="xunit" Version="2.9.3" />
    <PackageReference Include="xunit.runner.visualstudio" Version="2.8.2" />
    <PackageReference Include="coverlet.collector" Version="10.0.1" />
  </ItemGroup>
</Project>
```

- [ ] **Step 10: Add Infrastructure test project file**

Create `VoiceInk.Windows/tests/VoiceInk.Windows.Infrastructure.Tests/VoiceInk.Windows.Infrastructure.Tests.csproj`:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0-windows10.0.19041.0</TargetFramework>
    <TargetPlatformMinVersion>10.0.19041.0</TargetPlatformMinVersion>
    <IsPackable>false</IsPackable>
  </PropertyGroup>
  <ItemGroup>
    <ProjectReference Include="..\..\src\VoiceInk.Windows.Core\VoiceInk.Windows.Core.csproj" />
    <ProjectReference Include="..\..\src\VoiceInk.Windows.Infrastructure\VoiceInk.Windows.Infrastructure.csproj" />
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="18.5.1" />
    <PackageReference Include="xunit" Version="2.9.3" />
    <PackageReference Include="xunit.runner.visualstudio" Version="2.8.2" />
    <PackageReference Include="coverlet.collector" Version="10.0.1" />
  </ItemGroup>
</Project>
```

- [ ] **Step 11: Add projects to solution**

Run:

```powershell
dotnet sln VoiceInk.Windows\VoiceInk.Windows.sln add VoiceInk.Windows\src\VoiceInk.Windows.Core\VoiceInk.Windows.Core.csproj
dotnet sln VoiceInk.Windows\VoiceInk.Windows.sln add VoiceInk.Windows\src\VoiceInk.Windows.Infrastructure\VoiceInk.Windows.Infrastructure.csproj
dotnet sln VoiceInk.Windows\VoiceInk.Windows.sln add VoiceInk.Windows\src\VoiceInk.Windows.Native\VoiceInk.Windows.Native.csproj
dotnet sln VoiceInk.Windows\VoiceInk.Windows.sln add VoiceInk.Windows\src\VoiceInk.Windows.App\VoiceInk.Windows.App.csproj
dotnet sln VoiceInk.Windows\VoiceInk.Windows.sln add VoiceInk.Windows\tests\VoiceInk.Windows.Core.Tests\VoiceInk.Windows.Core.Tests.csproj
dotnet sln VoiceInk.Windows\VoiceInk.Windows.sln add VoiceInk.Windows\tests\VoiceInk.Windows.Infrastructure.Tests\VoiceInk.Windows.Infrastructure.Tests.csproj
```

Expected: each command reports the project was added.

- [ ] **Step 12: Restore solution**

Run:

```powershell
dotnet restore VoiceInk.Windows\VoiceInk.Windows.sln
```

Expected: restore completes with exit code `0`.

- [ ] **Step 13: Commit scaffold**

```powershell
git add VoiceInk.Windows
git commit -m "feat(windows): scaffold Windows solution"
```

## Task 2: Add Core Models, Interfaces, And Text Post-Processing

**Files:**
- Create: `VoiceInk.Windows/src/VoiceInk.Windows.Core/Audio/AudioCaptureResult.cs`
- Create: `VoiceInk.Windows/src/VoiceInk.Windows.Core/Services/IAudioCaptureService.cs`
- Create: `VoiceInk.Windows/src/VoiceInk.Windows.Core/Services/ITextInjectionService.cs`
- Create: `VoiceInk.Windows/src/VoiceInk.Windows.Core/Services/IHistoryStore.cs`
- Create: `VoiceInk.Windows/src/VoiceInk.Windows.Core/Services/ISettingsStore.cs`
- Create: `VoiceInk.Windows/src/VoiceInk.Windows.Core/Transcription/ITranscriptionService.cs`
- Create: `VoiceInk.Windows/src/VoiceInk.Windows.Core/Transcription/TranscriptionOptions.cs`
- Create: `VoiceInk.Windows/src/VoiceInk.Windows.Core/Transcription/TranscriptionResult.cs`
- Create: `VoiceInk.Windows/src/VoiceInk.Windows.Core/History/TranscriptionHistoryItem.cs`
- Create: `VoiceInk.Windows/src/VoiceInk.Windows.Core/Settings/AppSettings.cs`
- Create: `VoiceInk.Windows/src/VoiceInk.Windows.Core/Settings/TranscriptionProviderKind.cs`
- Create: `VoiceInk.Windows/src/VoiceInk.Windows.Core/Text/TextPostProcessingOptions.cs`
- Create: `VoiceInk.Windows/src/VoiceInk.Windows.Core/Text/TextPostProcessor.cs`
- Test: `VoiceInk.Windows/tests/VoiceInk.Windows.Core.Tests/Text/TextPostProcessorTests.cs`

- [ ] **Step 1: Write failing text post-processing tests**

Create `VoiceInk.Windows/tests/VoiceInk.Windows.Core.Tests/Text/TextPostProcessorTests.cs`:

```csharp
using VoiceInk.Windows.Core.Text;
using Xunit;

namespace VoiceInk.Windows.Core.Tests.Text;

public sealed class TextPostProcessorTests
{
    [Fact]
    public void Process_TrimsWhitespace()
    {
        var result = TextPostProcessor.Process("  hello from voice ink \r\n", new TextPostProcessingOptions());

        Assert.Equal("hello from voice ink", result);
    }

    [Fact]
    public void Process_AppendsTrailingSpaceWhenEnabled()
    {
        var result = TextPostProcessor.Process("hello", new TextPostProcessingOptions(AppendTrailingSpace: true));

        Assert.Equal("hello ", result);
    }

    [Fact]
    public void Process_ReturnsEmptyStringForWhitespaceOnlyInput()
    {
        var result = TextPostProcessor.Process(" \r\n\t ", new TextPostProcessingOptions(AppendTrailingSpace: true));

        Assert.Equal(string.Empty, result);
    }
}
```

- [ ] **Step 2: Run the test and verify it fails**

Run:

```powershell
dotnet test VoiceInk.Windows\tests\VoiceInk.Windows.Core.Tests\VoiceInk.Windows.Core.Tests.csproj --filter TextPostProcessorTests
```

Expected: build fails because `VoiceInk.Windows.Core.Text` does not exist.

- [ ] **Step 3: Add core records and interfaces**

Create `VoiceInk.Windows/src/VoiceInk.Windows.Core/Audio/AudioCaptureResult.cs`:

```csharp
namespace VoiceInk.Windows.Core.Audio;

public sealed record AudioCaptureResult(
    string FilePath,
    TimeSpan Duration,
    int SampleRate,
    int ChannelCount);
```

Create `VoiceInk.Windows/src/VoiceInk.Windows.Core/Services/IAudioCaptureService.cs`:

```csharp
using VoiceInk.Windows.Core.Audio;

namespace VoiceInk.Windows.Core.Services;

public interface IAudioCaptureService
{
    Task StartAsync(CancellationToken cancellationToken);
    Task<AudioCaptureResult> StopAsync(CancellationToken cancellationToken);
}
```

Create `VoiceInk.Windows/src/VoiceInk.Windows.Core/Services/ITextInjectionService.cs`:

```csharp
namespace VoiceInk.Windows.Core.Services;

public interface ITextInjectionService
{
    Task InsertAsync(string text, CancellationToken cancellationToken);
}
```

Create `VoiceInk.Windows/src/VoiceInk.Windows.Core/Services/IHistoryStore.cs`:

```csharp
using VoiceInk.Windows.Core.History;

namespace VoiceInk.Windows.Core.Services;

public interface IHistoryStore
{
    Task SaveAsync(TranscriptionHistoryItem item, CancellationToken cancellationToken);
    Task<IReadOnlyList<TranscriptionHistoryItem>> ListRecentAsync(int limit, CancellationToken cancellationToken);
}
```

Create `VoiceInk.Windows/src/VoiceInk.Windows.Core/Services/ISettingsStore.cs`:

```csharp
using VoiceInk.Windows.Core.Settings;

namespace VoiceInk.Windows.Core.Services;

public interface ISettingsStore
{
    Task<AppSettings> LoadAsync(CancellationToken cancellationToken);
    Task SaveAsync(AppSettings settings, CancellationToken cancellationToken);
}
```

Create `VoiceInk.Windows/src/VoiceInk.Windows.Core/Transcription/ITranscriptionService.cs`:

```csharp
using VoiceInk.Windows.Core.Audio;

namespace VoiceInk.Windows.Core.Transcription;

public interface ITranscriptionService
{
    Task<TranscriptionResult> TranscribeAsync(
        AudioCaptureResult audio,
        TranscriptionOptions options,
        CancellationToken cancellationToken);
}
```

Create `VoiceInk.Windows/src/VoiceInk.Windows.Core/Transcription/TranscriptionOptions.cs`:

```csharp
namespace VoiceInk.Windows.Core.Transcription;

public sealed record TranscriptionOptions(
    string ModelPath,
    string Language = "auto");
```

Create `VoiceInk.Windows/src/VoiceInk.Windows.Core/Transcription/TranscriptionResult.cs`:

```csharp
namespace VoiceInk.Windows.Core.Transcription;

public sealed record TranscriptionResult(
    string Text,
    TimeSpan Duration,
    string ProviderName);
```

Create `VoiceInk.Windows/src/VoiceInk.Windows.Core/History/TranscriptionHistoryItem.cs`:

```csharp
namespace VoiceInk.Windows.Core.History;

public sealed record TranscriptionHistoryItem(
    Guid Id,
    DateTimeOffset CreatedAt,
    string Text,
    string ProviderName,
    TimeSpan AudioDuration,
    TimeSpan TranscriptionDuration);
```

Create `VoiceInk.Windows/src/VoiceInk.Windows.Core/Settings/TranscriptionProviderKind.cs`:

```csharp
namespace VoiceInk.Windows.Core.Settings;

public enum TranscriptionProviderKind
{
    LocalWhisper = 0
}
```

Create `VoiceInk.Windows/src/VoiceInk.Windows.Core/Settings/AppSettings.cs`:

```csharp
namespace VoiceInk.Windows.Core.Settings;

public sealed record AppSettings
{
    public string ModelPath { get; init; } = string.Empty;
    public string Language { get; init; } = "auto";
    public bool AppendTrailingSpace { get; init; }
    public bool RestoreClipboard { get; init; } = true;
    public string Hotkey { get; init; } = "Ctrl+Alt+Space";
    public TranscriptionProviderKind TranscriptionProvider { get; init; } = TranscriptionProviderKind.LocalWhisper;
}
```

- [ ] **Step 4: Add text post-processor**

Create `VoiceInk.Windows/src/VoiceInk.Windows.Core/Text/TextPostProcessingOptions.cs`:

```csharp
namespace VoiceInk.Windows.Core.Text;

public sealed record TextPostProcessingOptions(bool AppendTrailingSpace = false);
```

Create `VoiceInk.Windows/src/VoiceInk.Windows.Core/Text/TextPostProcessor.cs`:

```csharp
namespace VoiceInk.Windows.Core.Text;

public static class TextPostProcessor
{
    public static string Process(string text, TextPostProcessingOptions options)
    {
        var trimmed = text.Trim();

        if (trimmed.Length == 0)
        {
            return string.Empty;
        }

        return options.AppendTrailingSpace ? $"{trimmed} " : trimmed;
    }
}
```

- [ ] **Step 5: Run tests and verify they pass**

Run:

```powershell
dotnet test VoiceInk.Windows\tests\VoiceInk.Windows.Core.Tests\VoiceInk.Windows.Core.Tests.csproj --filter TextPostProcessorTests
```

Expected: `Passed! - Failed: 0`.

- [ ] **Step 6: Commit core contracts and post-processing**

```powershell
git add VoiceInk.Windows\src\VoiceInk.Windows.Core VoiceInk.Windows\tests\VoiceInk.Windows.Core.Tests
git commit -m "feat(windows): add core dictation contracts"
```

## Task 3: Add Dictation Controller Orchestration

**Files:**
- Create: `VoiceInk.Windows/src/VoiceInk.Windows.Core/Dictation/DictationController.cs`
- Create: `VoiceInk.Windows/src/VoiceInk.Windows.Core/Dictation/DictationState.cs`
- Test: `VoiceInk.Windows/tests/VoiceInk.Windows.Core.Tests/Dictation/DictationControllerTests.cs`

- [ ] **Step 1: Write failing orchestration tests**

Create `VoiceInk.Windows/tests/VoiceInk.Windows.Core.Tests/Dictation/DictationControllerTests.cs`:

```csharp
using VoiceInk.Windows.Core.Audio;
using VoiceInk.Windows.Core.Dictation;
using VoiceInk.Windows.Core.History;
using VoiceInk.Windows.Core.Services;
using VoiceInk.Windows.Core.Settings;
using VoiceInk.Windows.Core.Transcription;
using Xunit;

namespace VoiceInk.Windows.Core.Tests.Dictation;

public sealed class DictationControllerTests
{
    [Fact]
    public async Task StopAsync_TranscribesInsertsAndSavesHistory()
    {
        var audio = new AudioCaptureResult("sample.wav", TimeSpan.FromSeconds(2), 16000, 1);
        var capture = new FakeAudioCaptureService(audio);
        var transcription = new FakeTranscriptionService(new TranscriptionResult(" hello world ", TimeSpan.FromMilliseconds(150), "local-whisper"));
        var insertion = new FakeTextInjectionService();
        var history = new FakeHistoryStore();
        var settings = new FakeSettingsStore(new AppSettings
        {
            ModelPath = "ggml-base.en.bin",
            AppendTrailingSpace = true
        });

        var controller = new DictationController(capture, transcription, insertion, history, settings);

        await controller.StartAsync(CancellationToken.None);
        await controller.StopAsync(CancellationToken.None);

        Assert.Equal(DictationState.Idle, controller.State);
        Assert.Equal("hello world ", insertion.InsertedText);
        var saved = Assert.Single(history.Items);
        Assert.Equal("hello world ", saved.Text);
        Assert.Equal("local-whisper", saved.ProviderName);
        Assert.Equal(TimeSpan.FromSeconds(2), saved.AudioDuration);
    }

    [Fact]
    public async Task StartAsync_DoesNotStartRecordingWhenModelPathIsMissing()
    {
        var capture = new FakeAudioCaptureService(new AudioCaptureResult("sample.wav", TimeSpan.FromSeconds(2), 16000, 1));
        var transcription = new FakeTranscriptionService(new TranscriptionResult("ignored", TimeSpan.Zero, "local-whisper"));
        var controller = new DictationController(
            capture,
            transcription,
            new FakeTextInjectionService(),
            new FakeHistoryStore(),
            new FakeSettingsStore(new AppSettings()));

        await controller.StartAsync(CancellationToken.None);

        Assert.Equal(DictationState.Error, controller.State);
        Assert.Equal("Select a local whisper model before dictating.", controller.LastError);
        Assert.False(capture.Started);
        Assert.Equal(0, transcription.CallCount);
    }

    private sealed class FakeAudioCaptureService(AudioCaptureResult result) : IAudioCaptureService
    {
        public bool Started { get; private set; }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            Started = true;
            return Task.CompletedTask;
        }

        public Task<AudioCaptureResult> StopAsync(CancellationToken cancellationToken)
        {
            Assert.True(Started);
            return Task.FromResult(result);
        }
    }

    private sealed class FakeTranscriptionService(TranscriptionResult result) : ITranscriptionService
    {
        public int CallCount { get; private set; }

        public Task<TranscriptionResult> TranscribeAsync(
            AudioCaptureResult audio,
            TranscriptionOptions options,
            CancellationToken cancellationToken)
        {
            CallCount++;
            Assert.Equal("ggml-base.en.bin", options.ModelPath);
            return Task.FromResult(result);
        }
    }

    private sealed class FakeTextInjectionService : ITextInjectionService
    {
        public string? InsertedText { get; private set; }

        public Task InsertAsync(string text, CancellationToken cancellationToken)
        {
            InsertedText = text;
            return Task.CompletedTask;
        }
    }

    private sealed class FakeHistoryStore : IHistoryStore
    {
        public List<TranscriptionHistoryItem> Items { get; } = [];

        public Task SaveAsync(TranscriptionHistoryItem item, CancellationToken cancellationToken)
        {
            Items.Add(item);
            return Task.CompletedTask;
        }

        public Task<IReadOnlyList<TranscriptionHistoryItem>> ListRecentAsync(int limit, CancellationToken cancellationToken)
        {
            return Task.FromResult<IReadOnlyList<TranscriptionHistoryItem>>(Items.Take(limit).ToList());
        }
    }

    private sealed class FakeSettingsStore(AppSettings settings) : ISettingsStore
    {
        public Task<AppSettings> LoadAsync(CancellationToken cancellationToken) => Task.FromResult(settings);

        public Task SaveAsync(AppSettings settings, CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
```

- [ ] **Step 2: Run the test and verify it fails**

Run:

```powershell
dotnet test VoiceInk.Windows\tests\VoiceInk.Windows.Core.Tests\VoiceInk.Windows.Core.Tests.csproj --filter DictationControllerTests
```

Expected: build fails because `DictationController` and `DictationState` do not exist.

- [ ] **Step 3: Add dictation state**

Create `VoiceInk.Windows/src/VoiceInk.Windows.Core/Dictation/DictationState.cs`:

```csharp
namespace VoiceInk.Windows.Core.Dictation;

public enum DictationState
{
    Idle = 0,
    Recording = 1,
    Transcribing = 2,
    Inserting = 3,
    Error = 4
}
```

- [ ] **Step 4: Add dictation controller**

Create `VoiceInk.Windows/src/VoiceInk.Windows.Core/Dictation/DictationController.cs`:

```csharp
using VoiceInk.Windows.Core.History;
using VoiceInk.Windows.Core.Services;
using VoiceInk.Windows.Core.Text;
using VoiceInk.Windows.Core.Transcription;

namespace VoiceInk.Windows.Core.Dictation;

public sealed class DictationController(
    IAudioCaptureService audioCapture,
    ITranscriptionService transcriptionService,
    ITextInjectionService textInjection,
    IHistoryStore historyStore,
    ISettingsStore settingsStore)
{
    public DictationState State { get; private set; } = DictationState.Idle;
    public string? LastError { get; private set; }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        LastError = null;
        var settings = await settingsStore.LoadAsync(cancellationToken);
        if (string.IsNullOrWhiteSpace(settings.ModelPath))
        {
            State = DictationState.Error;
            LastError = "Select a local whisper model before dictating.";
            return;
        }

        State = DictationState.Recording;
        await audioCapture.StartAsync(cancellationToken);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        var audio = await audioCapture.StopAsync(cancellationToken);
        var settings = await settingsStore.LoadAsync(cancellationToken);

        State = DictationState.Transcribing;
        var transcription = await transcriptionService.TranscribeAsync(
            audio,
            new TranscriptionOptions(settings.ModelPath, settings.Language),
            cancellationToken);

        var finalText = TextPostProcessor.Process(
            transcription.Text,
            new TextPostProcessingOptions(settings.AppendTrailingSpace));

        if (finalText.Length == 0)
        {
            State = DictationState.Idle;
            return;
        }

        State = DictationState.Inserting;
        await textInjection.InsertAsync(finalText, cancellationToken);

        await historyStore.SaveAsync(
            new TranscriptionHistoryItem(
                Guid.NewGuid(),
                DateTimeOffset.UtcNow,
                finalText,
                transcription.ProviderName,
                audio.Duration,
                transcription.Duration),
            cancellationToken);

        State = DictationState.Idle;
    }
}
```

- [ ] **Step 5: Run orchestration tests**

Run:

```powershell
dotnet test VoiceInk.Windows\tests\VoiceInk.Windows.Core.Tests\VoiceInk.Windows.Core.Tests.csproj --filter DictationControllerTests
```

Expected: `Passed! - Failed: 0`.

- [ ] **Step 6: Run all core tests**

Run:

```powershell
dotnet test VoiceInk.Windows\tests\VoiceInk.Windows.Core.Tests\VoiceInk.Windows.Core.Tests.csproj
```

Expected: all core tests pass.

- [ ] **Step 7: Commit controller**

```powershell
git add VoiceInk.Windows\src\VoiceInk.Windows.Core\Dictation VoiceInk.Windows\tests\VoiceInk.Windows.Core.Tests\Dictation
git commit -m "feat(windows): add dictation controller"
```

## Task 4: Add JSON Settings Persistence

**Files:**
- Create: `VoiceInk.Windows/src/VoiceInk.Windows.Infrastructure/Settings/JsonSettingsStore.cs`
- Test: `VoiceInk.Windows/tests/VoiceInk.Windows.Infrastructure.Tests/Settings/JsonSettingsStoreTests.cs`

- [ ] **Step 1: Write failing settings tests**

Create `VoiceInk.Windows/tests/VoiceInk.Windows.Infrastructure.Tests/Settings/JsonSettingsStoreTests.cs`:

```csharp
using VoiceInk.Windows.Core.Settings;
using VoiceInk.Windows.Infrastructure.Settings;
using Xunit;

namespace VoiceInk.Windows.Infrastructure.Tests.Settings;

public sealed class JsonSettingsStoreTests
{
    [Fact]
    public async Task LoadAsync_CreatesDefaultSettingsWhenFileIsMissing()
    {
        using var temp = new TempDirectory();
        var path = Path.Combine(temp.Path, "settings.json");
        var store = new JsonSettingsStore(path);

        var settings = await store.LoadAsync(CancellationToken.None);

        Assert.Equal("auto", settings.Language);
        Assert.True(settings.RestoreClipboard);
        Assert.True(File.Exists(path));
    }

    [Fact]
    public async Task SaveAsync_PersistsSettings()
    {
        using var temp = new TempDirectory();
        var path = Path.Combine(temp.Path, "settings.json");
        var store = new JsonSettingsStore(path);
        var expected = new AppSettings
        {
            ModelPath = "C:\\Models\\ggml-base.en.bin",
            Language = "en",
            AppendTrailingSpace = true,
            RestoreClipboard = false,
            Hotkey = "Ctrl+Shift+D"
        };

        await store.SaveAsync(expected, CancellationToken.None);
        var actual = await store.LoadAsync(CancellationToken.None);

        Assert.Equal(expected, actual);
    }

    private sealed class TempDirectory : IDisposable
    {
        public string Path { get; } = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"voiceink-{Guid.NewGuid():N}");

        public TempDirectory()
        {
            Directory.CreateDirectory(Path);
        }

        public void Dispose()
        {
            if (Directory.Exists(Path))
            {
                Directory.Delete(Path, recursive: true);
            }
        }
    }
}
```

- [ ] **Step 2: Run settings tests and verify failure**

Run:

```powershell
dotnet test VoiceInk.Windows\tests\VoiceInk.Windows.Infrastructure.Tests\VoiceInk.Windows.Infrastructure.Tests.csproj --filter JsonSettingsStoreTests
```

Expected: build fails because `JsonSettingsStore` does not exist.

- [ ] **Step 3: Implement JSON settings store**

Create `VoiceInk.Windows/src/VoiceInk.Windows.Infrastructure/Settings/JsonSettingsStore.cs`:

```csharp
using System.Text.Json;
using VoiceInk.Windows.Core.Services;
using VoiceInk.Windows.Core.Settings;

namespace VoiceInk.Windows.Infrastructure.Settings;

public sealed class JsonSettingsStore(string filePath) : ISettingsStore
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    public async Task<AppSettings> LoadAsync(CancellationToken cancellationToken)
    {
        if (!File.Exists(filePath))
        {
            var defaults = new AppSettings();
            await SaveAsync(defaults, cancellationToken);
            return defaults;
        }

        await using var stream = File.OpenRead(filePath);
        var settings = await JsonSerializer.DeserializeAsync<AppSettings>(stream, JsonOptions, cancellationToken);
        return settings ?? new AppSettings();
    }

    public async Task SaveAsync(AppSettings settings, CancellationToken cancellationToken)
    {
        var directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        await using var stream = File.Create(filePath);
        await JsonSerializer.SerializeAsync(stream, settings, JsonOptions, cancellationToken);
    }
}
```

- [ ] **Step 4: Run settings tests**

Run:

```powershell
dotnet test VoiceInk.Windows\tests\VoiceInk.Windows.Infrastructure.Tests\VoiceInk.Windows.Infrastructure.Tests.csproj --filter JsonSettingsStoreTests
```

Expected: `Passed! - Failed: 0`.

- [ ] **Step 5: Commit settings store**

```powershell
git add VoiceInk.Windows\src\VoiceInk.Windows.Infrastructure\Settings VoiceInk.Windows\tests\VoiceInk.Windows.Infrastructure.Tests\Settings
git commit -m "feat(windows): add JSON settings store"
```

## Task 5: Add SQLite History Persistence

**Files:**
- Create: `VoiceInk.Windows/src/VoiceInk.Windows.Infrastructure/History/SqliteHistoryStore.cs`
- Test: `VoiceInk.Windows/tests/VoiceInk.Windows.Infrastructure.Tests/History/SqliteHistoryStoreTests.cs`

- [ ] **Step 1: Write failing history tests**

Create `VoiceInk.Windows/tests/VoiceInk.Windows.Infrastructure.Tests/History/SqliteHistoryStoreTests.cs`:

```csharp
using VoiceInk.Windows.Core.History;
using VoiceInk.Windows.Infrastructure.History;
using Xunit;

namespace VoiceInk.Windows.Infrastructure.Tests.History;

public sealed class SqliteHistoryStoreTests
{
    [Fact]
    public async Task SaveAndListRecentAsync_ReturnsNewestFirst()
    {
        using var temp = new TempDirectory();
        var dbPath = Path.Combine(temp.Path, "history.db");
        var store = new SqliteHistoryStore(dbPath);
        var older = new TranscriptionHistoryItem(
            Guid.NewGuid(),
            DateTimeOffset.UtcNow.AddMinutes(-5),
            "older text",
            "local-whisper",
            TimeSpan.FromSeconds(1),
            TimeSpan.FromMilliseconds(100));
        var newer = older with
        {
            Id = Guid.NewGuid(),
            CreatedAt = DateTimeOffset.UtcNow,
            Text = "newer text"
        };

        await store.SaveAsync(older, CancellationToken.None);
        await store.SaveAsync(newer, CancellationToken.None);

        var results = await store.ListRecentAsync(10, CancellationToken.None);

        Assert.Collection(
            results,
            item => Assert.Equal("newer text", item.Text),
            item => Assert.Equal("older text", item.Text));
    }

    private sealed class TempDirectory : IDisposable
    {
        public string Path { get; } = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"voiceink-{Guid.NewGuid():N}");

        public TempDirectory()
        {
            Directory.CreateDirectory(Path);
        }

        public void Dispose()
        {
            if (Directory.Exists(Path))
            {
                Directory.Delete(Path, recursive: true);
            }
        }
    }
}
```

- [ ] **Step 2: Run history tests and verify failure**

Run:

```powershell
dotnet test VoiceInk.Windows\tests\VoiceInk.Windows.Infrastructure.Tests\VoiceInk.Windows.Infrastructure.Tests.csproj --filter SqliteHistoryStoreTests
```

Expected: build fails because `SqliteHistoryStore` does not exist.

- [ ] **Step 3: Implement SQLite history store**

Create `VoiceInk.Windows/src/VoiceInk.Windows.Infrastructure/History/SqliteHistoryStore.cs`:

```csharp
using Microsoft.Data.Sqlite;
using VoiceInk.Windows.Core.History;
using VoiceInk.Windows.Core.Services;

namespace VoiceInk.Windows.Infrastructure.History;

public sealed class SqliteHistoryStore : IHistoryStore
{
    private readonly string connectionString;

    public SqliteHistoryStore(string databasePath)
    {
        var directory = Path.GetDirectoryName(databasePath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        connectionString = new SqliteConnectionStringBuilder { DataSource = databasePath }.ToString();
        EnsureDatabase();
    }

    public async Task SaveAsync(TranscriptionHistoryItem item, CancellationToken cancellationToken)
    {
        await using var connection = new SqliteConnection(connectionString);
        await connection.OpenAsync(cancellationToken);

        var command = connection.CreateCommand();
        command.CommandText = """
            INSERT INTO transcriptions
                (id, created_at, text, provider_name, audio_duration_ms, transcription_duration_ms)
            VALUES
                ($id, $created_at, $text, $provider_name, $audio_duration_ms, $transcription_duration_ms);
            """;
        command.Parameters.AddWithValue("$id", item.Id.ToString());
        command.Parameters.AddWithValue("$created_at", item.CreatedAt.ToString("O"));
        command.Parameters.AddWithValue("$text", item.Text);
        command.Parameters.AddWithValue("$provider_name", item.ProviderName);
        command.Parameters.AddWithValue("$audio_duration_ms", item.AudioDuration.TotalMilliseconds);
        command.Parameters.AddWithValue("$transcription_duration_ms", item.TranscriptionDuration.TotalMilliseconds);

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<TranscriptionHistoryItem>> ListRecentAsync(int limit, CancellationToken cancellationToken)
    {
        await using var connection = new SqliteConnection(connectionString);
        await connection.OpenAsync(cancellationToken);

        var command = connection.CreateCommand();
        command.CommandText = """
            SELECT id, created_at, text, provider_name, audio_duration_ms, transcription_duration_ms
            FROM transcriptions
            ORDER BY created_at DESC
            LIMIT $limit;
            """;
        command.Parameters.AddWithValue("$limit", limit);

        var items = new List<TranscriptionHistoryItem>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            items.Add(new TranscriptionHistoryItem(
                Guid.Parse(reader.GetString(0)),
                DateTimeOffset.Parse(reader.GetString(1)),
                reader.GetString(2),
                reader.GetString(3),
                TimeSpan.FromMilliseconds(reader.GetDouble(4)),
                TimeSpan.FromMilliseconds(reader.GetDouble(5))));
        }

        return items;
    }

    private void EnsureDatabase()
    {
        using var connection = new SqliteConnection(connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = """
            CREATE TABLE IF NOT EXISTS transcriptions (
                id TEXT PRIMARY KEY,
                created_at TEXT NOT NULL,
                text TEXT NOT NULL,
                provider_name TEXT NOT NULL,
                audio_duration_ms REAL NOT NULL,
                transcription_duration_ms REAL NOT NULL
            );
            CREATE INDEX IF NOT EXISTS idx_transcriptions_created_at
                ON transcriptions(created_at DESC);
            """;
        command.ExecuteNonQuery();
    }
}
```

- [ ] **Step 4: Run history tests**

Run:

```powershell
dotnet test VoiceInk.Windows\tests\VoiceInk.Windows.Infrastructure.Tests\VoiceInk.Windows.Infrastructure.Tests.csproj --filter SqliteHistoryStoreTests
```

Expected: `Passed! - Failed: 0`.

- [ ] **Step 5: Commit history store**

```powershell
git add VoiceInk.Windows\src\VoiceInk.Windows.Infrastructure\History VoiceInk.Windows\tests\VoiceInk.Windows.Infrastructure.Tests\History
git commit -m "feat(windows): add SQLite history store"
```

## Task 6: Add Local Whisper Transcription Adapter

**Files:**
- Create: `VoiceInk.Windows/src/VoiceInk.Windows.Native/Transcription/WhisperNetTranscriptionService.cs`

- [ ] **Step 1: Add Whisper.net service**

Create `VoiceInk.Windows/src/VoiceInk.Windows.Native/Transcription/WhisperNetTranscriptionService.cs`:

```csharp
using System.Diagnostics;
using System.Text;
using VoiceInk.Windows.Core.Audio;
using VoiceInk.Windows.Core.Transcription;
using Whisper.net;

namespace VoiceInk.Windows.Native.Transcription;

public sealed class WhisperNetTranscriptionService : ITranscriptionService
{
    public async Task<TranscriptionResult> TranscribeAsync(
        AudioCaptureResult audio,
        TranscriptionOptions options,
        CancellationToken cancellationToken)
    {
        if (!File.Exists(options.ModelPath))
        {
            throw new FileNotFoundException("The configured whisper model file was not found.", options.ModelPath);
        }

        if (!File.Exists(audio.FilePath))
        {
            throw new FileNotFoundException("The recorded audio file was not found.", audio.FilePath);
        }

        var stopwatch = Stopwatch.StartNew();
        var text = new StringBuilder();

        using var factory = WhisperFactory.FromPath(options.ModelPath);
        var builder = factory.CreateBuilder();
        if (!string.Equals(options.Language, "auto", StringComparison.OrdinalIgnoreCase))
        {
            builder.WithLanguage(options.Language);
        }

        using var processor = builder.Build();

        await using var stream = File.OpenRead(audio.FilePath);
        await foreach (var segment in processor.ProcessAsync(stream, cancellationToken))
        {
            text.Append(segment.Text);
        }

        stopwatch.Stop();
        return new TranscriptionResult(text.ToString(), stopwatch.Elapsed, "local-whisper");
    }
}
```

- [ ] **Step 2: Build Native project**

Run:

```powershell
dotnet build VoiceInk.Windows\src\VoiceInk.Windows.Native\VoiceInk.Windows.Native.csproj -c Debug
```

Expected: build succeeds.

- [ ] **Step 3: Commit transcription adapter**

```powershell
git add VoiceInk.Windows\src\VoiceInk.Windows.Native\Transcription
git commit -m "feat(windows): add local whisper transcription adapter"
```

## Task 7: Add WASAPI Audio Capture

**Files:**
- Create: `VoiceInk.Windows/src/VoiceInk.Windows.Native/Audio/NAudioCaptureService.cs`

- [ ] **Step 1: Implement NAudio capture service**

Create `VoiceInk.Windows/src/VoiceInk.Windows.Native/Audio/NAudioCaptureService.cs`:

```csharp
using NAudio.Wave;
using VoiceInk.Windows.Core.Audio;
using VoiceInk.Windows.Core.Services;

namespace VoiceInk.Windows.Native.Audio;

public sealed class NAudioCaptureService(string recordingsDirectory) : IAudioCaptureService, IDisposable
{
    private WaveInEvent? waveIn;
    private WaveFileWriter? writer;
    private string? currentFilePath;
    private DateTimeOffset startedAt;

    public Task StartAsync(CancellationToken cancellationToken)
    {
        Directory.CreateDirectory(recordingsDirectory);

        currentFilePath = Path.Combine(recordingsDirectory, $"{Guid.NewGuid():N}.wav");
        startedAt = DateTimeOffset.UtcNow;

        waveIn = new WaveInEvent
        {
            WaveFormat = new WaveFormat(16000, 16, 1),
            BufferMilliseconds = 50
        };
        writer = new WaveFileWriter(currentFilePath, waveIn.WaveFormat);

        waveIn.DataAvailable += OnDataAvailable;
        waveIn.RecordingStopped += OnRecordingStopped;
        waveIn.StartRecording();

        return Task.CompletedTask;
    }

    public Task<AudioCaptureResult> StopAsync(CancellationToken cancellationToken)
    {
        if (waveIn is null || writer is null || currentFilePath is null)
        {
            throw new InvalidOperationException("Recording has not started.");
        }

        waveIn.StopRecording();
        var duration = DateTimeOffset.UtcNow - startedAt;
        var result = new AudioCaptureResult(currentFilePath, duration, 16000, 1);

        DisposeCurrentRecording();
        return Task.FromResult(result);
    }

    public void Dispose()
    {
        DisposeCurrentRecording();
    }

    private void OnDataAvailable(object? sender, WaveInEventArgs args)
    {
        writer?.Write(args.Buffer, 0, args.BytesRecorded);
        writer?.Flush();
    }

    private void OnRecordingStopped(object? sender, StoppedEventArgs args)
    {
        if (args.Exception is not null)
        {
            DisposeCurrentRecording();
        }
    }

    private void DisposeCurrentRecording()
    {
        if (waveIn is not null)
        {
            waveIn.DataAvailable -= OnDataAvailable;
            waveIn.RecordingStopped -= OnRecordingStopped;
            waveIn.Dispose();
            waveIn = null;
        }

        writer?.Dispose();
        writer = null;
    }
}
```

- [ ] **Step 2: Build Native project**

Run:

```powershell
dotnet build VoiceInk.Windows\src\VoiceInk.Windows.Native\VoiceInk.Windows.Native.csproj -c Debug
```

Expected: build succeeds.

- [ ] **Step 3: Commit audio capture**

```powershell
git add VoiceInk.Windows\src\VoiceInk.Windows.Native\Audio
git commit -m "feat(windows): add WASAPI audio capture"
```

## Task 8: Add Clipboard-Based Text Injection

**Files:**
- Create: `VoiceInk.Windows/src/VoiceInk.Windows.Native/Text/ClipboardTextInjectionService.cs`

- [ ] **Step 1: Implement clipboard paste injection**

Create `VoiceInk.Windows/src/VoiceInk.Windows.Native/Text/ClipboardTextInjectionService.cs`:

```csharp
using System.Runtime.InteropServices;
using System.Windows.Forms;
using VoiceInk.Windows.Core.Services;

namespace VoiceInk.Windows.Native.Text;

public sealed class ClipboardTextInjectionService(bool restoreClipboard) : ITextInjectionService
{
    public async Task InsertAsync(string text, CancellationToken cancellationToken)
    {
        var previousText = Clipboard.ContainsText() ? Clipboard.GetText() : null;

        Clipboard.SetText(text);
        await Task.Delay(TimeSpan.FromMilliseconds(80), cancellationToken);
        SendCtrlV();

        if (restoreClipboard && previousText is not null)
        {
            await Task.Delay(TimeSpan.FromMilliseconds(400), cancellationToken);
            Clipboard.SetText(previousText);
        }
    }

    private static void SendCtrlV()
    {
        var inputs = new[]
        {
            KeyboardInput(VirtualKeyControl, keyUp: false),
            KeyboardInput(VirtualKeyV, keyUp: false),
            KeyboardInput(VirtualKeyV, keyUp: true),
            KeyboardInput(VirtualKeyControl, keyUp: true)
        };

        var sent = SendInput((uint)inputs.Length, inputs, Marshal.SizeOf<INPUT>());
        if (sent != inputs.Length)
        {
            throw new InvalidOperationException("Windows did not accept the paste keyboard input.");
        }
    }

    private static INPUT KeyboardInput(ushort virtualKey, bool keyUp)
    {
        return new INPUT
        {
            type = InputKeyboard,
            U = new InputUnion
            {
                ki = new KEYBDINPUT
                {
                    wVk = virtualKey,
                    dwFlags = keyUp ? KeyEventKeyUp : 0
                }
            }
        };
    }

    private const int InputKeyboard = 1;
    private const ushort VirtualKeyControl = 0x11;
    private const ushort VirtualKeyV = 0x56;
    private const uint KeyEventKeyUp = 0x0002;

    [DllImport("user32.dll", SetLastError = true)]
    private static extern uint SendInput(uint nInputs, INPUT[] pInputs, int cbSize);

    [StructLayout(LayoutKind.Sequential)]
    private struct INPUT
    {
        public int type;
        public InputUnion U;
    }

    [StructLayout(LayoutKind.Explicit)]
    private struct InputUnion
    {
        [FieldOffset(0)]
        public KEYBDINPUT ki;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct KEYBDINPUT
    {
        public ushort wVk;
        public ushort wScan;
        public uint dwFlags;
        public uint time;
        public IntPtr dwExtraInfo;
    }
}
```

- [ ] **Step 2: Add Windows Forms support to Native project**

Modify `VoiceInk.Windows/src/VoiceInk.Windows.Native/VoiceInk.Windows.Native.csproj`:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0-windows10.0.19041.0</TargetFramework>
    <TargetPlatformMinVersion>10.0.19041.0</TargetPlatformMinVersion>
    <AllowUnsafeBlocks>true</AllowUnsafeBlocks>
    <UseWindowsForms>true</UseWindowsForms>
  </PropertyGroup>
  <ItemGroup>
    <ProjectReference Include="..\VoiceInk.Windows.Core\VoiceInk.Windows.Core.csproj" />
    <PackageReference Include="NAudio" Version="2.2.1" />
    <PackageReference Include="Whisper.net" Version="1.9.0" />
    <PackageReference Include="Whisper.net.Runtime" Version="1.9.0" />
  </ItemGroup>
</Project>
```

- [ ] **Step 3: Build Native project**

Run:

```powershell
dotnet build VoiceInk.Windows\src\VoiceInk.Windows.Native\VoiceInk.Windows.Native.csproj -c Debug
```

Expected: build succeeds.

- [ ] **Step 4: Commit text injection**

```powershell
git add VoiceInk.Windows\src\VoiceInk.Windows.Native\Text VoiceInk.Windows\src\VoiceInk.Windows.Native\VoiceInk.Windows.Native.csproj
git commit -m "feat(windows): add clipboard text injection"
```

## Task 9: Add Minimal WinUI Shell And Compose Services

**Files:**
- Create: `VoiceInk.Windows/src/VoiceInk.Windows.App/App.xaml`
- Create: `VoiceInk.Windows/src/VoiceInk.Windows.App/App.xaml.cs`
- Create: `VoiceInk.Windows/src/VoiceInk.Windows.App/MainWindow.xaml`
- Create: `VoiceInk.Windows/src/VoiceInk.Windows.App/MainWindow.xaml.cs`
- Create: `VoiceInk.Windows/src/VoiceInk.Windows.App/app.manifest`

- [ ] **Step 1: Add application manifest**

Create `VoiceInk.Windows/src/VoiceInk.Windows.App/app.manifest`:

```xml
<?xml version="1.0" encoding="utf-8"?>
<assembly manifestVersion="1.0" xmlns="urn:schemas-microsoft-com:asm.v1">
  <assemblyIdentity version="1.0.0.0" name="VoiceInk.Windows.App" />
  <trustInfo xmlns="urn:schemas-microsoft-com:asm.v2">
    <security>
      <requestedPrivileges xmlns="urn:schemas-microsoft-com:asm.v3">
        <requestedExecutionLevel level="asInvoker" uiAccess="false" />
      </requestedPrivileges>
    </security>
  </trustInfo>
</assembly>
```

- [ ] **Step 2: Add App XAML**

Create `VoiceInk.Windows/src/VoiceInk.Windows.App/App.xaml`:

```xml
<Application
    x:Class="VoiceInk.Windows.App.App"
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
    <Application.Resources>
    </Application.Resources>
</Application>
```

- [ ] **Step 3: Add App code-behind**

Create `VoiceInk.Windows/src/VoiceInk.Windows.App/App.xaml.cs`:

```csharp
using Microsoft.UI.Xaml;

namespace VoiceInk.Windows.App;

public partial class App : Application
{
    private Window? window;

    public App()
    {
        InitializeComponent();
    }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        window = new MainWindow();
        window.Activate();
    }
}
```

- [ ] **Step 4: Add MainWindow XAML**

Create `VoiceInk.Windows/src/VoiceInk.Windows.App/MainWindow.xaml`:

```xml
<Window
    x:Class="VoiceInk.Windows.App.MainWindow"
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    Title="VoiceInk for Windows">

    <Grid Padding="24" RowSpacing="16">
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto" />
            <RowDefinition Height="Auto" />
            <RowDefinition Height="Auto" />
            <RowDefinition Height="Auto" />
        </Grid.RowDefinitions>

        <TextBlock
            Grid.Row="0"
            Text="VoiceInk for Windows"
            FontSize="24"
            FontWeight="SemiBold" />

        <TextBox
            x:Name="ModelPathTextBox"
            Grid.Row="1"
            Header="Local whisper model path"
            PlaceholderText="C:\Models\ggml-base.en.bin" />

        <StackPanel Grid.Row="2" Orientation="Horizontal" Spacing="8">
            <Button x:Name="StartButton" Content="Start Recording" Click="StartButton_Click" />
            <Button x:Name="StopButton" Content="Stop And Insert" Click="StopButton_Click" IsEnabled="False" />
        </StackPanel>

        <TextBlock
            x:Name="StatusTextBlock"
            Grid.Row="3"
            Text="Idle"
            TextWrapping="Wrap" />
    </Grid>
</Window>
```

- [ ] **Step 5: Add MainWindow composition code**

Create `VoiceInk.Windows/src/VoiceInk.Windows.App/MainWindow.xaml.cs`:

```csharp
using Microsoft.UI.Xaml;
using VoiceInk.Windows.Core.Dictation;
using VoiceInk.Windows.Core.Settings;
using VoiceInk.Windows.Infrastructure.History;
using VoiceInk.Windows.Infrastructure.Settings;
using VoiceInk.Windows.Native.Audio;
using VoiceInk.Windows.Native.Text;
using VoiceInk.Windows.Native.Transcription;

namespace VoiceInk.Windows.App;

public sealed partial class MainWindow : Window
{
    private readonly JsonSettingsStore settingsStore;
    private readonly DictationController controller;

    public MainWindow()
    {
        InitializeComponent();

        var appData = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "VoiceInk.Windows");
        var recordings = Path.Combine(appData, "Recordings");

        settingsStore = new JsonSettingsStore(Path.Combine(appData, "settings.json"));
        controller = new DictationController(
            new NAudioCaptureService(recordings),
            new WhisperNetTranscriptionService(),
            new ClipboardTextInjectionService(restoreClipboard: true),
            new SqliteHistoryStore(Path.Combine(appData, "history.db")),
            settingsStore);

        _ = LoadSettingsAsync();
    }

    private async void StartButton_Click(object sender, RoutedEventArgs e)
    {
        await SaveSettingsAsync();
        await controller.StartAsync(CancellationToken.None);
        StatusTextBlock.Text = "Recording";
        StartButton.IsEnabled = false;
        StopButton.IsEnabled = true;
    }

    private async void StopButton_Click(object sender, RoutedEventArgs e)
    {
        await controller.StopAsync(CancellationToken.None);
        StatusTextBlock.Text = controller.LastError ?? controller.State.ToString();
        StartButton.IsEnabled = true;
        StopButton.IsEnabled = false;
    }

    private async Task LoadSettingsAsync()
    {
        var settings = await settingsStore.LoadAsync(CancellationToken.None);
        ModelPathTextBox.Text = settings.ModelPath;
    }

    private async Task SaveSettingsAsync()
    {
        var settings = await settingsStore.LoadAsync(CancellationToken.None);
        await settingsStore.SaveAsync(settings with
        {
            ModelPath = ModelPathTextBox.Text
        }, CancellationToken.None);
    }
}
```

- [ ] **Step 6: Build app project**

Run:

```powershell
dotnet build VoiceInk.Windows\src\VoiceInk.Windows.App\VoiceInk.Windows.App.csproj -c Debug -p:Platform=x64
```

Expected: build succeeds.

- [ ] **Step 7: Commit WinUI shell**

```powershell
git add VoiceInk.Windows\src\VoiceInk.Windows.App
git commit -m "feat(windows): add minimal WinUI dictation shell"
```

## Task 10: Add Global Hotkey Service

**Files:**
- Create: `VoiceInk.Windows/src/VoiceInk.Windows.Native/Hotkeys/GlobalHotkeyService.cs`
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.App/MainWindow.xaml.cs`

- [ ] **Step 1: Add hotkey service**

Create `VoiceInk.Windows/src/VoiceInk.Windows.Native/Hotkeys/GlobalHotkeyService.cs`:

```csharp
using System.Runtime.InteropServices;

namespace VoiceInk.Windows.Native.Hotkeys;

public sealed class GlobalHotkeyService : IDisposable
{
    public event EventHandler? HotkeyPressed;

    private const int HotkeyId = 0x5649;
    private const int WmHotkey = 0x0312;
    private const uint ModControl = 0x0002;
    private const uint ModAlt = 0x0001;
    private const uint VirtualKeySpace = 0x20;

    private readonly nint windowHandle;
    private readonly HotkeyNativeWindow nativeWindow;

    public GlobalHotkeyService(nint windowHandle)
    {
        this.windowHandle = windowHandle;
        nativeWindow = new HotkeyNativeWindow(windowHandle);
        nativeWindow.HotkeyPressed += (_, _) => HotkeyPressed?.Invoke(this, EventArgs.Empty);
    }

    public void RegisterCtrlAltSpace()
    {
        if (!RegisterHotKey(windowHandle, HotkeyId, ModControl | ModAlt, VirtualKeySpace))
        {
            throw new InvalidOperationException("Ctrl+Alt+Space is already registered by another application.");
        }
    }

    public void Dispose()
    {
        UnregisterHotKey(windowHandle, HotkeyId);
        nativeWindow.Dispose();
    }

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool RegisterHotKey(nint hWnd, int id, uint fsModifiers, uint vk);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool UnregisterHotKey(nint hWnd, int id);

    private sealed class HotkeyNativeWindow : System.Windows.Forms.NativeWindow, IDisposable
    {
        public event EventHandler? HotkeyPressed;

        public HotkeyNativeWindow(nint handle)
        {
            AssignHandle(handle);
        }

        protected override void WndProc(ref System.Windows.Forms.Message m)
        {
            if (m.Msg == WmHotkey && m.WParam.ToInt32() == HotkeyId)
            {
                HotkeyPressed?.Invoke(this, EventArgs.Empty);
            }

            base.WndProc(ref m);
        }

        public void Dispose()
        {
            ReleaseHandle();
        }
    }
}
```

- [ ] **Step 2: Wire hotkey registration into MainWindow**

Modify `VoiceInk.Windows/src/VoiceInk.Windows.App/MainWindow.xaml.cs` by adding these `using` statements:

```csharp
using WinRT.Interop;
using VoiceInk.Windows.Native.Hotkeys;
```

Add this field:

```csharp
private GlobalHotkeyService? hotkeyService;
```

At the end of the constructor, after `_ = LoadSettingsAsync();`, add:

```csharp
var handle = WindowNative.GetWindowHandle(this);
hotkeyService = new GlobalHotkeyService(handle);
hotkeyService.HotkeyPressed += async (_, _) => await ToggleRecordingAsync();
try
{
    hotkeyService.RegisterCtrlAltSpace();
}
catch (InvalidOperationException ex)
{
    StatusTextBlock.Text = ex.Message;
}
```

Add this method to `MainWindow`:

```csharp
private async Task ToggleRecordingAsync()
{
    if (controller.State == DictationState.Recording)
    {
        await StopCurrentRecordingAsync();
    }
    else
    {
        await StartCurrentRecordingAsync();
    }
}
```

Replace `StartButton_Click` and `StopButton_Click` with:

```csharp
private async void StartButton_Click(object sender, RoutedEventArgs e)
{
    await StartCurrentRecordingAsync();
}

private async void StopButton_Click(object sender, RoutedEventArgs e)
{
    await StopCurrentRecordingAsync();
}
```

Add these helper methods:

```csharp
private async Task StartCurrentRecordingAsync()
{
    await SaveSettingsAsync();
    await controller.StartAsync(CancellationToken.None);
    if (controller.State == DictationState.Error)
    {
        StatusTextBlock.Text = controller.LastError ?? "Unable to start recording";
        StartButton.IsEnabled = true;
        StopButton.IsEnabled = false;
        return;
    }

    StatusTextBlock.Text = "Recording";
    StartButton.IsEnabled = false;
    StopButton.IsEnabled = true;
}

private async Task StopCurrentRecordingAsync()
{
    await controller.StopAsync(CancellationToken.None);
    StatusTextBlock.Text = controller.LastError ?? controller.State.ToString();
    StartButton.IsEnabled = true;
    StopButton.IsEnabled = false;
}
```

- [ ] **Step 3: Build app project**

Run:

```powershell
dotnet build VoiceInk.Windows\src\VoiceInk.Windows.App\VoiceInk.Windows.App.csproj -c Debug -p:Platform=x64
```

Expected: build succeeds.

- [ ] **Step 4: Commit hotkey seam**

```powershell
git add VoiceInk.Windows\src\VoiceInk.Windows.Native\Hotkeys VoiceInk.Windows\src\VoiceInk.Windows.App\MainWindow.xaml.cs
git commit -m "feat(windows): add global hotkey registration"
```

## Task 11: Verify MVP From Source

**Files:**
- Modify: `README.md`

- [ ] **Step 1: Add Windows development notes**

Append to `README.md`:

````markdown
## Windows Fork Development

The Windows implementation lives under `VoiceInk.Windows/` and is separate from the macOS SwiftUI app.

### Requirements

- Windows 11
- .NET 10 SDK
- Visual Studio with Windows App SDK support, or equivalent Build Tools
- A local whisper.cpp-compatible `.bin` model file

### Build

```powershell
dotnet restore VoiceInk.Windows\VoiceInk.Windows.sln
dotnet build VoiceInk.Windows\VoiceInk.Windows.sln -c Debug -p:Platform=x64
```

### Run

```powershell
dotnet run --project VoiceInk.Windows\src\VoiceInk.Windows.App\VoiceInk.Windows.App.csproj -c Debug -p:Platform=x64
```

After launch, enter a local whisper model path, click `Start Recording`, speak, then click `Stop And Insert`.
````

- [ ] **Step 2: Run unit and integration tests**

Run:

```powershell
dotnet test VoiceInk.Windows\VoiceInk.Windows.sln
```

Expected: all tests pass.

- [ ] **Step 3: Build full solution**

Run:

```powershell
dotnet build VoiceInk.Windows\VoiceInk.Windows.sln -c Debug -p:Platform=x64
```

Expected: build succeeds.

- [ ] **Step 4: Manual smoke test**

Run:

```powershell
dotnet run --project VoiceInk.Windows\src\VoiceInk.Windows.App\VoiceInk.Windows.App.csproj -c Debug -p:Platform=x64
```

Manual expected result:

- The window opens.
- Enter a valid local whisper model path.
- Open Notepad and place the caret in an empty document.
- Return to VoiceInk for Windows.
- Click `Start Recording`.
- Speak a short sentence.
- Click `Stop And Insert`.
- The transcribed text appears in Notepad.
- A row exists in `%LOCALAPPDATA%\VoiceInk.Windows\history.db`.

- [ ] **Step 5: Commit verification docs**

```powershell
git add README.md
git commit -m "docs: add Windows MVP development notes"
```

## Plan Self-Review

Spec coverage:

- Core Windows solution under `VoiceInk.Windows/`: Task 1.
- WinUI 3 desktop shell: Task 9.
- Basic settings: Task 4 and Task 9.
- Microphone capture: Task 7.
- Local whisper.cpp transcription: Task 6 through Whisper.net over whisper.cpp.
- Insert text into active app: Task 8.
- SQLite history: Task 5.
- Post-processing: Task 2.
- Focused tests: Tasks 2, 3, 4, and 5.

Intentional gaps for follow-on plans:

- Cloud transcription providers.
- AI text enhancement.
- Installer packaging.
- Full tray-first behavior.
- True press-and-hold hotkey behavior.

Red-flag scan: no deferred-work markers are present.

Type consistency:

- `AudioCaptureResult`, `TranscriptionOptions`, `TranscriptionResult`, `TranscriptionHistoryItem`, `AppSettings`, and service interface names match across tests and implementation steps.
- `DictationController` constructor parameter order is consistent in tests and app composition.
