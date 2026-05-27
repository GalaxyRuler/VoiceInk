# Windows Model Library Accessible Rows

## Goal

Close a Model Management accessibility gap by giving local model library action rows, storage guidance rows, and catalog cards stable UI Automation names.

## Source Of Truth

- The original macOS model library presents model cards and library guidance as readable rows, not opaque data objects.
- Windows ListView templates with multiple text elements need explicit accessible names so screen readers announce the same model, state, size, speed, accuracy, and guidance visible on screen.

## Online Grounding

- Microsoft documents that complex WinUI ListView item templates can set `AutomationProperties.Name` on the DataTemplate root to avoid defaulting to the data item's `ToString()`: https://learn.microsoft.com/en-us/windows/apps/design/controls/item-templates-listview

## Requirements

- Add computed accessible names for model library action rows and storage guidance rows.
- Add computed accessible names for local Whisper catalog cards, including display name, status, language, size, speed, accuracy, and description.
- Bind AI Models action, storage guidance, and catalog templates to `AutomationProperties.Name`.
- Add focused tests for presenter names, catalog card names, and XAML bindings.
- Keep model download, import, deletion, default selection, path health, warmup, and language behavior unchanged.

## Acceptance

- Focused tests fail before the presenter/catalog names and missing bindings.
- Focused model-library accessibility tests pass after implementation.
- Full solution tests and Debug x64 build pass.
