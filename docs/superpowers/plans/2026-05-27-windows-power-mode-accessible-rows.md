# Windows Power Mode Accessible Rows Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use `superpowers:test-driven-development`.

**Goal:** Add presenter-backed accessible names to Power Mode rule rows and bind them in WinUI.

---

## Task 1: Red Tests

- [x] Add presenter coverage for `PowerModeRuleRowPresentation.AccessibleName`.
- [x] Add static XAML coverage for `PowerModeRulesListView`.
- [x] Confirm focused tests fail before implementation.

## Task 2: Core And XAML Implementation

- [x] Add accessible-name property/helper to `PowerModeRuleRowPresentation`.
- [x] Bind the Power Mode rule template to `AccessibleName`.
- [x] Run focused presenter and XAML accessibility tests.

## Task 3: Verification And Commit

- [x] Run full solution tests.
- [x] Run Debug x64 solution build.
- [x] Run whitespace check and commit the slice.

## Verification Notes

- Red: `dotnet test VoiceInk.Windows.Core.Tests.csproj --filter "PowerModePagePresenterTests.Present_RuleRowsExposeAccessibleNames|AppXamlAccessibilityTests.MainWindow_PowerModeRules_BindAccessibleName"` failed with CS1061 because `PowerModeRuleRowPresentation` did not expose `AccessibleName`.
- Green: same focused command passed, 2 tests.
- Full: `dotnet test VoiceInk.Windows.sln` passed, 764 Core tests and 266 Infrastructure tests.
- Build: first `dotnet build VoiceInk.Windows.sln -c Debug -p:Platform=x64` hit a transient `Microsoft.UI.Xaml.Markup.Compiler` file lock on `intermediatexaml\VoiceInk.Windows.App.dll`; the named process had already exited, and the exact retry passed with 0 warnings and 0 errors.
- Whitespace: `git diff --check` passed with only expected LF-to-CRLF working-copy warnings.
