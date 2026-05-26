# Windows Soniox Live Preview Plan

## Goal

Add Soniox realtime WebSocket support to the existing composite live transcript preview pipeline.

## Steps

- [x] Add failing infrastructure tests for Soniox live preview startup, config, audio streaming, token parsing, and empty-frame cleanup.
- [x] Implement Soniox streaming token parsing.
- [x] Implement `SonioxLiveTranscriptionPreviewService`.
- [x] Register Soniox in the composite live preview service.
- [x] Mark Soniox provider card metadata as realtime preview.
- [x] Run focused Soniox live-preview and provider catalog tests.
- [x] Update README/completion tracker.
- [x] Run full solution tests, Debug x64 build, and `git diff --check`.
- [ ] Commit the slice.
