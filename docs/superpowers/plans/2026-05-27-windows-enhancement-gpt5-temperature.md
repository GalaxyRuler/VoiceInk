# Windows Enhancement GPT-5 Temperature Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use `superpowers:test-driven-development`.

**Goal:** Make Windows enhancement pipeline temperature selection match macOS for OpenAI GPT-5 models.

---

## Task 1: Red Pipeline Test

- [x] Add Core pipeline coverage for OpenAI `gpt-5.4` expecting `Temperature = 1.0`.
- [x] Include guard cases for OpenAI `gpt-4.1` and Custom `gpt-5.4` remaining at `0.3`.
- [x] Run focused pipeline tests and confirm the OpenAI GPT-5 case fails before implementation.

## Task 2: Temperature Mapping

- [x] Add a small provider/model temperature resolver in `TextEnhancementPipeline`.
- [x] Resolve the provider through `EnhancementProviderPresetCatalog` so only the OpenAI preset receives GPT-5 temperature handling.
- [x] Preserve the existing `0.3` default for every other provider/model pair.

## Task 3: Verification And Commit

- [x] Run focused enhancement pipeline tests.
- [x] Run full Core/Infrastructure tests.
- [x] Run Debug x64 solution build.
- [x] Run whitespace check and commit the slice.

## Verification Notes

- Red focused pipeline tests failed because OpenAI `gpt-5.4` still used `0.3`.
- Focused pipeline tests passed: 16 tests.
- Full solution tests passed: 731 Core tests and 266 Infrastructure tests.
- Full Debug x64 solution build passed: 0 warnings, 0 errors.
- `git diff --check` passed with only LF-to-CRLF working-copy warnings.
