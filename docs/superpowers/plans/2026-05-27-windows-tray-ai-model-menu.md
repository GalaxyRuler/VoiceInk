# Windows Tray AI Model Menu Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use `superpowers:test-driven-development`. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add tray AI Model selection parity with macOS `MenuBarView`.

---

## Task 1: Red Presenter Tests

- [x] Add `TrayEnhancementModelOptionsPresenterTests`.
- [x] Cover checked known model, selected custom model, blank selected model, and distinct trimmed choices.
- [x] Run focused shell tests and confirm production presenter/state members are missing.

## Task 2: Core And Native Implementation

- [x] Add `TrayEnhancementModelOptionsPresenter`.
- [x] Add `EnhancementModels` to `TrayQuickSettingsState`.
- [x] Add native tray `AI Model` submenu and selection event.
- [x] Wire MainWindow tray state and selection handler.

## Task 3: Verification And Commit

- [x] Update project completion tracker.
- [x] Run focused shell tests, app project build, full solution tests, full Debug x64 build, and whitespace check.
- [x] Review the diff and fix Critical/Important findings.
- [x] Commit the slice.
