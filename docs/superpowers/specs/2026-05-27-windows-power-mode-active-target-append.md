# Windows Power Mode Active Target Append

## Goal

Make the Windows Power Mode active-window picker practical for multi-app and multi-site rules by allowing the current target to be appended to existing rule fields.

## Source Of Truth

- The macOS app lets Power Mode configurations contain multiple app and URL targets.
- The Windows app now supports semicolon/newline-separated process, title, and URL alternatives, but the active-window capture button only replaces the current fields.

## Online Grounding

- Windows foreground-window/process metadata is available through local Win32 APIs and is the most reliable source for classic unpackaged desktop apps.
- Installed app enumeration is fragmented across packaged apps, Start Menu shortcuts, and classic Win32 registrations, so this slice uses the already-available active-window target path instead of inventing a brittle installed-app crawler.
- Reference: Microsoft UI Automation and desktop app tooling guidance describe extracting window title/process identity from active windows: https://learn.microsoft.com/windows/apps/dev-tools/winapp-cli/ui-automation

## Requirements

- Add a user-visible Power Mode action that appends the current active-window target to the process/title/URL fields.
- Appending preserves existing values.
- Appending deduplicates existing alternatives case-insensitively.
- Empty target values do not add blank delimiters.
- The existing replace behavior remains available.

## Non-Goals

- No global installed-app database.
- No shell shortcut parsing.
- No registry-based app discovery.
- No destructive editing of existing rule targets.
