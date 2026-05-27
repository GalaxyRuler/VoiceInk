# Windows Enhancement Reasoning Parameters Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use `superpowers:test-driven-development`.

**Goal:** Add macOS-aligned provider/model reasoning parameters to Windows enhancement request construction.

---

## Task 1: Red Request Tests

- [x] Add infrastructure tests that expect OpenAI/Gemini `reasoning_effort`, Cerebras `reasoning_format`, and Groq `include_reasoning` request fields.
- [x] Run the focused enhancement service tests and confirm at least one new assertion fails because the fields are not sent yet.

## Task 2: Mapping Implementation

- [x] Add provider/model mapping equivalent to the macOS `ReasoningConfig`.
- [x] Include optional request fields only when the mapping applies.
- [x] Preserve existing OpenAI-compatible, Anthropic, Ollama, and Local CLI behavior.

## Task 3: Verification And Commit

- [x] Run focused enhancement service tests.
- [x] Run full solution tests/build and whitespace check.
- [x] Update the project completion bar and commit the slice.

## Verification Notes

- Red focused enhancement tests failed for all new cases because `reasoning_effort` was absent from request JSON.
- Focused reasoning-parameter tests passed: 5 cases.
- Focused enhancement service tests passed: 21 tests.
- Full solution tests passed: 934 tests.
- Full Debug x64 solution build passed: 0 warnings, 0 errors.
- `git diff --check` passed with only LF-to-CRLF working-copy warnings.
