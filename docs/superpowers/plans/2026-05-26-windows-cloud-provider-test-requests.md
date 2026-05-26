# Windows Cloud Provider Test Requests Implementation Plan

**Goal:** Add safe, metadata-only cloud transcription provider test requests for grounded providers.

**Tech Stack:** .NET 10, WinUI 3, `HttpClient`, `ISecretStore`, xUnit.

## Task 1: Add Probe Tests

Write focused tests for Deepgram, AssemblyAI, Groq/OpenAI-compatible, custom endpoint derivation, ElevenLabs, Soniox, Gemini, missing-key short circuiting, sanitized errors, and unsupported provider messaging.

Status: completed.

## Task 2: Implement Probe Service

Add `CloudTranscriptionProviderProbeService` and result type in Infrastructure. The service builds provider-specific lightweight metadata requests, reads stored keys through `ISecretStore`, and never includes body text or secrets in status messages.

Status: completed.

## Task 3: Wire AI Models UI

Add `Test Provider` to AI Models and route it through the probe service with a dedicated busy flag.

Status: completed.

## Task 4: Document And Verify

Update README and completion tracker, then run focused tests, full solution tests, Debug x64 build, and `git diff --check`.

Status: completed.
