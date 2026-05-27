# Windows Enhancement Cleanup Order Spec

Date: 2026-05-27

## Source of Truth

- The macOS recorder pipeline filters, formats, and applies word replacements, then runs prompt detection/enhancement against that text before applying user cleanup preferences for saved original text.
- The macOS audio-file transcription flow follows the same ordering: enhancement sees the formatted/replaced transcript, while saved history uses cleanup preferences.
- Apple dictation documentation treats punctuation, formatting, and capitalization as meaningful dictated text features, so enhancement prompts should not lose them before the AI cleanup step.

## Requirements

- Enhancement prompts must receive text after hallucination/filler cleanup, text formatting, and dictionary replacements.
- Enhancement prompts must not receive user cleanup preferences that remove punctuation or force lowercase.
- Saved recorder and audio-file history must continue to store the cleaned original text.
- Inserted recorder text must continue to use enhanced text when enhancement succeeds, or cleaned original text when it does not.
- Existing trigger-word, short-text skip, dictionary, and context behavior must remain unchanged.

## Non-Goals

- Do not change enhancement provider contracts or prompt templates.
- Do not alter already-saved history rows.
- Do not bypass user cleanup preferences for the saved original transcript.

## Acceptance Criteria

- Recorder and audio-file tests prove enhancement prompt text preserves punctuation/casing while history text reflects cleanup preferences.
- Text post-processing tests remain green.
- Full solution tests and Debug x64 build pass.
