# Windows Gemini Files API Transcription Design

## Goal

Allow the Gemini transcription adapter to handle longer recordings by uploading audio through the Gemini Files API before calling `generateContent`, while keeping short dictation recordings on the existing inline-audio path.

## Grounding

- Google Gemini audio docs support both inline audio input and uploaded file input for `generateContent`.
- Google Gemini Files API docs define resumable media upload at `https://generativelanguage.googleapis.com/upload/v1beta/files`.
- Gemini guidance says to use the Files API when the total request size would exceed the inline request limit.

## Requirements

- Keep short WAV recordings on the existing inline `inline_data` request path.
- Use the Files API for WAV recordings over the inline threshold.
- Start a resumable upload with `X-Goog-Upload-Protocol: resumable`, `X-Goog-Upload-Command: start`, content length/type metadata, and `x-goog-api-key`.
- Upload and finalize the audio bytes to the returned `X-Goog-Upload-URL`.
- Call `generateContent` with a `file_data` audio part using the returned file URI.
- Keep HTTP errors sanitized and do not expose provider response bodies or API keys.

## Non-Goals

- File deletion/retention management in the Gemini Files API.
- Realtime Gemini preview.
- User-visible tuning for the inline threshold.
