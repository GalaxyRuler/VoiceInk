# Windows Enhancement GPT-5 Temperature

## Goal

Match the macOS enhancement request construction for OpenAI GPT-5 family models so Windows sends the same generation temperature as the original app.

## Source Of Truth

- `VoiceInk/Services/AIEnhancement/AIEnhancementService.swift` sets `temperature` to `1.0` when the selected model name starts with `gpt-5`, otherwise `0.3`.
- Windows request construction is split between the Core `TextEnhancementPipeline` and Infrastructure provider adapters.

## Online Grounding

- OpenAI's Chat Completions API documents `temperature` as a request/response parameter and notes that parameter support can differ for newer reasoning models: https://platform.openai.com/docs/api-reference/chat

## Requirements

- When the selected Windows enhancement provider is the OpenAI preset and the model starts with `gpt-5`, send `Temperature = 1.0` to the provider adapter.
- Keep non-GPT-5 OpenAI models at `0.3`.
- Keep Custom OpenAI-compatible providers generic even when their model text starts with `gpt-5`.
- Do not change reasoning parameters, prompts, provider secrets, retry behavior, output filtering, or commercial surfaces.

## Acceptance

- Core pipeline tests cover OpenAI GPT-5, OpenAI GPT-4.1, and Custom GPT-5-like model strings.
- Existing enhancement pipeline behavior remains unchanged for non-OpenAI providers.
