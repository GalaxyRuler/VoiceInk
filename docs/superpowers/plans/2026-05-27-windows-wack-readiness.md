# Windows App Certification Kit Readiness Plan

**Goal:** Add non-mutating WACK readiness and default-off workflow evidence for signed MSIX release validation.

## Plan

- [x] Add failing packaging asset assertions for WACK readiness report and workflow text.
- [x] Confirm focused packaging tests fail because the WACK readiness text is absent.
- [x] Add read-only WACK guidance to `test-release-readiness.ps1`.
- [x] Add a default-off `run_wack` input and `wack_report_path` evidence path to the manual installer-smoke workflow.
- [x] Keep WACK execution gated to the self-hosted Windows workflow and avoid local active-desktop execution.
- [x] Run focused packaging tests, full solution tests, Debug x64 build, and `git diff --check`.

## Online Grounding

Microsoft documents Windows App Certification Kit command-line validation with `appcert.exe reset`, `appcert.exe test -packagefullname`, `appcert.exe test -appxpackagepath`, and an active user session requirement.

## Verification Notes

- RED: focused packaging tests failed because `Windows App Certification Kit readiness reference` and `wack_report_path` were absent.
- GREEN: focused packaging tests passed after adding readiness/workflow coverage.
