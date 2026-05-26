# Windows Power Mode Rule Shortcuts Implementation Plan

**Goal:** Add direct per-rule Power Mode shortcuts through the existing global hotkey registration system.

**Tech Stack:** .NET 10, WinUI 3, existing `RegisterHotKey` native bridge, xUnit.

## Task 1: Add Shortcut Registration Tests

Add failing tests that require enabled Power Mode rules to register shortcut payloads and report duplicate assignments.

Status: completed.

## Task 2: Extend Core Shortcut Model

Add `PowerModeRule.Shortcut`, `SelectPowerModeRule`, and a `PowerModeRuleId` payload on `GlobalShortcutRegistration`.

Status: completed.

## Task 3: Preserve Native Hotkey Payload

Teach the native hotkey service to store and raise the full registration payload instead of only the action enum.

Status: completed.

## Task 4: Wire WinUI Selection

Add a Power Mode shortcut field with shortcut capture and route `SelectPowerModeRule` hotkeys to persist the selected rule id.

Status: completed.

## Task 5: Document And Verify

Update README/tracker, run focused tests, app build, full solution tests/build, and commit.

Status: completed.
