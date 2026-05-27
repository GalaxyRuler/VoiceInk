# Windows Recorder Live Preview Disclosure Design

## Context

VoiceInk for Windows has a floating recorder with elapsed time, input level, Stop/Cancel controls, prompt/Power Mode controls, and optional live transcript preview for supported providers. The preview text is interim and shown only while recording, but the presenter did not expose a short disclosure string that the overlay could render next to the preview.

Dictation overlays should make clear that live preview is not the final inserted text. Windows should keep this behavior visible without adding telemetry or changing provider behavior.

## Goal

Add live-preview disclosure to the floating recorder:

- `FloatingRecorderViewState` exposes `LiveTranscriptDetail`;
- when preview text is shown, detail says `Live preview is interim and stays in the recorder until final insertion.`;
- when preview is hidden, detail is empty;
- the WinUI floating recorder renders the detail below the preview text.

## Non-Goals

- No live transcription provider changes.
- No new streaming provider.
- No capture or insertion behavior changes.
- No telemetry or cloud fallback.

## Testability

Focused floating recorder presenter tests cover the live-preview detail for visible and hidden preview states.
