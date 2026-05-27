# Windows Imported Model Show In Explorer

## Goal

Match the macOS local model card affordance for revealing model files by adding a Windows-native "Show in Explorer" action for imported local models, not only downloaded catalog models.

## Source Of Truth

- `VoiceInk/Views/AI Models/WhisperModelCardView.swift` includes a "Show in Finder" action for available local models.
- Windows already exposes "Show in Explorer" for downloaded catalog models.
- Microsoft documents `ProcessStartInfo.UseShellExecute` as the shell-backed way to start OS-associated targets, and Windows Explorer supports selecting a file by path.

## Requirements

- Add a "Show in Explorer" action beside imported local model actions.
- Use shared testable Core logic to build the Explorer select target.
- Refuse blank or missing model paths with friendly status text.
- Reuse the helper for downloaded catalog model reveal.
- Do not scan folders, move files, or delete files.

## Non-Goals

- No multi-select reveal.
- No custom shell integration.
- No Windows registry changes.
