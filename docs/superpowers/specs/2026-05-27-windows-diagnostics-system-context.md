# Windows Diagnostics System Context

## Goal

Bring Windows local diagnostics closer to macOS Metrics and performance diagnostics by including safe system context in copied/exported diagnostic reports.

## Source Of Truth

- macOS Metrics and performance analysis expose local system information such as device, processor, and memory near performance diagnostics.
- .NET exposes safe local runtime/system facts through `RuntimeInformation.OSArchitecture`, `RuntimeInformation.ProcessArchitecture`, and `Environment.ProcessorCount`.

## Windows Behavior

- The diagnostic report `System` section includes:
  - App version.
  - OS description.
  - OS architecture.
  - .NET runtime description.
  - Process architecture.
  - Logical processor count.
- This stays local-only and remains safe for copy/export:
  - No environment variables.
  - No API keys or Credential Manager values.
  - No clipboard contents.
  - No transcript/history text or rendered prompt contents.

## Verification

- Core diagnostic report tests cover the new fields.
- Debug x64 build verifies the WinUI report request populates the new fields.
