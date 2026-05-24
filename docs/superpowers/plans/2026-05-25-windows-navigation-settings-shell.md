# Windows Navigation Settings Shell Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Replace the single long Windows control surface with a macOS-aligned WinUI navigation shell that groups existing features into Dashboard, AI Models, Audio Input, Dictionary, History, Settings, and About/Open Source sections.

**Architecture:** Core owns a small navigation item presenter so the macOS information architecture and open-source replacement are testable without WinUI. `MainWindow` uses WinUI `NavigationView` with named section panels instead of page classes, keeping this slice focused and avoiding a risky multi-file UI split. Existing event handlers and persistence remain in place.

**Tech Stack:** .NET 10, WinUI 3 `NavigationView`, xUnit.

---

## Files

- Create `VoiceInk.Windows/src/VoiceInk.Windows.Core/Shell/ShellNavigationItem.cs`.
- Create `VoiceInk.Windows/src/VoiceInk.Windows.Core/Shell/ShellNavigationPresenter.cs`.
- Create `VoiceInk.Windows/tests/VoiceInk.Windows.Core.Tests/Shell/ShellNavigationPresenterTests.cs`.
- Modify `VoiceInk.Windows/src/VoiceInk.Windows.App/MainWindow.xaml`.
- Modify `VoiceInk.Windows/src/VoiceInk.Windows.App/MainWindow.xaml.cs`.
- Modify `README.md`.
- Modify `docs/superpowers/specs/2026-05-24-windows-open-source-parity-design.md`.

## Task 1: Planning Docs

- [ ] **Step 1: Update parity spec**

Record this Windows navigation shell slice in the Shell and Navigation section:

- Use macOS `ContentView.ViewType` order as the source of truth.
- Active Windows sections for this slice: Dashboard, AI Models, Audio Input, Dictionary, History, Settings, About/Open Source.
- Replace the commercial VoiceInk Pro section with About/Open Source.
- Keep Transcribe Audio, Enhancement, Power Mode, and Permissions as later slices unless they already have real Windows controls.
- Use WinUI `NavigationView` as the Windows-native adaptation of macOS `NavigationSplitView`.

- [ ] **Step 2: Commit planning docs**

Run:

```powershell
git add docs\superpowers\plans\2026-05-25-windows-navigation-settings-shell.md docs\superpowers\specs\2026-05-24-windows-open-source-parity-design.md
git commit -m "docs(windows): plan navigation shell"
```

Expected: docs-only commit.

## Task 2: Red Tests

- [ ] **Step 1: Add navigation presenter tests**

Create `VoiceInk.Windows/tests/VoiceInk.Windows.Core.Tests/Shell/ShellNavigationPresenterTests.cs`:

```csharp
using VoiceInk.Windows.Core.Shell;
using Xunit;

namespace VoiceInk.Windows.Core.Tests.Shell;

public sealed class ShellNavigationPresenterTests
{
    [Fact]
    public void BuildItems_ReturnsImplementedWindowsSectionsInMacOrder()
    {
        var items = ShellNavigationPresenter.BuildItems();

        Assert.Collection(
            items,
            item => Assert.Equal(("Dashboard", "Dashboard", true), (item.Tag, item.Label, item.IsEnabled)),
            item => Assert.Equal(("AI Models", "AI Models", true), (item.Tag, item.Label, item.IsEnabled)),
            item => Assert.Equal(("Audio Input", "Audio Input", true), (item.Tag, item.Label, item.IsEnabled)),
            item => Assert.Equal(("Dictionary", "Dictionary", true), (item.Tag, item.Label, item.IsEnabled)),
            item => Assert.Equal(("History", "History", true), (item.Tag, item.Label, item.IsEnabled)),
            item => Assert.Equal(("Settings", "Settings", true), (item.Tag, item.Label, item.IsEnabled)),
            item => Assert.Equal(("About", "About / Open Source", true), (item.Tag, item.Label, item.IsEnabled)));
    }

    [Fact]
    public void BuildItems_ReplacesCommercialVoiceInkProWithAboutOpenSource()
    {
        var labels = ShellNavigationPresenter.BuildItems().Select(item => item.Label);

        Assert.Contains("About / Open Source", labels);
        Assert.DoesNotContain("VoiceInk Pro", labels);
    }
}
```

- [ ] **Step 2: Run red tests**

Run:

```powershell
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" test VoiceInk.Windows\tests\VoiceInk.Windows.Core.Tests\VoiceInk.Windows.Core.Tests.csproj --filter FullyQualifiedName~ShellNavigationPresenterTests
```

Expected: compile failure because `ShellNavigationPresenter` and `ShellNavigationItem` do not exist yet.

## Task 3: Core Navigation Presenter

- [ ] **Step 1: Add item record**

Create `VoiceInk.Windows/src/VoiceInk.Windows.Core/Shell/ShellNavigationItem.cs`:

```csharp
namespace VoiceInk.Windows.Core.Shell;

public sealed record ShellNavigationItem(
    string Tag,
    string Label,
    string Icon,
    bool IsEnabled = true);
```

- [ ] **Step 2: Add presenter**

Create `VoiceInk.Windows/src/VoiceInk.Windows.Core/Shell/ShellNavigationPresenter.cs`:

```csharp
namespace VoiceInk.Windows.Core.Shell;

public static class ShellNavigationPresenter
{
    public static IReadOnlyList<ShellNavigationItem> BuildItems() =>
    [
        new("Dashboard", "Dashboard", "Home"),
        new("AI Models", "AI Models", "Library"),
        new("Audio Input", "Audio Input", "Microphone"),
        new("Dictionary", "Dictionary", "Character"),
        new("History", "History", "Document"),
        new("Settings", "Settings", "Setting"),
        new("About", "About / Open Source", "Help")
    ];
}
```

- [ ] **Step 3: Verify focused tests**

Run:

```powershell
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" test VoiceInk.Windows\tests\VoiceInk.Windows.Core.Tests\VoiceInk.Windows.Core.Tests.csproj --filter FullyQualifiedName~ShellNavigationPresenterTests
```

Expected: 2 tests pass.

## Task 4: WinUI Navigation Shell

- [ ] **Step 1: Replace root scroll with NavigationView**

Modify `MainWindow.xaml` so the root is:

```xml
<NavigationView
    x:Name="RootNavigationView"
    IsBackButtonVisible="Collapsed"
    IsSettingsVisible="False"
    PaneTitle="VoiceInk"
    SelectionChanged="RootNavigationView_SelectionChanged">
```

Inside `NavigationView.MenuItems`, add `NavigationViewItem` entries with these names and tags:

- `DashboardNavigationItem`, `Tag="Dashboard"`, `Content="Dashboard"`, `Icon="Home"`.
- `ModelsNavigationItem`, `Tag="AI Models"`, `Content="AI Models"`, `Icon="Library"`.
- `AudioInputNavigationItem`, `Tag="Audio Input"`, `Content="Audio Input"`, `Icon="Microphone"`.
- `DictionaryNavigationItem`, `Tag="Dictionary"`, `Content="Dictionary"`, `Icon="Character"`.
- `HistoryNavigationItem`, `Tag="History"`, `Content="History"`, `Icon="Document"`.
- `SettingsNavigationItem`, `Tag="Settings"`, `Content="Settings"`, `Icon="Setting"`.
- `AboutNavigationItem`, `Tag="About"`, `Content="About / Open Source"`, `Icon="Help"`.

Move existing controls into named panels under a `ScrollViewer` content area:

- `DashboardSectionPanel`: title, Start/Stop/Cancel buttons, status.
- `ModelsSectionPanel`: existing model path/import/select/download controls.
- `AudioInputSectionPanel`: existing audio input controls.
- `DictionarySectionPanel`: existing dictionary controls.
- `HistorySectionPanel`: existing history controls.
- `SettingsSectionPanel`: existing shortcuts and cleanup controls.
- `AboutSectionPanel`: neutral open-source information and diagnostics actions.

- [ ] **Step 2: Add section navigation code**

In `MainWindow.xaml.cs`, add:

```csharp
private const string DashboardSectionTag = "Dashboard";
private string activeSectionTag = DashboardSectionTag;
```

Add `RootNavigationView_SelectionChanged`:

```csharp
private void RootNavigationView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
{
    if (args.SelectedItem is NavigationViewItem { Tag: string tag })
    {
        ShowShellSection(tag);
    }
}
```

Add `ShowShellSection(string tag)` to set each section panel `Visibility` to `Visible` only for its tag and to keep `activeSectionTag` in sync.

- [ ] **Step 3: Route existing History command through navigation**

Update `OpenHistoryWindowAsync` so it calls:

```csharp
ShowShellSection("History");
RootNavigationView.SelectedItem = HistoryNavigationItem;
```

before focusing `HistorySearchTextBox`.

- [ ] **Step 4: Add About/Open Source actions**

Add an About/Open Source section with:

- App name: `VoiceInk for Windows`.
- License text: `GPL-3.0 open-source Windows fork`.
- Source path read-only text using `AppContext.BaseDirectory`.
- Buttons:
  - `OpenDiagnosticsFolderButton` opens `%LOCALAPPDATA%\VoiceInk.Windows`.
  - `CopyDiagnosticsSummaryButton` copies a local-only diagnostics summary to the clipboard.

Add handlers:

```csharp
private void OpenDiagnosticsFolderButton_Click(object sender, RoutedEventArgs e) => OpenDiagnosticsFolder();
private async void CopyDiagnosticsSummaryButton_Click(object sender, RoutedEventArgs e) => await CopyDiagnosticsSummaryAsync();
```

The diagnostics summary must include app data path, recordings path, settings/history/dictionary file paths, active section, dictation state, and current model path. It must not include API keys, environment variables, clipboard contents, transcript text, or full history contents.

- [ ] **Step 5: Verify app build**

Run:

```powershell
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" build VoiceInk.Windows\VoiceInk.Windows.sln -c Debug -p:Platform=x64
```

Expected: build passes with 0 warnings/errors.

## Task 5: Docs, Review, Commit

- [ ] **Step 1: Update README/spec**

Document:

- Windows shell now uses a sidebar navigation view.
- Existing controls are grouped into Dashboard, AI Models, Audio Input, Dictionary, History, and Settings.
- VoiceInk Pro is replaced by About/Open Source with local diagnostics actions.
- Transcribe Audio, Enhancement, Power Mode, and Permissions remain later slices.

- [ ] **Step 2: Full verification**

Run:

```powershell
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" test VoiceInk.Windows\VoiceInk.Windows.sln
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" build VoiceInk.Windows\VoiceInk.Windows.sln -c Debug -p:Platform=x64
```

Expected: all tests pass and Debug x64 build passes with 0 warnings/errors.

- [ ] **Step 3: Request review and fix findings**

Request subagent review against this plan, `VoiceInk/Views/ContentView.swift`, and `VoiceInk/Views/Settings/SettingsView.swift`. Fix all Critical and Important findings before committing.

- [ ] **Step 4: Commit**

Run:

```powershell
git add README.md `
  VoiceInk.Windows\src\VoiceInk.Windows.Core\Shell\ShellNavigationItem.cs `
  VoiceInk.Windows\src\VoiceInk.Windows.Core\Shell\ShellNavigationPresenter.cs `
  VoiceInk.Windows\tests\VoiceInk.Windows.Core.Tests\Shell\ShellNavigationPresenterTests.cs `
  VoiceInk.Windows\src\VoiceInk.Windows.App\MainWindow.xaml `
  VoiceInk.Windows\src\VoiceInk.Windows.App\MainWindow.xaml.cs `
  docs\superpowers\plans\2026-05-25-windows-navigation-settings-shell.md `
  docs\superpowers\specs\2026-05-24-windows-open-source-parity-design.md
git commit -m "feat(windows): add navigation settings shell"
```
