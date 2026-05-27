# Windows Mistral Live Preview Metadata Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use `superpowers:test-driven-development`.

**Goal:** Correct Mistral provider card metadata so Windows does not claim an unimplemented recorder live-preview path.

---

## Task 1: Red Test

- [x] Add provider catalog coverage that Mistral remains a batch provider in the Windows UI until a live-preview service exists.
- [x] Run the focused catalog test and confirm failure before implementation.

## Task 2: Metadata And Docs

- [x] Change Mistral `StreamingDisplay` to `Batch`.
- [x] Update project completion wording to make the remaining Mistral live-preview gap explicit.

## Task 3: Verification And Commit

- [x] Run focused provider catalog tests.
- [x] Run full solution tests/build and whitespace check.
- [x] Commit the slice.

## Verification Notes

- RED: `& '..\.dotnet-sdk-10\dotnet.exe' test VoiceInk.Windows\tests\VoiceInk.Windows.Core.Tests\VoiceInk.Windows.Core.Tests.csproj --filter 'FullyQualifiedName~TranscriptionProviderPresetCatalogTests.All_ExposesCloudProviderCardMetadata'` failed as expected with Mistral `StreamingDisplay` expected `Batch` but actual `Realtime preview`.
- GREEN focused: `& '..\.dotnet-sdk-10\dotnet.exe' test VoiceInk.Windows\tests\VoiceInk.Windows.Core.Tests\VoiceInk.Windows.Core.Tests.csproj --filter 'FullyQualifiedName~TranscriptionProviderPresetCatalogTests.All_ExposesCloudProviderCardMetadata|FullyQualifiedName~TranscriptionProviderPresetCatalogTests.All_IncludesMacCloudProviderPresets'` passed 2 tests.
- Full tests: `& '..\.dotnet-sdk-10\dotnet.exe' test VoiceInk.Windows\VoiceInk.Windows.sln` passed 709 Core tests and 257 Infrastructure tests.
- Full build: `& '..\.dotnet-sdk-10\dotnet.exe' build VoiceInk.Windows\VoiceInk.Windows.sln -c Debug -p:Platform=x64` succeeded with 0 warnings and 0 errors.
