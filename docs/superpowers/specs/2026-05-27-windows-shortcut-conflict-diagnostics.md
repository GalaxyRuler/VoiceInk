# Windows Shortcut Conflict Diagnostics

VoiceInk for Windows already validates shortcut syntax, rejects Windows-key shortcuts, rejects F12, detects duplicate in-app assignments, and registers shortcuts through the native `RegisterHotKey` API. The remaining user-facing gap is the rare case where Windows rejects a syntactically valid shortcut because another app or Windows component already owns it.

Microsoft's `RegisterHotKey` documentation says registration fails when another hot key has already registered the same chord, and also documents that F12 is reserved for debugger use. Windows reports the conflict to the caller, but does not provide a supported owner-application lookup. VoiceInk should therefore make the conflict actionable without claiming it can identify the owning process.

## Requirements

- Native `RegisterHotKey` failures must still include:
  - the VoiceInk action;
  - the shortcut display text;
  - the Win32 error code and message.
- Win32 error `1409` must additionally explain that the shortcut is already registered by another app or Windows component.
- The message must state that Windows does not expose which app owns the shortcut and recommend choosing another shortcut or closing the conflicting app.
- Unknown Win32 errors must keep the previous concise diagnostic shape.
- The presenter must live in Core so behavior can be tested without native UI automation.

## Non-Goals

- No global keyboard hook replacement.
- No attempt to enumerate the owning app for a registered hotkey.
- No registry, service, or system-wide diagnostics.
