# Windows Permissions Accessible Rows Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use `superpowers:test-driven-development`.

**Goal:** Add presenter-backed accessible names to Permissions readiness rows and bind them in WinUI.

---

## Task 1: Red Tests

- [x] Add presenter coverage for `PermissionReadinessItem.AccessibleName`.
- [x] Add static XAML coverage for `PermissionsChecklistListView`.
- [x] Confirm focused tests fail before implementation.

## Task 2: Core And XAML Implementation

- [x] Add accessible-name property/helper to `PermissionReadinessItem`.
- [x] Bind the Permissions checklist template to `AccessibleName`.
- [x] Run focused presenter and XAML accessibility tests.

## Task 3: Verification And Commit

- [x] Run full solution tests.
- [x] Run Debug x64 solution build.
- [x] Run whitespace check and commit the slice.

## Verification Notes

- Red: `dotnet test VoiceInk.Windows.Core.Tests.csproj --filter "PermissionsReadinessPresenterTests.Build_ItemsExposeAccessibleNames|AppXamlAccessibilityTests.MainWindow_PermissionsChecklistRows_BindAccessibleName"` failed with CS1061 because `PermissionReadinessItem` did not expose `AccessibleName`.
- Green: same focused command passed, 2 tests.
- Full: `dotnet test VoiceInk.Windows.sln` passed, 762 Core tests and 266 Infrastructure tests.
- Build: `dotnet build VoiceInk.Windows.sln -c Debug -p:Platform=x64` passed with 0 warnings and 0 errors.
- Whitespace: `git diff --check` passed with only expected LF-to-CRLF working-copy warnings.
