# Windows Speechmatics Transcription Plan

## Scope

Add the Speechmatics batch cloud transcription path to the existing Windows provider architecture.

## Steps

1. Add red tests for the preset catalog and provider-specific secret name.
2. Add red router coverage proving `speechmatics` routes to a provider-specific adapter.
3. Add red adapter tests for multipart job creation, polling, transcript fetch, cleanup, automatic language, missing key, rejected status, timeout, and sanitized HTTP errors.
4. Implement the catalog, secret, router, adapter, and app wiring.
5. Update the completion tracker.
6. Run focused tests, full tests, build, diff hygiene, and commit the slice.

## Verification

- `dotnet test VoiceInk.Windows\tests\VoiceInk.Windows.Core.Tests\VoiceInk.Windows.Core.Tests.csproj --filter "FullyQualifiedName~TranscriptionProviderPresetCatalogTests"`
- `dotnet test VoiceInk.Windows\tests\VoiceInk.Windows.Infrastructure.Tests\VoiceInk.Windows.Infrastructure.Tests.csproj --filter "FullyQualifiedName~SpeechmaticsCloudTranscriptionServiceTests|FullyQualifiedName~TranscriptionServiceRouterTests"`
- `dotnet test VoiceInk.Windows\VoiceInk.Windows.sln`
- `dotnet build VoiceInk.Windows\VoiceInk.Windows.sln -c Debug -p:Platform=x64`
