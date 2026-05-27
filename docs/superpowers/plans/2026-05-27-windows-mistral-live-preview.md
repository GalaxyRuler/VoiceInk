# Windows Mistral Live Preview Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use `superpowers:test-driven-development`.

**Goal:** Add a tested Mistral realtime recorder preview adapter using the official Mistral WebSocket contract.

---

## Task 1: Red Tests

- [x] Add URI factory tests for Mistral realtime URL/model mapping.
- [x] Add message parser tests for text deltas, done events, and irrelevant messages.
- [x] Add live-preview service tests for provider gating, API-key gating, headers, JSON audio messages, transcript accumulation, and completion.
- [x] Update provider-card metadata coverage to expect Mistral realtime preview.
- [x] Run the focused tests and confirm failure before implementation.

## Task 2: Infrastructure

- [x] Add `MistralStreamingUriFactory`.
- [x] Add `MistralStreamingMessageParser`.
- [x] Add `MistralLiveTranscriptionPreviewService`.
- [x] Register the service in the app composite live-preview pipeline.
- [x] Update Mistral provider metadata.

## Task 3: Docs, Verification, And Commit

- [x] Update the completion tracker.
- [x] Run focused Mistral/catalog tests.
- [x] Run full solution tests/build and whitespace check.
- [x] Commit the slice.

## Verification Notes

- Online grounding: official Mistral realtime docs and the Mistral Python SDK source show WebSocket path `/v1/audio/transcriptions/realtime`, query `model`, `Authorization: Bearer ...`, `session.update`, `input_audio.append`, `input_audio.flush`, `input_audio.end`, and `transcription.text.delta` events.
- RED: `& '..\.dotnet-sdk-10\dotnet.exe' test VoiceInk.Windows\tests\VoiceInk.Windows.Infrastructure.Tests\VoiceInk.Windows.Infrastructure.Tests.csproj --filter 'FullyQualifiedName~Mistral'` failed with missing Mistral realtime classes. `& '..\.dotnet-sdk-10\dotnet.exe' test VoiceInk.Windows\tests\VoiceInk.Windows.Core.Tests\VoiceInk.Windows.Core.Tests.csproj --filter 'FullyQualifiedName~TranscriptionProviderPresetCatalogTests.All_ExposesCloudProviderCardMetadata'` failed with expected `Realtime preview` but actual `Batch`.
- Review regression: added `Build_PreservesAlreadyRealtimeEndpointPath`; it failed because the URI factory appended the realtime path twice, then passed after the factory preserved existing realtime paths.
- Focused GREEN: `& '..\.dotnet-sdk-10\dotnet.exe' test VoiceInk.Windows\tests\VoiceInk.Windows.Infrastructure.Tests\VoiceInk.Windows.Infrastructure.Tests.csproj --filter 'FullyQualifiedName~Mistral'` passed 10 tests, and the provider-card metadata test passed 1 test.
- Full tests: `& '..\.dotnet-sdk-10\dotnet.exe' test VoiceInk.Windows\VoiceInk.Windows.sln` passed 709 Core tests and 266 Infrastructure tests.
- Full build: `& '..\.dotnet-sdk-10\dotnet.exe' build VoiceInk.Windows\VoiceInk.Windows.sln -c Debug -p:Platform=x64` succeeded with 0 warnings and 0 errors.
