# Windows Audio Queue Accessible Rows

## Goal

Close a Transcribe Audio accessibility polish gap by giving queued audio-file rows explicit UI Automation names.

## Source Of Truth

- The macOS app's audio-file transcription flow presents each queued file with status and progress/error detail.
- Windows should expose each queue row as a coherent accessible item while preserving the existing queue behavior.

## Online Grounding

- Microsoft Accessibility Insights guidance for XAML ListView items recommends binding `AutomationProperties.Name` in the item template rather than relying on `ToString()`: https://learn.microsoft.com/en-us/accessibility-tools-docs/items/uwpxaml/listitem_name

## Requirements

- Add an embedded audio queue row model with display text and accessible name.
- Bind `AudioFileQueueListView` row template to `AccessibleName`.
- Keep queue add/remove/retry/cancel/start, selection, snapshot restore, and details pane behavior unchanged.

## Acceptance

- Focused XAML/code tests fail before the row model and template binding exist.
- Focused tests pass after implementation.
- Full solution tests and Debug x64 build pass.
