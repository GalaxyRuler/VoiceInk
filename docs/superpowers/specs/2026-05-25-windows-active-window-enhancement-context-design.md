# Windows Active Window Enhancement Context Design

Date: 2026-05-25

## Goal

Add best-effort active-window context to AI enhancement prompts so the enhancer can use the foreground app and title to disambiguate transcript text.

## Source Of Truth

- macOS parity area: enhancement context includes app/window context where available.
- Windows current state: Power Mode already captures foreground process/title with native `GetForegroundWindow`, `GetWindowText`, and `GetWindowThreadProcessId` plumbing.
- External grounding: Microsoft documents `GetForegroundWindow`, `GetWindowText`, and `GetWindowThreadProcessId` as the standard Win32 APIs for identifying the active window and owning process.

## Behavior

- When enhancement runs, Windows requests active-window context by default.
- The Windows enhancement context provider reads active-window process name and window title through the existing Power Mode target provider.
- Prompt rendering adds an `<ACTIVE_WINDOW_CONTEXT>` section before selected text, clipboard, and vocabulary context.
- Failures or unavailable foreground-window data degrade to no active-window context.
- Context remains local to the enhancement request and is not used for telemetry or commercial gating.

## Testing

- Prompt renderer includes active-window process/title and preserves context ordering.
- Windows context provider includes active-window process/title when requested.
- Existing selected-text and clipboard context behavior remains unchanged.

## Non-Goals

- No browser URL extraction in this slice.
- No OCR or screenshot capture in this slice.
- No new Settings toggle in this slice; the context is best-effort and non-invasive.
