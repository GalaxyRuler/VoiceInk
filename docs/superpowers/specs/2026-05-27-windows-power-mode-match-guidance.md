# Windows Power Mode Match Guidance Design

## Context

VoiceInk for Windows supports Power Mode rules that match process name, window title, and sanitized browser URL, with explicit recorder/tray/shortcut selection and default fallback behavior. The Power Mode page exposed manual switching and rule rows, but it did not spell out matching precedence at the page level.

Windows per-application rule UX is clearest when users can see that process/title/URL rules are evaluated in order and default rules are fallback-only. The Windows fork should make that behavior visible in the Power Mode page without duplicating matcher logic.

## Goal

Add page-level Power Mode match guidance:

- specific process, title, and sanitized URL rules are checked in list order;
- the default fallback applies only when no specific rule matches;
- guidance is emitted by `PowerModePagePresenter`;
- the WinUI Power Mode page renders the guidance string.

## Non-Goals

- No change to matching behavior.
- No rule persistence change.
- No shortcut behavior change.
- No telemetry or cloud rule evaluation.

## Testability

Focused Power Mode presenter tests cover the match guidance text.
