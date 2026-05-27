# Windows Dictionary Overview Order Guidance Spec

## Goal

Make the Dictionary overview summary use the same replacement-order language as the row-level Dictionary guidance and transcript pipeline.

## Source Of Truth

The Windows transcript cleanup pipeline formats text before applying dictionary replacements. The Dictionary page already shows that order in summary rows and rule guidance; the top overview sentence must match it for both singular and plural replacement counts.

## Windows Behavior

The Dictionary overview must:

- Say `1 active replacement runs after text formatting` for one active replacement.
- Say `<n> active replacements run after text formatting` for multiple active replacements.
- Keep the no-active-replacements and disabled-replacement wording unchanged.
- Keep dictionary storage, matching, import/export, prompt rendering, and transcript processing unchanged.

## Open-Source Boundary

This is local UI presentation text only. It introduces no paid feature, account, telemetry, hosted dictionary sync, or commercial update path.

## Verification

- Add focused Dictionary presenter assertions for singular and plural active replacement overview copy.
- Run focused Dictionary presenter tests.
- Run full Windows tests/build and `git diff --check` before committing.
