# Windows Transcript Replacement Order Spec

## Goal

Match the macOS VoiceInk transcription post-processing order for text formatting and dictionary replacements.

## Source Of Truth

The macOS pipeline in `VoiceInk/Transcription/Engine/TranscriptionPipeline.swift` formats transcript text with `WhisperTextFormatter.format(text)` before calling `WordReplacementService.shared.applyReplacements(to:using:)`.

## Windows Behavior

Windows transcript post-processing must:

1. Normalize and filter transcription text.
2. Apply macOS-style paragraph formatting when enabled.
3. Apply enabled dictionary word replacements to the formatted text.
4. Apply user cleanup preferences such as punctuation removal and lowercase output.
5. Return the trimmed/paste-ready text.

This allows replacements to target the same formatted paragraph boundaries that macOS users see before cleanup preferences modify punctuation or casing.

## Open-Source Boundary

The change is local-only Core logic. It adds no telemetry, licensing, paid provider behavior, account flow, or commercial updater surface.

## Verification

- Add a focused Core regression test proving formatting runs before dictionary replacement.
- Run the focused test red before implementation.
- Run the focused test green after implementation.
- Run the full Windows test suite and Debug x64 build before committing.
