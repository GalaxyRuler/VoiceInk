# Windows Shortcut Session Recovery Plan

Date: 2026-05-27

## Goal

Polish rare native shortcut lifecycle behavior by clearing transient pressed shortcut state at Windows lock/unlock/desktop-ready boundaries.

## Grounding

- Official source: Microsoft Learn, `WTSRegisterSessionNotification`.
- Official source: Microsoft Learn, `WM_WTSSESSION_CHANGE`.
- The Windows app already subclasses the main window handle for `RegisterHotKey`; session notifications can be handled in the same native window wrapper.

## Steps

1. Add a focused Core test for session-change events that should reset pressed shortcut state.
2. Add a small Core policy with the relevant `WM_WTSSESSION_CHANGE` and WTS event constants.
3. Register the native hotkey window for current-session notifications.
4. Clear pressed recording shortcut state, mini-recorder shortcut state, and pending middle-click timers on lock, unlock, and desktop-ready events.
5. Unregister notifications before releasing the native window handle.
6. Update the completion tracker.
7. Verify with focused tests, native build, full solution tests/build, and whitespace checks.
8. Commit the slice.

## Verification

- `dotnet test VoiceInk.Windows\tests\VoiceInk.Windows.Core.Tests\VoiceInk.Windows.Core.Tests.csproj --filter FullyQualifiedName~GlobalShortcutSessionChangePolicyTests`
- `dotnet build VoiceInk.Windows\src\VoiceInk.Windows.Native\VoiceInk.Windows.Native.csproj -c Debug -p:Platform=x64`
- `dotnet test VoiceInk.Windows\VoiceInk.Windows.sln`
- `dotnet build VoiceInk.Windows\VoiceInk.Windows.sln -c Debug -p:Platform=x64`
- `git diff --check`
