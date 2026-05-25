# Windows Cancel Processing Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add cancel support for post-recording transcription/enhancement/insertion work.

**Architecture:** Keep the cancel semantics in `DictationController`, which owns pipeline state and history writes. Use a linked cancellation token in `MainWindow` so Cancel can interrupt the current stop pipeline without canceling the whole app.

**Tech Stack:** .NET 10, WinUI 3, xUnit, existing Core dictation services.

---

## Task 1: Core Cancellation History

- [ ] Add a failing `DictationControllerTests` test for canceling during transcription after capture stop.
- [ ] Run the focused test and confirm it fails because no canceled history row is saved.
- [ ] Update `DictationController.StopAsync` to save a canceled history item best-effort when cancellation happens after audio capture completes.
- [ ] Re-run the focused test and the full `DictationControllerTests` filter.

## Task 2: App Cancel Wiring

- [ ] Add `CancellationTokenSource? stopOperationCancellation` to `MainWindow`.
- [ ] In `StopCurrentRecordingAsync`, create a linked token source and pass it through pending recorder-control wait, `controller.StopAsync`, and post-stop refresh work.
- [ ] In `CancelCurrentRecordingAsync`, when `isStopping` and the controller is `Transcribing` or `Inserting`, cancel the stop token, update the status, and return without running recording-cancel logic.
- [ ] Enable the Cancel button while the controller is `Recording`, `Transcribing`, or `Inserting` and no unrelated operation blocks it.
- [ ] Build the app project.

## Task 3: Docs, Verification, Commit

- [ ] Update the parity spec, README if needed, and completion tracker.
- [ ] Run focused Core tests.
- [ ] Run full solution tests.
- [ ] Run full Debug x64 build.
- [ ] Review the diff for Critical/Important issues.
- [ ] Commit with `feat(windows): cancel dictation processing`.
