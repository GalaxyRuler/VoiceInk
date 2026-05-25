# Windows Selected-Text Enhancement Context Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add macOS-style selected-text context to Windows AI enhancement as a read-only, best-effort context source that never changes the user's clipboard.

**Architecture:** Core expands enhancement context from clipboard-only to selected-text plus clipboard, and the prompt renderer emits sections in macOS order. Native Windows adds a UI Automation selected-text reader and a Windows context aggregator that reads selected text automatically and clipboard only when enabled. WinUI keeps the existing Clipboard Context toggle; selected text is automatic and best-effort like macOS.

**Tech Stack:** .NET 10, WinUI 3, xUnit, Windows UI Automation (`System.Windows.Automation`), existing `TextEnhancementPipeline`.

**Grounding:**

- macOS source of truth: `VoiceInk/Services/SelectedTextService.swift` uses SelectedTextKit strategies `.accessibility` and `.menuAction`; `AIEnhancementService.swift` appends selected text inside `<CURRENTLY_SELECTED_TEXT>` when Accessibility is trusted and selected text is available.
- Windows grounding: Microsoft UI Automation exposes text selection through `AutomationElement.FocusedElement`, `TextPattern`, `TextPattern.GetSelection()`, and `TextPatternRange.GetText(maxLength)`.
- Open-source/privacy adaptation: this slice does not use clipboard-copy fallback for selected text because that would temporarily mutate clipboard contents. It uses read-only UI Automation and degrades to no selected-text context when unsupported.

---

## Files

- Create `VoiceInk.Windows/src/VoiceInk.Windows.Core/Enhancement/EnhancementContextRequest.cs`.
- Modify `VoiceInk.Windows/src/VoiceInk.Windows.Core/Enhancement/EnhancementContext.cs`.
- Modify `VoiceInk.Windows/src/VoiceInk.Windows.Core/Enhancement/IEnhancementContextProvider.cs`.
- Modify `VoiceInk.Windows/src/VoiceInk.Windows.Core/Enhancement/EmptyEnhancementContextProvider.cs`.
- Modify `VoiceInk.Windows/src/VoiceInk.Windows.Core/Enhancement/EnhancementPromptRenderer.cs`.
- Modify `VoiceInk.Windows/src/VoiceInk.Windows.Core/Enhancement/TextEnhancementPipeline.cs`.
- Modify `VoiceInk.Windows/src/VoiceInk.Windows.Native/VoiceInk.Windows.Native.csproj` if UI Automation references are needed.
- Modify `VoiceInk.Windows/src/VoiceInk.Windows.Native/Text/ClipboardEnhancementContextProvider.cs`.
- Create `VoiceInk.Windows/src/VoiceInk.Windows.Native/Text/SelectedTextEnhancementContextProvider.cs`.
- Create `VoiceInk.Windows/src/VoiceInk.Windows.Native/Text/WindowsEnhancementContextProvider.cs`.
- Modify `VoiceInk.Windows/src/VoiceInk.Windows.App/MainWindow.xaml.cs`.
- Modify `VoiceInk.Windows/tests/VoiceInk.Windows.Core.Tests/Enhancement/EnhancementPromptTests.cs`.
- Modify `VoiceInk.Windows/tests/VoiceInk.Windows.Core.Tests/Enhancement/TextEnhancementPipelineTests.cs`.
- Modify `README.md` and `docs/superpowers/specs/2026-05-24-windows-open-source-parity-design.md`.

## Task 1: Planning Docs

- [x] **Step 1: Update parity spec**

Add the selected-text context MVP target under Context.

- [ ] **Step 2: Commit planning docs**

Run:

```powershell
git add docs\superpowers\plans\2026-05-25-windows-selected-text-enhancement-context.md docs\superpowers\specs\2026-05-24-windows-open-source-parity-design.md
git diff --check --cached
git commit -m "docs(windows): plan selected text enhancement context"
```

Expected: docs-only commit with no whitespace errors.

## Task 2: Prompt Rendering Red/Green

- [ ] **Step 1: Add failing prompt-renderer test**

Add this test to `EnhancementPromptTests`:

```csharp
[Fact]
public void Render_AppendsSelectedTextBeforeClipboardAndVocabulary()
{
    var prompt = EnhancementPromptCatalog.CreateDefaultPrompts()
        .Single(item => item.Id == EnhancementPromptCatalog.DefaultPromptId);
    var vocabulary = new[]
    {
        new VocabularyWord(Guid.NewGuid(), "VoiceInk", DateTimeOffset.UtcNow)
    };

    var rendered = EnhancementPromptRenderer.Render(
        prompt,
        "fix this",
        vocabulary,
        new EnhancementContext(
            ClipboardText: "clipboard note",
            SelectedText: "selected note"));

    Assert.Contains("<CURRENTLY_SELECTED_TEXT>", rendered.SystemMessage);
    Assert.Contains("selected note", rendered.SystemMessage);
    Assert.Contains("</CURRENTLY_SELECTED_TEXT>", rendered.SystemMessage);
    Assert.True(
        rendered.SystemMessage.IndexOf("<CURRENTLY_SELECTED_TEXT>", StringComparison.Ordinal)
        < rendered.SystemMessage.IndexOf("<CLIPBOARD_CONTEXT>", StringComparison.Ordinal));
    Assert.True(
        rendered.SystemMessage.IndexOf("<CLIPBOARD_CONTEXT>", StringComparison.Ordinal)
        < rendered.SystemMessage.IndexOf("<CUSTOM_VOCABULARY>", StringComparison.Ordinal));
}
```

- [ ] **Step 2: Run red prompt-renderer test**

Run:

```powershell
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" test VoiceInk.Windows\tests\VoiceInk.Windows.Core.Tests\VoiceInk.Windows.Core.Tests.csproj --filter "FullyQualifiedName~EnhancementPromptTests.Render_AppendsSelectedTextBeforeClipboardAndVocabulary"
```

Expected: compile failure because `EnhancementContext.SelectedText` does not exist.

- [ ] **Step 3: Implement selected-text prompt rendering**

Change `EnhancementContext` to:

```csharp
public sealed record EnhancementContext(
    string ClipboardText = "",
    string SelectedText = "")
{
    public static EnhancementContext Empty { get; } = new();
}
```

Add `SelectedTextContextSection` to `EnhancementPromptRenderer` and append it before `ClipboardContextSection(context)`.

- [ ] **Step 4: Verify prompt-renderer test passes**

Run the same focused command. Expected: test passes.

## Task 3: Pipeline Context Request Red/Green

- [ ] **Step 1: Add failing pipeline tests**

Add tests to `TextEnhancementPipelineTests`:

```csharp
[Fact]
public async Task EnhanceAsync_IncludesSelectedTextContextWhenEnhancementRuns()
{
    var provider = new FakeTextEnhancementService("Enhanced note.");
    var contextProvider = new FakeEnhancementContextProvider(new EnhancementContext(SelectedText: "Selected note"));
    var pipeline = new TextEnhancementPipeline(provider, contextProvider: contextProvider);
    var settings = ConfiguredSettings() with { IsEnhancementEnabled = true };

    await pipeline.EnhanceAsync("clean this", settings, [], CancellationToken.None);

    Assert.Equal(new EnhancementContextRequest(IncludeClipboard: false, IncludeSelectedText: true), contextProvider.LastRequest);
    Assert.Contains("<CURRENTLY_SELECTED_TEXT>", provider.LastRequest!.SystemMessage);
    Assert.Contains("Selected note", provider.LastRequest.SystemMessage);
}

[Fact]
public async Task EnhanceAsync_RequestsClipboardAndSelectedTextWhenClipboardContextEnabled()
{
    var provider = new FakeTextEnhancementService("Enhanced note.");
    var contextProvider = new FakeEnhancementContextProvider(new EnhancementContext("Clipboard note", "Selected note"));
    var pipeline = new TextEnhancementPipeline(provider, contextProvider: contextProvider);
    var settings = ConfiguredSettings() with
    {
        IsEnhancementEnabled = true,
        UseClipboardContext = true
    };

    await pipeline.EnhanceAsync("clean this", settings, [], CancellationToken.None);

    Assert.Equal(new EnhancementContextRequest(IncludeClipboard: true, IncludeSelectedText: true), contextProvider.LastRequest);
    Assert.Contains("<CURRENTLY_SELECTED_TEXT>", provider.LastRequest!.SystemMessage);
    Assert.Contains("<CLIPBOARD_CONTEXT>", provider.LastRequest.SystemMessage);
}
```

Update `FakeEnhancementContextProvider` to accept `EnhancementContextRequest`.

- [ ] **Step 2: Run red pipeline tests**

Run:

```powershell
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" test VoiceInk.Windows\tests\VoiceInk.Windows.Core.Tests\VoiceInk.Windows.Core.Tests.csproj --filter "FullyQualifiedName~TextEnhancementPipelineTests.EnhanceAsync_IncludesSelectedTextContextWhenEnhancementRuns|FullyQualifiedName~TextEnhancementPipelineTests.EnhanceAsync_RequestsClipboardAndSelectedTextWhenClipboardContextEnabled"
```

Expected: compile failure because `EnhancementContextRequest` and the new provider signature do not exist.

- [ ] **Step 3: Implement Core context request flow**

Create:

```csharp
public sealed record EnhancementContextRequest(bool IncludeClipboard, bool IncludeSelectedText);
```

Change `IEnhancementContextProvider.GetContextAsync` to take `EnhancementContextRequest request`. Change `TextEnhancementPipeline` to call the context provider after enhancement eligibility is confirmed with:

```csharp
var rendered = EnhancementPromptRenderer.Render(
    prompt,
    detection.ProcessedText,
    vocabulary,
    await GetContextAsync(
        new EnhancementContextRequest(settings.UseClipboardContext, IncludeSelectedText: true),
        cancellationToken));
```

- [ ] **Step 4: Verify pipeline tests pass**

Run the same focused command. Expected: tests pass.

## Task 4: Native UI Automation Provider

- [ ] **Step 1: Add UI Automation references if required**

If the build cannot resolve `System.Windows.Automation`, add framework references to `VoiceInk.Windows.Native.csproj`:

```xml
<Reference Include="UIAutomationClient" />
<Reference Include="UIAutomationTypes" />
```

- [ ] **Step 2: Refactor clipboard provider to request-aware helper**

Change `ClipboardEnhancementContextProvider` so it exposes:

```csharp
public async Task<string> GetClipboardTextAsync(CancellationToken cancellationToken)
```

It should keep the same read-only WinRT clipboard behavior and cancellation-aware `AsTask(cancellationToken)`.

- [ ] **Step 3: Add selected-text provider**

Create `SelectedTextEnhancementContextProvider` using `AutomationElement.FocusedElement`, `TextPattern.Pattern`, `GetSelection()`, and `GetText(maxCharacters + 1)`. Catch unsupported/unavailable UI Automation exceptions and return empty text.

- [ ] **Step 4: Add Windows context aggregator**

Create `WindowsEnhancementContextProvider` implementing `IEnhancementContextProvider`. It reads selected text when `request.IncludeSelectedText` is true, reads clipboard when `request.IncludeClipboard` is true, catches failures per source, and returns a combined `EnhancementContext`.

- [ ] **Step 5: Wire app composition**

In `MainWindow.xaml.cs`, replace `new ClipboardEnhancementContextProvider()` with `new WindowsEnhancementContextProvider()`.

- [ ] **Step 6: Verify build**

Run:

```powershell
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" build VoiceInk.Windows\VoiceInk.Windows.sln -c Debug -p:Platform=x64
```

Expected: build succeeds with zero errors.

## Task 5: Docs, Review, And Commit

- [ ] **Step 1: Update README and parity spec**

Document:

- Selected text is read best-effort through UI Automation when enhancement runs.
- It is inserted into `<CURRENTLY_SELECTED_TEXT>` and does not mutate clipboard contents.
- Unsupported controls, unavailable UI Automation, and empty selections degrade to no selected-text context.
- Clipboard-copy fallback, OCR/screen context, and browser URL context remain later.

- [ ] **Step 2: Run focused and full verification**

Run:

```powershell
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" test VoiceInk.Windows\VoiceInk.Windows.sln --filter "FullyQualifiedName~EnhancementPromptTests|FullyQualifiedName~TextEnhancementPipelineTests"
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" test VoiceInk.Windows\VoiceInk.Windows.sln
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" build VoiceInk.Windows\VoiceInk.Windows.sln -c Debug -p:Platform=x64
```

Expected: focused tests pass, full tests pass, build succeeds.

- [ ] **Step 3: Request review and fix Critical/Important findings**

Ask a subagent to review UI Automation provider risk, no clipboard mutation, prompt parity, and context-provider request semantics.

- [ ] **Step 4: Commit implementation**

Run:

```powershell
git add VoiceInk.Windows README.md docs\superpowers\specs\2026-05-24-windows-open-source-parity-design.md docs\superpowers\plans\2026-05-25-windows-selected-text-enhancement-context.md
git diff --check --cached
git commit -m "feat(windows): add selected text enhancement context"
```

Expected: implementation commit with no whitespace errors.
