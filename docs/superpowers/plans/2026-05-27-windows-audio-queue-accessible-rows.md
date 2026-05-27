# Windows Audio Queue Accessible Rows Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use `superpowers:test-driven-development`.

**Goal:** Add accessible row names to the Transcribe Audio queue list.

---

## Task 1: Red Tests

- [x] Add static XAML coverage for `AudioFileQueueListView`.
- [x] Add static code coverage for the audio queue row accessible-name model/helper.
- [x] Confirm focused tests fail before implementation.

## Task 2: Row Model And Template

- [x] Add embedded audio queue row display/accessibility model in `MainWindow`.
- [x] Bind the audio queue list template to `AccessibleName`.
- [x] Run focused XAML/code accessibility tests.

## Task 3: Verification And Commit

- [x] Run full solution tests.
- [x] Run Debug x64 solution build.
- [x] Run whitespace check and commit the slice.

## Verification Notes

- Red: `dotnet test VoiceInk.Windows.Core.Tests.csproj --filter "AppXamlAccessibilityTests.MainWindow_AudioFileQueueRows_BindAccessibleName|AppXamlAccessibilityTests.MainWindow_AudioFileQueueRows_UseAccessibleNameModel"` failed because the queue row accessible-name model/helper did not exist.
- Green: same focused command passed, 2 tests.
- Full: `dotnet test VoiceInk.Windows.sln` passed, 770 Core tests and 266 Infrastructure tests.
- Build: `dotnet build VoiceInk.Windows.sln -c Debug -p:Platform=x64` passed with 0 warnings and 0 errors.
- Whitespace: `git diff --check` passed with only expected LF-to-CRLF working-copy warnings.
