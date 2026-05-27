# Windows Power Mode Setup Directory Spec

## Intent

Bring the Windows Power Mode page closer to the macOS app's scan-friendly workflow by summarizing the rule setup flow before users edit detailed fields.

## Requirements

- Show a compact presenter-backed setup directory for Targets, Overrides, Shortcuts, and Session behavior.
- Keep Power Mode behavior unchanged: matching, overrides, shortcuts, validation, persistence, and dictation resolution must not change.
- Expose accessible names for each repeated setup row so screen readers get the row title, value, and detail together.
- Keep the presenter in Core so the copy and accessibility text are unit-testable without UI automation.

## Open-Source Boundary

This slice has no commercial behavior. It adds only local UI copy and accessibility metadata.

## Verification

- Focused Core tests for `PowerModePagePresenterTests`.
- Focused XAML accessibility tests for repeated summary rows.
- WinUI app build because `MainWindow.xaml` changed.
