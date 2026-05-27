# Windows Onboarding Hero Promise

## Goal

Bring the Windows first-run setup closer to the macOS onboarding by surfacing the same product promise before the checklist: VoiceInk is a new way to type, a writing assistant, and a private/offline-first dictation tool.

## Source Of Truth

- `VoiceInk/Views/Onboarding/OnboardingView.swift` presents "Welcome to the Future of Typing", "A New Way to Type", and rotating role lines such as "Your Writing Assistant" and "100% offline & private".
- `VoiceInk/Views/Onboarding/OnboardingTutorialView.swift` then guides the user through clicking a text field, pressing the shortcut, speaking, and stopping.

## Windows Design

- Keep the Windows setup dialog practical and native, using existing WinUI controls and the current presenter-driven layout.
- Add presenter-backed hero tagline rows so the first-run dialog communicates the same macOS promise without adding animated or decorative complexity.
- Adapt platform-specific wording from "Mac with a click" to "Windows with your shortcut".
- Keep local/offline privacy wording free/open-source and non-commercial.

## Online Grounding

- Microsoft WinUI guidance describes TeachingTip as a way to teach users how a task should be completed, which supports keeping onboarding guidance short, contextual, and task-focused: https://learn.microsoft.com/en-us/windows/apps/design/controls/dialogs-and-flyouts/teaching-tip

## Requirements

- `OnboardingChecklistPresenter` exposes testable hero tagline strings.
- The first-run dialog renders those taglines above the setup checklist.
- Existing checklist, summary, tutorial, and completion behavior remain unchanged.

## Non-Goals

- No particle animation or custom hero background.
- No commercial onboarding, account prompts, trial messaging, or upgrade calls.
- No changes to onboarding completion persistence.
