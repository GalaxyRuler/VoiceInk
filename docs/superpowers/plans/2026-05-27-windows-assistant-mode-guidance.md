# Windows Assistant Mode Guidance Plan

**Goal:** Make Assistant Mode usage visible in the Windows Enhancement behavior surface.

## Steps

- [x] Add failing Core presenter tests for Assistant Mode available/selected guidance.
- [x] Add an Assistant Mode action row in `EnhancementContextReadinessPresenter`.
- [x] Update existing presenter tests for the new row.
- [x] Update parity docs and completion tracker.
- [x] Run focused Core tests.
- [x] Run full solution tests/build and whitespace checks.
- [x] Commit the slice and request review.

## Verification

- Red focused Core presenter tests failed because the Assistant Mode behavior row was missing.
- Green focused Core presenter tests passed: 3 tests.
- Full solution tests with `-nr:false -p:UseSharedCompilation=false` passed: Core 800/800 and Infrastructure 267/267.
- Debug x64 build with `-nr:false -p:UseSharedCompilation=false` succeeded with 0 warnings and 0 errors.
- `git diff --check` exited 0 with line-ending warnings only.
