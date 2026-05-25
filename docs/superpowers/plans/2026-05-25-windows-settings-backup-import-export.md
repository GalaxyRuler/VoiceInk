# Windows Settings Backup Import Export Implementation Plan

## Goal

Add macOS-aligned local settings backup import/export to the Windows Settings section while preserving the fork's open-source rule: no licensing data, no telemetry, and no API keys/secrets in backup files.

## Grounding

- macOS source: `VoiceInk/Views/Settings/SettingsView.swift`, `VoiceInk/Services/ImportExportService.swift`, `VoiceInk/Services/BackupTypes.swift`, and `VoiceInk/Services/BackupImporter.swift`.
- Windows persistence: `VoiceInk.Windows/src/VoiceInk.Windows.Core/Settings/AppSettings.cs`, `VoiceInk.Windows/src/VoiceInk.Windows.Infrastructure/Settings/JsonSettingsStore.cs`, and `VoiceInk.Windows/src/VoiceInk.Windows.Infrastructure/Dictionary/JsonDictionaryStore.cs`.
- Windows picker guidance: Microsoft Learn recommends Windows App SDK file pickers for user-selected open/save locations in WinUI apps.
- JSON guidance: Microsoft Learn documents `System.Text.Json` as the built-in .NET serializer/deserializer for structured JSON payloads.

## Scope

- Export a single JSON backup from Settings.
- Include current app settings, custom prompts, Power Mode rules, imported local model references, and dictionary vocabulary/replacements.
- Exclude Credential Manager API keys and any secret store data.
- Import with category selection matching macOS where Windows has equivalents: General Settings, Custom Prompts, Power Mode, Dictionary, Custom Model Definitions.
- Validate/register imported shortcuts before saving imported settings.
- Refresh UI controls and in-memory app state after import.
- Document that history and metrics remain separate CSV exports.

## Task 1: Core Backup Contract And Tests

Files:

- Create `VoiceInk.Windows/src/VoiceInk.Windows.Core/Backup/VoiceInkSettingsBackup.cs`.
- Create `VoiceInk.Windows/src/VoiceInk.Windows.Core/Backup/VoiceInkSettingsBackupMerger.cs`.
- Create `VoiceInk.Windows/src/VoiceInk.Windows.Core/Backup/VoiceInkSettingsBackupCategory.cs`.
- Create `VoiceInk.Windows/tests/VoiceInk.Windows.Core.Tests/Backup/VoiceInkSettingsBackupTests.cs`.

Tests first:

- Backup JSON round-trips app settings, custom prompts, Power Mode rules, imported model references, and dictionary data.
- Backup JSON contains a clear secret-exclusion notice and does not contain API-key/secret fields.
- General import merges general settings while preserving prompts, Power Mode, and imported models when those categories are not selected.
- Category import updates prompts, Power Mode, and imported models only when selected.
- Dictionary backup payload remains compatible with the existing dictionary import parser.

Verification:

```powershell
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" test VoiceInk.Windows\tests\VoiceInk.Windows.Core.Tests\VoiceInk.Windows.Core.Tests.csproj --filter "FullyQualifiedName~VoiceInkSettingsBackup"
```

## Task 2: WinUI Settings Export/Import

Files:

- Modify `VoiceInk.Windows/src/VoiceInk.Windows.App/MainWindow.xaml`.
- Modify `VoiceInk.Windows/src/VoiceInk.Windows.App/MainWindow.xaml.cs`.

Implementation:

- Add a Backup group to Settings with `Export Settings` and `Import Settings` buttons.
- Export current UI-backed settings plus dictionary data through `FileSavePicker`.
- Import JSON through `FileOpenPicker`, show a category-selection `ContentDialog`, validate selected categories, pre-register imported shortcut settings, save settings, merge dictionary entries, refresh controls/state, and show status.
- Keep API keys out of export and show a post-import reminder when provider settings, prompts, or custom model definitions were imported.
- Clear provider endpoint fields from backups when they contain embedded credentials or key/token query parameters.
- If a later category import or UI refresh fails after settings were saved, restore the previous settings and hotkey registrations before reporting the failure.

Verification:

```powershell
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" build VoiceInk.Windows\VoiceInk.Windows.sln -c Debug -p:Platform=x64
```

## Task 3: Docs, Review, And Final Verification

Files:

- Update `README.md`.
- Update `docs/superpowers/specs/2026-05-24-windows-open-source-parity-design.md`.
- Update this plan checklist as work completes.

Review:

- Request focused review for backup schema, secret exclusion, category merge behavior, shortcut rollback, and UI stale-state risk.
- Fix all Critical and Important findings before commit.

Final verification:

```powershell
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" test VoiceInk.Windows\VoiceInk.Windows.sln
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" build VoiceInk.Windows\VoiceInk.Windows.sln -c Debug -p:Platform=x64
```

## Checklist

- [x] Core backup contract tests red.
- [x] Core backup contract implemented.
- [x] WinUI export/import wired.
- [x] Docs/spec updated.
- [x] Focused review requested and Critical/Important findings fixed.
- [x] Full tests/build pass.
- [x] Commit slice.
