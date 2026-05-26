# Windows Ollama Enhancement Model Refresh Design

## Goal

Let users refresh installed Ollama enhancement models from the local Ollama server instead of typing model IDs manually.

## Grounding

- Ollama documents `GET /api/tags` as the local model list endpoint.
- The Windows app already treats Ollama as a local OpenAI-compatible enhancement provider at `http://localhost:11434/v1/chat/completions`.
- .NET `HttpClient` and `System.Text.Json` are already used for provider integrations and JSON parsing.

## Behavior

- Add a `Refresh Ollama Models` action in Enhancement.
- Enable the action only when the selected enhancement provider is Ollama and no other operation is active.
- Derive the Ollama API origin from the configured endpoint, then call `/api/tags`.
- Populate the model combo with returned local model names.
- If the current model is blank or absent from the refreshed list, select the first returned model and copy it to the model text box.
- If Ollama is not running, returns invalid JSON, or returns no models, report a nonfatal status without changing saved settings.

## Verification

- Add Infrastructure tests for `/api/tags` parsing, endpoint-origin derivation, empty response handling, and sanitized HTTP failure.
- Run focused tests, full solution tests, Debug x64 build, and `git diff --check`.
