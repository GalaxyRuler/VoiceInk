# Windows Enhancement Reasoning Parameters

## Goal

Match the macOS enhancement request construction for provider/model-specific reasoning controls so fast dictation cleanup does not expose or spend extra reasoning tokens where the provider supports a lower-effort or hidden-reasoning mode.

## Source Of Truth

- `VoiceInk/Services/AIEnhancement/ReasoningConfig.swift` maps Gemini, OpenAI, Cerebras, and Groq models to reasoning parameters.
- `VoiceInk/Services/AIEnhancement/AIEnhancementService.swift` uses those parameters while building provider requests.

## Online Grounding

- OpenAI's Chat Completions API documents `reasoning_effort` for reasoning models: https://platform.openai.com/docs/api-reference/chat/create
- Groq's reasoning docs document `include_reasoning` for GPT-OSS reasoning output control: https://console.groq.com/docs/reasoning

## Requirements

- Add request-construction coverage for at least OpenAI GPT-5.x, Gemini Flash, Cerebras GPT-OSS, and Groq GPT-OSS/Qwen model mappings.
- Add `reasoning_effort` only for the provider/model pairs where macOS sends it.
- Add `reasoning_format: "hidden"` for Cerebras `gpt-oss-120b`.
- Add `include_reasoning: false` for Groq `openai/gpt-oss-120b` and `openai/gpt-oss-20b`.
- Keep custom/OpenAI-compatible providers generic unless they use a known provider preset.
- Do not change API keys, provider selection, retries, prompts, output filtering, or commercial flows.

## Non-Goals

- No new UI controls for manual reasoning effort.
- No Responses API migration.
- No provider account or signup flow.
