# Windows Ollama Native Enhancement Design

## Goal

Use Ollama's native local API for AI text enhancement while preserving the existing keyless local-provider behavior and model refresh.

## Grounding

- Ollama's official API documents `POST /api/chat` for chat messages and supports `stream: false`.
- VoiceInk enhancement already renders a system message and user message, which maps directly to Ollama chat messages.
- Windows already refreshes installed Ollama model names from `/api/tags`.

## Behavior

- Change the Ollama enhancement preset endpoint to `http://localhost:11434/api/chat`.
- Send native Ollama chat payloads with `model`, `messages`, `stream: false`, and `options.temperature`.
- Parse native Ollama responses from `message.content`.
- Keep Ollama keyless and localhost HTTP-compatible.
- Preserve retry, timeout, output filtering, and model refresh behavior.

## Verification

- Add/adjust Infrastructure tests to assert native Ollama request construction and response parsing.
- Run focused enhancement tests, full solution tests, Debug x64 build, and `git diff --check`.
