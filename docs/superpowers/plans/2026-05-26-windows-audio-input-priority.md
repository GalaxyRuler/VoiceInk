# Windows Audio Input Priority Implementation Plan

## Task 1: Core Selection

- Add `AudioInputModeSettings` values for System Default, Custom, and Prioritized.
- Add `PrioritizedAudioInputDevice`.
- Extend `AppSettings` equality/hash with mode and priority list.
- Extend `AudioInputDeviceSelection.BuildChoices` to select prioritized devices in order and report active/fallback/unavailable notices.
- Verify with focused Core tests.

## Task 2: Priority List Operations

- Add `AudioInputPriorityList` helpers for add, remove, move up, move down, and normalization.
- Keep behavior UI-independent so WinUI only binds controls and calls helpers.
- Verify with focused Core tests.

## Task 3: WinUI Audio Input Page

- Add an input mode picker to the Audio Input page.
- Add a priority list with add/remove/move controls.
- Persist mode and priority list from `CurrentSettingsAsync`.
- Rebuild the active selection after applying settings so the capture service receives the resolved priority choice.
- Preserve legacy custom-device settings by selecting Custom mode when a saved device number exists.
- Verify with Debug x64 build.

## Task 4: Docs And Tracker

- Update the open-source parity spec, README, and completion bar.
- Run full tests/build and commit the slice.
