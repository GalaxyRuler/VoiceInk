# Windows Transcript Text Formatting Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add opt-in macOS-style paragraph formatting to the Windows transcript post-processing pipeline.

**Architecture:** Keep all formatting logic in Core, behind `TextPostProcessingOptions`, so dictation, file transcription, and retry paths share the same behavior. Use `AppSettings` and `PowerModeRule` properties to carry the setting without tying the formatter to WinUI.

**Tech Stack:** .NET 10, xUnit, existing VoiceInk Windows Core settings and text-processing types.

---

### Task 1: Red Tests

**Files:**
- Modify: `VoiceInk.Windows/tests/VoiceInk.Windows.Core.Tests/Text/TextPostProcessorTests.cs`
- Modify: `VoiceInk.Windows/tests/VoiceInk.Windows.Core.Tests/PowerMode/PowerModeMatcherTests.cs`

- [x] **Step 1: Add post-processor tests**

Add tests proving formatting is off by default, paragraphs split after four significant sentences when enabled, and punctuation cleanup runs after formatting.

- [x] **Step 2: Add Power Mode override test**

Extend the existing process/title match test to assert that `IsTextFormattingEnabledOverride = true` flows into effective settings.

- [x] **Step 3: Verify red**

Run:

```powershell
& '..\.dotnet-sdk-10\dotnet.exe' test VoiceInk.Windows\tests\VoiceInk.Windows.Core.Tests\VoiceInk.Windows.Core.Tests.csproj --filter "FullyQualifiedName~TextPostProcessorTests|FullyQualifiedName~PowerModeMatcherTests"
```

Expected: compile failure because the new options and settings properties do not exist.

### Task 2: Core Formatter

**Files:**
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.Core/Text/TextPostProcessingOptions.cs`
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.Core/Text/TextPostProcessor.cs`

- [x] **Step 1: Add `ApplyTextFormatting` option**

Add a default-false option at the end of the `TextPostProcessingOptions` record. Keep the app-level setting default enabled for macOS parity.

- [x] **Step 2: Implement formatter**

Add deterministic sentence splitting, word counting, and paragraph chunking in `TextPostProcessor`.

- [x] **Step 3: Verify green**

Run the focused text tests and confirm the new paragraph assertions pass.

### Task 3: Settings And Power Mode Plumbing

**Files:**
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.Core/Settings/AppSettings.cs`
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.Core/PowerMode/PowerModeRule.cs`
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.Core/PowerMode/PowerModeMatcher.cs`
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.Core/Dictation/DictationController.cs`
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.Core/AudioFiles/AudioFileTranscriptionService.cs`
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.Core/History/HistoryRetryService.cs`
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.App/MainWindow.xaml`
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.App/MainWindow.xaml.cs`

- [x] **Step 1: Add settings property**

Add `IsTextFormattingEnabled` to `AppSettings`, equality, and hash code.

- [x] **Step 2: Add Power Mode override**

Add `IsTextFormattingEnabledOverride` to `PowerModeRule` and apply it in `PowerModeMatcher`.

- [x] **Step 3: Pass setting into post-processing**

Set `ApplyTextFormatting: settings.IsTextFormattingEnabled` in dictation, audio-file transcription, and history retry.

- [x] **Step 4: Add WinUI controls**

Add cleanup and Power Mode checkboxes using existing settings patterns.

### Task 4: Docs, Verification, Commit

**Files:**
- Modify: `docs/superpowers/project-completion.md`

- [x] **Step 1: Update completion tracker**

Change the current slice to transcript text formatting and update the core pipeline status.

- [x] **Step 2: Run verification**

Run focused tests, full solution tests, Debug x64 build, and `git diff --check`.

- [x] **Step 3: Commit**

Commit with:

```powershell
git add docs/superpowers VoiceInk.Windows
git commit -m "feat(windows): add transcript text formatting"
```
