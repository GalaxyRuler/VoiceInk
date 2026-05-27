# Windows App Installer Package URI Guard Spec

Date: 2026-05-27

## Goal

Harden the open-source App Installer generator and validator by rejecting `MainPackage` URIs that do not point to an MSIX package artifact.

## Source Of Truth

Microsoft App Installer manifests identify a `MainPackage` package URI for deployment. VoiceInk's release scripts should fail before publishing or validating a `.appinstaller` file that points to documentation, HTML, ZIP, or another non-MSIX asset.

## Windows Behavior

- `write-appinstaller.ps1` requires `-MainPackageUri` to be an absolute URI ending in `.msix` or `.msixbundle`.
- `test-appinstaller.ps1` validates that the manifest `MainPackage Uri` ends in `.msix` or `.msixbundle`.
- The guard is schema/readiness-only; it does not publish, install, launch, sign, create certificates, import certificates, or mutate trust stores.
- Existing identity, artifacts-root, and on-launch update checks remain in place.

## Non-Goals

- Do not validate live network reachability.
- Do not build or sign an MSIX.
- Do not run `Add-AppxPackage`, `Remove-AppxPackage`, or App Installer launch flows.
