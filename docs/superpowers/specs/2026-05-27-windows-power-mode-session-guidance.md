# Windows Power Mode Session Guidance

## Goal

Make the Windows Power Mode page explain how rule overrides are scoped while preserving the original macOS intent of temporary workflow-specific settings.

## Source Of Truth

- The macOS app has a Power Mode session manager that captures original settings and restores them when the session ends.
- The Windows controller resolves Power Mode effective settings for each dictation instead of permanently editing base Settings.
- Microsoft WinUI `TextBlock` supports wrapped guidance copy for dense settings pages.

## Requirements

- Add a presenter-backed Power Mode session guidance string.
- The guidance must explain that automatic matches and manual selections are resolved for each recording.
- The guidance must reassure users that base Settings remain unchanged by rule overrides.
- Surface the guidance in the WinUI Power Mode section with wrapped text.
- Cover the presenter copy in Core tests.

## Non-Goals

- No new persistent Power Mode session store.
- No change to rule matching or effective settings resolution.
- No macOS-only bundle identifier behavior.
