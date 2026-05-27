# Windows Enhancement Behavior Guidance Plan

**Goal:** Surface macOS-default short-phrase and timeout/retry enhancement behavior in the Windows Enhancement page.

## Steps

- [x] Add failing Core presenter coverage for the short-phrase guard default.
- [x] Add failing Core presenter coverage for the disabled short-phrase guard state.
- [x] Add failing Core presenter coverage for the single-attempt timeout state.
- [x] Add `Short Phrase Guard` and `Timeout Policy` rows to the Enhancement readiness/action presenter.
- [x] Rename the WinUI list heading from `Context Actions` to `Enhancement Behavior`.
- [x] Update the parity spec and project completion tracker.
- [ ] Run focused tests, full solution tests, Debug x64 build, and whitespace check.
- [ ] Commit the completed slice and request review.

## Verification

- Red: focused `EnhancementContextReadinessPresenterTests` failed because the new rows were absent.
- Green: focused `EnhancementContextReadinessPresenterTests` passed with 14 tests.

## References

- `VoiceInk/AppDefaults.swift` for macOS enhancement defaults.
- VoiceInk public docs for enhancement trigger words and Assistant mode behavior.
