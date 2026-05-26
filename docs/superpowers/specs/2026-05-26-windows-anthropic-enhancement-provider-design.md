# Windows Anthropic Enhancement Provider Design

## Goal

Add Anthropic as a first-class AI Enhancement provider, matching the macOS provider list without using an OpenAI-compatible request shape.

## Grounding

- The macOS app exposes `Anthropic` at `https://api.anthropic.com/v1/messages`, defaulting to `claude-sonnet-4-6`, with Claude Opus/Sonnet/Haiku model choices.
- Anthropic's Messages API uses `POST /v1/messages`, `x-api-key`, `anthropic-version`, top-level `system`, and user messages.
- Windows already stores provider-specific API keys in Windows Credential Manager and has provider-presets for OpenAI-compatible enhancement services.

## Behavior

- Add an Anthropic provider preset to Enhancement using the macOS endpoint, default model, and static model list.
- Store Anthropic API keys under a provider-specific Credential Manager secret.
- Route Anthropic enhancement requests through Anthropic Messages JSON rather than OpenAI chat-completions JSON.
- Send the prompt renderer's system message as top-level `system` and user message as a single user text message.
- Parse returned text from Anthropic `content` blocks.
- Reuse existing timeout, retry, provider metadata, and sanitized HTTP error behavior.
- Keep this fully user-owned-key and open-source: no bundled key, sign-up gate, telemetry, trial, upgrade surface, or commercial lock.

## Verification

- Add Core tests for the Anthropic preset, secret name, and stable provider metadata.
- Add Infrastructure tests for Anthropic request headers/body, response parsing, missing-key behavior, and sanitized HTTP failure.
- Run focused tests, full solution tests, Debug x64 build, and `git diff --check`.
