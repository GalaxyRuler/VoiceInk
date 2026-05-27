# Windows Settings Section Directory Plan

**Goal:** Improve Settings form polish by adding a compact, accessible directory of the grouped Settings sections at the top of the page.

## Steps

- [x] Add failing presenter coverage for `SettingsSectionCopy.AccessibleName`.
- [x] Add failing static XAML coverage for `SettingsSectionDirectoryListView` accessible row binding.
- [x] Add `AccessibleName` to `SettingsSectionCopy`.
- [x] Render and bind `SettingsSectionDirectoryListView` from `SettingsSectionPresentation.Sections`.
- [x] Update parity docs and completion tracker.
- [x] Run focused Settings/accessibility verification.

## Verification

```powershell
& '..\.dotnet-sdk-10\dotnet.exe' test 'VoiceInk.Windows\tests\VoiceInk.Windows.Core.Tests\VoiceInk.Windows.Core.Tests.csproj' --filter "FullyQualifiedName~SettingsSectionPresenterTests|FullyQualifiedName~AppXamlAccessibilityTests.MainWindow_SummaryRows_BindAccessibleName"
```

Expected: focused presenter/static UI tests pass.
