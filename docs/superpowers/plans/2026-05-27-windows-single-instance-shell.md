# Windows Single Instance Shell Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use `superpowers:test-driven-development`.

**Goal:** Make the Windows shell single-instanced so duplicate launches restore the existing VoiceInk window.

---

## Task 1: Red Shell Lifecycle Tests

- [x] Add static App lifecycle coverage for `AppInstance.FindOrRegisterForKey`, activation redirection, duplicate process exit, and redirected activation restore.
- [x] Add static MainWindow coverage for an internal redirected-activation restore method.
- [x] Confirm the focused tests fail before implementation.

## Task 2: Single Instance Wiring

- [x] Register a stable VoiceInk app instance key before creating `MainWindow`.
- [x] Redirect secondary activations and exit the duplicate instance.
- [x] Restore the existing main window on redirected activation.
- [x] Run focused shell lifecycle tests.

## Task 3: Verification And Commit

- [x] Run full solution tests.
- [x] Run Debug x64 solution build.
- [x] Run whitespace check and commit the slice.

## Verification Notes

- First red run caught a test harness missing `using Xunit`; after fixing the harness, focused tests failed on missing `Microsoft.Windows.AppLifecycle` and activation restore wiring.
- Focused shell lifecycle tests passed after implementation: 2 tests.
- Debug x64 solution build passed with 0 warnings and 0 errors after routing redirected activation through `MainWindow.DispatcherQueue`.
- Full solution tests passed: Core 749, Infrastructure 266.
- Final Debug x64 solution build passed with 0 warnings and 0 errors.
- `git diff --check` passed with only LF-to-CRLF normalization warnings.
