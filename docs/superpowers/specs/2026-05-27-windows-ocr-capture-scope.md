# Windows OCR Capture Scope Design

## Context

VoiceInk's Windows enhancement context can include local screen OCR text. The existing readiness rows show whether OCR is off, full-screen, or constrained to a region, but the privacy section did not separately name the capture scope that could feed prompt rendering.

Windows screen capture APIs and privacy controls make capture scope an important user-facing boundary. The Windows fork should make full-screen capture, selected-region capture, and missing-region setup states visible without adding telemetry, account flows, or commercial gating.

## Goal

Show an OCR-specific privacy row whenever OCR context is enabled:

- full-screen OCR shows that visible screen text can be captured locally before prompt rendering;
- selected-region OCR shows that only the configured rectangle is captured before local text recognition;
- invalid constrained OCR shows that a valid rectangle is required before constrained capture can run;
- the row is produced by `EnhancementContextReadinessPresenter` so it is testable without UI automation.

## Non-Goals

- No new OCR capture implementation.
- No change to prompt rendering or provider payloads.
- No settings schema migration.
- No background screen capture while idle.

## Testability

Focused Core presenter tests cover full-screen, selected-region, and missing-region OCR capture scope privacy rows.
