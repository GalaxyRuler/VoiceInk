# Windows Onboarding Current-Step Focus Plan

**Goal:** Make the first-run onboarding dialog feel more like the macOS guided flow by surfacing a prominent current setup step backed by testable Core presenter state.

## Steps

- [x] Add failing Core presenter coverage for current-step label, title, description, status, and accessible name.
- [x] Add failing static WinUI coverage requiring a current-step onboarding control and presenter-backed automation name.
- [x] Add current-step fields to `OnboardingChecklistPresentation`, derived from existing setup stages.
- [x] Render the current-step summary in the onboarding `ContentDialog` before the dense setup/action lists.
- [x] Run focused onboarding/accessibility verification.
- [x] Update parity docs and completion tracker.

## Verification

```powershell
& '..\.dotnet-sdk-10\dotnet.exe' test 'VoiceInk.Windows\tests\VoiceInk.Windows.Core.Tests\VoiceInk.Windows.Core.Tests.csproj' --filter "FullyQualifiedName~OnboardingChecklistPresenterTests|FullyQualifiedName~AppXamlAccessibilityTests.MainWindow_OnboardingDialogControls_SetAutomationNames"
```

Expected: focused presenter/static UI tests pass.
