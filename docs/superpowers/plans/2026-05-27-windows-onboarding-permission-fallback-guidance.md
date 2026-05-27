# Windows Onboarding Permission Fallback Guidance Plan

Date: 2026-05-27

## Goal

Add first-run setup guidance for manually navigating to Windows microphone privacy settings if direct Settings links are unavailable.

## Steps

1. Add a failing onboarding presenter assertion for a `Manual Privacy Path` setup action.
2. Add the setup action after `Check Microphone` in `OnboardingChecklistPresenter`.
3. Run focused onboarding presenter tests.
4. Update project completion tracker and slice documentation.
5. Run the full verification gate:
   - `dotnet test VoiceInk.Windows\VoiceInk.Windows.sln`
   - `dotnet build VoiceInk.Windows\VoiceInk.Windows.sln -c Debug -p:Platform=x64`
   - `git diff --check`
6. Commit the completed slice.

## Review Notes

- Keep this advisory; do not mutate Windows privacy settings.
- Preserve onboarding completion semantics.
- Keep language short enough for setup rows.
