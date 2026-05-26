# Windows Context Privacy Boundaries Design

## Context

VoiceInk for Windows can add clipboard text, selected text, active app/site details, and OCR text to enhancement prompts. The app already presents source readiness and action rows, but users still need an explicit boundary summary for when local context is captured and when it may be included in cloud enhancement calls.

Recent dictation products increasingly advertise context-aware capture while users expect clear disclosure around selected text, active windows, OCR/screenshot-like capture, and provider-bound prompt data. VoiceInk should keep the original local-first feel while making those boundaries visible.

## Goal

Add presenter-backed privacy boundary rows to the Enhancement Context area:

- local capture happens only during enhancement prompt construction;
- selected text and clipboard context are transient and best effort;
- screen OCR stays local before prompt rendering but may be included in a cloud prompt if enhancement uses a cloud provider;
- disabling context toggles prevents those sources from being requested.

## Non-Goals

- No new telemetry.
- No screenshot upload feature.
- No provider request changes.
- No settings schema changes.

## Testability

Rows are produced by `EnhancementContextReadinessPresenter`, covered by Core presenter tests, and rendered read-only by WinUI.
