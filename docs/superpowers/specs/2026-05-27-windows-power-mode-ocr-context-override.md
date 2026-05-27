# Windows Power Mode OCR Context Override

## Goal

Let a Power Mode rule enable or disable screen/OCR context for that workflow without changing the global Settings value.

## Source Of Truth

- `VoiceInk/PowerMode/PowerModeConfig.swift` has `useScreenCapture` per Power Mode configuration.
- `VoiceInk/PowerMode/PowerModeSessionManager.swift` applies `useScreenCapture` while the mode is active and restores base settings afterward.

## Windows Design

- Add a nullable `UseOcrContextOverride` to `PowerModeRule`.
- Resolve it through `PowerModeMatcher.Apply` into the effective `AppSettings.UseOcrContext`.
- Render and edit it as a tri-state override next to the existing text cleanup overrides.
- Keep global context settings unchanged; the override applies only to the effective settings for the resolved rule.

## Online Grounding

- Microsoft documents Windows Graphics Capture as user-consent based and visibly bordered while capture is active, so the Windows override must remain explicit and should not bypass consent: https://learn.microsoft.com/windows/apps/develop/media-authoring-processing/screen-capture

## Requirements

- Power Mode matching can enable OCR context when base Settings disables it.
- Power Mode matching can disable OCR context when base Settings enables it.
- Power Mode row summaries count/display the OCR override.
- WinUI form preserves the tri-state override.

## Non-Goals

- No automatic screen-capture permission grant.
- No borderless capture.
- No changes to OCR region selection.
