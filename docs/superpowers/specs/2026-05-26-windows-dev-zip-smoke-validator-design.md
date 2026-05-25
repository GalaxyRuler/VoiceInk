# Windows Dev ZIP Smoke Validator Design

## Goal

Add a repeatable smoke validation step for the unpackaged Windows dev ZIP so release testing can catch missing files before anyone launches or distributes a package.

## External Grounding

Microsoft documents Windows App SDK self-contained deployment as copying Windows App SDK framework contents into build output when `WindowsAppSDKSelfContained=true`. Microsoft also documents MSIX command-line packaging as a separate MakeAppx/SignTool/certificate flow. This slice strengthens the existing self-contained dev ZIP path without requiring signing certificates or machine-level install actions.

## Behavior

- Validate either a dev ZIP file or an extracted package folder.
- Require validation targets to stay under `VoiceInk.Windows\artifacts`.
- Expand ZIP files into an artifact-local scratch folder.
- Reject ZIP files that live inside the scratch extraction cleanup folder.
- Verify the package contains:
  - `VoiceInk.Windows.App.exe`
  - `VoiceInk.Windows.App.deps.json`
  - `VoiceInk.Windows.App.runtimeconfig.json`
  - `Microsoft.WindowsAppRuntime.Bootstrap.dll`
  - `VOICEINK-WINDOWS-README.txt`
- Verify the README includes the manual local GGML model smoke path.
- Do not launch, install, sign, remove input artifacts, or trust certificates.

## Non-Goals

- No MSIX install/uninstall automation in this slice.
- No certificate creation or machine trust-store mutation.
- No app launch automation; that remains a manual smoke step because microphone/model access may be machine-specific.
