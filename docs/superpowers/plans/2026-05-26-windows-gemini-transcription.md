# Windows Gemini Transcription Plan

## Scope

Add the Gemini batch transcription provider path to the Windows cloud transcription architecture.

## Steps

1. Add red catalog and secret-name tests for the Gemini transcription preset.
2. Add red router coverage proving `gemini` routes to a Gemini-specific adapter.
3. Add red Gemini adapter tests for API-key auth, generateContent URL construction, inline WAV audio payload, transcript parsing, missing key, sanitized HTTP errors, and empty response handling.
4. Implement the catalog, secret mapping, adapter, router, and app wiring.
5. Update the completion tracker.
6. Run focused tests, full tests, build, diff hygiene, and commit the slice.

## Verification

- `dotnet test VoiceInk.Windows\tests\VoiceInk.Windows.Core.Tests\VoiceInk.Windows.Core.Tests.csproj --filter "FullyQualifiedName~TranscriptionProviderPresetCatalogTests"`
- `dotnet test VoiceInk.Windows\tests\VoiceInk.Windows.Infrastructure.Tests\VoiceInk.Windows.Infrastructure.Tests.csproj --filter "FullyQualifiedName~GeminiCloudTranscriptionServiceTests|FullyQualifiedName~TranscriptionServiceRouterTests"`
- `dotnet test VoiceInk.Windows\VoiceInk.Windows.sln`
- `dotnet build VoiceInk.Windows\VoiceInk.Windows.sln -c Debug -p:Platform=x64`
