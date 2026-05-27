# Windows Enhancement Cleanup Order Plan

Date: 2026-05-27

## Goal

Match macOS transcript pipeline ordering by sending enhancement pre-cleanup text while saving cleaned original text.

## Grounding

- Local source: `VoiceInk/Transcription/Engine/TranscriptionPipeline.swift`.
- Local source: `VoiceInk/Services/AudioFileTranscriptionService.swift`.
- Online source: Apple Support, "Commands for dictating text", for punctuation/capitalization/formatting as meaningful dictation output.

## Steps

1. Add failing recorder and audio-file tests that enable punctuation removal/lowercase while enhancement is enabled.
2. Record the fake enhancement request body so tests can inspect the transcript sent to enhancement.
3. Add a Core post-processing helper for enhancement input that keeps filtering, formatting, and replacements but skips punctuation/lowercase/trailing-space cleanup.
4. Route recorder and audio-file enhancement through that helper.
5. Keep saved history and fallback insertion on the cleaned original text.
6. Update the completion tracker.
7. Verify focused tests, full solution tests/build, and whitespace checks.
8. Commit the slice.

## Verification

- `dotnet test VoiceInk.Windows\tests\VoiceInk.Windows.Core.Tests\VoiceInk.Windows.Core.Tests.csproj --filter FullyQualifiedName~StopAsync_EnhancementReceivesTextBeforeUserCleanupPreferences|FullyQualifiedName~TranscribeAsync_EnhancementReceivesTextBeforeUserCleanupPreferences|FullyQualifiedName~TextPostProcessorTests`
- `dotnet test VoiceInk.Windows\VoiceInk.Windows.sln`
- `dotnet build VoiceInk.Windows\VoiceInk.Windows.sln -c Debug -p:Platform=x64`
- `git diff --check`
