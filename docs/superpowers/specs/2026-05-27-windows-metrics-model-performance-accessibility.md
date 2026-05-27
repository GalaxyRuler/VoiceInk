# Windows Metrics Model Performance Accessibility Spec

Date: 2026-05-27

## Goal

Improve Metrics visual polish and accessibility by giving model performance rows a readable UI Automation name.

## Source Of Truth

Microsoft WinUI guidance identifies `AutomationProperties.Name` as the human-readable identifier reported by assistive technologies when a control or templated element does not already expose a useful name.

## Windows Behavior

- Model performance presenter rows expose an `AccessibleName` combining title, primary value, status badge, subtitle, and detail.
- Transcription and enhancement model performance row templates bind `AutomationProperties.Name` to `AccessibleName`.
- Existing visible text, metric calculations, CSV export, filters, and model performance status badges remain unchanged.

## Non-Goals

- Do not add UI automation tests for this slice.
- Do not change metrics storage or aggregation.
- Do not restyle the Metrics page beyond the accessible row name binding.
