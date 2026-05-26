# Windows Context Provider Boundary Design

## Context

VoiceInk enhancement prompts can include active app/site details, selected text, clipboard text, custom vocabulary, and optional local OCR text. The Windows fork already explains capture timing and local OCR boundaries, but provider-specific disclosure matters because cloud enhancement providers receive the rendered prompt while local providers keep it on the PC.

The macOS source of truth uses context to improve spelling and formatting while preserving a local-first user expectation. Windows should make the same boundary visible in the Enhancement settings without adding telemetry, account flows, or commercial gating.

## Goal

Show a provider-aware privacy row whenever Enhancement is enabled:

- cloud enhancement providers show that enabled context can be included in prompts sent to the selected provider;
- local providers such as Ollama and Local CLI show that enabled context stays on this PC;
- the row uses stable presenter output so it is testable without UI automation.

## Non-Goals

- No change to provider request payloads.
- No new context capture source.
- No settings schema migration.
- No telemetry, cloud upload outside the chosen enhancement provider, or paid-provider gating.

## Testability

`EnhancementContextReadinessPresenter` emits provider boundary rows covered by focused Core presenter tests for both cloud and local enhancement providers.
