# Windows Prompt Trigger Labels

## Purpose

Make Windows prompt selection surfaces show trigger-word hints like the macOS VoiceInk prompt cards.

## Source of Truth

- The Swift `CustomPrompt.promptIcon` view shows the first trigger word and an additional-count suffix when prompts have trigger words.
- VoiceInk public docs present enhancement trigger words as a first-class workflow for switching prompts by speech.
- The Windows prompt model already stores normalized trigger words and the enhancement pipeline already uses them for prompt detection.

## Requirements

- Add a Core prompt choice label helper that returns the prompt title when no trigger words exist.
- When one trigger word exists, append a concise `"trigger..."` hint.
- When multiple trigger words exist, append the first trigger hint plus a `+N` count.
- Use the helper in the Enhancement prompt picker.
- Use the helper in the Power Mode prompt override picker.
- Do not change trigger-word parsing, detection, persistence, prompt rendering, or selected-prompt IDs.

## Non-Goals

- Do not add prompt cards or a new prompt gallery layout in this slice.
- Do not add a prompt marketplace, account sync, telemetry, paid templates, or commercial prompt surfaces.
