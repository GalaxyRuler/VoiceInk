# Windows Dictionary Replacement Order Guidance Spec

## Goal

Align Dictionary page guidance with the Windows transcript pipeline: enabled word replacements run after optional text formatting and before final cleanup/insertion.

## Source Of Truth

The macOS transcription pipeline formats text before applying word replacements. The Windows `TextPostProcessor` now follows that order, so Dictionary presenter copy must not describe replacements as running before formatting or after generic cleanup.

## Windows Behavior

The Dictionary page must:

- Describe active replacements as running after text formatting.
- Keep disabled replacement guidance unchanged.
- Keep the provider privacy boundary clear: vocabulary may be sent as prompt context, while replacements are applied locally after formatting.
- Avoid changing replacement storage, matching, import/export, or provider behavior.

## Open-Source Boundary

This is local presentation text only. It adds no telemetry, account flow, paid prompt, hosted dictionary service, or commercial feature gate.

## Verification

- Add/update focused `DictionaryPagePresenterTests` assertions that fail on stale replacement-order wording.
- Run focused Dictionary presenter tests.
- Run full Windows tests/build and `git diff --check` before committing.
