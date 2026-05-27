# Windows Recorder Waveform Parity

## Goal

Bring the Windows floating recorder waveform closer to the macOS mini/notch recorder visualizer while keeping the implementation local, testable, and UI-light.

## Source Of Truth

- macOS visualizer: `VoiceInk/Views/Recorder/AudioVisualizerView.swift`
- macOS mini recorder: `VoiceInk/Views/Recorder/MiniRecorderView.swift`
- macOS notch recorder: `VoiceInk/Views/Recorder/NotchRecorderView.swift`
- Windows recorder surface: `VoiceInk.Windows/src/VoiceInk.Windows.App/FloatingRecorderWindow.xaml`
- Windows recorder presenter: `VoiceInk.Windows/src/VoiceInk.Windows.Core/Recorder/FloatingRecorderPresenter.cs`

## Requirements

- Render the recorder waveform with 15 slim bars, matching the macOS visualizer density.
- Keep the height range aligned with macOS intent: 4 px minimum, 28 px maximum.
- Use a deterministic Core presenter for bar heights so waveform behavior is unit-testable without WinUI automation.
- Preserve existing recorder states, controls, Prompt/Power Mode popovers, live transcript preview, and no-activate behavior.
- Keep the idle/low-input animation visible instead of static silence.
- Clamp invalid or out-of-range audio levels before calculating bars.
- Do not add dependencies or telemetry.

## Non-Goals

- Do not attempt pixel-perfect SwiftUI animation timing.
- Do not change microphone capture, live transcription, or recorder positioning.
- Do not introduce commercial or paid surfaces.
