# Windows Trailing Space Parity Plan

Date: 2026-05-27

## Goal

Align Windows transcript post-processing with macOS by applying `AppendTrailingSpace` only to inserted recorder text, not saved history or enhancement input.

## Grounding

- Local source: `VoiceInk/Transcription/Engine/TranscriptionPipeline.swift`.
- Local source: `VoiceInk/Services/AudioFileTranscriptionService.swift`.
- Online source: Apple Support, "Commands for dictating text on iPhone", for dictation spacing/formatting behavior.

## Steps

1. Add failing recorder and audio-file tests for paste-only trailing-space behavior.
2. Keep `TextPostProcessor.Process` available for UI/paste callers, but add a small helper for insertion-time trailing-space application.
3. Update `DictationController` to process cleaned text without trailing space, pass that text through enhancement/history, and append only when inserting.
4. Update `AudioFileTranscriptionService` to save cleaned text without trailing space.
5. Update old test expectations that previously encoded the mismatch.
6. Update the completion tracker.
7. Verify focused tests, full solution tests/build, and whitespace checks.
8. Commit the slice.

## Verification

- `dotnet test VoiceInk.Windows\tests\VoiceInk.Windows.Core.Tests\VoiceInk.Windows.Core.Tests.csproj --filter FullyQualifiedName~StopAsync_AppendsTrailingSpaceOnlyWhenInsertingText|FullyQualifiedName~TranscribeAsync_DoesNotAppendTrailingSpaceToSavedHistory|FullyQualifiedName~TextPostProcessorTests`
- `dotnet test VoiceInk.Windows\VoiceInk.Windows.sln`
- `dotnet build VoiceInk.Windows\VoiceInk.Windows.sln -c Debug -p:Platform=x64`
- `git diff --check`
