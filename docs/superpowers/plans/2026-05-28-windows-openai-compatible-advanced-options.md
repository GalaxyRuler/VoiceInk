# Windows OpenAI-Compatible Advanced Options Plan

## Slice

Promote safe endpoint-query options into multipart fields for OpenAI-compatible transcription providers.

## Steps

1. Add a failing focused test for safe query options such as `temperature` and repeated/bracketed provider fields.
2. Assert VoiceInk-owned fields from the query cannot override model, language, prompt, file, or response format.
3. Add generic query parsing to `OpenAICompatibleCloudTranscriptionService`.
4. Keep `response_format` explicitly handled and preserve the request URI query for custom endpoints.
5. Run focused OpenAI-compatible transcription tests.
6. Run broader transcription infrastructure tests.
7. Update the project completion tracker and commit the slice.

## Verification

- Focused red run failed because `temperature` was not present in multipart content.
- Focused green run passed after implementation:
  - `dotnet test VoiceInk.Windows/tests/VoiceInk.Windows.Infrastructure.Tests/VoiceInk.Windows.Infrastructure.Tests.csproj --filter FullyQualifiedName~OpenAICompatibleCloudTranscriptionServiceTests`
