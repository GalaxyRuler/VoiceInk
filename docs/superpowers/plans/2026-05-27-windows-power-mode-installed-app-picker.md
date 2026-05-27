# Windows Power Mode Installed App Picker Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use `superpowers:test-driven-development`.

**Goal:** Add a conservative installed app picker that appends discovered process targets to Power Mode rules.

---

## Task 1: Red Tests

- [x] Add Core presenter tests for sorting, de-duplicating, and dropping unusable installed-app choices.
- [x] Add Core append tests proving selected app process names append to existing Power Mode process targets without duplicates.
- [x] Run the focused tests and confirm failure before implementation.

## Task 2: Core And Native Plumbing

- [x] Add installed application choice models and provider interface in Core.
- [x] Add a presenter that normalizes choices and applies selected choices to Power Mode target fields.
- [x] Add a native Start menu shortcut provider that resolves `.lnk` target processes best-effort.

## Task 3: WinUI Integration

- [x] Add installed app combo/button controls to the Power Mode editor.
- [x] Populate choices on startup and append selected app process targets from the button.
- [x] Update the completion tracker.

## Task 4: Verification And Commit

- [x] Run focused Power Mode tests.
- [x] Run full solution tests/build and whitespace check.
- [x] Commit the slice.

## Verification Notes

- Online grounding: Microsoft documents AppX package enumeration and AUMID discovery, while Start Menu shortcuts remain the practical local discovery surface for Win32 app process targets.
- RED: `& '..\.dotnet-sdk-10\dotnet.exe' test VoiceInk.Windows\tests\VoiceInk.Windows.Core.Tests\VoiceInk.Windows.Core.Tests.csproj --filter 'FullyQualifiedName~PowerModeInstalledApplicationPresenterTests'` failed because `PowerModeInstalledApplicationPresenter` and `PowerModeInstalledApplicationChoice` did not exist.
- Focused GREEN: the same command passed 2 tests.
- Build check during implementation: `& '..\.dotnet-sdk-10\dotnet.exe' build VoiceInk.Windows\VoiceInk.Windows.sln -c Debug -p:Platform=x64` succeeded with 0 warnings and 0 errors.
- Full tests: `& '..\.dotnet-sdk-10\dotnet.exe' test VoiceInk.Windows\VoiceInk.Windows.sln` passed 711 Core tests and 266 Infrastructure tests.
- Full build: `& '..\.dotnet-sdk-10\dotnet.exe' build VoiceInk.Windows\VoiceInk.Windows.sln -c Debug -p:Platform=x64` succeeded with 0 warnings and 0 errors.
