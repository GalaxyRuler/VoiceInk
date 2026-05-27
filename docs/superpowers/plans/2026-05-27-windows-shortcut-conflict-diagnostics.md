# Windows Shortcut Conflict Diagnostics Plan

## Goal

Make native global shortcut registration conflicts understandable and actionable while preserving the existing `RegisterHotKey` path.

## Steps

1. Add failing Core tests for a shortcut conflict presenter:
   - `1409` explains another app or Windows component already owns the shortcut;
   - the message says Windows does not expose the owning app;
   - unknown errors retain action, shortcut, code, and Win32 message.
2. Add `GlobalShortcutRegistrationFailurePresenter` under Core.
3. Route `GlobalHotkeyService` `RegisterHotKey` failures through the presenter.
4. Update the project completion tracker.
5. Verify with focused shortcut tests, full solution tests/build, and whitespace checking.

## Verification

```powershell
& '..\.dotnet-sdk-10\dotnet.exe' test VoiceInk.Windows\tests\VoiceInk.Windows.Core.Tests\VoiceInk.Windows.Core.Tests.csproj --filter FullyQualifiedName~GlobalShortcutTests
& '..\.dotnet-sdk-10\dotnet.exe' test VoiceInk.Windows\VoiceInk.Windows.sln
& '..\.dotnet-sdk-10\dotnet.exe' build VoiceInk.Windows\VoiceInk.Windows.sln -c Debug -p:Platform=x64
git diff --check
```
