# Windows Metrics Summary Line Plan

**Goal:** Improve Metrics visual polish by adding a compact selected-filter summary line near the dashboard hero.

## Steps

- [x] Add failing Metrics presenter coverage for non-empty and empty summary lines.
- [x] Add failing static XAML/accessibility coverage for the summary TextBlock and binding.
- [x] Add `SummaryLine` to `SessionMetricsDashboardPresentation`.
- [x] Render `MetricsSummaryLineTextBlock` near the hero and clear it for unavailable metrics.
- [x] Update parity docs and completion tracker.
- [x] Run focused Metrics/accessibility verification.

## Verification

```powershell
& '..\.dotnet-sdk-10\dotnet.exe' test 'VoiceInk.Windows\tests\VoiceInk.Windows.Core.Tests\VoiceInk.Windows.Core.Tests.csproj' --filter "FullyQualifiedName~SessionMetricsDashboardPresenterTests|FullyQualifiedName~AppXamlAccessibilityTests.MainWindow_MetricsSummaryLine_HasAutomationName"
```

Expected: focused presenter/static UI tests pass.
