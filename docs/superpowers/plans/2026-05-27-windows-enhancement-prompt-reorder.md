# Windows Enhancement Prompt Reordering Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use `superpowers:test-driven-development`.

**Goal:** Add Windows-native Move Up / Move Down controls for custom enhancement prompt ordering.

---

## Task 1: Red Tests

- [x] Add Core tests for custom prompt reorder and predefined/boundary no-op behavior.
- [x] Add static XAML/code coverage for Move Up / Move Down controls.
- [x] Confirm focused tests fail before implementation.

## Task 2: Core And UI Implementation

- [x] Add `EnhancementPromptLibrary.MovePrompt`.
- [x] Add Move Up / Move Down controls and click handlers.
- [x] Persist reordered prompts through the existing settings path while keeping the moved prompt selected.
- [x] Run focused tests.

## Task 3: Verification And Commit

- [x] Run full solution tests.
- [x] Run Debug x64 solution build.
- [x] Run whitespace check and commit the slice.

## Verification Notes

- Online grounding: WinUI ComboBox is a selection control, while item reordering is ListView-style behavior; this slice uses explicit buttons to keep the existing prompt editor simple and accessible.
- Red: focused tests failed because `EnhancementPromptLibrary.MovePrompt` and the prompt move controls were missing.
- Green: focused prompt reorder tests passed after adding the Core reorder helper and editor controls.
- Full test: solution test passed with 774 Core tests and 266 Infrastructure tests.
- Build: Debug x64 solution build passed with 0 warnings and 0 errors.
- Whitespace: `git diff --check` passed with only line-ending normalization warnings.
