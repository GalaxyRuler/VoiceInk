# Windows Transcript Text Formatting Spec

## Goal

Port the original macOS VoiceInk transcript formatting behavior into the Windows core text pipeline as a free/open-source local feature.

## Source Of Truth

- macOS implementation: `VoiceInk/Transcription/Processing/WhisperTextFormatter.swift`
- Windows post-processing entry point: `VoiceInk.Windows/src/VoiceInk.Windows.Core/Text/TextPostProcessor.cs`

## Requirements

- Add an opt-in transcript formatting setting to Windows.
- Format long transcripts into readable paragraphs using the macOS constants:
  - target word count: 50
  - maximum significant sentences per paragraph: 4
  - significant sentence threshold: 4 words
- Keep formatting off by default to preserve existing behavior.
- Run dictionary replacements before formatting so replacement text participates in final output.
- Run formatting before punctuation cleanup so sentence punctuation can still guide paragraph splitting.
- Keep the logic UI-independent and testable in Core.
- Add a Power Mode override equivalent to the macOS `isTextFormattingEnabled` option.
- Preserve line breaks if the user later chooses Remove All punctuation.

## Non-Goals

- Do not add paid formatting providers or commercial enhancement gates.
- Do not introduce a NaturalLanguage dependency on Windows for this slice.
- Do not attempt full language-aware sentence segmentation yet; use deterministic punctuation-based segmentation and document it as a Windows approximation.

## Verification

- Focused Core tests for text post-processing and Power Mode.
- Full Core test project.
- Full solution tests and Debug x64 build before commit.
