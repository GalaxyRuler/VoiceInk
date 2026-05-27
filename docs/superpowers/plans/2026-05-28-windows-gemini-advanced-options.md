# Windows Gemini Advanced Options Plan

## Slice

Add safe Gemini endpoint-query advanced option mapping into `generationConfig`.

## Steps

1. Add focused Gemini tests for:
   - endpoint-query tuning options in `generationConfig`;
   - query-free generated `:generateContent` URI;
   - unsupported routing fields ignored;
   - secret-like query keys rejected before HTTP.
2. Update `GeminiCloudTranscriptionService` to use shared cloud endpoint validation.
3. Strip query from the generated request URI.
4. Build the generateContent payload through a dictionary so optional `generationConfig` can be included only when present.
5. Keep inline audio and Files API upload payloads unchanged.
6. Run focused Gemini tests.
7. Run the broader transcription infrastructure tests.
8. Update the completion tracker and commit the slice.

## Verification

- Focused red run failed before implementation because endpoint queries stayed in the generated URI and secret-query validation happened too late.
- Focused green run passed after implementation:
  - `dotnet test VoiceInk.Windows/tests/VoiceInk.Windows.Infrastructure.Tests/VoiceInk.Windows.Infrastructure.Tests.csproj --filter FullyQualifiedName~GeminiCloudTranscriptionServiceTests`
