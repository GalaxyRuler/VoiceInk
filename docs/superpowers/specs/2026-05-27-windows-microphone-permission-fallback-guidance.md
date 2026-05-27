# Windows Microphone Permission Fallback Guidance

## Goal

Give users a plain manual path to Windows microphone permissions in addition to the direct `ms-settings:privacy-microphone` action.

## Source Of Truth

The macOS app guides users through required permissions during onboarding and from the Permissions page. On Windows, VoiceInk launches the microphone privacy settings URI, but the UI should also show the manual Settings path so users are not blocked if URI launch fails, Settings behaves differently, or the machine is managed.

## Requirements

- Permission readiness items must support optional fallback guidance.
- The Microphone Access readiness card must show `Manual path: Settings > Privacy & security > Microphone.`
- The direct action target must remain `ms-settings:privacy-microphone`.
- Existing permission cards must keep working without fallback text.
- The WinUI Permissions checklist must render the fallback guidance from the presenter model.

## Non-Goals

- This slice does not mutate Windows privacy settings.
- This slice does not try to bypass enterprise policy or OS-level microphone permissions.
