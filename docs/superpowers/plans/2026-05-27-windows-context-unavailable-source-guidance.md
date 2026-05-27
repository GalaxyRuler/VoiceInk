# Windows Context Unavailable Source Guidance Plan

Date: 2026-05-27

## Goal

Make Context Awareness graceful degradation explicit when Windows or a foreground app cannot provide a context source.

## Steps

1. Add a failing Context readiness presenter assertion for an `Unavailable Sources` privacy row.
2. Add the row to baseline privacy rows in `EnhancementContextReadinessPresenter`.
3. Run the focused Context readiness presenter test.
4. Update the project completion tracker and slice documentation.
5. Run the full verification gate:
   - `dotnet test VoiceInk.Windows\VoiceInk.Windows.sln`
   - `dotnet build VoiceInk.Windows\VoiceInk.Windows.sln -c Debug -p:Platform=x64`
   - `git diff --check`
6. Commit the completed slice.

## Review Notes

- Keep behavior disclosure separate from provider fallback.
- Preserve user-controlled toggles and local-first language.
- Do not claim VoiceInk can bypass Windows permission or capture limitations.
