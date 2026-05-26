# Windows Onboarding Microphone Action Target Plan

## Status

Completed on 2026-05-27.

## Tasks

1. Add failing onboarding status assertions for the microphone action target.
2. Add `MicrophoneActionTarget` to `OnboardingSetupStatus`.
3. Update onboarding UI wiring to use the status target for the microphone settings button.
4. Run focused onboarding status tests.
5. Run the full solution test/build/diff gate.
6. Update the project completion tracker.

## Verification

- `& '..\.dotnet-sdk-10\dotnet.exe' test VoiceInk.Windows\tests\VoiceInk.Windows.Core.Tests\VoiceInk.Windows.Core.Tests.csproj --filter OnboardingSetupStatusServiceTests`
- Full solution gate before commit:
  `& '..\.dotnet-sdk-10\dotnet.exe' test VoiceInk.Windows\VoiceInk.Windows.sln; if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }; & '..\.dotnet-sdk-10\dotnet.exe' build VoiceInk.Windows\VoiceInk.Windows.sln -c Debug -p:Platform=x64; if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }; git diff --check`
