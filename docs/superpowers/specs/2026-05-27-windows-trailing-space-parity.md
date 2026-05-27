# Windows Trailing Space Parity Spec

Date: 2026-05-27

## Source of Truth

- The macOS `TranscriptionPipeline` saves cleaned transcription text before applying `AppendTrailingSpace`; the extra space is added only to the pasted text.
- The macOS audio-file retranscription flow saves cleaned text and does not paste, so it never applies `AppendTrailingSpace`.
- Apple dictation guidance treats spacing as a text-entry behavior, including explicit "No space" commands, which reinforces that paste spacing belongs at insertion time rather than in stored history.

## Requirements

- Recorded dictation history must store the cleaned original text without a paste-only trailing space.
- Enhancement prompts must receive the cleaned original text without a paste-only trailing space.
- Inserted dictation text must still honor `AppendTrailingSpace`.
- Audio-file transcription history must store cleaned text without applying `AppendTrailingSpace`.
- Existing cleanup preferences, dictionary replacements, punctuation cleanup, lowercase conversion, and text formatting must keep their current order.

## Non-Goals

- Do not change the user-facing `AppendTrailingSpace` setting or default.
- Do not change saved history that already exists on disk.
- Do not remove explicit spaces returned by a transcription/enhancement provider except for trailing paste-only spacing.

## Acceptance Criteria

- A focused recorder pipeline test proves inserted text receives the trailing space while saved history does not.
- A focused audio-file transcription test proves saved history does not receive a trailing space.
- Existing post-processing tests still pass.
- Full solution tests and Debug x64 build pass.
