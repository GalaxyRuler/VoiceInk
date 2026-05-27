# Windows Diagnostics System Context Plan

## Slice

Add safe local OS architecture and logical-processor context to Windows diagnostic reports.

## Steps

- [x] Ground the slice against .NET runtime/system-information APIs and macOS metrics diagnostics.
- [x] Add failing Core diagnostic report coverage for OS architecture and logical processor count.
- [x] Extend `DiagnosticReportRequest` and `DiagnosticReportBuilder`.
- [x] Populate the new report fields from `MainWindow`.
- [x] Run focused diagnostic report tests.
- [x] Update the parity spec and project completion tracker.
- [x] Run full solution tests, Debug x64 build, and whitespace diff check.
- [x] Commit the completed slice.
