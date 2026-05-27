# Windows MSIX Trust Status Smoke

## Purpose

Make the signed MSIX install smoke plan report whether the signer certificate is already trusted on the test machine, without mutating certificate stores.

## Source of Truth

- Microsoft MSIX guidance requires a package to be signed and trusted on the target device before installation can succeed.
- Existing VoiceInk Windows packaging scripts intentionally avoid certificate creation, certificate import, package install, package uninstall, and trust-store mutation unless a maintainer explicitly runs the gated smoke path on a prepared machine.

## Requirements

- The default `smoke-msix-install.ps1` plan remains non-mutating.
- When a signer certificate is available, the plan reads `Cert:\LocalMachine\TrustedPeople` and reports whether the signer thumbprint is present.
- If the certificate store cannot be read, the plan reports that as diagnostic context instead of failing the non-mutating plan.
- The read-only trust status must be covered by packaging asset tests.
- The `-Execute` path must remain gated by Authenticode `Valid` status before `Add-AppxPackage`.

## Non-Goals

- Do not create certificates.
- Do not import certificates.
- Do not trust certificates.
- Do not install, uninstall, launch, publish, or sign packages in this slice.
