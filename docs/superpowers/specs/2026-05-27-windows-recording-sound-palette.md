# Windows Recording Sound Palette

## Goal

Give Windows users built-in start/stop sound choices beyond `System Default` and `Custom Sound`, while keeping all feedback local and open-source friendly.

## Source Of Truth

- The macOS app has recording feedback sound choices and test controls.
- The Windows app already has sound feedback enablement, start/stop modes, custom sound import/reset/test, and native playback.

## Online Grounding

- Microsoft documents `System.Media.SystemSounds` as access to the current user's Windows sound scheme events: `Asterisk`, `Beep`, `Exclamation`, `Hand`, and `Question`.
- These sounds are provided by Windows and configurable by the user in their sound scheme, so no bundled commercial sound assets are required.
- Reference: https://learn.microsoft.com/dotnet/api/system.media.systemsounds

## Requirements

- Add built-in start and stop sound choices for Asterisk, Beep, Exclamation, Hand, and Question.
- Keep `System Default` as the default start/stop behavior.
- Keep `Custom Sound` import/reset/test behavior unchanged.
- Persist selected built-in choices through existing settings.
- Test buttons play the selected built-in sound.

## Non-Goals

- No bundled paid or proprietary audio assets.
- No audio marketplace/download flow.
- No global Windows sound scheme modification.
