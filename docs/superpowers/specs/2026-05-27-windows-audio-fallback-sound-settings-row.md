# Windows Audio Fallback Sound Settings Row Design

## Context

VoiceInk for Windows supports System Default, Custom Device, and Prioritized microphone modes. When every prioritized microphone is unavailable, the app already shows that Windows system default will be used. The health list did not also point users to the Windows Sound settings page where they can choose or test the default input.

Microsoft documents `ms-settings:sound` as the Windows Settings route for Sound. The Windows fork should surface that route in the fallback state so users have an obvious recovery path without mutating system settings.

## Goal

When prioritized audio input falls back to the Windows system default because every prioritized device is unavailable, add a device health row that:

- is titled `Windows Sound Settings`;
- explains `ms-settings:sound` as the route to choose or test the default input device;
- uses an informational `Open Settings` badge;
- appears only in the all-prioritized-devices-unavailable fallback state.

## Non-Goals

- No automatic Windows Settings launch from the presenter.
- No audio device mutation.
- No registry, policy, or default-device changes.
- No changes to normal System Default or Custom Device health rows.

## Testability

Focused audio presenter tests cover the fallback health list and verify the Windows Sound settings row.
