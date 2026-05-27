# Windows Shell Footer Navigation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use `superpowers:test-driven-development`.

**Goal:** Move Settings and About / Open Source into the WinUI navigation footer.

---

## Task 1: Red Tests

- [x] Add Core shell expectations that Settings and About are footer items.
- [x] Add static app coverage that navigation initialization clears and populates `FooterMenuItems`.
- [x] Confirm focused tests fail before implementation.

## Task 2: Shell Implementation

- [x] Add an `IsFooter` shell item flag.
- [x] Mark Settings and About / Open Source as footer entries.
- [x] Route footer entries into `RootNavigationView.FooterMenuItems` while keeping the shared tag lookup.
- [x] Run focused tests.

## Task 3: Verification And Commit

- [x] Run full solution tests.
- [x] Run Debug x64 solution build.
- [x] Run whitespace check and commit the slice.

## Verification Notes

- Online grounding: Microsoft documents `NavigationView.FooterMenuItems` for items at the end of the navigation pane, and Windows app settings guidance recommends a distinct settings entry point.
- Red: focused Core tests failed because `ShellNavigationItem` had no `IsFooter` property.
- Green: focused Core tests passed after adding footer metadata and app footer wiring.
- Full test: solution test passed with 776 Core tests and 266 Infrastructure tests.
- Build: Debug x64 solution build passed with 0 warnings and 0 errors.
- Whitespace: `git diff --check` passed with only line-ending normalization warnings.
