# Windows Failed Dictation History Design

Date: 2026-05-25

## Goal

Persist failed recorder-stop dictation attempts in History when audio capture has already completed, so users can see failed sessions instead of only a transient error label.

## Behavior

- If recording capture stops successfully but transcription, post-processing, enhancement, insertion, or completed-history save fails before a completed row is saved, the controller records a `Failed` History item best-effort.
- Failed rows include provider, language, model metadata, audio duration, audio file path, Power Mode metadata, and the sanitized error message.
- Failed rows do not insert text and do not record session metrics.
- If failed-history saving itself fails, the controller keeps the original error as `LastError` and records a warning for the history-save failure.
- If capture start or capture stop fails before an `AudioCaptureResult` exists, no failed History row is written because there is no reliable session/audio metadata to preserve.
- Cancellation remains separate and continues to write `Canceled` rows only for cancellation paths already implemented.

## Testing

- Transcription failure after capture stop saves one failed History item with error metadata and no inserted text.
- Capture stop failure still does not write History because no stopped audio result exists.
- Full solution tests must pass.

## Non-Goals

- No retry UI changes in this slice.
- No metrics for failed rows.
- No commercial telemetry or remote error reporting.
