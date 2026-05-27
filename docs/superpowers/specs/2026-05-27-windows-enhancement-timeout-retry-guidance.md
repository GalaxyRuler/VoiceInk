# Windows Enhancement Timeout Retry Guidance Spec

Date: 2026-05-27

## Context

VoiceInk for macOS makes enhancement feel predictable: prompt providers are user-selected, retries are bounded, and failures do not quietly move private context to a different service. The Windows fork already supports enhancement timeouts, retries, secure keys, local providers, and explicit no-automatic-provider-fallback behavior. The remaining parity gap for this slice is user-visible guidance in the context readiness surface.

Current Microsoft .NET resilience guidance treats retry and timeout as explicit policies with a total request bound and per-attempt handling for transient failures. OpenAI-compatible providers also require clear user expectations around request failures and rate limits. Windows VoiceInk should expose this behavior in privacy/readiness language without adding a commercial or telemetry surface.

## Requirements

- When enhancement is enabled, the Context Awareness readiness presenter must disclose that enhancement timeout and retry behavior is bounded.
- The disclosure must sit next to provider-boundary and provider-fallback guidance because all three explain where prompt/context data can go during enhancement.
- The row must say failures keep the original dictation text rather than changing providers.
- The row must be core/view-model data, not WinUI-only text, so it remains testable and shell-independent.
- No paid-provider fallback, upgrade prompt, commercial telemetry, or automatic routing may be introduced.

## Non-Goals

- Do not change provider retry counts or timeout settings in this slice.
- Do not add a new dependency for resilience policies.
- Do not perform live cloud API smoke tests; those remain user-key/manual tests.

## Acceptance Criteria

- `EnhancementContextReadinessPresenter.Present` includes a `Timeout and Retry` privacy row when `IsEnhancementEnabled` is true.
- Tests cover the exact title, value, detail, and status badge for the new row.
- Project completion documentation reflects the slice and the AI enhancement parity increase.
- Focused tests, full solution tests, Debug x64 build, and whitespace validation pass.
