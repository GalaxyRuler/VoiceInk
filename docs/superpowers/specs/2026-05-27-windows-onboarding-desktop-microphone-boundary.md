# Windows Onboarding Desktop Microphone Boundary

## Goal

Make first-run onboarding explain the Windows microphone privacy boundary for source-built desktop apps.

## Source Of Truth

- Microsoft documents `ms-settings:privacy-microphone` as the Settings URI for microphone privacy.
- Microsoft support guidance notes Windows microphone privacy includes general microphone access and desktop-app access, and desktop apps are not always represented the same way as Store apps.

## Windows Behavior

- Onboarding continues to allow setup completion when no microphone is visible, matching the existing graceful-degradation behavior.
- The microphone privacy advisory now explains that source-built VoiceInk may not appear as a separate app entry.
- The setup summary and manual privacy action tell users to check both `Microphone access` and desktop-app microphone access.
- The app still does not change Windows privacy settings automatically.

## Verification

- Core onboarding presenter tests cover the desktop-app microphone privacy boundary text.
- Full solution tests and Debug x64 build remain the commit gate.
