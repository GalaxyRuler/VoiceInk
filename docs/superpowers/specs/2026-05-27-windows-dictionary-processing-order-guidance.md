# Windows Dictionary Processing Order Guidance

## Goal

Clarify the Dictionary page processing order: vocabulary guides recognition and prompt context, while enabled replacements rewrite final text after transcription.

## Source Of Truth

The macOS Dictionary separates vocabulary from word replacements. The Windows app already applies vocabulary to supported prompts and replacements after transcription; the UI should explain that boundary so users know which feature to use for recognition help versus deterministic cleanup.

## Requirements

- Dictionary rule guidance must include a `Processing Order` row.
- The row value must be `Vocabulary then replacements`.
- The row detail must explain that vocabulary guides recognition and prompts, while replacements rewrite final text after transcription.
- The row must be presenter-backed so the existing WinUI dictionary guidance list renders it.

## Non-Goals

- This slice does not change dictionary application order.
- This slice does not add regex replacements or provider-specific vocabulary uploads.
