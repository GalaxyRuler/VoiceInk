# Windows xAI Advanced Options Plan

## Slice

Add safe xAI endpoint-query advanced option mapping for provider-specific transcription parity.

## Steps

1. Add failing infrastructure tests for:
   - safe endpoint query values copied into multipart fields;
   - query stripped from the request URI;
   - reserved multipart fields ignored;
   - secret-like endpoint query rejected before HTTP.
2. Update `XaiCloudTranscriptionService` to use shared cloud endpoint validation.
3. Strip query from the outgoing request URI.
4. Copy safe query values into multipart fields after VoiceInk-owned model/format/language fields.
5. Run focused xAI tests.
6. Run the broader transcription infrastructure test slice.
7. Update the completion tracker and commit the slice.

## Verification

- Focused red run failed before implementation because xAI kept query parameters on the request URI and secret query validation happened too late.
- Focused green run passed after implementation:
  - `dotnet test VoiceInk.Windows/tests/VoiceInk.Windows.Infrastructure.Tests/VoiceInk.Windows.Infrastructure.Tests.csproj --filter FullyQualifiedName~XaiCloudTranscriptionServiceTests`
