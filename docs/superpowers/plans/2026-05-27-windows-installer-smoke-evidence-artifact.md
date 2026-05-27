# Windows Installer Smoke Evidence Artifact Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use `superpowers:test-driven-development`. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add safe evidence artifact upload to the manual Windows installer-smoke workflow.

---

## Task 1: Red Packaging Test

- [x] Add packaging asset coverage that expects an evidence directory, summary file, `actions/upload-artifact@v4`, and `if: ${{ always() }}`.
- [x] Run the focused packaging test and confirm it fails because the workflow does not upload evidence.

## Task 2: Workflow Implementation

- [x] Add an evidence preparation step under the artifact root.
- [x] Upload only the evidence directory.
- [x] Preserve the manual install-smoke execution gate and no-certificate boundary.

## Task 3: Verification And Commit

- [x] Run focused packaging tests, full solution tests/build, and whitespace check.
- [x] Review the diff and fix Critical/Important findings.
- [x] Update the project completion bar and commit the slice.
