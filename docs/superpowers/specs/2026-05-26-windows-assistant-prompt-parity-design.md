# Windows Assistant Prompt Parity Design

## Goal

Make Windows AI Enhancement prompt rendering closer to the macOS VoiceInk assistant workflow while preserving the open-source, user-owned-provider model.

## Grounding

- The macOS source defines a predefined `Assistant` prompt with stable ID `00000000-0000-0000-0000-000000000002`, raw assistant instructions, and no transcription-enhancer wrapping.
- The macOS source uses a stricter default transcription-enhancer prompt and richer Chat/Email/Rewrite prompt templates than the current Windows catalog text.
- VoiceInk's public feature copy describes the AI Assistant as a workflow for asking questions, summarizing text, or giving commands with the Assistant prompt.

## Behavior

- Keep `Assistant` as a predefined prompt that does not use the transcription-enhancer system wrapper.
- Render assistant context inside a `<CONTEXT_INFORMATION>` envelope so the assistant instruction "Use the information within the <CONTEXT_INFORMATION> section" has a concrete matching section.
- Keep transcript text in the user message inside `<TRANSCRIPT>` tags.
- Preserve the existing context subsections for active window, browser URL, OCR, selected text, clipboard, and custom vocabulary inside assistant context.
- Align Windows predefined Default, Chat, Email, and Rewrite prompt text more closely with the macOS Swift source.
- Do not add bundled provider credentials, paid services, account flows, telemetry, or commercial gates.

## Verification

- Add focused Core tests for assistant context wrapping and prompt template fidelity.
- Run focused Core enhancement tests, full solution tests, Debug x64 build, and `git diff --check`.
