# Windows Transcript Unicode Normalization Plan

## Goal

Make dictionary replacement and cleanup stable across canonically equivalent Unicode text emitted by local or cloud transcription providers.

## Steps

1. Add a failing `TextPostProcessor` test for a decomposed-accent transcript matching a composed dictionary replacement.
2. Normalize text to Unicode Form C at the start of post-processing.
3. Update the project completion tracker.
4. Verify with focused text tests, full solution tests/build, and whitespace checking.

## Verification

```powershell
& '..\.dotnet-sdk-10\dotnet.exe' test VoiceInk.Windows\tests\VoiceInk.Windows.Core.Tests\VoiceInk.Windows.Core.Tests.csproj --filter FullyQualifiedName~TextPostProcessorTests
& '..\.dotnet-sdk-10\dotnet.exe' test VoiceInk.Windows\VoiceInk.Windows.sln
& '..\.dotnet-sdk-10\dotnet.exe' build VoiceInk.Windows\VoiceInk.Windows.sln -c Debug -p:Platform=x64
git diff --check
```
