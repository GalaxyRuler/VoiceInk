# Windows Enhancement Behavior Guidance

## Purpose

Make macOS-default AI enhancement behavior visible in the Windows Enhancement page.

## Source of Truth

- The Swift app registers `SkipShortEnhancement = true`, `ShortEnhancementWordThreshold = 3`, `EnhancementTimeoutSeconds = 7`, and `EnhancementRetryOnTimeout = true`.
- Public VoiceInk docs describe enhancement trigger words and Assistant mode as user-facing ways to select a prompt deliberately.
- The Windows Core pipeline already implements the short-phrase guard, trigger-word bypass, timeout, retry, and original-text fallback behavior.

## Requirements

- Show a presenter-backed `Short Phrase Guard` row in the Enhancement page behavior list.
- Show the effective word threshold when the guard is enabled.
- Show that trigger-word prompt selection can still run enhancement for a short transcript.
- Show an `Off` state when the short-phrase guard is disabled.
- Show a presenter-backed `Timeout Policy` row with the effective timeout and retry state.
- Keep the existing enhancement pipeline behavior, provider requests, settings persistence, and secrets unchanged.
- Keep all behavior local/testable in Core presenters; WinUI should only bind the presentation.

## Non-Goals

- Do not add new enhancement providers.
- Do not change prompt rendering or trigger-word detection.
- Do not add commercial prompt catalogs, accounts, telemetry, or paid assistant surfaces.
