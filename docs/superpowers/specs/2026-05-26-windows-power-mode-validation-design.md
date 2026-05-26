# Windows Power Mode Validation Design

## Goal

Make Power Mode rule editing safer and clearer by validating rules in Core and surfacing non-blocking WinUI messages in the Power Mode page.

## Grounding

- Microsoft WinUI `InfoBar` guidance positions it as visible, non-modal page feedback for important state. Power Mode validation fits that pattern better than modal dialogs because users may need to edit several fields before warnings clear.
- macOS Power Mode rules depend on clear app/site matching and predictable fallback behavior; Windows should avoid silently saving rules that can never match.

## Requirements

- Core exposes a UI-independent validator for one rule or a rule list.
- Enabled, non-default rules require at least one process, window title, or browser URL match field.
- Default fallback rules are allowed to have no match fields.
- Default fallback rules with match fields are valid but warn that those fields are ignored.
- Multiple enabled default fallback rules are invalid.
- Duplicate enabled specific match signatures warn that only the first matching rule applies.
- WinUI blocks saving invalid add/update candidates and displays validation text in an inline Power Mode `InfoBar`.
- WinUI shows list-level warnings/errors for persisted rules without blocking unrelated editing.

## Non-Goals

- Regex or wildcard matching.
- Per-field red validation chrome.
- Reordering rules automatically to resolve duplicates.
