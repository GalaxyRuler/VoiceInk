# Windows Release Signing Checklist Implementation Plan

**Goal:** Make release signing/trust prerequisites explicit in the non-mutating readiness report.

**Tech Stack:** PowerShell script, xUnit static asset tests.

## Task 1: Failing Packaging Tests

- Modify: `VoiceInk.Windows/tests/VoiceInk.Windows.Core.Tests/Packaging/WindowsPackagingAssetsTests.cs`

Extend `ReleaseReadinessScript_PrintsNonMutatingPackagingChecklist` to expect:

- publisher/certificate subject match;
- timestamping reminder;
- Trusted People test-machine trust prerequisite;
- AppxDeployment/AppxPackaging troubleshooting logs.

Verification:

```powershell
& "..\.dotnet-sdk-10\dotnet.exe" test VoiceInk.Windows\tests\VoiceInk.Windows.Core.Tests\VoiceInk.Windows.Core.Tests.csproj --filter WindowsPackagingAssetsTests
```

## Task 2: Readiness Script

- Modify: `VoiceInk.Windows/scripts/test-release-readiness.ps1`

Print a release signing checklist without executing signing, trust, install, or certificate commands.

## Task 3: Docs and Verification

- Update: `docs/superpowers/project-completion.md`

Run the focused packaging tests, the readiness script, full solution tests, Debug x64 build, and `git diff --check`.
