# Windows Transcribe Audio Per File Enhancement Design

## Goal

Let users enhance a selected completed Transcribe Audio queue item even when the global Enhancement toggle is off, matching the original app's per-result action model while preserving the user's global default.

## Grounding

- The macOS Transcribe Audio row exposes actions on completed items and can present original/enhanced text independently of the global capture flow.
- Microsoft WinUI docs support simple button/toggle controls in detail surfaces; the Windows queue currently uses selected-row details rather than a custom row template.
- The Windows app already has a tested `TextEnhancementPipeline`, prompt library, secure provider keys, and History re-enhancement path.

## Behavior

- Add an `Enhance` action near the selected Transcribe Audio transcript detail.
- Enable the action only for selected completed queue items with text.
- Run enhancement with the current provider, model, prompt, timeout, context, and dictionary vocabulary.
- Force enhancement for this explicit per-file action without changing `AppSettings.IsEnhancementEnabled`.
- Save the enhanced result as a new History row, matching the existing re-enhance behavior.
- Update the selected queue item to the newly enhanced History row so copy/save uses the enhanced text.
- If provider settings or API keys are missing, report the existing enhancement warning/status without changing the queue.

## Out Of Scope

- Inline original/enhanced tab UI for the queue detail pane.
- Batch enhance selected queue items.
- Editing prompts from inside the Transcribe Audio page.

## Verification

- Add Core tests proving forced re-enhancement runs even when global enhancement is disabled and non-forced re-enhancement still respects the global toggle.
- Build the WinUI app after wiring the selected-row action.
- Run full solution tests, Debug x64 build, and `git diff --check`.
