# Windows Context Screenshot Lifetime Guidance Spec

## Source of Truth

- VoiceInk Contextual Awareness docs describe a temporary, one-time screenshot of the active window.
- The same docs state OCR runs locally and the screenshot image itself is never uploaded; only extracted text may be included in a cloud enhancement prompt.
- Windows already implements opt-in OCR context and provider-boundary disclosure, but the readiness surface should make screenshot lifetime explicit.

## Windows Behavior

- When Screen OCR context is enabled, show a context privacy row for screenshot lifetime.
- The row should say capture is one-time and the image is discarded after local OCR.
- The row should distinguish image handling from extracted text handling.
- Keep existing provider boundary rows and OCR scope rows.

## Non-Goals

- No new screen capture permissions or OCR capture behavior.
- No telemetry, remote image upload, commercial feature gates, or account flows.
