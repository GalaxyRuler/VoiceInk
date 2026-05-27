# Windows MSIX Trust Store Readiness

## Goal

Make the Windows release readiness report explicit about the trust-store prerequisite for signed MSIX smoke testing.

## Online Grounding

- Microsoft documents that MSIX packages must be signed: https://learn.microsoft.com/en-us/windows/msix/package/signing-package-overview
- Microsoft's current signing and troubleshooting guidance says self-signed or private signing certificates must be trusted on the target test machine before `Add-AppxPackage` or App Installer can install the package: https://learn.microsoft.com/en-us/windows/msix/package/sign-msix-package-guide
- Microsoft troubleshooting guidance calls out `0x800B0109` as a common untrusted-root/signing trust failure and recommends the Local Computer Trusted People store: https://learn.microsoft.com/en-us/windows/msix/msix-troubleshooting-guide

## Requirements

- Keep release readiness read-only and non-mutating.
- Mention the exact machine trust-store target for disposable test-machine setup: `Cert:\LocalMachine\TrustedPeople`.
- Mention the common `0x800B0109` failure code in the release readiness checklist.
- Preserve the existing no-certificate-creation and no-certificate-import boundary.
- Cover the guidance with packaging asset tests.

## Non-Goals

- No certificate generation.
- No certificate import.
- No global trust-store mutation.
- No signed install smoke execution on the active desktop.
