# Windows MSIX Uninstall Verification

## Goal

Make the signed MSIX install smoke helper verify that the package is gone after uninstall, so maintainer-run installer evidence covers install, identity, launch reference, and cleanup.

## Source Of Truth

- Microsoft documents `Get-AppxPackage` as the query primitive for installed MSIX/AppX packages.
- Microsoft documents `Remove-AppxPackage` as the uninstall primitive for signed packages.
- VoiceInk Windows packaging must avoid mutating the active desktop unless `-Execute` is explicitly provided on a prepared disposable test machine.

## Requirements

- Keep `smoke-msix-install.ps1` non-mutating by default.
- After `Remove-AppxPackage -Package`, query the package name again.
- Throw if any package with the expected name remains installed.
- Print a positive cleanup verification line when no package remains.
- Update packaging tests so the behavior is locked without running an installer.

## Non-Goals

- No certificate creation or trust import.
- No install smoke execution in this worktree.
- No machine-wide uninstall or `-AllUsers` cleanup.
