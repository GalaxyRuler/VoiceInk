# Windows Local CLI Enhancement Provider Design

## Goal

Add a local-only AI Enhancement provider that can call a user-configured CLI command and use stdout as the enhanced text.

## Grounding

- The macOS app has a `Local CLI` provider with command templates for tools such as Claude, Codex, Copilot, and Pi.
- The macOS command receives `VOICEINK_SYSTEM_PROMPT`, `VOICEINK_USER_PROMPT`, and `VOICEINK_FULL_PROMPT` environment variables.
- .NET process execution supports redirected stdout/stderr when `UseShellExecute` is false.

## Behavior

- Add a `Local CLI` Enhancement provider preset that does not require an API key.
- Treat the Enhancement endpoint field as the local command template for this provider.
- Keep the model field as local metadata, defaulting to `local-cli`.
- Run the command through `cmd.exe /S /C` with redirected stdin/stdout/stderr and no visible window.
- Set `VOICEINK_SYSTEM_PROMPT`, `VOICEINK_USER_PROMPT`, and `VOICEINK_FULL_PROMPT` environment variables.
- Also write the full prompt to stdin for CLIs that prefer piped input.
- Return trimmed stdout as the enhanced text.
- Fail with sanitized, local-only messages for missing command, timeout, empty output, command-not-found, and nonzero exit.

## Verification

- Add Core tests for the Local CLI preset and provider-aware settings validation.
- Add Infrastructure tests with a fake process runner for command request construction, output capture, timeout/nonzero/empty failures, and no API-key read.
- Run focused tests, full solution tests, Debug x64 build, and `git diff --check`.
