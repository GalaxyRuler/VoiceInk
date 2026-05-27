# Windows Gemini Advanced Transcription Options

## Goal

Bring Gemini transcription closer to the provider-specific cloud parity target by supporting safe endpoint-query tuning options without allowing API keys or routing fields in the endpoint text.

## Behavior

- Gemini endpoints may include safe generation tuning query parameters such as `temperature`, `topP`, `topK`, and `maxOutputTokens`.
- Safe generation query parameters are mapped into the JSON `generationConfig` request object.
- Unsupported or routing-oriented query parameters such as `model` are ignored.
- Query parameters are stripped from the generated `:generateContent` URI before sending the request.
- Secret-like query parameters such as `token` or `api_key` are rejected before API-key lookup and before HTTP.
- Inline audio and Files API upload behavior remain unchanged.
- Existing Gemini prompt, language guidance, API-key header, sanitized errors, and provider metadata remain unchanged.

## Open-Source Boundary

The feature uses user-owned Gemini API keys only. It adds no bundled credentials, telemetry, purchase flow, or commercial gate.
