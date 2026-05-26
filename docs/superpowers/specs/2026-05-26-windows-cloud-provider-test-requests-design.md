# Windows Cloud Provider Test Requests Design

## Goal

Add safe cloud transcription provider test requests so users can verify stored provider API keys before recording, without uploading microphone audio or exposing secrets in logs/status text.

## Grounding

Provider documentation supports lightweight authenticated metadata probes for several providers:

- OpenAI-compatible APIs expose `GET /v1/models` with bearer authentication.
- Deepgram documents `GET /v1/auth/token` with `Authorization: Token <key>` as an API-key validation request.
- AssemblyAI documents authenticated REST requests with the API key in the `Authorization` header.

This slice implements only those grounded probe shapes and returns a clear "not available yet" message for providers where a safe metadata probe has not been implemented.

## Requirements

- Add a testable infrastructure service for provider probes.
- Use stored API keys from Windows Credential Manager through `ISecretStore`; do not read or print secrets.
- Do not upload audio or transcript text.
- Support initial probes for:
  - Custom OpenAI-compatible endpoint;
  - Groq;
  - Mistral;
  - xAI;
  - Deepgram;
  - AssemblyAI.
- Return sanitized success/failure messages that do not include response bodies or API keys.
- Add a `Test Provider` control in AI Models near cloud transcription settings.

## Non-Goals

- No probe audio uploads.
- No account creation, paid setup, trial flow, or telemetry.
- No undocumented provider probes.
- No provider body logging.

## Verification

- Focused tests cover request URLs, authentication headers, missing-key short circuiting, unsupported-provider messaging, and sanitized HTTP errors.
- Debug x64 build validates UI wiring.
