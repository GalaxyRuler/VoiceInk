# Windows Shortcut Reserved Key Guidance Spec

Date: 2026-05-27

## Context

VoiceInk for Windows already supports primary and secondary recording shortcuts, modifier-only recording, paste/retry/cancel/history/dictionary/enhancement/Power Mode shortcuts, duplicate detection, and Windows-key rejection. One remaining shortcut parity gap is Windows-reserved key handling before the native `RegisterHotKey` call.

Microsoft's `RegisterHotKey` documentation states that F12 is reserved for debugger use and should not be registered as a hotkey. VoiceInk should reject F12 during shortcut parsing and key capture so users receive an immediate configuration error instead of a runtime registration failure.

## Requirements

- Typed shortcuts containing F12 must be rejected with a clear Windows-reserved-key message.
- Captured key shortcuts using F12 must also be rejected.
- The behavior must be covered by unit tests.
- Existing Windows-key rejection and duplicate assignment behavior must remain unchanged.

## Non-Goals

- Do not add process-level hotkey owner detection.
- Do not inspect other apps' registered hotkeys.
- Do not change the native `RegisterHotKey` implementation in this slice.
- Do not reject all function keys; only F12 is reserved by this documented rule.

## Acceptance Criteria

- `GlobalShortcut.TryParse("Ctrl+F12", ...)` returns false and explains F12 is reserved.
- The focused shortcut test suite passes.
- Project completion documentation reflects the reserved-key slice.
- Full solution tests, Debug x64 build, and whitespace validation pass.
