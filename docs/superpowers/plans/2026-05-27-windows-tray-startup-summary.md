# Windows Tray Startup Summary Plan

**Goal:** Surface the start-hidden-to-tray setting in the Settings current-state summary.

## Steps

- [x] Add failing Settings presenter assertions for default visible startup and tray-first startup.
- [x] Add the `Tray Startup` preference summary row.
- [x] Run focused Settings presenter tests.
- [x] Run full solution tests and whitespace check.
- [ ] Commit the slice and request review.

## Verification

- Red focused Settings presenter tests failed because the summary still had four rows.
- Green focused Settings presenter tests passed: 4 tests.
- Full solution tests passed: Core 793/793 and Infrastructure 267/267.
- `git diff --check` exited 0 with line-ending warnings only.

## References

- `docs/superpowers/specs/2026-05-27-windows-start-hidden-to-tray.md`
