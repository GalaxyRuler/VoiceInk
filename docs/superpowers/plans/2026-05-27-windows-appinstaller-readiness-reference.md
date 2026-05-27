# Windows App Installer Readiness Reference Plan

## Steps

1. Add a failing packaging asset test assertion that expects the release readiness script to include `.appinstaller`, `MainPackage`, `Name/Publisher/Version`, and `Package.appxmanifest` guidance.
2. Run the focused packaging test and confirm the expected red failure.
3. Add a read-only "App Installer readiness reference" section to `test-release-readiness.ps1`.
4. Run the focused packaging test again.
5. Update the project completion tracker.
6. Run the full solution test/build/whitespace verification gate.
7. Commit the completed slice.

## Verification

- Focused: `dotnet test VoiceInk.Windows\tests\VoiceInk.Windows.Core.Tests\VoiceInk.Windows.Core.Tests.csproj --filter WindowsPackagingAssetsTests.ReleaseReadinessScript_PrintsNonMutatingPackagingChecklist`
- Full gate: solution tests, Debug x64 build, and `git diff --check`.
