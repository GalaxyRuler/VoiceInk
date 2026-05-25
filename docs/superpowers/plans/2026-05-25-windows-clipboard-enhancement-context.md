# Windows Clipboard Enhancement Context Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add macOS-style clipboard context to Windows AI enhancement as a default-off, local-only setting that enriches enhancement prompts without modifying clipboard contents.

**Architecture:** Core owns the setting, context model, prompt rendering, and enhancement-pipeline orchestration behind a testable context-provider interface. Native Windows owns the clipboard read adapter. WinUI exposes the setting in the existing Enhancement section and persists it through JSON settings.

**Tech Stack:** .NET 10, WinUI 3, xUnit, JSON settings, Windows desktop clipboard APIs, existing `TextEnhancementPipeline`.

**Grounding:**

- macOS source of truth: `VoiceInk/Services/AIEnhancement/AIEnhancementService.swift` persists `useClipboardContext`, captures `NSPasteboard.general.string(forType: .string)`, and appends `<CLIPBOARD_CONTEXT>` to the system message.
- macOS recording flow: `VoiceInk/Transcription/Engine/VoiceInkEngine.swift` captures clipboard context when recording starts before enhancement runs.
- Windows grounding: Microsoft Learn documents reading clipboard text through `Clipboard.GetContent()`, `StandardDataFormats.Text`, and `GetTextAsync()`, with empty/unavailable clipboard represented as an empty data package.
- Open-source adaptation: the Windows fork adds only a local toggle and read-only capture. It does not add cloud telemetry, commercial context services, paid OCR, or account flows.

---

## Files

- Create `VoiceInk.Windows/src/VoiceInk.Windows.Core/Enhancement/EnhancementContext.cs`.
- Create `VoiceInk.Windows/src/VoiceInk.Windows.Core/Enhancement/IEnhancementContextProvider.cs`.
- Create `VoiceInk.Windows/src/VoiceInk.Windows.Core/Enhancement/EmptyEnhancementContextProvider.cs`.
- Create `VoiceInk.Windows/src/VoiceInk.Windows.Native/Text/ClipboardEnhancementContextProvider.cs`.
- Modify `VoiceInk.Windows/src/VoiceInk.Windows.Core/Settings/AppSettings.cs`.
- Modify `VoiceInk.Windows/src/VoiceInk.Windows.Core/Enhancement/EnhancementPromptRenderer.cs`.
- Modify `VoiceInk.Windows/src/VoiceInk.Windows.Core/Enhancement/TextEnhancementPipeline.cs`.
- Modify `VoiceInk.Windows/src/VoiceInk.Windows.App/MainWindow.xaml`.
- Modify `VoiceInk.Windows/src/VoiceInk.Windows.App/MainWindow.xaml.cs`.
- Modify `VoiceInk.Windows/tests/VoiceInk.Windows.Core.Tests/Enhancement/EnhancementPromptTests.cs`.
- Modify `VoiceInk.Windows/tests/VoiceInk.Windows.Core.Tests/Enhancement/TextEnhancementPipelineTests.cs`.
- Modify `VoiceInk.Windows/tests/VoiceInk.Windows.Infrastructure.Tests/Settings/JsonSettingsStoreTests.cs`.
- Modify `README.md` and `docs/superpowers/specs/2026-05-24-windows-open-source-parity-design.md`.

## Task 1: Planning Docs

- [x] **Step 1: Update parity spec**

Add a Clipboard enhancement context MVP target under the Context section:

```markdown
Clipboard enhancement context Windows MVP target:

- Add a default-off `UseClipboardContext` setting matching macOS `useClipboardContext`.
- Capture current plain-text clipboard content only when enhancement is about to run, never modifying clipboard contents.
- Add captured text to the enhancement system message inside `<CLIPBOARD_CONTEXT>` tags, matching the macOS prompt contract.
- Gracefully skip context when the clipboard is empty, unavailable, non-text, or cannot be read.
- Keep context capture local to the enhancement request and persist only the rendered AI request messages already saved for local History diagnostics.
- Leave selected-text context, screen/OCR context, and browser URL context for later Windows context slices.
```

- [x] **Step 2: Commit planning docs**

Run:

```powershell
git add docs\superpowers\plans\2026-05-25-windows-clipboard-enhancement-context.md docs\superpowers\specs\2026-05-24-windows-open-source-parity-design.md
git diff --check --cached
git commit -m "docs(windows): plan clipboard enhancement context"
```

Expected: docs-only commit with no whitespace errors.

## Task 2: Prompt Rendering Red/Green

- [x] **Step 1: Add failing prompt-renderer test**

Add this test to `VoiceInk.Windows/tests/VoiceInk.Windows.Core.Tests/Enhancement/EnhancementPromptTests.cs`:

```csharp
[Fact]
public void Render_AppendsClipboardContext()
{
    var prompt = EnhancementPromptCatalog.CreateDefaultPrompts()
        .Single(item => item.Id == EnhancementPromptCatalog.DefaultPromptId);

    var rendered = EnhancementPromptRenderer.Render(
        prompt,
        "send the note",
        vocabulary: [],
        context: new EnhancementContext(ClipboardText: "Project Zephyr release notes"));

    Assert.Contains("<CLIPBOARD_CONTEXT>", rendered.SystemMessage);
    Assert.Contains("Project Zephyr release notes", rendered.SystemMessage);
    Assert.Contains("</CLIPBOARD_CONTEXT>", rendered.SystemMessage);
}
```

- [x] **Step 2: Run red prompt-renderer test**

Run:

```powershell
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" test VoiceInk.Windows\tests\VoiceInk.Windows.Core.Tests\VoiceInk.Windows.Core.Tests.csproj --filter "FullyQualifiedName~EnhancementPromptTests.Render_AppendsClipboardContext"
```

Expected: compile failure because `EnhancementContext` and the renderer overload do not exist.

- [x] **Step 3: Implement minimal prompt context model and rendering**

Create:

```csharp
namespace VoiceInk.Windows.Core.Enhancement;

public sealed record EnhancementContext(string ClipboardText)
{
    public static EnhancementContext Empty { get; } = new(string.Empty);
}
```

Change `EnhancementPromptRenderer.Render` to accept `EnhancementContext? context = null`, call a private `ClipboardContextSection(context)`, and append that section before `VocabularySection(vocabulary)`:

```csharp
private static string ClipboardContextSection(EnhancementContext? context)
{
    var clipboardText = context?.ClipboardText.Trim();
    if (string.IsNullOrEmpty(clipboardText))
    {
        return string.Empty;
    }

    return $"""


        <CLIPBOARD_CONTEXT>
        {clipboardText}
        </CLIPBOARD_CONTEXT>
        """;
}
```

- [x] **Step 4: Verify prompt-renderer test passes**

Run the same focused command. Expected: test passes.

## Task 3: Enhancement Pipeline Context Provider Red/Green

- [x] **Step 1: Add failing pipeline tests**

Extend `VoiceInk.Windows/tests/VoiceInk.Windows.Core.Tests/Enhancement/TextEnhancementPipelineTests.cs` with:

```csharp
[Fact]
public async Task EnhanceAsync_IncludesClipboardContextWhenEnabled()
{
    var provider = new FakeTextEnhancementService("Enhanced note.");
    var contextProvider = new FakeEnhancementContextProvider(new EnhancementContext("Clipboard note"));
    var pipeline = new TextEnhancementPipeline(provider, contextProvider: contextProvider);
    var settings = ConfiguredSettings() with
    {
        IsEnhancementEnabled = true,
        UseClipboardContext = true
    };

    await pipeline.EnhanceAsync("clean this", settings, [], CancellationToken.None);

    Assert.Equal(1, contextProvider.CallCount);
    Assert.Contains("<CLIPBOARD_CONTEXT>", provider.LastRequest!.SystemMessage);
    Assert.Contains("Clipboard note", provider.LastRequest.SystemMessage);
}

[Fact]
public async Task EnhanceAsync_WhenClipboardContextProviderFails_EnhancesWithoutContext()
{
    var provider = new FakeTextEnhancementService("Enhanced note.");
    var contextProvider = new FakeEnhancementContextProvider(EnhancementContext.Empty)
    {
        Exception = new InvalidOperationException("clipboard unavailable")
    };
    var pipeline = new TextEnhancementPipeline(provider, contextProvider: contextProvider);
    var settings = ConfiguredSettings() with
    {
        IsEnhancementEnabled = true,
        UseClipboardContext = true
    };

    var result = await pipeline.EnhanceAsync("clean this", settings, [], CancellationToken.None);

    Assert.True(result.AttemptedEnhancement);
    Assert.Equal("Enhanced note.", result.FinalText);
    Assert.DoesNotContain("<CLIPBOARD_CONTEXT>", provider.LastRequest!.SystemMessage);
}
```

Add a test fake:

```csharp
private sealed class FakeEnhancementContextProvider(EnhancementContext context) : IEnhancementContextProvider
{
    public int CallCount { get; private set; }
    public Exception? Exception { get; init; }

    public Task<EnhancementContext> GetContextAsync(CancellationToken cancellationToken)
    {
        CallCount++;
        if (Exception is not null)
        {
            throw Exception;
        }

        return Task.FromResult(context);
    }
}
```

- [x] **Step 2: Run red pipeline tests**

Run:

```powershell
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" test VoiceInk.Windows\tests\VoiceInk.Windows.Core.Tests\VoiceInk.Windows.Core.Tests.csproj --filter "FullyQualifiedName~TextEnhancementPipelineTests.EnhanceAsync_IncludesClipboardContextWhenEnabled|FullyQualifiedName~TextEnhancementPipelineTests.EnhanceAsync_WhenClipboardContextProviderFails_EnhancesWithoutContext"
```

Expected: compile failure because `IEnhancementContextProvider`, `UseClipboardContext`, and constructor wiring do not exist.

- [x] **Step 3: Implement Core context-provider wiring**

Create:

```csharp
namespace VoiceInk.Windows.Core.Enhancement;

public interface IEnhancementContextProvider
{
    Task<EnhancementContext> GetContextAsync(CancellationToken cancellationToken);
}
```

Create:

```csharp
namespace VoiceInk.Windows.Core.Enhancement;

public sealed class EmptyEnhancementContextProvider : IEnhancementContextProvider
{
    public Task<EnhancementContext> GetContextAsync(CancellationToken cancellationToken) =>
        Task.FromResult(EnhancementContext.Empty);
}
```

Add `public bool UseClipboardContext { get; init; }` to `AppSettings`, equality, and hashing.

Change `TextEnhancementPipeline` so its constructor accepts optional `IEnhancementContextProvider? contextProvider = null`, defaults to `EmptyEnhancementContextProvider`, and fetches context only after enhancement is enabled/configured/not short-skipped:

```csharp
var context = settings.UseClipboardContext
    ? await GetContextAsync(cancellationToken)
    : EnhancementContext.Empty;
var rendered = EnhancementPromptRenderer.Render(prompt, detection.ProcessedText, vocabulary, context);
```

Add:

```csharp
private async Task<EnhancementContext> GetContextAsync(CancellationToken cancellationToken)
{
    try
    {
        return await contextProvider.GetContextAsync(cancellationToken);
    }
    catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
    {
        throw;
    }
    catch
    {
        return EnhancementContext.Empty;
    }
}
```

- [x] **Step 4: Verify pipeline tests pass**

Run the same focused command. Expected: tests pass.

## Task 4: Settings Persistence Red/Green

- [x] **Step 1: Add failing settings test update**

In `JsonSettingsStoreTests.SaveAsync_PersistsSettings`, set:

```csharp
UseClipboardContext = true,
```

- [x] **Step 2: Run red settings test**

Run:

```powershell
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" test VoiceInk.Windows\tests\VoiceInk.Windows.Infrastructure.Tests\VoiceInk.Windows.Infrastructure.Tests.csproj --filter "FullyQualifiedName~JsonSettingsStoreTests.SaveAsync_PersistsSettings"
```

Expected: compile failure until `AppSettings.UseClipboardContext` exists, then pass through JSON serialization with no custom converter.

- [x] **Step 3: Verify settings test passes**

Run the same focused command. Expected: test passes after Task 3.

## Task 5: Native Clipboard Adapter And WinUI Wiring

- [x] **Step 1: Add native read-only clipboard context provider**

Create `VoiceInk.Windows/src/VoiceInk.Windows.Native/Text/ClipboardEnhancementContextProvider.cs`:

```csharp
using Windows.ApplicationModel.DataTransfer;
using VoiceInk.Windows.Core.Enhancement;

namespace VoiceInk.Windows.Native.Text;

public sealed class ClipboardEnhancementContextProvider(int maxCharacters = 4_000) : IEnhancementContextProvider
{
    public async Task<EnhancementContext> GetContextAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        try
        {
            var data = Clipboard.GetContent();
            if (!data.Contains(StandardDataFormats.Text))
            {
                return EnhancementContext.Empty;
            }

            var text = (await data.GetTextAsync()).Trim();
            if (text.Length == 0)
            {
                return EnhancementContext.Empty;
            }

            return new EnhancementContext(text.Length <= maxCharacters ? text : text[..maxCharacters]);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch
        {
            return EnhancementContext.Empty;
        }
    }
}
```

- [x] **Step 2: Wire provider into app composition**

In `MainWindow.xaml.cs`, construct:

```csharp
textEnhancementPipeline = new TextEnhancementPipeline(
    textEnhancementService,
    enhancementPrompts,
    new ClipboardEnhancementContextProvider());
```

- [x] **Step 3: Add Enhancement toggle to WinUI**

In `MainWindow.xaml`, add under `EnhancementEnabledCheckBox`:

```xml
<CheckBox
    x:Name="UseClipboardContextCheckBox"
    Content="Clipboard Context" />
```

- [x] **Step 4: Load and save setting**

When settings load, set:

```csharp
UseClipboardContextCheckBox.IsChecked = settings.UseClipboardContext;
```

In `CurrentSettingsAsync`, set:

```csharp
UseClipboardContext = UseClipboardContextCheckBox.IsChecked == true,
```

In `RefreshUiFromControllerState`, set:

```csharp
UseClipboardContextCheckBox.IsEnabled = enhancementControlsEnabled;
```

- [x] **Step 5: Verify app build**

Run:

```powershell
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" build VoiceInk.Windows\VoiceInk.Windows.sln -c Debug -p:Platform=x64
```

Expected: build succeeds with zero errors.

## Task 6: Docs, Review, And Commit

- [x] **Step 1: Update README and parity spec**

Document:

- Enhancement can optionally include clipboard text as `<CLIPBOARD_CONTEXT>`.
- Clipboard context is default-off, local-only, read-only, and skips empty/unavailable clipboard.
- Selected-text, OCR/screen, browser URL, and richer context status remain later context slices.

- [x] **Step 2: Run focused and full verification**

Run:

```powershell
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" test VoiceInk.Windows\VoiceInk.Windows.sln --filter "FullyQualifiedName~EnhancementPromptTests|FullyQualifiedName~TextEnhancementPipelineTests|FullyQualifiedName~JsonSettingsStoreTests"
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" test VoiceInk.Windows\VoiceInk.Windows.sln
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" build VoiceInk.Windows\VoiceInk.Windows.sln -c Debug -p:Platform=x64
```

Expected: focused tests pass, full tests pass, build succeeds.

- [x] **Step 3: Request review and fix Critical/Important findings**

Ask a subagent to review privacy behavior, clipboard-read failure handling, prompt rendering, app setting persistence, and UI wiring.

- [ ] **Step 4: Commit implementation**

Run:

```powershell
git add VoiceInk.Windows README.md docs\superpowers\specs\2026-05-24-windows-open-source-parity-design.md docs\superpowers\plans\2026-05-25-windows-clipboard-enhancement-context.md
git diff --check --cached
git commit -m "feat(windows): add clipboard enhancement context"
```

Expected: implementation commit with no whitespace errors.
