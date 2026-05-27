# Windows Metrics Model Performance Accessible Rows Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use `superpowers:test-driven-development`.

**Goal:** Bind existing model-performance accessible names into Metrics WinUI row templates.

---

## Task 1: Red XAML Accessibility Tests

- [x] Add focused static test coverage for `TranscriptionModelPerformanceListView`.
- [x] Add focused static test coverage for `EnhancementModelPerformanceListView`.
- [x] Confirm the focused tests fail before implementation.

## Task 2: Template Bindings

- [x] Add `AutomationProperties.Name="{Binding AccessibleName}"` to the transcription model performance template root.
- [x] Add `AutomationProperties.Name="{Binding AccessibleName}"` to the enhancement model performance template root.
- [x] Run focused accessibility tests.

## Task 3: Verification And Commit

- [x] Run full solution tests.
- [x] Run Debug x64 solution build.
- [x] Run whitespace check and commit the slice.

## Verification Notes

- Red: `dotnet test VoiceInk.Windows.Core.Tests.csproj --filter "AppXamlAccessibilityTests.MainWindow_ModelPerformanceRows_BindAccessibleName"` failed for both model performance lists because their templates did not bind `AutomationProperties.Name`.
- Green: `dotnet test VoiceInk.Windows.Core.Tests.csproj --filter "AppXamlAccessibilityTests.MainWindow_ModelPerformanceRows_BindAccessibleName"` passed, 2 tests.
- Full: `dotnet test VoiceInk.Windows.sln` passed, 752 Core tests and 266 Infrastructure tests.
- Build: `dotnet build VoiceInk.Windows.sln -c Debug -p:Platform=x64` passed with 0 warnings and 0 errors.
- Whitespace: `git diff --check` passed with only expected LF-to-CRLF working-copy warnings.
