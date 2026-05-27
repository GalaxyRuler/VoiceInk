# Windows History Accessible List Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use `superpowers:test-driven-development`.

**Goal:** Add explicit UI Automation names to dedicated History window list rows.

---

## Task 1: Red History Accessibility Tests

- [x] Add static XAML coverage expecting `HistoryListView` to define an item template with `AutomationProperties.Name`.
- [x] Add static code coverage expecting `HistoryWindowListRow` to expose `AccessibleName`.
- [x] Confirm focused tests fail before implementation.

## Task 2: Row Template And Names

- [x] Add `HistoryListView` item template with bound display text and accessible name.
- [x] Add row accessible-name construction in `HistoryWindow`.
- [x] Run focused accessibility tests.

## Task 3: Verification And Commit

- [x] Run full solution tests.
- [x] Run Debug x64 solution build.
- [x] Run whitespace check and commit the slice.

## Verification Notes

- Red: `dotnet test VoiceInk.Windows.Core.Tests.csproj --filter "AppXamlAccessibilityTests.HistoryWindow_HistoryRows_BindAccessibleName"` failed because `HistoryListView` had no `AutomationProperties.Name` item template.
- Green: `dotnet test VoiceInk.Windows.Core.Tests.csproj --filter "AppXamlAccessibilityTests.HistoryWindow_HistoryRows_BindAccessibleName"` passed, 1 test.
- Full: `dotnet test VoiceInk.Windows.sln` passed, 750 Core tests and 266 Infrastructure tests.
- Build: `dotnet build VoiceInk.Windows.sln -c Debug -p:Platform=x64` passed with 0 warnings and 0 errors.
- Whitespace: `git diff --check` passed with only expected LF-to-CRLF working-copy warnings.
