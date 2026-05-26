# Windows Cloud Provider Cards Design

## Goal

Bring the Windows AI Models cloud-provider section closer to the macOS VoiceInk model cards by showing useful provider metadata instead of only raw endpoint/model fields.

## Grounding

The macOS `CloudModelCardView` shows each cloud model with provider, language, speed, accuracy, streaming capability, description, API-key state, and configure/set-default actions. Windows already has provider selection, model selection, secure key storage, and apply actions. This slice adds the missing provider-card metadata while preserving the existing simple WinUI controls.

## Requirements

- Each Windows cloud transcription preset exposes:
  - description;
  - language coverage;
  - speed label;
  - accuracy label;
  - batch or realtime-preview capability label.
- The AI Models page shows the selected cloud provider's metadata beside the provider selector.
- Existing endpoint/model/API-key controls remain available.
- No commercial wording, upgrade prompts, trials, accounts, paid gates, or telemetry are introduced.

## Non-Goals

- No live API-key verification calls.
- No provider-specific account setup pages.
- No new dependencies or icon libraries.
- No removal of the existing custom endpoint path.

## Verification

- Core catalog tests assert nonblank card metadata and streaming labels for providers with Windows live-preview support.
- Debug x64 build validates the XAML bindings compile.
