# Windows Shell Footer Navigation

## Goal

Polish the Windows shell so settings-style destinations are placed in the navigation footer instead of the main workflow list, preserving VoiceInk's feature order while following Windows navigation guidance.

## Source Of Truth

- The macOS app keeps settings and app-information surfaces distinct from primary dictation workflows.
- Microsoft WinUI `NavigationView` guidance supports footer menu items for destinations that should sit at the end of the navigation pane.
- Windows app settings guidance recommends a clear settings entry point that is visually separate from regular workflow commands.

## Requirements

- Primary workflow sections remain in the main NavigationView menu.
- Settings and About / Open Source are marked as footer navigation items by the Core shell presenter.
- The WinUI shell adds footer-marked items to `RootNavigationView.FooterMenuItems`.
- Existing tag-based restoration, tray Open Settings routing, and section selection continue to work for footer items.

## Acceptance

- Focused tests fail before implementation because shell items have no footer marker and the app does not use `FooterMenuItems`.
- Focused tests pass after implementation.
- Full solution tests and Debug x64 build pass.
