# Windows Dev ZIP Smoke Validator Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add and verify a non-installing smoke validator for the Windows dev ZIP package.

**Architecture:** Add a PowerShell script under `VoiceInk.Windows/scripts` and extend the existing packaging asset tests to assert the script's safety and expected checks.

**Tech Stack:** PowerShell, .NET 10 xUnit packaging tests, Windows App SDK self-contained publish output.

---

### Task 1: Script Contract Test

**Files:**
- Modify: `VoiceInk.Windows/tests/VoiceInk.Windows.Core.Tests/Packaging/WindowsPackagingAssetsTests.cs`

- [x] **Step 1: Write failing script test**

Assert the script exists, checks expected app/runtime files, uses `Expand-Archive`, refuses paths outside artifacts, and avoids install/launch commands.

- [x] **Step 2: Verify red**

Focused packaging tests failed because `scripts/test-dev-zip.ps1` did not exist.

### Task 2: Smoke Validator Script

**Files:**
- Create: `VoiceInk.Windows/scripts/test-dev-zip.ps1`

- [x] **Step 1: Implement validator**

Add artifact-root path safety, ZIP expansion, extracted-folder support, expected-file checks, README model-path check, and success output.

- [x] **Step 2: Verify focused tests**

Focused packaging tests passed.

- [x] **Step 3: Fix review finding**

Added an extraction-root guard so the validator rejects ZIP input inside the scratch cleanup folder before removing any extraction directory.

### Task 3: Real Package Smoke

**Files:**
- Modify: `docs/superpowers/project-completion.md`

- [x] **Step 1: Build a dev ZIP**

Run `package-dev-zip.ps1` with the repo-local .NET 10 SDK into `VoiceInk.Windows\artifacts\dev-zip-smoke`.

- [x] **Step 2: Validate the ZIP**

Run `test-dev-zip.ps1` against the produced ZIP and confirm the smoke validation passes.

- [ ] **Step 3: Full verification and commit**

Run full solution tests/build, review the slice, and commit.
