# Windows Context Desktop Screenshot Boundary Spec

## Goal

Correct the Context Awareness privacy wording so it matches the implemented Windows OCR capture path.

## Source Of Truth

- The current Windows implementation captures a one-time PNG through `WindowsDesktopScreenImageCapture`, which uses `System.Drawing.Graphics.CopyFromScreen`.
- Microsoft `Windows.Graphics.Capture` APIs use secure picker consent and visible yellow borders, but this app slice does not use those APIs today.
- Existing VoiceInk context privacy guidance must describe what the app actually does, not a future capture backend.

## Requirements

- Context readiness privacy rows must stop implying that current OCR capture uses Windows Graphics Capture consent UI or capture borders.
- The OCR privacy row must explain that the source-built app takes a transient desktop screenshot for local OCR when screen context is enabled.
- The row must preserve the local-only image lifetime boundary: image bytes are discarded after OCR and cloud providers receive extracted text, not the image.
- The row must warn that protected or unavailable screen content can produce empty context and should be skipped gracefully.
- No capture implementation change is part of this slice.

## Non-Goals

- No migration to Windows.Graphics.Capture.
- No GUI automation or screenshots on the active desktop.
- No registry changes, permissions prompts, telemetry, or commercial support flow.
