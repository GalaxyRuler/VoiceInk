# Windows Prompt Trigger Labels Plan

**Goal:** Show macOS-style trigger-word hints in Windows prompt picker labels.

## Steps

- [x] Add failing Core tests for one-trigger and multi-trigger prompt choice labels.
- [x] Add `EnhancementPromptLibrary.PromptChoiceLabel`.
- [x] Bind the Enhancement prompt picker to the prompt choice labels.
- [x] Bind the Power Mode prompt override picker to the prompt choice labels.
- [x] Update the parity spec and project completion tracker.
- [ ] Run focused tests, full solution tests, Debug x64 build, and whitespace check.
- [ ] Commit the completed slice and request review.

## Verification

- Red focused prompt-label tests failed at compile time because `PromptChoiceLabel` did not exist.
- Green focused prompt-label tests passed: 2 tests.

## References

- `VoiceInk/Models/CustomPrompt.swift` for trigger-word prompt-card hints.
- VoiceInk public docs for enhancement trigger-word behavior.
