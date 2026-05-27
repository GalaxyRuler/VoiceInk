# Windows Context Prompt Spelling Priority

## Goal

Align the Windows enhancement prompt with the macOS VoiceInk context behavior so context can correct likely speech-recognition spelling mistakes.

## Source Of Truth

- `VoiceInk\Models\AIPrompts.swift` tells enhancement to reference clipboard and current-window context for better accuracy because transcript text may contain speech-recognition errors.
- The macOS prompt prioritizes spelling from custom vocabulary, clipboard context, and current-window context when similar phonetic terms appear in the transcript.
- VoiceInk public contextual-awareness documentation describes context as a way to improve accuracy for jargon, code, and specific names shown on screen.

## Requirements

- Add an explicit system-instruction rule for phonetic spelling priority.
- The rule must include custom vocabulary, clipboard context, current-window context, selected text, and active app/site context.
- Keep the change inside Core prompt rendering; do not add telemetry, commercial analytics, or provider-specific behavior.
- Preserve Assistant Mode behavior, which uses its own raw assistant instructions.
