# Windows Audio Priority Fallback Row Design

## Context

VoiceInk supports System Default, Custom, and Prioritized microphone modes. The macOS source of truth describes prioritized input as a ranked fallback list that uses the next available microphone, then falls back to the system default when none of the prioritized devices are available.

The Windows fork already performs that fallback in selection logic, but the device health list did not show the final System Default fallback row when every prioritized microphone was unavailable.

## Goal

When Audio Input is in Prioritized mode and all prioritized microphones are unavailable:

- keep showing unavailable priority rows;
- append a selected System Default Fallback row;
- mark the fallback with a warning badge so the user can understand why recordings use Windows default input.

## Non-Goals

- No changes to NAudio capture.
- No changes to device persistence or priority editing.
- No attempt to set Windows default devices.

## Testability

`AudioInputDeviceHealthPresenter` emits the fallback row, covered by focused Core tests. The existing audio input health list renders the row without new UI plumbing.
