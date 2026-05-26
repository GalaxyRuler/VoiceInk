# Windows Signed MSIX Build Validation Design

## Goal

Close another packaging gap by letting maintainers validate the signed MSIX artifact immediately after a real certificate-backed package build, without installing the app or mutating certificate stores.

## Grounding

Microsoft documents MSIX as a signed packaging workflow: packages must be signed before install, and self-signed packages are only installable on devices where the certificate is explicitly trusted. Microsoft also documents command-line MSIX packaging for Windows App SDK projects. This slice therefore keeps signing certificate ownership outside the repository and validates only the produced artifact contents after build.

## Requirements

- `VoiceInk.Windows/scripts/package-msix.ps1` exposes `-ValidateAfterBuild`.
- The normal signed build still requires `-PackageCertificateKeyFile`.
- After a successful `dotnet publish`, the script locates exactly one `.msix` under the fresh publish root.
- With `-ValidateAfterBuild`, the script invokes `VoiceInk.Windows/scripts/test-msix-package.ps1` for non-installing artifact validation.
- The script fails if no package, multiple packages, or artifact validation failure is observed.
- The script does not create certificates, import certificates, trust certificates, install packages, uninstall packages, launch the app, or register shortcuts.

## Non-Goals

- No certificate generation.
- No certificate import or trust-store changes.
- No automatic `Add-AppxPackage` install.
- No release signing provider integration.
- No installer shortcut registration.

## Verification

- Focused packaging asset tests cover the new switch and safety boundaries.
- `package-msix.ps1 -Help` and `-Preflight` print the new path without requiring a certificate.
- Full solution tests and Debug x64 build remain the final slice gate.
