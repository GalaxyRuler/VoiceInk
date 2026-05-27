# Windows Context Unavailable Source Guidance Spec

Date: 2026-05-27

## Context

VoiceInk for Windows already exposes clipboard, selected text, active app/site, and OCR context readiness with privacy rows for capture timing, transient clipboard/selection handling, OCR boundary, toggles, provider boundary, and Windows capture consent. The remaining parity gap for this slice is graceful degradation visibility: users should know context sources can disappear because of Windows permissions, app boundaries, or screen capture limitations without blocking dictation.

Microsoft guidance notes that Windows permissions can be changed by users, and screen capture flows use secure consent UI and visible capture boundaries. VoiceInk should disclose that unavailable context sources are skipped while available sources continue.

## Requirements

- The Context Awareness presenter must include an `Unavailable Sources` privacy row.
- The row must say blocked or unavailable sources are omitted and VoiceInk continues with available context.
- The row must be present even when enhancement/OCR are off, because the behavior applies to baseline context probing.
- The row must be core presenter output with unit-test coverage.

## Non-Goals

- Do not add new Windows permission probes in this slice.
- Do not change prompt rendering order.
- Do not change OCR capture behavior.
- Do not add telemetry or cloud fallback.

## Acceptance Criteria

- `EnhancementContextReadinessPresenter.Present` includes `Unavailable Sources` in baseline privacy rows.
- The focused Context readiness presenter test covers title, value, detail, and status badge.
- Project completion documentation reflects the context unavailable-source slice.
- Focused tests, full solution tests, Debug x64 build, and whitespace validation pass.
