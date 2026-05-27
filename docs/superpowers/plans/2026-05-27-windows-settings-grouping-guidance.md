# Windows Settings Grouping Guidance Plan

Date: 2026-05-27

## Goal

Improve Settings discoverability by adding presenter-backed grouped-section guidance to the Settings overview.

## Steps

1. Add a failing Settings presenter test assertion for a `Find Settings` action summary.
2. Add the action summary to `SettingsSectionPresenter`.
3. Run the focused Settings presenter test.
4. Update the project completion tracker and slice documentation.
5. Run the full verification gate:
   - `dotnet test VoiceInk.Windows\VoiceInk.Windows.sln`
   - `dotnet build VoiceInk.Windows\VoiceInk.Windows.sln -c Debug -p:Platform=x64`
   - `git diff --check`
6. Commit the completed slice.

## Review Notes

- Keep the row informational and aligned with the existing section order.
- Keep data-safety and diagnostics rows local/open-source oriented.
- Avoid introducing a search UI without a full page-level design pass.
