# Windows OCR Capture Consent Row

## Goal

Make the Windows-specific capture boundary visible in Context Awareness settings whenever Screen OCR is enabled.

## Source Of Truth

The macOS app explains that screen context captures on-screen text locally and is not stored. On Windows, screen capture may also involve system-controlled consent UI or a visible capture border, depending on the capture API and OS policy. The Windows app should disclose that platform behavior without weakening the local-only OCR promise.

## Requirements

- When Screen OCR context is enabled, the Context Awareness privacy presentation must include a Windows capture consent row.
- The row must say that Windows controls capture consent UI or visible capture borders.
- The row must state that VoiceInk only uses the captured image for local OCR during enhancement.
- The row must remain presenter-backed so the WinUI settings list renders it without fragile UI automation.
- Screen OCR disabled state must remain unchanged and should not show the consent row.

## Non-Goals

- This slice does not change capture APIs, OCR behavior, provider routing, or storage.
- This slice does not bypass Windows capture consent or border behavior.
