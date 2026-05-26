# Windows Gemini Files API Plan

## Goal

Add Gemini Files API upload support for long recordings while preserving the existing inline audio behavior for normal short dictation.

## Steps

- [x] Add a failing infrastructure test for large WAV upload plus `file_data` `generateContent`.
- [x] Add sanitized upload-start failure coverage.
- [x] Implement resumable Gemini file upload behind `GeminiCloudTranscriptionService`.
- [x] Keep existing inline-audio tests green.
- [x] Update specs, README, and completion tracker.
- [x] Run focused Gemini tests, full solution tests, Debug x64 build, and `git diff --check`.
- [ ] Commit the slice.
