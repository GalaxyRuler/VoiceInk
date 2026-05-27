# Windows Context Browser URL Privacy Row

## Goal

Make the Windows Context Awareness settings explain how browser URL context is sanitized before it is appended to enhancement prompts.

## Source Of Truth

- The Windows prompt renderer already includes sanitized browser URL context.
- OWASP documents that URL query strings can expose sensitive information through logs and histories.
- The macOS app emphasizes privacy boundaries for contextual awareness.

## Requirements

- Add a Context Awareness privacy row for browser URL sanitization.
- State that VoiceInk keeps only origin/path and strips query strings/fragments before prompt rendering.
- Keep the row presenter-backed and testable in Core.
- Do not change URL capture behavior.

## Non-Goals

- No new browser integration.
- No network calls.
- No storing browser history.
