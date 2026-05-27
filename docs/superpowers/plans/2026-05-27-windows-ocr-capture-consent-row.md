# Windows OCR Capture Consent Row Plan

## Steps

1. Add a failing context readiness presenter assertion for a Windows capture consent/privacy row when full-screen OCR is enabled.
2. Run the focused test to confirm the red failure.
3. Add the presenter privacy row when `UseOcrContext` is enabled.
4. Run the focused test again.
5. Update the project completion tracker.
6. Run the full solution test/build/whitespace verification gate.
7. Commit the completed slice.

## Verification

- Focused: `dotnet test VoiceInk.Windows\tests\VoiceInk.Windows.Core.Tests\VoiceInk.Windows.Core.Tests.csproj --filter EnhancementContextReadinessPresenterTests.Present_WithClipboardAndFullScreenOcr_ShowsEnabledSources`
- Full gate: solution tests, Debug x64 build, and `git diff --check`.
