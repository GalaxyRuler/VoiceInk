# Windows Onboarding Health Checklist Implementation Plan

**Goal:** Add a first-run readiness checklist for model, shortcut, microphone device, privacy settings, and microphone capability.

**Tech Stack:** .NET 10, WinUI 3, xUnit, existing onboarding status presenter.

## Task 1: Add Checklist Tests

Extend onboarding status tests to assert the readiness checklist for model path, shortcut, microphone device, privacy review, and app capability.

Status: completed.

## Task 2: Extend Onboarding Status Model

Add health-check text to `OnboardingSetupStatus` and build it from `OnboardingSetupStatusService`.

Status: completed.

## Task 3: Surface Checklist In Dialog

Add a wrapped text block to first-run setup and refresh it whenever setup status changes.

Status: completed.

## Task 4: Document And Verify

Update README and completion tracker, then run focused tests, full solution tests, Debug x64 build, and `git diff --check`.

Status: completed.
