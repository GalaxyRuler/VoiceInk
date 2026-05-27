# Windows Audio System Default Guidance Spec

## Goal

Make the normal `System Default` audio input path as explainable as the prioritized fallback path.

## Source Of Truth

- Microsoft Core Audio documentation describes default capture endpoints as Windows-owned choices, with role-specific defaults for console, communications, and multimedia capture.
- Microsoft microphone privacy guidance requires desktop apps to be allowed microphone access before recording can work reliably.
- The Windows app already exposes fallback guidance when prioritized microphones are unavailable, but the intentional `System Default` selection only shows `Follows the Windows default microphone`.

## Requirements

- When `System Default` is the selected audio input outside prioritized fallback, the Device Health list must include a Windows Sound Settings guidance row.
- The Sound Settings row must point users to `ms-settings:sound` to choose or test the Windows default input device.
- The same state must include a Microphone Privacy guidance row for `ms-settings:privacy-microphone` and the desktop-app microphone toggle.
- Custom selected microphones must keep the existing concise device rows without adding System Default guidance noise.
- Prioritized fallback rows must keep their existing warning and guidance rows.

## Non-Goals

- Do not change capture behavior.
- Do not add a new audio backend.
- Do not expose a role picker until the capture layer explicitly supports role-targeted recording.
