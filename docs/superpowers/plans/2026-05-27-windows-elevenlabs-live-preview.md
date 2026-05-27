# Windows ElevenLabs Live Preview Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use `superpowers:test-driven-development`.

**Goal:** Add ElevenLabs realtime Speech to Text live transcript preview.

---

## Task 1: Red Tests

- [x] Add parser tests for partial, committed, timestamped, and irrelevant ElevenLabs realtime events.
- [x] Add service tests for provider gating, WebSocket headers/query, JSON audio chunk sending, partial emission, committed emission, and manual final commit.
- [x] Add preset catalog test coverage that ElevenLabs advertises realtime preview.
- [x] Run focused tests and confirm they fail before implementation.

## Task 2: Implementation

- [x] Add ElevenLabs realtime URI factory.
- [x] Add ElevenLabs realtime message parser.
- [x] Add ElevenLabs live preview service.
- [x] Register the service in the WinUI app composite live-preview service.
- [x] Update ElevenLabs preset realtime display and model choices where needed.

## Task 3: Verification And Commit

- [x] Run focused ElevenLabs live preview and preset tests.
- [x] Run full solution tests/build and whitespace check.
- [x] Update project completion tracker and commit the slice.

## Verification Notes

- Red check: `& '..\.dotnet-sdk-10\dotnet.exe' test VoiceInk.Windows\VoiceInk.Windows.sln --filter 'FullyQualifiedName~ElevenLabsStreamingMessageParserTests|FullyQualifiedName~ElevenLabsLiveTranscriptionPreviewServiceTests|FullyQualifiedName~TranscriptionProviderPresetCatalogTests.All_ExposesCloudProviderCardMetadata'` failed because the ElevenLabs realtime service/parser did not exist and the preset still reported `Batch`.
- Focused check: the same command, broadened to include `All_IncludesMacCloudProviderPresets` after review, passed 2 Core catalog tests and 10 Infrastructure realtime tests after implementation.
- Full test: `& '..\.dotnet-sdk-10\dotnet.exe' test VoiceInk.Windows\VoiceInk.Windows.sln` passed 698 Core tests and 257 Infrastructure tests.
- Build: `& '..\.dotnet-sdk-10\dotnet.exe' build VoiceInk.Windows\VoiceInk.Windows.sln -c Debug -p:Platform=x64` succeeded with 0 warnings and 0 errors.
