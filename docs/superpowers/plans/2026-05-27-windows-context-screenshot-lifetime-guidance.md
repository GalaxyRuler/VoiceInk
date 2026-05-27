# Windows Context Screenshot Lifetime Guidance Plan

**Goal:** Make Windows OCR context privacy guidance match VoiceInk's one-time screenshot documentation.

## Steps

- [x] Add a failing Core presenter test for one-time screenshot/image-not-uploaded guidance.
- [x] Add the OCR privacy row in `EnhancementContextReadinessPresenter`.
- [x] Update parity docs and completion tracker.
- [x] Run focused Core tests.
- [x] Run full solution tests/build and whitespace checks.
- [x] Commit the slice and request review.

## Verification

- Red focused Core presenter test failed because the screenshot lifetime row was missing.
- Green focused Core presenter tests passed: 3 tests.
- Initial parallel full test/build attempt failed with `CS2012` file-in-use output because concurrent `dotnet` processes wrote shared `obj` DLLs.
- Sequential full solution tests with `-nr:false -p:UseSharedCompilation=false` passed: Core 798/798 and Infrastructure 267/267.
- Sequential Debug x64 build with `-nr:false -p:UseSharedCompilation=false` succeeded with 0 warnings and 0 errors.
- `git diff --check` exited 0 with line-ending warnings only.
