# Windows Enhancement System Default Prompt Spec

## Goal

Align the Windows built-in default enhancement prompt metadata with the macOS VoiceInk prompt template.

## Source Of Truth

The macOS `PromptTemplates.swift` default template is titled `System Default` and uses the description `Default system prompt`. Windows kept the stable predefined prompt ID and prompt text, but showed the shorter title `Default` with a Windows-specific description.

## Windows Behavior

The predefined default enhancement prompt must:

- Keep the stable `EnhancementPromptCatalog.DefaultPromptId`.
- Show title `System Default`.
- Show description `Default system prompt`.
- Keep prompt text, icon, trigger-word override persistence, renderer behavior, and selection by ID unchanged.

## Open-Source Boundary

This is local prompt metadata only. It adds no paid prompt, commercial provider, account flow, telemetry, hosted prompt sync, or upgrade path.

## Verification

- Update focused Enhancement prompt tests so stale `Default` metadata fails.
- Run focused Enhancement prompt tests.
- Run full Windows tests/build and `git diff --check` before committing.
