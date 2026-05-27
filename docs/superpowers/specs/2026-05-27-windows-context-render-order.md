# Windows Context Render Order Spec

## Goal

Align Windows enhancement context rendering with the macOS VoiceInk context order while preserving Windows-only active app and browser metadata.

## Source Of Truth

The macOS enhancement service appends context sections in this order:

1. `<CURRENTLY_SELECTED_TEXT>`
2. `<CLIPBOARD_CONTEXT>`
3. `<CURRENT_WINDOW_CONTEXT>`
4. `<CUSTOM_VOCABULARY>`

Windows also has separate active-window and sanitized browser URL metadata. Those sections should follow the selected-text and clipboard context that macOS surfaces first, then precede OCR/current-window text so the OCR block can remain the main current-window content source.

## Windows Behavior

The Windows enhancement prompt renderer must emit available context sections in this order:

1. Selected text.
2. Clipboard text.
3. Windows active-window metadata.
4. Sanitized browser URL metadata.
5. Current-window OCR text.
6. Custom vocabulary.

The Enhancement page Context Source Order row must describe this order so the visible guidance matches actual prompt rendering.

## Open-Source Boundary

The slice changes local prompt construction and local UI guidance only. It adds no telemetry, accounts, paid prompt catalog, licensing, or commercial provider fallback.

## Verification

- Add a focused renderer test that fails when Windows metadata precedes selected/clipboard context.
- Update existing renderer and readiness tests to assert the new order.
- Run the focused enhancement tests, then the full Windows test/build suite before committing.
