# Windows OCR Language Pack Guidance

## Goal

Improve context-awareness guidance so users understand that Windows screen OCR depends on installed local OCR language packs, not VoiceInk cloud services or hidden downloads.

## Source Of Truth

- The Windows OCR APIs expose available recognizer languages from installed language packs.
- Microsoft documents that users can install OCR language packs through Windows Settings.
- The VoiceInk Windows fork keeps OCR local before prompt rendering and must disclose when unavailable sources gracefully produce missing context.

## Requirements

- When OCR context is enabled, the context privacy rows mention installed Windows OCR language packs.
- The row explains that missing packs can produce empty or partial OCR context for unsupported languages.
- The guidance remains local-only and does not imply automatic cloud OCR, automatic language-pack installation, or background downloads.

## Acceptance

- Focused tests fail before implementation because the OCR language row lacks language-pack guidance.
- Focused tests pass after implementation.
- Full solution tests and Debug x64 build pass.
