# Windows Onboarding Text Insertion Readiness Spec

## Goal

Adapt the macOS onboarding "Accessibility Access" permission step into a Windows-native first-run disclosure for text insertion readiness.

## Source Of Truth

- The macOS app asks for Accessibility Access during onboarding because it needs permission for system-wide control and insertion workflows.
- The Windows MVP inserts by placing text on the clipboard and pasting into the focused field, with History as the recovery path.
- Microsoft Windows app accessibility guidance expects primary flows to expose clear accessible names and descriptions for controls and guidance rows.

## Requirements

- Onboarding must include a visible "Text insertion" setup action.
- The action must explain that VoiceInk inserts through the focused field by using clipboard paste.
- The action must explain that History can recover the transcript if the target app blocks paste or loses focus.
- The summary rows must include the same Windows insertion readiness concept so screen-reader and scan users see it before saving setup.
- The setup checklist must include a Windows insertion advisory that replaces the macOS Accessibility Access permission expectation with an accurate Windows behavior.
- Tutorial steps must keep the final insertion and History verification path visible.
- This slice must not add a fake Windows permission gate, global registry dependency, or commercial/account flow.

## Non-Goals

- No GUI automation or installed-app smoke testing on the active desktop.
- No change to the underlying clipboard insertion service.
- No new OS-level permission prompt.
