# Windows Audio Priority Accessibility

## Goal

Close the remaining Audio Input list accessibility gap by giving prioritized microphone rows stable UI Automation names.

## Source Of Truth

- VoiceInk Audio Input docs describe Prioritized mode as a ranked fallback list.
- The Windows UI already binds priority-list item names from `PrioritizedAudioInputDevice.AccessibleName`.
- Microsoft accessibility guidance expects dynamic list items to expose meaningful names after data binding.

## Requirements

- `PrioritizedAudioInputDevice` exposes an `AccessibleName`.
- The accessible name includes the one-based priority label, device name, and shortened endpoint ID when present.
- The property must stay UI-independent and deterministic.
- Existing priority list normalization, add/remove, and movement behavior must remain unchanged.
