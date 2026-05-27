# Windows Dictionary Quick Add Guidance

## Goal

Make the Windows Dictionary page explain the Quick Add path that matches the original macOS Dictionary settings and the Windows tray/global shortcut surface.

## Source Of Truth

- The macOS Dictionary settings panel includes a "Quick Add to Dictionary" shortcut row.
- The Windows app already exposes Quick Add through the notification-area menu and the configurable global shortcut.
- Microsoft documents notification-area icons as a place for quick commands through the icon context menu, and Windows apps commonly use keyboard shortcuts for frequent actions.

## Requirements

- Add a Dictionary rule guidance row for Quick Add.
- Mention the tray menu and global shortcut as entry points.
- Keep the guidance presenter-backed and covered by Core tests.
- Do not change dictionary storage or quick-add behavior.

## Non-Goals

- No new shortcut registration behavior.
- No new modal workflow.
- No cloud sync or telemetry.
