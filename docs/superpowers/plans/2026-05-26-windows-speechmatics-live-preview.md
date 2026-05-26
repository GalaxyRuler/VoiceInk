# Windows Speechmatics Live Preview Plan

## Goal

Add Speechmatics realtime WebSocket support to the existing composite live transcript preview pipeline.

## Steps

- [x] Add failing infrastructure tests for Speechmatics live preview startup, audio streaming, transcript parsing, and end-of-stream cleanup.
- [x] Implement Speechmatics streaming message parsing.
- [x] Implement `SpeechmaticsLiveTranscriptionPreviewService`.
- [x] Register Speechmatics in the composite live preview service.
- [x] Mark Speechmatics provider card metadata as realtime preview.
- [x] Run focused Speechmatics live-preview and provider catalog tests.
- [x] Update README/completion tracker.
- [x] Run full solution tests, Debug x64 build, and `git diff --check`.
- [ ] Commit the slice.
