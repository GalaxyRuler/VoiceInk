# Windows Recorder Power Label

## Purpose

Make the floating recorder Power Mode button display a tested indicator label for both automatic/default mode and selected Power Mode rules.

## Source of Truth

- VoiceInk docs describe the mini recorder Power Mode button as showing the active mode indicator, and showing a default sparkles-style indicator when no mode is active.
- The Windows recorder already has a Core `FloatingRecorderControlPresenter` for prompt and Power Mode control state.

## Requirements

- Add a presenter-backed `PowerModeButtonLabel`.
- The automatic/default Power Mode state should show the indicator plus `Auto`.
- Selected Power Mode rules should show their emoji/indicator plus the trimmed rule name.
- WinUI should bind the button label from Core instead of recomputing it.
- Do not change Power Mode matching, explicit selection, rule persistence, recorder activation, or stop/cancel behavior.

## Non-Goals

- Do not implement pixel-perfect macOS notch geometry.
- Do not change hover timing or chooser persistence.
- Do not add commercial Power Mode surfaces.
