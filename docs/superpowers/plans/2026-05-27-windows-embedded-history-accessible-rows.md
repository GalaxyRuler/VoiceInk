# Windows Embedded History Accessible Rows Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use `superpowers:test-driven-development`.

**Goal:** Add accessible row names to the embedded main-window History list and analysis rows.

---

## Task 1: Red Tests

- [x] Add presenter coverage for `HistoryAnalysisRow.AccessibleName`.
- [x] Add static XAML coverage for embedded `HistoryListView`.
- [x] Add static XAML coverage for `HistoryAnalysisListView`.
- [x] Add static code coverage for the embedded History row accessible-name helper.
- [x] Confirm focused tests fail before implementation.

## Task 2: Core And XAML Implementation

- [x] Add accessible-name property/helper to `HistoryAnalysisRow`.
- [x] Add embedded History row display/accessibility model in `MainWindow`.
- [x] Bind embedded History list and analysis templates to `AccessibleName`.
- [x] Run focused presenter and XAML accessibility tests.

## Task 3: Verification And Commit

- [x] Run full solution tests.
- [x] Run Debug x64 solution build.
- [x] Run whitespace check and commit the slice.

## Verification Notes

- Red: `dotnet test VoiceInk.Windows.Core.Tests.csproj --filter "HistoryAnalysisPresenterTests.Present_RowsExposeAccessibleNames|AppXamlAccessibilityTests.MainWindow_HistoryRows_BindAccessibleName|AppXamlAccessibilityTests.MainWindow_HistoryRows_UseAccessibleNameModel"` failed with CS1061 because `HistoryAnalysisRow` did not expose `AccessibleName`.
- Green: same focused command passed, 4 tests.
- Full: `dotnet test VoiceInk.Windows.sln` passed, 768 Core tests and 266 Infrastructure tests.
- Build: `dotnet build VoiceInk.Windows.sln -c Debug -p:Platform=x64` passed with 0 warnings and 0 errors.
- Whitespace: `git diff --check` passed with only expected LF-to-CRLF working-copy warnings.
