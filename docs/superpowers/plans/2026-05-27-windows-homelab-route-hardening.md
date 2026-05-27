# Windows Homelab Route Hardening Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use `superpowers:subagent-driven-development` or `superpowers:executing-plans` to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add tests and metadata correction so Homelab release-route discovery remains safe and the headless profile is not accidentally routed through the container preference.

---

## Task 1: Test The Route Contract

- [x] Add a failing packaging asset test for `.codex/homelab-runner.json`.
- [x] Assert allowed classes, preferred routes, profile paths, dry-run-only flags, no-network mode, and disabled live execution.
- [x] Run the focused packaging test and confirm the headless route assertion fails.

## Task 2: Fix Metadata

- [x] Change `preferredRoutes.headless` from `container` to `headless`.
- [x] Run the focused packaging test and confirm it passes.

## Task 3: Verification And Commit

- [x] Update the project completion tracker.
- [x] Run focused packaging tests, full solution tests, full Debug x64 build, and whitespace check.
- [x] Review the diff and fix Critical/Important findings.
- [ ] Commit the slice.
