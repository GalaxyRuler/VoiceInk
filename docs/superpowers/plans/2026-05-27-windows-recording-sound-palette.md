# Windows Recording Sound Palette Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use `superpowers:test-driven-development`.

**Goal:** Add built-in Windows system-sound choices for recording feedback.

---

## Task 1: Red Tests

- [x] Add Core tests that built-in sound modes normalize and unknown values still fall back to `systemDefault`.
- [x] Add Core tests that built-in modes are exposed as selectable mode metadata.
- [x] Run focused tests and confirm failures before implementation.

## Task 2: Implementation

- [x] Add built-in recording sound mode constants and choice metadata.
- [x] Map built-in modes to `SystemSounds` in native playback.
- [x] Add built-in choices to Start/Stop sound combo boxes.
- [x] Update selection/import/reset helpers to use mode tags instead of brittle selected indexes.

## Task 3: Verification And Commit

- [x] Run focused recording/settings tests.
- [x] Run full solution tests/build and whitespace check.
- [x] Update project completion tracker and commit the slice.

## Verification Notes

- Red check: `& '..\.dotnet-sdk-10\dotnet.exe' test VoiceInk.Windows\tests\VoiceInk.Windows.Core.Tests\VoiceInk.Windows.Core.Tests.csproj --filter 'FullyQualifiedName~CustomRecordingSoundImporterTests.Normalize_ReturnsSupportedSoundModes|FullyQualifiedName~CustomRecordingSoundImporterTests.SelectableModes_ExposeBuiltInWindowsSoundChoices'` failed because `RecordingSoundModeChoice` and `RecordingSoundModeSettings.SelectableModes` did not exist.
- Focused check: `& '..\.dotnet-sdk-10\dotnet.exe' test VoiceInk.Windows\tests\VoiceInk.Windows.Core.Tests\VoiceInk.Windows.Core.Tests.csproj --filter 'FullyQualifiedName~CustomRecordingSoundImporterTests|FullyQualifiedName~RecordingFeedbackCoordinatorTests|FullyQualifiedName~SettingsSectionPresenterTests'` passed 33 tests.
- Full test: `& '..\.dotnet-sdk-10\dotnet.exe' test VoiceInk.Windows\VoiceInk.Windows.sln` passed 709 Core tests and 257 Infrastructure tests.
- Build: `& '..\.dotnet-sdk-10\dotnet.exe' build VoiceInk.Windows\VoiceInk.Windows.sln -c Debug -p:Platform=x64` succeeded with 0 warnings and 0 errors.
