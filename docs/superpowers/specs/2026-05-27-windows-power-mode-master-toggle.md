# Windows Power Mode Master Toggle

## Purpose

Match the macOS Settings Power Mode toggle by letting users disable all Power Mode matching and manual selections without deleting individual rules.

## Source of Truth

- `VoiceInk/Views/Settings/SettingsView.swift` has a Settings `Power Mode` toggle.
- `VoiceInk/AppDefaults.swift` keeps Power Mode preferences separate from individual Power Mode configurations.
- Windows already supports per-rule enabled states; the missing surface is the global master enable/disable flag.

## Requirements

- Add `AppSettings.IsPowerModeEnabled`, defaulting to true for compatibility with existing Windows rules.
- Persist the flag in JSON settings and local settings backup/import.
- Add an `Enable Power Mode` checkbox to the Windows Power Mode page.
- When disabled, `PowerModeMatcher` must ignore explicit selections, target matches, and default fallback rules.
- When disabled, the floating recorder Power Mode chooser must show Auto and not open rule choices.
- Keep individual rule enabled states, ordering, shortcuts, and override data unchanged.

## Non-Goals

- Do not delete or mutate existing Power Mode rules when disabling Power Mode.
- Do not implement macOS `Persist Configured Preferences` session restoration in this slice.
- Do not change tray, shortcut, or recorder direct-selection commands beyond respecting the disabled matcher/presenter state.
