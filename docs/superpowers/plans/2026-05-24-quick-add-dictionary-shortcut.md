# Quick Add Dictionary Shortcut Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add the macOS-style Quick Add to Dictionary shortcut to the Windows MVP with a focused vocabulary/replacement entry dialog.

**Architecture:** Core owns the quick-add command semantics through a small service that validates the selected mode and calls the writable dictionary store. The WinUI shell owns the Windows-specific presentation: restore/focus the main window, show a quick-add dialog with Vocabulary and Word Replacement modes, refresh dictionary state after success, and register the optional global shortcut. This preserves the macOS panel intent without adding a new window manager yet.

**Tech Stack:** .NET 10, WinUI 3 `ContentDialog`, existing JSON dictionary store, existing global hotkey service, xUnit.

---

## Files

- Create `VoiceInk.Windows/src/VoiceInk.Windows.Core/Dictionary/DictionaryQuickAddMode.cs` for quick-add modes.
- Create `VoiceInk.Windows/src/VoiceInk.Windows.Core/Dictionary/DictionaryQuickAddResult.cs` for command result messages.
- Create `VoiceInk.Windows/src/VoiceInk.Windows.Core/Dictionary/DictionaryQuickAddService.cs` for testable quick-add behavior.
- Create `VoiceInk.Windows/tests/VoiceInk.Windows.Core.Tests/Dictionary/DictionaryQuickAddServiceTests.cs`.
- Modify `VoiceInk.Windows/src/VoiceInk.Windows.Core/Settings/AppSettings.cs` for `QuickAddDictionaryHotkey`.
- Modify `VoiceInk.Windows/src/VoiceInk.Windows.Core/Shortcuts/GlobalShortcutAction.cs` and `GlobalShortcutSettings.cs`.
- Modify `VoiceInk.Windows/tests/VoiceInk.Windows.Core.Tests/Shortcuts/GlobalShortcutTests.cs`.
- Modify `VoiceInk.Windows/tests/VoiceInk.Windows.Infrastructure.Tests/Settings/JsonSettingsStoreTests.cs`.
- Modify `VoiceInk.Windows/src/VoiceInk.Windows.App/MainWindow.xaml` and `MainWindow.xaml.cs` for the shortcut field, quick-add button, dialog, load/save, and action route.
- Modify `README.md` and `docs/superpowers/specs/2026-05-24-windows-open-source-parity-design.md`.

## Task 1: Planning Docs

- [ ] **Step 1: Mark Quick Add active**

Update the parity spec to say Quick Add is being implemented as a Windows dialog equivalent to the macOS floating panel. Keep the richer dedicated dictionary page/edit/sorting work as a later gap.

- [ ] **Step 2: Commit the plan**

Run:

```powershell
git add docs\superpowers\plans\2026-05-24-quick-add-dictionary-shortcut.md docs\superpowers\specs\2026-05-24-windows-open-source-parity-design.md
git commit -m "docs(windows): plan quick add dictionary shortcut"
```

Expected: docs-only commit.

## Task 2: Red Tests

- [ ] **Step 1: Add quick-add service tests**

Create tests that prove:

- Vocabulary mode trims and adds comma-separated vocabulary words.
- Replacement mode trims original/replacement text and adds a replacement.
- Blank vocabulary input returns `Enter a vocabulary word.`
- Blank replacement original returns `Enter original text.`
- Blank replacement value returns `Enter replacement text.`
- Store duplicate errors are surfaced unchanged.

- [ ] **Step 2: Add shortcut tests**

Update `BuildRegistrations_UsesPrimaryAndOptionalHotkeys` with `QuickAddDictionaryHotkey = "Ctrl+Alt+D"` and assert `GlobalShortcutAction.QuickAddToDictionary`. Add duplicate assignment coverage expecting `Quick Add to Dictionary already uses Ctrl+Alt+Space.`.

- [ ] **Step 3: Add settings test expectation**

In `JsonSettingsStoreTests.SaveAsync_PersistsSettings`, add `QuickAddDictionaryHotkey = "Ctrl+Alt+D"`.

- [ ] **Step 4: Run red tests**

Run:

```powershell
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" test VoiceInk.Windows\tests\VoiceInk.Windows.Core.Tests\VoiceInk.Windows.Core.Tests.csproj --filter "FullyQualifiedName~DictionaryQuickAddServiceTests|FullyQualifiedName~GlobalShortcutTests"
```

Expected: compile failure because the new service/action/setting do not exist.

## Task 3: Core Implementation

- [ ] **Step 1: Add quick-add types**

Create `DictionaryQuickAddMode` with `Vocabulary` and `WordReplacement`. Create `DictionaryQuickAddResult(bool Succeeded, string Message)`.

- [ ] **Step 2: Add quick-add service**

Implement `DictionaryQuickAddService.SubmitAsync(mode, vocabularyInput, replacementOriginal, replacementText, cancellationToken)`:

- Vocabulary: trim `vocabularyInput`; if blank return `false, "Enter a vocabulary word."`; call `AddVocabularyWordsAsync`; return duplicate/store errors unchanged; on success return `true, "Vocabulary updated"`.
- Word Replacement: trim `replacementOriginal` and `replacementText`; validate each separately; call `AddWordReplacementAsync`; return duplicate/store errors unchanged; on success return `true, "Word replacements updated"`.

- [ ] **Step 3: Add setting and action**

Add `QuickAddDictionaryHotkey` to `AppSettings` and `QuickAddToDictionary` to `GlobalShortcutAction`.

- [ ] **Step 4: Add shortcut registration**

Register optional `settings.QuickAddDictionaryHotkey` in `GlobalShortcutSettings.BuildRegistrations` with display name `Quick Add to Dictionary`.

- [ ] **Step 5: Verify focused tests**

Run:

```powershell
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" test VoiceInk.Windows\tests\VoiceInk.Windows.Core.Tests\VoiceInk.Windows.Core.Tests.csproj --filter "FullyQualifiedName~DictionaryQuickAddServiceTests|FullyQualifiedName~GlobalShortcutTests"
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" test VoiceInk.Windows\tests\VoiceInk.Windows.Infrastructure.Tests\VoiceInk.Windows.Infrastructure.Tests.csproj --filter FullyQualifiedName~JsonSettingsStoreTests
```

Expected: selected tests pass.

## Task 4: WinUI Wiring

- [ ] **Step 1: Add UI controls**

Add `QuickAddHotkeyTextBox` under Shortcuts with header `Quick Add to Dictionary` and placeholder `Ctrl+Alt+D`. Add a `Quick Add` button in the Dictionary section.

- [ ] **Step 2: Load/save and route**

Load/save `QuickAddDictionaryHotkey` and route `GlobalShortcutAction.QuickAddToDictionary` to `ShowQuickAddDictionaryAsync`.

- [ ] **Step 3: Implement quick-add dialog**

`ShowQuickAddDictionaryAsync` should restore/focus the main window, display a `ContentDialog` titled `Quick Add to Dictionary`, include a mode selector for `Vocabulary` and `Word Replacement`, include vocabulary/replacement input fields, submit through `DictionaryQuickAddService`, keep the dialog open with the result message on validation or duplicate errors, refresh dictionary on success, and set the shell status to the service message.

- [ ] **Step 4: Verify app build**

Run:

```powershell
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" build VoiceInk.Windows\VoiceInk.Windows.sln -c Debug -p:Platform=x64
```

Expected: build passes with 0 warnings/errors.

## Task 5: Review, Docs, Commit

- [ ] **Step 1: Update docs**

Update README/spec implemented shortcut and dictionary lists. Leave the dedicated macOS-style dictionary page, edit flow, and sorting controls as remaining dictionary gaps.

- [ ] **Step 2: Full verification**

Run full solution tests and Debug x64 build.

- [ ] **Step 3: Request review**

Request review, fix Critical and Important issues.

- [ ] **Step 4: Commit**

Run:

```powershell
git add README.md VoiceInk.Windows docs\superpowers\specs\2026-05-24-windows-open-source-parity-design.md
git commit -m "feat(windows): add quick add dictionary shortcut"
```
