# Windows Power Mode URL Matching Design

## Purpose

Power Mode should support site-specific rules on Windows so users can switch model, language, enhancement, prompt, paste cleanup, and formatting behavior for browser workflows.

## Source of Truth

The macOS app supports app/site-aware behavior. Windows already has Power Mode process/title rules and now has sanitized best-effort browser URL context through UI Automation.

## Desired Behavior

- Power Mode rules can include an optional browser URL pattern.
- URL patterns match against a sanitized active browser URL that has no query string, fragment, username, or password.
- URL matching respects the rule's existing match kind:
  - `Contains` matches case-insensitively within the sanitized URL.
  - `Equals` matches the full sanitized URL case-insensitively.
- A rule with a URL pattern should not match targets that have no browser URL.
- Process, window title, and URL patterns are combined with AND semantics when more than one is configured.
- Existing rules without a URL pattern keep their current behavior.
- The Power Mode editor exposes a URL pattern field and can populate it from the active target when available.

## Non-Goals

- No browser extension.
- No query/fragment matching.
- No regex matching.
- No network calls.
- No commercial behavior.

## Verification

- Core matcher tests cover URL-only rules, combined process/title/URL rules, sanitized URL matching, and missing URL fallback.
- Debug x64 build verifies the WinUI editor additions.
- Full solution tests must pass.
