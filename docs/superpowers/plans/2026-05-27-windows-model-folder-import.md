# Windows Model Folder Import Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use `superpowers:test-driven-development`.

**Goal:** Add a safe folder-level import flow for local whisper.cpp `.bin` models.

---

## Task 1: Red Tests

- [x] Add Core tests for importing multiple `.bin` model paths.
- [x] Add Core tests for duplicate and non-bin skips.
- [x] Run the focused tests and confirm failure before implementation.

## Task 2: Core Merge Logic

- [x] Add import-many result metadata.
- [x] Add `ImportMany` to `LocalWhisperModelService`.

## Task 3: WinUI Flow

- [x] Add `Import Folder` button near the existing import controls.
- [x] Use a folder picker, scan top-level `.bin` files, merge imports, save settings, and refresh model choices.
- [x] Update the completion tracker.

## Task 4: Verification And Commit

- [x] Run focused model tests.
- [x] Run full solution tests/build and whitespace check.
- [x] Commit the slice.

## Verification Notes

- Online grounding: whisper.cpp model documentation uses `ggml-*.bin` names such as `ggml-base.en.bin` and describes GGML `.bin` files as the local model format.
- RED: `& '..\.dotnet-sdk-10\dotnet.exe' test VoiceInk.Windows\tests\VoiceInk.Windows.Core.Tests\VoiceInk.Windows.Core.Tests.csproj --filter 'FullyQualifiedName~LocalWhisperModelServiceTests.ImportMany'` failed because `LocalWhisperModelService.ImportMany` did not exist.
- Focused GREEN: the same command passed 1 test.
- Build check during implementation: `& '..\.dotnet-sdk-10\dotnet.exe' build VoiceInk.Windows\VoiceInk.Windows.sln -c Debug -p:Platform=x64` succeeded with 0 warnings and 0 errors.
- Full tests: `& '..\.dotnet-sdk-10\dotnet.exe' test VoiceInk.Windows\VoiceInk.Windows.sln` passed 712 Core tests and 266 Infrastructure tests.
- Full build: `& '..\.dotnet-sdk-10\dotnet.exe' build VoiceInk.Windows\VoiceInk.Windows.sln -c Debug -p:Platform=x64` succeeded with 0 warnings and 0 errors.
