# Windows Dictionary Accessible Rows Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use `superpowers:test-driven-development`.

**Goal:** Bind existing Dictionary row accessible names into WinUI templates.

---

## Task 1: Red XAML Accessibility Tests

- [x] Add static coverage for `DictionarySummaryListView`.
- [x] Add static coverage for `DictionaryRuleGuidanceListView`.
- [x] Add static coverage for `VocabularyListView`.
- [x] Add static coverage for `ReplacementListView`.
- [x] Confirm focused tests fail before implementation.

## Task 2: Template Bindings

- [x] Bind summary rows to `AccessibleName`.
- [x] Bind rule guidance rows to `AccessibleName`.
- [x] Bind vocabulary rows to `AccessibleName`.
- [x] Bind replacement rows to `AccessibleName`.
- [x] Run focused accessibility tests.

## Task 3: Verification And Commit

- [x] Run full solution tests.
- [x] Run Debug x64 solution build.
- [x] Run whitespace check and commit the slice.

## Verification Notes

- Red: `dotnet test VoiceInk.Windows.Core.Tests.csproj --filter "AppXamlAccessibilityTests.MainWindow_DictionaryRows_BindAccessibleName"` failed for all four Dictionary lists because their templates did not bind `AutomationProperties.Name`.
- Green: same focused command passed, 4 tests.
- Full: `dotnet test VoiceInk.Windows.sln` passed, 760 Core tests and 266 Infrastructure tests.
- Build: `dotnet build VoiceInk.Windows.sln -c Debug -p:Platform=x64` passed with 0 warnings and 0 errors.
- Whitespace: `git diff --check` passed with only expected LF-to-CRLF working-copy warnings.
