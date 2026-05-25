# Windows Selected Text Clipboard Fallback Design

Date: 2026-05-25

## Goal

Improve enhancement context capture by falling back to a guarded clipboard copy when UI Automation cannot read selected text.

## Source Of Truth

- macOS parity area: selected text context should be available for enhancement prompts where possible.
- Windows current state: selected text uses UI Automation `TextPattern.GetSelection`; clipboard context reads existing clipboard text separately.
- External grounding: Microsoft UI Automation exposes selected text through `TextPattern.GetSelection`; WinUI/Windows clipboard APIs expose text through clipboard data packages. Some Windows apps do not expose selection through UI Automation, so a copy shortcut fallback is needed.

## Behavior

- When selected text context is requested, Windows first tries UI Automation.
- If UI Automation returns non-empty selected text, clipboard fallback is skipped.
- If UI Automation returns empty text, Windows sends a copy shortcut to the focused app, waits briefly, reads clipboard text, trims/truncates it, and restores the prior clipboard state.
- If clipboard fallback fails, selected text context is empty and enhancement continues.
- If clipboard context is also requested, it is read only after selected-text fallback restores the clipboard so ordinary clipboard context remains the user's original clipboard text.
- The fallback is local-only and does not send clipboard content anywhere by itself; it only contributes to the existing enhancement prompt context when enhancement runs.

## Testing

- `WindowsEnhancementContextProvider` falls back to clipboard-selected text when UI Automation returns empty.
- It does not use fallback when UI Automation returns selected text.
- It restores before reading ordinary clipboard context when both selected text and clipboard context are requested.

## Non-Goals

- No OCR or browser URL context in this slice.
- No app-specific permission UI in this slice.
- No telemetry or commercial surface.
