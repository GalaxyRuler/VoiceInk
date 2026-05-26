# Windows WinApp Signing Reference Implementation Plan

**Goal:** Add read-only WinApp CLI local signing guidance to release readiness.

**Tech Stack:** PowerShell packaging scripts, xUnit packaging asset tests.

## Task 1: Packaging Test

- Modify: `VoiceInk.Windows/tests/VoiceInk.Windows.Core.Tests/Packaging/WindowsPackagingAssetsTests.cs`

Add failing assertions that release readiness includes:

- `WinApp CLI local signing reference`;
- `winapp cert generate`;
- `winapp sign`.

Verification:

```powershell
& "..\.dotnet-sdk-10\dotnet.exe" test VoiceInk.Windows\tests\VoiceInk.Windows.Core.Tests\VoiceInk.Windows.Core.Tests.csproj --filter "FullyQualifiedName~WindowsPackagingAssetsTests.ReleaseReadinessScript_PrintsNonMutatingPackagingChecklist"
```

## Task 2: Readiness Script

- Modify: `VoiceInk.Windows/scripts/test-release-readiness.ps1`

Print WinApp CLI local signing reference rows while preserving the script's no-mutation contract.

## Task 3: Docs and Verification

- Add this spec and plan.
- Update `docs/superpowers/project-completion.md`.
- Run focused packaging tests, the readiness report, full solution tests, Debug x64 build, and `git diff --check`.
