# Windows Power Mode Active Target Append Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use `superpowers:test-driven-development`.

**Goal:** Add an active-window append flow for multi-target Power Mode rules.

---

## Task 1: Red Tests

- [x] Add Core tests for appending process/title/URL targets to existing semicolon-separated fields.
- [x] Add Core tests for duplicate and empty target handling.
- [x] Run focused tests and confirm failures before implementation.

## Task 2: Implementation

- [x] Add a testable Power Mode target field composer.
- [x] Add an `Add Active Window` button next to the existing refresh/replace actions.
- [x] Wire the button to append the current target into the rule editor fields.
- [x] Keep the existing `Use Active Window` replace behavior unchanged.

## Task 3: Verification And Commit

- [x] Run focused Power Mode tests.
- [x] Run full solution tests/build and whitespace check.
- [x] Update project completion tracker and commit the slice.

## Verification Notes

- Red check: `& '..\.dotnet-sdk-10\dotnet.exe' test VoiceInk.Windows\tests\VoiceInk.Windows.Core.Tests\VoiceInk.Windows.Core.Tests.csproj --filter 'FullyQualifiedName~PowerModeTargetFieldComposerTests'` failed because `PowerModeTargetFieldComposer` and `PowerModeTargetFields` did not exist.
- Focused check: `& '..\.dotnet-sdk-10\dotnet.exe' test VoiceInk.Windows\tests\VoiceInk.Windows.Core.Tests\VoiceInk.Windows.Core.Tests.csproj --filter 'FullyQualifiedName~PowerModeTargetFieldComposerTests|FullyQualifiedName~PowerModeMatcherTests|FullyQualifiedName~PowerModePagePresenterTests'` passed 23 tests.
- Full test: `& '..\.dotnet-sdk-10\dotnet.exe' test VoiceInk.Windows\VoiceInk.Windows.sln` passed 701 Core tests and 257 Infrastructure tests.
- Build: `& '..\.dotnet-sdk-10\dotnet.exe' build VoiceInk.Windows\VoiceInk.Windows.sln -c Debug -p:Platform=x64` succeeded with 0 warnings and 0 errors.
