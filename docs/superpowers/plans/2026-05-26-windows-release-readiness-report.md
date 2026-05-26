# Windows Release Readiness Report Implementation Plan

**Goal:** Add a non-mutating packaging readiness report that ties together the existing dev ZIP and signed MSIX release workflow.

**Tech Stack:** PowerShell, .NET 10 xUnit packaging tests, repo-local documentation.

## Task 1: Packaging Asset Test

- Modify: `VoiceInk.Windows/tests/VoiceInk.Windows.Core.Tests/Packaging/WindowsPackagingAssetsTests.cs`

Add a focused test asserting `VoiceInk.Windows/scripts/test-release-readiness.ps1`:

- exists;
- prints `Release readiness report`;
- points to `package-msix.ps1 -Preflight`;
- points to `package-msix.ps1 -ValidateAfterBuild`;
- points to `test-msix-package.ps1`;
- points to `smoke-msix-install.ps1`;
- states `This script does not create or import certificates`;
- does not contain certificate creation/import/trust-store commands;
- does not execute install/uninstall commands or `dotnet publish`.

Verification:

```powershell
& "..\.dotnet-sdk-10\dotnet.exe" test VoiceInk.Windows\tests\VoiceInk.Windows.Core.Tests\VoiceInk.Windows.Core.Tests.csproj --filter WindowsPackagingAssetsTests
```

Expected first result: failure because the readiness script does not exist.

## Task 2: Readiness Script

- Create: `VoiceInk.Windows/scripts/test-release-readiness.ps1`

Implement a read-only report that:

- supports `-Help`;
- checks required repo assets with `Test-Path`;
- prints non-mutating validation commands;
- prints maintainer-gated signing and smoke commands;
- throws when any required asset is missing;
- avoids package install/uninstall, publish, signing, and certificate-store mutation.

Verification:

```powershell
.\VoiceInk.Windows\scripts\test-release-readiness.ps1 -Help
.\VoiceInk.Windows\scripts\test-release-readiness.ps1
```

## Task 3: Docs and Completion Bar

- Update: `README.md`
- Update: `docs/superpowers/specs/2026-05-24-windows-open-source-parity-design.md`
- Update: `docs/superpowers/project-completion.md`

Document the release readiness report and raise Packaging progress modestly while leaving signed release installation gated on maintainer-owned signing/trust setup.

## Task 4: Verification and Commit

Run:

```powershell
& "..\.dotnet-sdk-10\dotnet.exe" test VoiceInk.Windows\tests\VoiceInk.Windows.Core.Tests\VoiceInk.Windows.Core.Tests.csproj --filter WindowsPackagingAssetsTests
.\VoiceInk.Windows\scripts\test-release-readiness.ps1 -Help
.\VoiceInk.Windows\scripts\test-release-readiness.ps1
& "..\.dotnet-sdk-10\dotnet.exe" test VoiceInk.Windows\VoiceInk.Windows.sln
& "..\.dotnet-sdk-10\dotnet.exe" build VoiceInk.Windows\VoiceInk.Windows.sln -c Debug -p:Platform=x64
git diff --check
```

Commit with:

```text
feat(windows): add release readiness report
```
