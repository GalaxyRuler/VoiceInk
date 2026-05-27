# Windows Dictation Command Punctuation Plan

## Slice

Swallow punctuation attached to spoken line and paragraph formatting commands so inserted text does not start a line with stray punctuation.

## Tasks

1. Add a failing text post-processor test for `new line,` and `new paragraph.`.
2. Extend the formatting command regexes to consume optional adjacent punctuation.
3. Confirm existing formatting-disabled behavior remains covered.
4. Update the completion tracker and verify focused Core tests.

## Review Notes

- Do not change punctuation cleanup modes, word replacement order, enhancement input cleanup, or trailing-space behavior.
- Keep this Core-only so all transcription entry points share the fix.
