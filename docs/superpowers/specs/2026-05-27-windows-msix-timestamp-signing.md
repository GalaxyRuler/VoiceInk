# Windows MSIX Timestamp Signing

## Goal

Close the signed MSIX release-readiness gap by exposing timestamped signing in the repo-local packaging script without creating certificates, importing certificates, installing packages, or reading secrets.

## Source Of Truth

- Microsoft recommends timestamping signed MSIX packages so package signatures remain verifiable after certificate expiry.
- Microsoft App Installer/MSIX guidance keeps signing and installation as maintainer-owned release operations.

## Online Grounding

- Microsoft SignTool and Azure Trusted Signing guidance use `/tr "http://timestamp.acs.microsoft.com"` with SHA-256 timestamping (`/td SHA256`) for RFC 3161 timestamping.
- Microsoft App Installer schema documents the `.appinstaller` `MainPackage` release path separately from signing/trust operations: https://learn.microsoft.com/en-us/uwp/schemas/appinstallerschema/schema-root

## Requirements

- Add a `TimestampServerUrl` parameter to `package-msix.ps1`.
- Default the timestamp URL to `http://timestamp.acs.microsoft.com`.
- Default the timestamp digest algorithm to `SHA256`.
- Pass the URL to MSIX signing through `AppxPackageSigningTimestampServerUrl`.
- Pass the digest algorithm through `AppxPackageSigningTimestampDigestAlgorithm`.
- Show the timestamp URL in `-Preflight` and release-readiness output.
- Keep certificate creation/import, package install/uninstall, and secret handling out of repo scripts.

## Acceptance

- A focused packaging asset test fails before the timestamp parameter and passes after.
- `package-msix.ps1 -Help` and `package-msix.ps1 -Preflight` show timestamp behavior without mutating machine state.
- Full packaging asset tests pass.
