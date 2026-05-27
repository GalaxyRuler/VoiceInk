# Windows History Enhancement Terminology Spec

## Goal

Align Windows History analysis terminology with the original VoiceInk enhancement language.

## Source Of Truth

The macOS app labels history metadata as `Enhancement Model`, `Enhancement Time`, and uses `Re-enhancement` for repeat AI passes. The Windows History analysis row already has the title `Enhancement`, but its detail said `AI cleanup completed`, which drifted from the product terminology.

## Windows Behavior

The History analysis presenter must:

- Keep the row title `Enhancement`.
- Keep values such as `Enhanced`, `Original only`, `Failed`, and `Canceled`.
- Say `AI enhancement completed` or `AI enhancement completed in <duration>` for enhanced history items.
- Leave retry/re-enhance logic, history persistence, export, audio playback, and provider metadata unchanged.

## Open-Source Boundary

This is local presenter text only. It introduces no commercial analytics, hosted history service, paid AI provider, account flow, or telemetry.

## Verification

- Update focused History presenter tests so stale `AI cleanup` wording fails.
- Run focused History presenter tests.
- Run full Windows tests/build and `git diff --check` before committing.
