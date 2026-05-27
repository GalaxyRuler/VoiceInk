# Windows MSIX Execute Signature Gate

## Goal

Make the signed MSIX install-smoke helper fail before mutating the disposable test machine when Authenticode reports that the package signature is not valid or trusted.

## Source Of Truth

- Microsoft MSIX guidance requires app packages to be signed before deployment.
- Microsoft `Add-AppxPackage` installs signed `.msix` packages, but trust failures surface as deployment errors. VoiceInk should provide a clearer open-source maintainer diagnostic before running install/uninstall smoke.

## Windows Behavior

- `smoke-msix-install.ps1` remains non-mutating by default.
- The default plan still prints signature status, signer subject, signer thumbprint, trust guidance, and the signed smoke command sequence.
- When `-Execute` is passed, the helper checks `Get-AuthenticodeSignature` immediately before `Add-AppxPackage`.
- If the signature status is not `Valid`, the helper throws a clear message and does not run `Add-AppxPackage`, `Get-AppxPackage`, or `Remove-AppxPackage`.
- The helper still does not create certificates, import certificates, trust certificates, sign packages, build packages, or mutate certificate stores.

## Verification

- Packaging asset tests must assert that the execute-time signature gate exists and that the helper still avoids certificate mutation commands.
- Full solution tests and Debug x64 build remain the commit gate.
