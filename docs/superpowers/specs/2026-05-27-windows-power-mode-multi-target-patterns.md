# Windows Power Mode Multi-Target Patterns

## Goal

Let one Windows Power Mode rule match multiple apps, titles, or sites, matching the macOS ability to attach multiple app and URL configs to a single mode.

## Source Of Truth

- `VoiceInk/PowerMode/PowerModeConfig.swift` stores `appConfigs: [AppConfig]?` and `urlConfigs: [URLConfig]?`.
- `PowerModeManager.getConfigurationForURL` checks every configured URL for a mode.
- `PowerModeManager.getConfigurationForApp` checks every configured app for a mode.

## Windows Design

- Keep the current WinUI fields, but treat semicolon- or newline-separated process/title/URL entries as multiple alternatives.
- A rule with multiple process entries matches when any process entry matches; the same applies to title and URL fields.
- Existing single-value rules keep their behavior.
- URL entries still use sanitized URL comparison so query strings and fragments do not become match requirements.

## Online Grounding

- .NET's `Process.ProcessName` documentation describes process names as executable names without the `.exe` extension, which supports the current Windows process-name field wording: https://learn.microsoft.com/dotnet/api/system.diagnostics.process.processname
- Microsoft WinUI list controls support multi-item UI patterns, but this slice deliberately uses the existing field-based UI to avoid a larger layout refactor: https://learn.microsoft.com/windows/apps/design/controls/listview-and-gridview

## Requirements

- Add tests for semicolon/newline process and URL alternatives.
- Update Power Mode matching to use any configured alternative per field.
- Update row summaries/guidance so users can discover the delimiter behavior.
- Preserve rule order, default fallback behavior, explicit selected-rule behavior, and existing single-pattern behavior.

## Non-Goals

- No installed-app picker in this slice.
- No new persisted array schema.
- No changes to Power Mode override application.
