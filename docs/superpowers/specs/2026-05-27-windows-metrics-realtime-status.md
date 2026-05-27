# Windows Metrics Realtime Status

## Goal

Bring the Windows Metrics model-performance rows closer to the macOS `ModelPerformancePanel` tiles by labeling transcription model speed as faster or slower than real-time.

## Source Of Truth

- macOS `VoiceInk/Views/Metrics/ModelPerformancePanel.swift` renders transcription model tiles with a speed factor and the text `Faster than Real-time` when the factor is at least `1.0`, otherwise `Slower than Real-time`.
- Microsoft WinUI ListView item-template guidance supports presenting these rows as templated local data, which matches the existing Windows Metrics presenter approach.

## Windows Behavior

- Transcription model-performance rows continue to show the speed factor as the primary value.
- The row badge now communicates realtime status:
  - `Faster than Real-time` when `SpeedFactor >= 1`.
  - `Slower than Real-time` when `SpeedFactor < 1`.
- Enhancement rows keep their existing latency wording because the macOS enhancement tiles present enhancement duration rather than realtime speed.
- The change is local-only presentation logic; it does not alter metrics storage, exports, or provider behavior.

## Verification

- Focused Core tests must cover both faster-than-realtime and slower-than-realtime transcription rows.
- Full solution tests and Debug x64 build remain the release gate before committing the slice.
