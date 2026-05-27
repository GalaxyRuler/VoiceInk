# Windows Audio Input Diagnostics Guidance Plan

Date: 2026-05-27

## Goal

Improve Windows audio input troubleshooting parity by surfacing microphone privacy guidance when prioritized microphones are unavailable and VoiceInk falls back to System Default.

## Steps

1. Add a failing assertion to the prioritized-unavailable device health test for a microphone privacy row.
2. Add the `Microphone Privacy` informational row beside the existing Windows Sound Settings row in `AudioInputDeviceHealthPresenter`.
3. Run the focused audio presenter test.
4. Update the project completion tracker and slice documentation.
5. Run the full verification gate:
   - `dotnet test VoiceInk.Windows\VoiceInk.Windows.sln`
   - `dotnet build VoiceInk.Windows\VoiceInk.Windows.sln -c Debug -p:Platform=x64`
   - `git diff --check`
6. Commit the completed slice.

## Review Notes

- Keep this as guidance only; do not mutate system privacy settings.
- Keep the row in core presenter output rather than hardcoding it in WinUI.
- Preserve existing System Default fallback behavior.
