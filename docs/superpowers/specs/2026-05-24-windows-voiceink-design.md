# Windows VoiceInk MVP Design

## Status

Approved for planning.

## Goal

Build a Windows-only VoiceInk-inspired dictation app using .NET, WinUI 3, and whisper.cpp. The first milestone must deliver the core daily workflow: press a global hotkey, speak, release, transcribe locally, and insert the text into the currently focused Windows application. The v1 scope also includes cloud transcription providers, AI text enhancement, and installer packaging.

## Context

The upstream VoiceInk repository is a native macOS app. It uses SwiftUI, AppKit, CoreAudio, AVFoundation, SwiftData, Sparkle, AppIntents, AppleScript, Accessibility APIs, ScreenCaptureKit, Vision, Keychain, LaunchAtLogin, and an Apple-oriented whisper.xcframework build. Those pieces cannot be compiled directly into a Windows app.

The Windows app should therefore be a new native Windows implementation inside the fork. The macOS source remains a product and behavior reference, especially for the recording workflow, transcription pipeline, text post-processing, dictionary concepts, model management, and history concepts.

## Assumptions

- The first target is Windows 11, with Windows 10 considered only if the chosen Windows App SDK and runtime combination continues to support it.
- The app is Windows-only. Cross-platform reuse is not a first milestone.
- Offline transcription is a core requirement.
- The app does not need commercial licensing, Polar activation, Sparkle updates, CloudKit sync, or macOS-specific context capture in v1.
- The first implementation can support one local whisper.cpp model path before adding model download and management.
- Distribution packaging is part of v1, but should follow after the source-built app can dictate successfully.
- Cloud transcription and AI enhancement should be implemented through provider interfaces so local-only use still works without API keys or network access.

## Recommended Stack

- UI: WinUI 3 desktop app through the Windows App SDK stable channel.
- Runtime: .NET 10 LTS for long-lived client support.
- Native transcription: whisper.cpp built for Windows and called from .NET through a narrow native interop boundary.
- Cloud transcription: provider adapters over `HttpClient`, starting with an OpenAI-compatible adapter and adding named providers behind the same interface.
- AI enhancement: provider adapters over `HttpClient`, starting with an OpenAI-compatible chat/completions-style adapter and prompt templates.
- Audio capture: Windows WASAPI through a replaceable `IAudioCaptureService` implementation.
- Hotkeys: Win32 `RegisterHotKey` for MVP toggle behavior, with a later low-level keyboard hook if true press-and-hold behavior requires key-up detection.
- Text injection: Win32 `SendInput` with a clipboard paste fallback that restores the previous clipboard content when possible.
- Storage: SQLite for transcription history and JSON for lightweight settings.
- Secrets: Windows DPAPI or Windows Credential Manager if API keys are added later.
- Packaging: MSIX for first-class Windows install/uninstall behavior, plus an unpackaged zip/dev build while the app is still moving quickly.

## Scope

### In Scope For MVP

- Create a new Windows solution under `VoiceInk.Windows/`.
- Add a WinUI 3 desktop shell with a small main window and tray-first behavior.
- Add configurable basic settings: hotkey, model path, language, append trailing space, restore clipboard.
- Capture microphone audio into 16 kHz mono PCM suitable for whisper.cpp.
- Transcribe recorded audio with whisper.cpp locally.
- Insert transcribed text into the active application.
- Save transcription history locally in SQLite.
- Add post-processing for trimming whitespace and optional trailing space.
- Add focused tests for configuration, post-processing, history persistence, and pipeline orchestration.

### In Scope For V1 After Core Dictation Works

- Add cloud transcription provider infrastructure.
- Add at least an OpenAI-compatible custom transcription provider.
- Add named cloud transcription adapters for Groq and Deepgram in v1, because they give two concrete non-custom provider paths while the provider abstraction is still young.
- Keep provider metadata extensible for later adapters matching the macOS app's broader provider list: ElevenLabs, Mistral, Gemini, Soniox, Speechmatics, AssemblyAI, xAI, and Cartesia.
- Add API key storage using Windows DPAPI or Windows Credential Manager.
- Add AI text enhancement with configurable prompts.
- Add at least one OpenAI-compatible AI enhancement provider.
- Add settings for choosing local transcription, cloud transcription, or local transcription followed by AI enhancement.
- Add installer packaging with MSIX.
- Add a zip/dev distribution path for testing without installation.

### Out Of Scope For MVP

- macOS feature parity.
- Screen context capture and OCR.
- Browser URL detection.
- Per-application Power Mode.
- Native Apple transcription.
- FluidAudio or Parakeet models.
- License checks or trial behavior.
- Auto-update infrastructure.

## Architecture

The Windows implementation should be split into small projects with explicit boundaries:

```text
VoiceInk.Windows/
  VoiceInk.Windows.sln
  src/
    VoiceInk.Windows.App/
    VoiceInk.Windows.Core/
    VoiceInk.Windows.Infrastructure/
    VoiceInk.Windows.Native/
  tests/
    VoiceInk.Windows.Core.Tests/
    VoiceInk.Windows.Infrastructure.Tests/
```

`VoiceInk.Windows.App` owns WinUI 3 views, tray integration, app lifecycle, and user-facing state.

`VoiceInk.Windows.Core` owns the dictation pipeline, settings contracts, transcription interfaces, audio capture interfaces, text injection interfaces, post-processing, and app use cases. It should not reference WinUI, Win32, whisper.cpp, or SQLite directly.

`VoiceInk.Windows.Infrastructure` implements storage, JSON settings, SQLite history, cloud provider HTTP adapters, AI enhancement provider HTTP adapters, secret storage, and Windows platform services that can be unit or integration tested without the WinUI app.

`VoiceInk.Windows.Native` owns the low-level Windows and whisper.cpp integration. This project should expose narrow C# interfaces to the rest of the app so native details remain isolated.

`VoiceInk.Windows.Packaging` may be added when the app reaches distribution work. It owns MSIX packaging assets, manifest configuration, app identity, install capabilities, and publish scripts.

## Core Flow

```text
Hotkey pressed
  -> DictationController starts capture
  -> AudioCaptureService records PCM samples
Hotkey released or toggled off
  -> DictationController stops capture
  -> selected ITranscriptionService transcribes locally or through a cloud provider
  -> TextPostProcessor cleans text
  -> optional ITextEnhancementService rewrites or formats text
  -> TextInjectionService inserts text into the focused app
  -> HistoryStore saves the result
  -> UI/tray state returns to idle
```

The MVP may implement hotkey as a toggle if `RegisterHotKey` is used first. Press-and-hold can be added by replacing only the hotkey service with a low-level keyboard hook implementation.

## Interfaces

The core project should define these interfaces early:

```csharp
public interface IAudioCaptureService
{
    Task StartAsync(CancellationToken cancellationToken);
    Task<AudioCaptureResult> StopAsync(CancellationToken cancellationToken);
}

public interface ITranscriptionService
{
    Task<TranscriptionResult> TranscribeAsync(AudioCaptureResult audio, TranscriptionOptions options, CancellationToken cancellationToken);
}

public interface ITextInjectionService
{
    Task<TextInjectionResult> InsertAsync(string text, TextInjectionOptions options, CancellationToken cancellationToken);
}

public interface ITextEnhancementService
{
    Task<TextEnhancementResult> EnhanceAsync(string text, EnhancementOptions options, CancellationToken cancellationToken);
}

public interface ISecretStore
{
    Task SaveSecretAsync(string key, string value, CancellationToken cancellationToken);
    Task<string?> GetSecretAsync(string key, CancellationToken cancellationToken);
    Task DeleteSecretAsync(string key, CancellationToken cancellationToken);
}

public interface IHistoryStore
{
    Task SaveAsync(TranscriptionHistoryItem item, CancellationToken cancellationToken);
    Task<IReadOnlyList<TranscriptionHistoryItem>> ListRecentAsync(int limit, CancellationToken cancellationToken);
}

public interface ISettingsStore
{
    Task<AppSettings> LoadAsync(CancellationToken cancellationToken);
    Task SaveAsync(AppSettings settings, CancellationToken cancellationToken);
}
```

These interfaces keep the dictation pipeline testable without a microphone, a focused window, a loaded whisper model, configured cloud credentials, or live network calls.

## Cloud Transcription

Cloud transcription should be implemented as provider adapters behind `ITranscriptionService`. The OpenAI-compatible provider is the first adapter because it can support custom endpoints and reduces duplicate provider code. Named providers can then be thin configuration wrappers or specialized adapters when APIs differ.

Provider settings should include display name, endpoint, model name, supported languages, streaming capability where applicable, and the secret key identifier used by `ISecretStore`. API keys must never be stored in JSON settings or written to logs.

The app should keep local whisper.cpp as the default provider. If a cloud provider is selected and unavailable, the app should show a clear provider error and avoid silently sending audio to another provider.

## AI Enhancement

AI enhancement runs after transcription and text post-processing but before insertion into the active app. It should be optional, provider-based, and prompt-driven.

The first enhancement provider should support OpenAI-compatible chat/completions-style APIs. The first prompt set should cover:

- Clean up grammar while preserving meaning.
- Make text concise.
- Format as email.
- Format as bullet points.
- Custom user prompt.

Enhancement requests should include only the transcribed text and selected prompt by default. Screen context, selected text context, browser URL context, and app-specific Power Mode context remain out of scope until the Windows baseline is stable.

If enhancement fails, the app should allow the user to insert the original transcription instead of losing the dictated text.

## Packaging

Installer packaging is part of v1, after the app can dictate successfully from source. MSIX is the preferred installer format because it gives clean install/uninstall behavior and a familiar Windows app identity. A zip/dev build should remain available for fast testing, debugging, and environments where MSIX signing is not ready.

The packaging work should include:

- App icon and display name.
- MSIX manifest.
- Runtime dependency strategy.
- Native whisper.cpp binary placement.
- First-run model path handling.
- Installer smoke test on a clean Windows user profile.

## Error Handling

- If no microphone is available, the app should show a clear recording error and stay idle.
- If no model path is configured, the app should open settings instead of trying to transcribe.
- If whisper.cpp fails to load or transcribe, the app should keep the audio result in memory only for the active operation, show an error, and avoid inserting failure text into the target app.
- If a cloud provider is selected without a configured API key, the app should open provider settings instead of sending a request.
- If cloud transcription fails, the app should show the provider error without logging API keys or full request payloads.
- If AI enhancement fails, the app should offer to insert the unenhanced transcription.
- If direct text injection fails, the app should try clipboard paste fallback.
- If clipboard restoration fails, the app should show a non-blocking warning.
- If history save fails, dictation should still complete and the UI should show a non-blocking warning.

## Testing Strategy

- Unit test `TextPostProcessor` with whitespace, empty text, and trailing-space settings.
- Unit test `DictationController` with fake audio, transcription, text injection, settings, and history services.
- Unit test provider selection between local transcription, cloud transcription, and enhancement-enabled flows.
- Unit test AI enhancement fallback when the provider fails.
- Unit test JSON settings load/save with default creation and invalid JSON recovery.
- Integration test SQLite history create/save/list against a temporary database.
- Integration test secret storage using a test key namespace.
- Contract test cloud provider request construction with a fake HTTP handler that asserts URL, headers, body shape, and secret redaction.
- Packaging smoke test MSIX install, launch, uninstall, and native binary presence once packaging is added.
- Smoke test the WinUI app manually after the MVP builds: start app, configure model path, trigger hotkey, speak, verify text appears in Notepad.

## Risks

- WinUI 3 tray behavior is not first-class in the same way as classic Win32 tray apps. The app may need a small Win32 interop helper for tray behavior.
- whisper.cpp interop can become unstable if native lifetime management leaks across the app. The interop boundary must stay narrow.
- Reliable text insertion across elevated apps, remote desktop sessions, terminals, and games may vary. MVP should support normal desktop apps first.
- `RegisterHotKey` is simpler but not ideal for hold-to-talk. True hold-to-talk may require a low-level keyboard hook with careful cleanup.
- GPU acceleration support can widen scope quickly. CPU transcription should work first; GPU can follow after the pipeline is stable.
- Cloud providers add privacy and cost concerns. The UI must make provider selection explicit and keep local transcription as the default.
- Cloud provider APIs differ in authentication, upload format, streaming semantics, and error shape. The OpenAI-compatible adapter should land first, with named providers added incrementally.
- AI enhancement can alter meaning. The app should keep enhancement optional and preserve original transcription in history.
- MSIX packaging introduces signing and app identity decisions. Development should keep an unpackaged path until release packaging stabilizes.

## Verification Criteria

The MVP is successful when a developer can build the Windows solution, run the app on Windows, configure a whisper.cpp model path, use a global hotkey to dictate into Notepad, and see a local history entry created without any network call.

The v1 scope is successful when the same app can also use a configured cloud transcription provider, optionally enhance text through a configured AI provider, and install/uninstall through MSIX while preserving an unpackaged development path.

## References

- Upstream VoiceInk repository inspected at commit `0df2a9a` from `main`.
- Microsoft .NET support documentation lists .NET 10 as LTS supported until November 2028.
- Microsoft Windows App SDK downloads list the stable Windows App SDK 1.8 channel as current in 2026.
