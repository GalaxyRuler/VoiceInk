# Windows Context Prompt Spelling Priority Plan

## Task 1: Red Prompt Test

- [x] Add a focused `EnhancementPromptTests` case expecting a macOS-style phonetic/context spelling priority instruction.
- [x] Run the focused test and confirm it fails because the instruction is absent.

## Task 2: Prompt Renderer Implementation

- [x] Add the instruction to `EnhancementPromptRenderer` system instructions.
- [x] Keep Assistant Mode raw prompt behavior unchanged.

## Verification Notes

- RED: focused `Render_InstructsEnhancementToPrioritizeContextSpellingForPhoneticMatches` failed because the instruction was absent.
- GREEN: focused `EnhancementPromptTests` passed after adding the system-instruction rule.
- TEST CONTRACT FIX: full tests exposed that `EnhanceAsync_WhenClipboardContextProviderFails_EnhancesWithoutContext` treated any `<CLIPBOARD_CONTEXT>` mention as a rendered context block. The test now checks for absence of the rendered closing block, preserving the no-context assertion while allowing macOS-aligned tag references in instructions.
- FOCUSED RECOVERY: `EnhancementPromptTests` plus the adjusted clipboard-failure pipeline test passed 26/26.
- FULL TEST: `dotnet test VoiceInk.Windows\VoiceInk.Windows.sln -nr:false -p:UseSharedCompilation=false` passed with Core 809/809 and Infrastructure 267/267.
- BUILD: `dotnet build VoiceInk.Windows\VoiceInk.Windows.sln -c Debug -p:Platform=x64 -nr:false -p:UseSharedCompilation=false` succeeded with 0 warnings and 0 errors.
- DIFF CHECK: `git diff --check` exited 0 with only LF-to-CRLF normalization warnings.

## Task 3: Docs, Verification, Review, Commit

- [x] Update the project completion tracker.
- [x] Run focused Enhancement prompt tests.
- [x] Run full solution tests and Debug x64 build sequentially.
- [x] Run `git diff --check`.
- [ ] Commit and request code review.
