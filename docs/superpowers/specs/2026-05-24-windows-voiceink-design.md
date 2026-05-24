# Windows VoiceInk MVP Design

## Status

Approved for planning.

## Goal

Build a Windows-only VoiceInk-inspired dictation app using .NET, WinUI 3, and whisper.cpp. The first milestone must deliver the core daily workflow: press a global hotkey, speak, release, transcribe locally, and insert the text into the currently focused Windows application.

## Context

The upstream VoiceInk repository is a native macOS app. It uses SwiftUI, AppKit, CoreAudio, AVFoundation, SwiftData, Sparkle, AppIntents, AppleScript, Accessibility APIs, ScreenCaptureKit, Vision, Keychain, LaunchAtLogin, and an Apple-oriented whisper.xcframework build. Those pieces cannot be compiled directly into a Windows app.

The Windows app should therefore be a new native Windows implementation inside the fork. The macOS source remains a product and behavior reference, especially for the recording workflow, transcription pipeline, text post-processing, dictionary concepts, model management, and history concepts.

## Assumptions

- The first target is Windows 11, with Windows 10 considered only if the chosen Windows App SDK and runtime combination continues to support it.
- The app is Windows-only. Cross-platform reuse is not a first milestone.
- Offline transcription is a core requirement.
- The app does not need commercial licensing, Polar activation, Sparkle updates, CloudKit sync, or macOS-specific context capture in the first milestone.
- The first implementation can support one local whisper.cpp model path before adding model download and management.
- Distribution can wait until after the app is usable from source.

## Recommended Stack

- UI: WinUI 3 desktop app through the Windows App SDK stable channel.
- Runtime: .NET 10 LTS for long-lived client support.
- Native transcription: whisper.cpp built for Windows and called from .NET through a narrow native interop boundary.
- Audio capture: Windows WASAPI through a replaceable `IAudioCaptureService` implementation.
- Hotkeys: Win32 `RegisterHotKey` for MVP toggle behavior, with a later low-level keyboard hook if true press-and-hold behavior requires key-up detection.
- Text injection: Win32 `SendInput` with a clipboard paste fallback that restores the previous clipboard content when possible.
- Storage: SQLite for transcription history and JSON for lightweight settings.
- Secrets: Windows DPAPI or Windows Credential Manager if API keys are added later.

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

### Out Of Scope For MVP

- macOS feature parity.
- Screen context capture and OCR.
- Browser URL detection.
- Per-application Power Mode.
- Cloud transcription providers.
- AI text enhancement.
- Native Apple transcription.
- FluidAudio or Parakeet models.
- License checks or trial behavior.
- Auto-update infrastructure.
- Installer packaging.

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

`VoiceInk.Windows.Infrastructure` implements storage, JSON settings, SQLite history, and Windows platform services that can be unit or integration tested without the WinUI app.

`VoiceInk.Windows.Native` owns the low-level Windows and whisper.cpp integration. This project should expose narrow C# interfaces to the rest of the app so native details remain isolated.

## Core Flow

```text
Hotkey pressed
  -> DictationController starts capture
  -> AudioCaptureService records PCM samples
Hotkey released or toggled off
  -> DictationController stops capture
  -> WhisperCppTranscriptionService transcribes locally
  -> TextPostProcessor cleans text
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

These interfaces keep the dictation pipeline testable without a microphone, a focused window, or a loaded whisper model.

## Error Handling

- If no microphone is available, the app should show a clear recording error and stay idle.
- If no model path is configured, the app should open settings instead of trying to transcribe.
- If whisper.cpp fails to load or transcribe, the app should keep the audio result in memory only for the active operation, show an error, and avoid inserting failure text into the target app.
- If direct text injection fails, the app should try clipboard paste fallback.
- If clipboard restoration fails, the app should show a non-blocking warning.
- If history save fails, dictation should still complete and the UI should show a non-blocking warning.

## Testing Strategy

- Unit test `TextPostProcessor` with whitespace, empty text, and trailing-space settings.
- Unit test `DictationController` with fake audio, transcription, text injection, settings, and history services.
- Unit test JSON settings load/save with default creation and invalid JSON recovery.
- Integration test SQLite history create/save/list against a temporary database.
- Smoke test the WinUI app manually after the MVP builds: start app, configure model path, trigger hotkey, speak, verify text appears in Notepad.

## Risks

- WinUI 3 tray behavior is not first-class in the same way as classic Win32 tray apps. The app may need a small Win32 interop helper for tray behavior.
- whisper.cpp interop can become unstable if native lifetime management leaks across the app. The interop boundary must stay narrow.
- Reliable text insertion across elevated apps, remote desktop sessions, terminals, and games may vary. MVP should support normal desktop apps first.
- `RegisterHotKey` is simpler but not ideal for hold-to-talk. True hold-to-talk may require a low-level keyboard hook with careful cleanup.
- GPU acceleration support can widen scope quickly. CPU transcription should work first; GPU can follow after the pipeline is stable.

## Verification Criteria

The MVP is successful when a developer can build the Windows solution, run the app on Windows, configure a whisper.cpp model path, use a global hotkey to dictate into Notepad, and see a local history entry created without any network call.

## References

- Upstream VoiceInk repository inspected at commit `0df2a9a` from `main`.
- Microsoft .NET support documentation lists .NET 10 as LTS supported until November 2028.
- Microsoft Windows App SDK downloads list the stable Windows App SDK 1.8 channel as current in 2026.
