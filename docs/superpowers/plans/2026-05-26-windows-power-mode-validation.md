# Windows Power Mode Validation Plan

## Goal

Add UI-independent Power Mode rule validation and surface the results in the WinUI Power Mode editor.

## Steps

- [x] Add failing Core tests for required match fields, default fallback behavior, multiple defaults, and duplicate match warnings.
- [x] Implement `PowerModeRuleValidator` and validation result models in Core.
- [x] Add a Power Mode validation `InfoBar` to the WinUI editor.
- [x] Block add/update when a candidate rule is invalid.
- [x] Keep list-level warnings visible for duplicate match fields and ignored default match fields.
- [x] Run focused Core validator tests and app build.
- [x] Update README/completion tracker.
- [x] Run full solution tests, Debug x64 build, and `git diff --check`.
- [ ] Commit the slice.
