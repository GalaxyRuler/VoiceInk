# Windows MSIX Packaging Preflight Design

## Goal

Add a certificate-free MSIX packaging preflight path so maintainers can verify packaging inputs, project paths, artifact safety, and release smoke instructions before supplying a real signing certificate.

## Source Grounding

Microsoft documents MSIX as a signed package workflow: packages must be signed, and sideload install also requires the signing certificate to be trusted on the target device. Microsoft also documents self-signed development signing as valid for local testing only when the certificate is explicitly trusted. This slice does not create certificates, import certificates, trust certificates, install packages, uninstall packages, or read certificate passwords.

## Behavior

- `VoiceInk.Windows/scripts/package-msix.ps1` exposes a `-Preflight` switch.
- `-Preflight` resolves the repository root, Windows project root, app project, package manifest, artifact root, publish root, and dotnet path.
- `-Preflight` validates that `OutputRoot` remains inside `VoiceInk.Windows/artifacts`.
- `-Preflight` validates that the app project and `Package.appxmanifest` exist.
- `-Preflight` prints the MSIX publish properties used by the real packaging path.
- `-Preflight` prints the manual signed-package flow:
  - run `package-msix.ps1` with `-PackageCertificateKeyFile`;
  - run `test-msix-package.ps1` against the produced `.msix`;
  - trust the signing certificate on a test machine;
  - run `Add-AppxPackage`, `Get-AppxPackage`, and `Remove-AppxPackage`.
- `-Preflight` does not require `PackageCertificateKeyFile`.
- `-Preflight` does not create output directories, delete output directories, run `dotnet publish`, sign packages, create certificates, import certificates, install packages, uninstall packages, or modify machine/user certificate stores.

## Non-Goals

- No production release signing automation.
- No self-signed certificate generation.
- No certificate trust automation.
- No MSIX install/uninstall automation.
- No cryptographic signature validation.

## Verification

- Static packaging test covers the `-Preflight` contract and forbidden commands.
- Script `-Help` runs.
- Script `-Preflight` runs from the repository root without a certificate.
- Focused packaging tests pass.
- Full solution tests and Debug x64 build pass before committing.
