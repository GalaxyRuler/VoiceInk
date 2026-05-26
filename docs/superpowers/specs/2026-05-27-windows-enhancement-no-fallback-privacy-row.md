# Windows Enhancement No-Fallback Privacy Row Design

## Context

VoiceInk for Windows supports cloud and local AI enhancement providers, including OpenAI-compatible endpoints, Ollama, and Local CLI. The pipeline already returns original text with a warning when the selected provider fails; it does not silently route prompts to another provider. The Enhancement context privacy surface did not make that provider-fallback boundary visible.

Local provider guidance matters because local-first users should know that a local provider timeout or failure will not automatically leak prompts to a cloud provider.

## Goal

Add an enhancement privacy row when enhancement is enabled:

- title `Provider Fallback`;
- value `None automatic`;
- detail explains that VoiceInk returns original text if the selected enhancement provider fails instead of silently routing prompts to another provider;
- status badge `Explicit`.

## Non-Goals

- No behavior change to fallback handling.
- No new provider fallback chain.
- No retry or timeout changes.
- No telemetry, account, or commercial gating.

## Testability

Focused enhancement context readiness tests cover the no-automatic-fallback privacy row.
