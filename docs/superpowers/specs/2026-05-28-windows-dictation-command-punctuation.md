# Windows Dictation Command Punctuation Spec

## Intent

Improve transcript polish when speech models include punctuation immediately after spoken formatting commands such as "new line," or "new paragraph.".

## Requirements

- When text formatting is enabled, consume optional command-adjacent punctuation after `new line` and `new paragraph`.
- Preserve the existing behavior where formatting commands are left untouched when text formatting is disabled.
- Preserve paragraph chunking, word replacements, punctuation cleanup, lowercasing, and trailing-space behavior.
- Keep the implementation in Core so recorder, audio-file transcription, retry, and re-enhancement paths share the same cleanup.

## Open-Source Boundary

This is local transcript cleanup. It adds no account flow, telemetry, licensing, paid gate, or commercial surface.

## Verification

- Focused Core text post-processor tests.
