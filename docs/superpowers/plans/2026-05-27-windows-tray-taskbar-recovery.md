# Windows Tray Taskbar Recovery Plan

Date: 2026-05-27

## Goal

Make the Windows notification-area icon recover after Explorer/taskbar recreation by handling the shell `TaskbarCreated` broadcast.

## Grounding

- Official source: Microsoft Learn, "The Taskbar", section "Taskbar Creation Notification".
- The documented behavior is to register the `TaskbarCreated` message and add taskbar icons again after receiving it.

## Steps

1. Add a focused Core test for a taskbar-created recovery policy.
2. Add the minimal Core policy used by native tray plumbing.
3. Add a hidden WinForms `NativeWindow` in the native tray service that registers `TaskbarCreated`.
4. Force the existing `NotifyIcon` to re-add itself by toggling visibility and restoring its icon/menu when the registered message arrives.
5. Update the completion tracker.
6. Verify with focused tests, full solution tests/build, and whitespace checks.
7. Commit the slice.

## Verification

- `dotnet test VoiceInk.Windows\tests\VoiceInk.Windows.Core.Tests\VoiceInk.Windows.Core.Tests.csproj --filter FullyQualifiedName~TrayIconRecoveryPolicyTests`
- `dotnet build VoiceInk.Windows\src\VoiceInk.Windows.Native\VoiceInk.Windows.Native.csproj -c Debug -p:Platform=x64`
- `dotnet test VoiceInk.Windows\VoiceInk.Windows.sln`
- `dotnet build VoiceInk.Windows\VoiceInk.Windows.sln -c Debug -p:Platform=x64`
- `git diff --check`
