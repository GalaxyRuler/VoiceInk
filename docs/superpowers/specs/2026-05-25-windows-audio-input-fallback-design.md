# Windows Audio Input Fallback Design

## Purpose

VoiceInk for Windows must respect the user's saved microphone choice when it is still available, recover gracefully when Windows renumbers devices, and fall back to System Default when the saved device is unavailable.

## Source of Truth

The macOS app treats audio input selection as a persistent user preference and gives the user a clear recording workflow. On Windows, NAudio exposes capture devices through transient device numbers, so the Windows implementation must avoid assuming that a saved device number remains valid across restarts or dock changes.

## Current Behavior

- The Audio Input settings view already builds a choice list with a System Default option.
- The selection logic can rebind a saved device by exact name when the device number changes.
- The selection logic warns when a saved custom device is unavailable.
- Startup creates the capture service before settings are applied.
- After applying settings to the UI, startup does not explicitly recreate the capture service with the resolved selected device.

## Desired Behavior

- Startup should apply the same resolved audio input choice used by the UI to the active capture service.
- If a saved custom microphone is still available, recording should use that device immediately after app launch.
- If Windows renumbered the saved microphone and there is exactly one name match, recording should use the rebound device number immediately after app launch.
- If the saved device is unavailable or ambiguous, recording should use System Default and surface the existing warning.
- This slice does not add live Core Audio hot-plug notifications; manual refresh remains the explicit update path.

## Non-Goals

- No new dependencies.
- No direct UI automation.
- No global machine audio configuration changes.
- No commercial or telemetry behavior.

## Verification

- Add focused core tests for the resolved selected audio input choice.
- Run the focused audio input selection tests.
- Run the full Windows solution test suite.
- Run the Debug x64 build.
