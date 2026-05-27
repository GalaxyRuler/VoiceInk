# Windows Power Mode OCR Context Override Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use `superpowers:test-driven-development`.

**Goal:** Add per-rule OCR context override parity to Power Mode.

---

## Task 1: Red Core Tests

- [x] Add matcher tests for enabling and disabling `UseOcrContext` from a matched Power Mode rule.
- [x] Add page presenter test coverage for the OCR/context override summary.
- [x] Run focused Power Mode tests and confirm failures before implementation.

## Task 2: Rule, Matcher, And UI

- [x] Add nullable `UseOcrContextOverride` to `PowerModeRule`.
- [x] Apply it in `PowerModeMatcher`.
- [x] Include it in Power Mode row override summaries.
- [x] Bind it to a tri-state WinUI checkbox.

## Task 3: Verification And Commit

- [x] Run focused Power Mode tests.
- [x] Run full solution tests/build and whitespace check.
- [x] Update the project completion bar and commit the slice.

## Verification Notes

- Red check: `& '..\.dotnet-sdk-10\dotnet.exe' test VoiceInk.Windows\tests\VoiceInk.Windows.Core.Tests\VoiceInk.Windows.Core.Tests.csproj --filter 'FullyQualifiedName~PowerModeMatcherTests.Resolve_AppliesOcrContextOverrideWithoutChangingBaseSettings|FullyQualifiedName~PowerModePagePresenterTests.Present_BuildsRuleRowsWithOverrideSummaries'` failed with `CS0117` because `PowerModeRule` did not yet expose `UseOcrContextOverride`.
- Focused check: `& '..\.dotnet-sdk-10\dotnet.exe' test VoiceInk.Windows\tests\VoiceInk.Windows.Core.Tests\VoiceInk.Windows.Core.Tests.csproj --filter 'FullyQualifiedName~PowerModeMatcherTests|FullyQualifiedName~PowerModePagePresenterTests|FullyQualifiedName~PowerModeRuleValidatorTests'` passed 25 tests.
- Persistence check: `& '..\.dotnet-sdk-10\dotnet.exe' test VoiceInk.Windows\tests\VoiceInk.Windows.Infrastructure.Tests\VoiceInk.Windows.Infrastructure.Tests.csproj --filter 'FullyQualifiedName~JsonSettingsStoreTests.SaveAsync_PersistsSettings'` passed 1 test after adding the override to the settings round-trip fixture.
- Full test: `& '..\.dotnet-sdk-10\dotnet.exe' test VoiceInk.Windows\VoiceInk.Windows.sln` passed 698 Core tests and 247 Infrastructure tests.
- Build: `& '..\.dotnet-sdk-10\dotnet.exe' build VoiceInk.Windows\VoiceInk.Windows.sln -c Debug -p:Platform=x64` succeeded with 0 warnings and 0 errors.
