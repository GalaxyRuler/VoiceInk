# Windows Middle-Click Recording Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use `superpowers:subagent-driven-development` or `superpowers:executing-plans` to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add default-off middle-click recording parity to the Windows app.

**Architecture:** Persist the new settings in Core `AppSettings`, surface them in the WinUI Shortcuts settings area, and wire them into `GlobalHotkeyService` as an optional low-level mouse hook. The native hook only observes middle-button down/up, starts/cancels a UI-thread timer, and raises the same recording toggle event path as keyboard shortcuts.

**Tech Stack:** .NET 10, WinUI 3, Win32 `SetWindowsHookEx` with `WH_MOUSE_LL`, xUnit.

---

## Task 1: Settings Persistence

- [x] Add failing settings tests for default and saved middle-click settings.
- [x] Add `AppSettings` properties and value equality/hash participation.
- [x] Run focused JSON settings tests.

## Task 2: Native Hook And UI Wiring

- [x] Add Shortcuts settings controls for enabling middle-click recording and its activation delay.
- [x] Load and save the controls through the existing settings path.
- [x] Extend `GlobalHotkeyService` with an optional `WH_MOUSE_LL` middle-button hook.
- [x] Register the hook only when enabled and unhook on replacement/disposal.
- [x] Route the delayed trigger through the existing recording toggle event.

## Task 3: Docs, Verification, Review, Commit

- [x] Update the project completion tracker.
- [x] Run focused settings tests, app build, full solution tests, full solution build, and whitespace check.
- [x] Review the diff and fix Critical/Important findings.
- [ ] Commit the completed slice.
