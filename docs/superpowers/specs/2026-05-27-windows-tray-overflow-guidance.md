# Windows Tray Overflow Guidance

## Goal

Expose Windows tray overflow guidance inside the tray menu so users can recover VoiceInk visibility when Windows hides the notification area icon.

## Source Of Truth

VoiceInk on macOS lives in the menu bar. On Windows, notification area icons are user controlled and may appear in the hidden icon overflow. VoiceInk should not mutate taskbar settings, but it should tell users the manual path near the tray commands.

## Requirements

- The tray shell state must include a short menu-safe visibility guidance label.
- The native tray menu must show that guidance as a non-mutating help row.
- Existing tray visibility guidance text must remain available for richer surfaces.
- The tray menu must keep recording, quick settings, history, dictionary, and quit commands intact.

## Non-Goals

- This slice does not change global Windows taskbar settings.
- This slice does not try to force VoiceInk into the visible notification area.
