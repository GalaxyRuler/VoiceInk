# Windows In-App Notification Surface Spec

## Goal

Add a Windows-native equivalent for the macOS transient in-app notification surface so user actions, warnings, and errors are visible beyond the footer status text.

## Source Of Truth

- macOS notification UI: `VoiceInk/Notifications/AppNotificationView.swift`
- macOS notification manager: `VoiceInk/Notifications/NotificationManager.swift`
- Windows status source: `VoiceInk.Windows/src/VoiceInk.Windows.App/MainWindow.xaml.cs`

## Requirements

- Use a Windows-native WinUI surface instead of macOS panels.
- Keep classification logic testable in Core.
- Suppress passive state labels such as Idle, Recording, Transcribing, and Inserting.
- Classify user-facing statuses into info, success, warning, and error.
- Auto-dismiss notifications with longer visibility for warnings and errors.
- Continue updating the existing status footer, tray tooltip, and floating recorder status.
- Do not use system toast registration, commercial telemetry, or external notification services.

## Verification

- Core presenter tests for message classification and passive-status suppression.
- App project build for WinUI wiring.
- Full solution tests and Debug x64 build before commit.
