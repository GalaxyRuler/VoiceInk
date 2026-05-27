# Windows Metrics Realtime Status Plan

## Slice

Add macOS-style realtime speed badges to Windows transcription model-performance rows.

## Steps

- [x] Ground the slice against Microsoft WinUI templated-list guidance and the macOS `ModelPerformancePanel` wording.
- [x] Add failing presenter tests for faster-than-realtime and slower-than-realtime transcription rows.
- [x] Update `ModelPerformancePresenter` to emit realtime status badges from `SpeedFactor`.
- [x] Run focused Metrics presenter tests.
- [x] Update the project completion tracker.
- [x] Run full solution tests, Debug x64 build, and whitespace diff check.
- [x] Commit the completed slice.
