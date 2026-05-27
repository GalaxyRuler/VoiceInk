# Windows Transcript Formatting Commands Spec

## Goal

Support common dictation formatting commands in the local Windows transcript post-processing pipeline when text formatting is enabled.

## Source Of Truth

VoiceInk's macOS prompt template explicitly tells enhancement providers to respect spoken `new line` and `new paragraph` commands. Apple and Windows dictation documentation also treat those phrases as authoring commands. Windows already has a local `ApplyTextFormatting` option; this slice makes that option handle the same line/paragraph commands without requiring cloud enhancement.

## Windows Behavior

When `TextPostProcessingOptions.ApplyTextFormatting` is enabled:

- Replace spoken `new line` with a single line break.
- Replace spoken `new paragraph` with a paragraph break.
- Preserve those line breaks through punctuation cleanup.

When `ApplyTextFormatting` is disabled:

- Leave `new line` and `new paragraph` as literal dictated text.

The command pass must keep dictionary replacement, punctuation cleanup, lowercase conversion, trailing-space handling, enhancement order, and persistence behavior unchanged.

## Open-Source Boundary

This is deterministic local text processing. It adds no hosted formatting service, telemetry, paid enhancement feature, account flow, or commercial lock.

## Verification

- Add focused `TextPostProcessorTests` for enabled and disabled formatting-command behavior.
- Run focused text post-processor tests.
- Run full Windows tests/build and `git diff --check` before committing.
