# Windows Tray Tooltip Guidance

## Goal

Surface the Windows notification-area visibility guidance in the tray icon hover tooltip, not only inside the tray menu.

## Source Of Truth

- `VoiceInk/MenuBarManager.swift` keeps menu-bar shell status visible from the macOS system shell.
- Windows adapts that shell to `NotifyIcon` under `VoiceInk.Windows.Native/Tray`.
- `TrayShellPresenter` already owns notification-area visibility language and can expose concise tooltip copy without WinForms UI automation.

## Online Grounding

- Microsoft documents `NotifyIcon.Text` as the tooltip displayed when the pointer rests on a notification-area icon, with a bounded maximum length: https://learn.microsoft.com/en-us/dotnet/api/system.windows.forms.notifyicon.text

## Requirements

- Add presenter-backed `TooltipText` to `TrayShellState`.
- Include the current VoiceInk status and concise taskbar-corner-overflow pin guidance.
- Keep the native `NotifyIcon.Text` assignment bounded by the existing truncation guard.
- Do not change tray commands, menu order, quick settings, or Explorer/taskbar recovery behavior.

## Acceptance

- Core shell presenter tests assert the new tooltip copy.
- Native tray service uses `state.TooltipText` for the `NotifyIcon.Text` value.
