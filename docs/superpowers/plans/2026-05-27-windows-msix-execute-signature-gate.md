# Windows MSIX Execute Signature Gate Plan

## Slice

Add a read-only Authenticode validity gate before the signed MSIX smoke helper runs mutating install/query/uninstall commands.

## Steps

- [x] Ground the slice against Microsoft MSIX signing and `Add-AppxPackage` guidance.
- [x] Add a failing packaging asset test for the execute-time signature gate.
- [x] Update `smoke-msix-install.ps1` with a safe PowerShell signature gate before `Add-AppxPackage`.
- [x] Run focused packaging asset tests.
- [x] Update README/spec/completion tracker.
- [x] Run full solution tests, Debug x64 build, and whitespace diff check.
- [x] Commit the completed slice.
