# Windows Shortcut Reserved Key Guidance Plan

Date: 2026-05-27

## Goal

Reject Windows-reserved F12 global shortcuts before native registration.

## Steps

1. Add a failing shortcut parser test for `Ctrl+F12`.
2. Add F12 rejection to typed parsing and captured-key shortcut creation.
3. Run the focused shortcut test suite.
4. Update the project completion tracker and slice documentation.
5. Run the full verification gate:
   - `dotnet test VoiceInk.Windows\VoiceInk.Windows.sln`
   - `dotnet build VoiceInk.Windows\VoiceInk.Windows.sln -c Debug -p:Platform=x64`
   - `git diff --check`
6. Commit the completed slice.

## Review Notes

- Keep other function keys available.
- Keep the message aligned with Windows reserved-key wording.
- Native runtime registration can still report conflicts for shortcuts owned by other apps.
