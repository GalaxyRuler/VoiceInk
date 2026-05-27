# Windows Dictionary Provider Boundary

## Goal

Make the Dictionary page explicit about the privacy boundary between vocabulary prompt context and local replacement cleanup.

## Source Of Truth

- The macOS app uses custom vocabulary as prompt context for AI enhancement.
- Windows already renders vocabulary into transcription/enhancement prompt paths and applies word replacements locally after transcription.
- VoiceInk's open-source Windows fork must avoid hidden commercial or telemetry behavior and explain when user-owned cloud providers may receive context.

## Requirements

- Add a presenter-backed Dictionary rule guidance row for provider privacy boundaries.
- The row must explain that vocabulary may be included in prompts sent to the selected enhancement or transcription provider.
- The row must explain that word replacements are applied locally after transcription.
- Keep the behavior in Core presenter logic and existing WinUI bindings.
- Do not add telemetry, accounts, paid gates, or commercial flows.
