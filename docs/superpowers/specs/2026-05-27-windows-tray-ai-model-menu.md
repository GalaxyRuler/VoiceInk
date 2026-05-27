# Windows Tray AI Model Menu

## Goal

Add an AI Model submenu to the Windows tray menu so the tray shell matches the macOS menu-bar enhancement provider/model controls more closely.

## Source Of Truth

- `VoiceInk/Views/MenuBarView.swift` exposes both `AI Provider` and `AI Model` menus.
- `VoiceInk.Windows/src/VoiceInk.Windows.Native/Tray/TrayIconService.cs` already adapts macOS menu-bar controls to a Windows notification-area menu.
- Microsoft notification-area guidance recommends placing useful primary and secondary commands in the tray context menu while leaving icon visibility under user control.

## Requirements

- Add `EnhancementModels` to `TrayQuickSettingsState`.
- Add a testable presenter that builds tray model options from available enhancement model choices and the selected model.
- Include a selected custom model even when it is not in the provider's known model list.
- Add a native tray `AI Model` submenu after `AI Provider`.
- Selecting a tray AI model updates `EnhancementModelTextBox`, refreshes model choices, and applies enhancement settings.
- Keep the tray menu disabled during busy states through the existing quick-settings enable path.

## Non-Goals

- Do not fetch models from the tray in this slice.
- Do not add provider credentials or paid-provider flows.
- Do not change enhancement request construction beyond selected model persistence.
