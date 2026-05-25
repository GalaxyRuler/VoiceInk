# Windows MSIX Artifact Validator Design

## Goal

Add a repeatable, certificate-free MSIX content validation step so packaging smoke can catch malformed packages before a tester installs them.

## External Grounding

Microsoft documents MSIX packaging as a signed package workflow and documents Windows App SDK self-contained deployment as including Windows App SDK dependencies inside packaged apps. This slice inspects the produced package artifact only; it does not cryptographically verify the signature, create certificates, trust certificates, install packages, or uninstall packages.

## Behavior

- Validate a `.msix` file under `VoiceInk.Windows\artifacts`.
- Require validation scratch paths to stay under `VoiceInk.Windows\artifacts`.
- Extract the `.msix` into an artifact-local scratch folder without launching or installing it.
- Reject package paths inside the scratch extraction cleanup folder.
- Reject reparse-point package/scratch/extraction paths so cleanup cannot be redirected outside artifacts through a junction or symlink.
- Verify the extracted package contains:
  - `AppxManifest.xml`
  - `AppxBlockMap.xml`
  - `AppxSignature.p7x`
  - `VoiceInk.Windows.App.exe`
- Parse `AppxManifest.xml` and verify:
  - package identity name is `VoiceInk.Windows`
  - publisher is `CN=VoiceInkOpenSource`
  - app executable is `VoiceInk.Windows.App.exe`
  - full-trust and microphone capabilities exist
  - no trial, purchase, store, license, or paid wording appears
- Print manual install/query/uninstall smoke commands after validation.

## Non-Goals

- No certificate generation.
- No certificate import or trust-store mutation.
- No `Add-AppxPackage` execution.
- No `Remove-AppxPackage` execution.
- No app launch automation.
- No cryptographic signature, signer identity, timestamp, or trust-chain validation.
