# Windows Dictionary Import Conflict Guidance Plan

Date: 2026-05-27

## Goal

Make dictionary import conflict behavior visible in presenter-backed rule guidance.

## Steps

1. Add a failing Dictionary presenter assertion for an `Import Conflicts` guidance row.
2. Add the row to `DictionaryPagePresenter.RuleGuidanceRows`.
3. Run the focused Dictionary presenter test.
4. Update the project completion tracker and slice documentation.
5. Run the full verification gate:
   - `dotnet test VoiceInk.Windows\VoiceInk.Windows.sln`
   - `dotnet build VoiceInk.Windows\VoiceInk.Windows.sln -c Debug -p:Platform=x64`
   - `git diff --check`
6. Commit the completed slice.

## Review Notes

- Keep this as guidance only; import behavior remains unchanged.
- Preserve local JSON language and open-source local-first posture.
- Keep the row close to backup/import workflow guidance.
