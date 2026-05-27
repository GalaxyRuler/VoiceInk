# Windows Context Accessible Rows Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use `superpowers:test-driven-development`.

**Goal:** Add presenter-backed accessible names to Enhancement context rows and bind them in WinUI.

---

## Task 1: Red Tests

- [x] Add presenter coverage for readiness/action/privacy `AccessibleName` values.
- [x] Add static XAML coverage for `EnhancementContextReadinessListView`.
- [x] Add static XAML coverage for `EnhancementContextPrivacyListView`.
- [x] Add static XAML coverage for `EnhancementContextActionsListView`.
- [x] Confirm focused tests fail before implementation.

## Task 2: Core And XAML Implementation

- [x] Add accessible-name helper/property plumbing to the three Enhancement context row records.
- [x] Bind the three context ListView templates to `AccessibleName`.
- [x] Run focused presenter and XAML accessibility tests.

## Task 3: Verification And Commit

- [x] Run full solution tests.
- [x] Run Debug x64 solution build.
- [x] Run whitespace check and commit the slice.

## Verification Notes

- Red: `dotnet test VoiceInk.Windows.Core.Tests.csproj --filter "EnhancementContextReadinessPresenterTests.Present_RowsExposeAccessibleNames|AppXamlAccessibilityTests.MainWindow_EnhancementContextRows_BindAccessibleName"` failed with CS1061 because the three Enhancement context row records did not expose `AccessibleName`.
- Green: same focused command passed, 4 tests.
- Full: `dotnet test VoiceInk.Windows.sln` passed, 756 Core tests and 266 Infrastructure tests.
- Build: `dotnet build VoiceInk.Windows.sln -c Debug -p:Platform=x64` passed with 0 warnings and 0 errors.
- Whitespace: `git diff --check` passed with only expected LF-to-CRLF working-copy warnings.
