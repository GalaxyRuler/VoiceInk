# Windows OpenRouter Enhancement Model Refresh Design

## Goal

Let users refresh OpenRouter enhancement models from OpenRouter's public model catalog instead of typing model IDs manually.

## Grounding

- OpenRouter documents `GET https://openrouter.ai/api/v1/models` as the model catalog endpoint.
- The Windows app already treats OpenRouter as an OpenAI-compatible enhancement provider at `https://openrouter.ai/api/v1/chat/completions`.
- The existing Enhancement page already has a preset model combo and now has a provider-specific refresh pattern from Ollama.

## Behavior

- Add a `Refresh Models` action in Enhancement for providers that support dynamic model loading.
- Enable the action for OpenRouter and Ollama only while no other operation is active.
- For OpenRouter, derive the API origin from the configured endpoint and call `/api/v1/models`.
- Populate the model combo with returned model IDs.
- If the current model is blank or absent from the refreshed list, select a sensible preferred model when available, otherwise select the first returned model and copy it to the model text box.
- If OpenRouter is unavailable, returns invalid JSON, or returns no models, report a nonfatal status without changing saved settings.
- Do not require an API key for model catalog refresh; model usage still requires the existing user-owned key.

## Verification

- Add Infrastructure tests for `/api/v1/models` parsing, endpoint-origin derivation, preferred model selection input ordering, empty response handling, and sanitized HTTP failure.
- Run focused tests, full solution tests, Debug x64 build, and `git diff --check`.
