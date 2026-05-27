# Windows Onboarding Health Summary Plan

**Goal:** Add the remaining onboarding health summary lines for text insertion and optional context awareness without changing setup completion behavior.

## Steps

- [x] Add a failing setup-status test for text insertion and context-awareness health summary rows.
- [x] Add the two informational health checks to `OnboardingSetupStatusService`.
- [x] Update parity spec, completion tracker, and slice plan/spec docs.
- [x] Run focused onboarding setup-status verification.

## Verification

```powershell
& '..\.dotnet-sdk-10\dotnet.exe' test 'VoiceInk.Windows\tests\VoiceInk.Windows.Core.Tests\VoiceInk.Windows.Core.Tests.csproj' --filter FullyQualifiedName~OnboardingSetupStatusServiceTests
```

Expected: setup-status tests pass.
