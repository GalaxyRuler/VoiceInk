# Windows Browser URL Context Design

## Purpose

VoiceInk for Windows should give AI enhancement limited awareness of the active browser page, matching the macOS context direction while preserving local-only, open-source behavior.

## Source of Truth

The macOS app uses surrounding app/site context to improve transcript cleanup and Power Mode decisions. On Windows, browser URL access is not a stable OS API; it must be best-effort through UI Automation and must degrade silently when unavailable.

## Desired Behavior

- When enhancement runs, the context request should include browser URL context by default.
- If the active foreground app is a supported browser and its address box exposes an HTTP or HTTPS URL through UI Automation, include a sanitized URL in the enhancement prompt.
- Sanitization must remove query strings, fragments, usernames, passwords, and non-HTTP schemes.
- If URL reading fails, times out, finds no browser, or sees an unsupported value, enhancement should continue without browser URL context.
- Browser URL context should render after active window context and before selected text, clipboard, and vocabulary.

## Supported Browser Processes

Initial best-effort allowlist:

- `chrome`
- `msedge`
- `firefox`
- `brave`
- `opera`
- `vivaldi`
- `arc`

## Non-Goals

- No browser extensions.
- No network calls.
- No browsing history collection.
- No query string or fragment forwarding.
- No UI setting in this slice; this follows the existing enhancement context request path.
- No Power Mode browser URL matching in this slice.

## Verification

- Core tests prove URL sanitization and prompt ordering.
- Pipeline tests prove enhancement requests browser URL context by default.
- Infrastructure tests prove the Windows provider includes browser URL context only when requested and gracefully ignores reader failures.
- Full solution tests and Debug x64 build must pass.
