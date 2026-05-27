# Windows Permissions Package Capability Plan

**Goal:** Surface the packaged microphone capability as a first-class, non-mutating Permissions readiness row.

## Steps

- [x] Add failing presenter coverage for an `App Microphone Capability` row.
- [x] Add the row to `PermissionsReadinessPresenter` after runtime microphone access.
- [x] Add a safe action-target branch so the informational row does not navigate nowhere.
- [x] Update parity docs and completion tracker.
- [x] Run focused Permissions presenter verification.

## Verification

```powershell
& '..\.dotnet-sdk-10\dotnet.exe' test 'VoiceInk.Windows\tests\VoiceInk.Windows.Core.Tests\VoiceInk.Windows.Core.Tests.csproj' --filter FullyQualifiedName~PermissionsReadinessPresenterTests
```

Expected: Permissions presenter tests pass.
