# Windows MSIX Packaging Foundation Design

## Purpose

VoiceInk for Windows needs a first-class installer path in addition to the current source-run and developer ZIP workflows. This slice adds the MSIX packaging foundation without introducing commercial channels, secrets, or machine-wide configuration changes.

## Desired Behavior

- Keep the app source-runnable and unpackaged by default.
- Add a checked-in package manifest with open-source VoiceInk identity, display metadata, desktop/full-trust capability, microphone capability, and no commercial surfaces.
- Add a repo-local `package-msix.ps1` script that:
  - resolves the repo-local .NET SDK by default;
  - writes artifacts under `VoiceInk.Windows\artifacts\msix`;
  - refuses output paths outside `VoiceInk.Windows\artifacts`;
  - requires a maintainer-provided certificate path for signed package creation;
  - does not create certificates, import certificates, or mutate certificate stores;
  - documents manual sideload/install/uninstall smoke commands.
- Document the packaging workflow and known signing requirement.

## Non-Goals

- No certificate generation.
- No certificate import.
- No store/private update channel.
- No paid updater.
- No machine-wide install or uninstall changes.

## Verification

- Add tests that parse the manifest and assert package identity, app executable, app id, full trust capability, microphone capability, and no forbidden commercial/update fields.
- Add tests that inspect the MSIX packaging script for safety gates and expected build properties.
- Run focused packaging tests, full solution tests, Debug x64 build, and script help output.
