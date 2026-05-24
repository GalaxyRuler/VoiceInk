# Windows Open-Source Parity Design

## Status

Approved by user directive for continuous execution.

## Goal

Bring the Windows fork to faithful user-feature parity with the macOS VoiceInk app while keeping the Windows app fully free and open source.

## Source Of Truth

Use the Swift app under `VoiceInk/` as the behavioral and language reference. The Windows app remains a native .NET / WinUI 3 implementation under `VoiceInk.Windows/`.

Key macOS references inspected:

- `VoiceInk/Views/ContentView.swift`: main information architecture.
- `VoiceInk/AppDefaults.swift`: default settings and behavior flags.
- `VoiceInk/Transcription/Engine/TranscriptionPipeline.swift`: post-recording pipeline order.
- `VoiceInk/Views/Recorder/*`: mini and notch recorder experience.
- `VoiceInk/Shortcuts/*`: global shortcut actions and recording modes.
- `VoiceInk/Models/TranscriptionModel*.swift`: model and provider catalog.
- `VoiceInk/Transcription/Cloud/*`: cloud transcription provider catalog.
- `VoiceInk/Services/AIEnhancement/*`: enhancement provider and prompt flow.
- `VoiceInk/PowerMode/*`: app and URL scoped rules.
- `VoiceInk/Views/Dictionary/*`: vocabulary and replacement workflows.
- `VoiceInk/Views/History/*`: history list, detail, retry, copy, paste behavior.
- `VoiceInk/Views/Metrics/*`: session and model performance views.
- `VoiceInk/Views/Settings/SettingsView.swift`: general settings, shortcuts, privacy, backup, diagnostics.
- `VoiceInk/Views/Onboarding/*`: first-run setup.

## Open-Source Rule

The Windows fork must omit commercial surfaces completely.

Omit:

- License checks, trial state, purchase prompts, Pro badges, upgrade banners, paywall flows, paid support prompts, commercial telemetry, and private updater channels.
- The trial-expired paste injection in `TranscriptionPipeline.swift`.
- Polar license activation and management flows.
- Sparkle-style commercial update prompts.

Replace with:

- A neutral About/Open Source view with version, license, source path, diagnostics export, and community issue guidance.
- Local-only diagnostics and log export.
- Open-source friendly update documentation instead of an in-app paid updater.

## Current Windows Baseline

The Windows MVP already has:

- .NET 10 / WinUI 3 solution in `VoiceInk.Windows/`.
- Core dictation contracts and `DictationController`.
- JSON settings persistence.
- SQLite transcription history.
- Whisper.net local transcription over whisper.cpp.
- NAudio microphone capture.
- Clipboard-based text insertion.
- Minimal WinUI shell.
- Configurable global key+modifier shortcuts for recording toggle, paste last, and paste last enhanced.
- README notes for repo root commands, local .NET 10 SDK, and Windows App SDK short-path workaround.
- Core dictionary models and replacement logic.
- Persistent JSON-backed dictionary storage.
- Shell add/remove controls for vocabulary words and word replacements.
- Vocabulary prompt biasing for local Whisper transcription.
- Text cleanup for hallucination markers, filler words, punctuation cleanup, lowercase output, trailing-space handling, and dictionary replacements.
- Expanded history metadata and migration for original/final text, status, language, model path, prompt, and enhancement timing.
- Recent history list/detail shell view for original, final, enhanced, status, timing, model, prompt, and error metadata.
- Local CSV history export under `%LOCALAPPDATA%\VoiceInk.Windows\Exports`.
- Paste-last final and enhanced-preferred history actions.
- Shortcut parser validation and duplicate detection for supported global shortcut actions.

Fresh baseline verification on 2026-05-24:

```powershell
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" test VoiceInk.Windows\VoiceInk.Windows.sln
```

Result: 71 Core tests and 22 Infrastructure tests passed after the shortcut slice.

## Parity Inventory

### Shell And Navigation

macOS has a sidebar app with Dashboard, Transcribe Audio, History, AI Models, Enhancement, Power Mode, Permissions, Audio Input, Dictionary, Settings, and VoiceInk Pro. Windows should keep the same information architecture except replace VoiceInk Pro with About/Open Source.

Windows gaps:

- Tray-first shell.
- Navigation sidebar.
- Dedicated settings pages.
- About/Open Source replacement.

### Floating Recorder

macOS has mini and notch recorder styles with record/stop states, waveform, processing indicators, live partial transcript, AI prompt picker, Power Mode button, cancel behavior, and compact keyboard-driven controls.

Windows gaps:

- Floating recorder window.
- Waveform/level visualization.
- Live partial transcript.
- Prompt and Power Mode controls in recorder.
- Cancel recording state persisted to history.

### Shortcuts

macOS supports primary and secondary recording shortcuts, toggle/push-to-talk/hybrid modes, paste last original, paste last enhanced, retry last transcription, cancel recording, open history, quick add to dictionary, toggle enhancement, per-Power Mode shortcuts, and mini-recorder numeric shortcuts.

Implemented:

- Configurable key+modifier shortcuts for primary recording toggle, paste last transcription, and paste last enhanced transcription.
- Validation for unsupported keys, Windows-key reservations, missing modifiers, and duplicate assignments.

Windows gaps:

- Press-and-hold key-up handling.
- Secondary shortcut.
- Retry-last, cancel-recording, open-history, quick-add, toggle-enhancement, and Power Mode shortcuts.
- Shortcut recorder UI instead of text entry.
- Rich OS-level conflict recovery beyond reporting `RegisterHotKey` failures.

### Transcription Pipeline

macOS pipeline order:

1. Transcribe.
2. Filter hallucinations and filler words.
3. Format text when enabled.
4. Apply word replacements.
5. Apply cleanup preferences.
6. Detect prompt triggers.
7. Enhance when enabled and configured.
8. Paste original or enhanced text.
9. Save history and metrics.

Windows gaps:

- Filler-word filtering.
- Formatting.
- Word replacements.
- Punctuation cleanup.
- Lowercase option.
- Prompt detection.
- Enhancement.
- Metrics.
- Canceled and failed history states.

### Model Management

macOS has local Whisper cards, imported Whisper models, Parakeet/FluidAudio cards, native Apple model, cloud model cards, language selection, custom cloud models, API key management, model download/import, default model selection, and prewarm on wake.

Windows gaps:

- Model catalog and cards.
- Import/download flow.
- Language picker bound to model capabilities.
- Default model management.
- Warmup/preload.
- Provider cards and secure API key storage.

Windows adaptation:

- Skip Native Apple transcription.
- Treat Parakeet/FluidAudio as optional only if a compatible Windows runtime is practical.

### Transcribe Audio

macOS has a dedicated `Transcribe Audio` workflow for queued audio/video files. It supports drag/drop or file choosing, pending/processing/completed/failed states, start/cancel/clear controls, retry, copy/save, and optional AI enhancement.

Windows gaps:

- File queue model.
- Supported media detection.
- Batch transcription orchestration.
- Per-file retry/cancel/copy/save states.
- Optional enhancement in file transcription.

### Cloud Transcription

macOS provider catalog includes Groq, ElevenLabs, Deepgram, Mistral, Gemini, Soniox, Speechmatics, AssemblyAI, xAI, Cartesia, and custom OpenAI-compatible models. Some providers support streaming only.

Windows gaps:

- Provider metadata.
- Batch HTTP adapters.
- Streaming adapters.
- API key verification and secure local storage.
- Custom OpenAI-compatible provider.

### AI Enhancement

macOS has providers for Cerebras, Groq, Gemini, Anthropic, OpenAI, OpenRouter, Mistral, Ollama, Local CLI, custom, and several speech providers where applicable. Enhancement supports prompt templates, assistant mode, custom prompts, trigger words, clipboard context, selected text context, screen/OCR context, retries, timeout, and output filtering.

Windows gaps:

- Enhancement contracts.
- Prompt template persistence.
- OpenAI-compatible provider.
- Ollama and Local CLI hooks.
- Trigger detection.
- Context capture.
- Retry and timeout settings.
- Original text fallback on enhancement failure.

### Context

macOS can use clipboard context, selected text, active window/screen OCR, browser URL, and app-specific context.

Windows gaps:

- Clipboard context capture.
- Selected text context through UI Automation or clipboard fallback.
- Active window title/process.
- OCR via Windows OCR APIs if available.
- Browser URL detection.

Windows adaptation:

- Implement graceful degradation with explicit status when permissions or APIs are unavailable.

### Power Mode

macOS Power Mode supports app and URL rules, configurable model/language/enhancement/prompt/paste/session preferences, emoji/name display, auto-send keys, enabled states, ordering, validation, shortcut integration, and persistence choice.

Windows gaps:

- Power Mode config model.
- Active window and URL matching.
- Per-mode application of settings.
- Auto-send keys.
- UI and recorder integration.

### Dictionary

macOS Dictionary has Word Replacements and Vocabulary sections. Vocabulary stores words for enhancement prompts. Word replacements support comma-separated originals, duplicate detection across variants, enabled state, edit/delete, sorting, quick add, and replacement application in the transcription pipeline.

Implemented core:

- Dictionary models.
- JSON dictionary store.
- Word replacement application.
- Vocabulary prompt rendering.
- Vocabulary prompt pass-through to local Whisper transcription.
- Shell add/delete controls for vocabulary and replacements.

Windows gaps:

- Dedicated macOS-style Dictionary page with edit flow, sorting controls, and richer guidance.
- Import/export.
- Quick add shortcut.

### History

macOS history stores original and enhanced text, timestamp, audio duration, audio file URL, transcription model, enhancement model, prompt name, transcription and enhancement duration, AI request messages, Power Mode name/emoji, and status pending/completed/failed/canceled. UI supports list/detail, retry, copy/paste, audio playback, inline history, and CSV export.

Implemented core:

- SQLite schema now stores original text, final text, enhanced text, status, language, model path, prompt name, enhancement duration, and error message.
- Existing MVP history databases migrate in place and map legacy text to original/final text.
- Core CSV export formatting for stored metadata.
- Shell recent-history list/detail and local CSV export.
- Paste-last final and enhanced-preferred Core primitives with shell buttons.

Windows gaps:

- Audio file URL and playback.
- Enhancement model and AI request messages.
- Power Mode name/emoji.
- Retry-last flow.
- Search, delete, picker-based export, and batch actions.

### Metrics

macOS metrics include session metrics, model speed factors, enhancement timing, dashboard summaries, model performance panels, and system diagnostics.

Windows gaps:

- Session metric persistence.
- Dashboard.
- Model performance aggregation.
- Diagnostics copy/export.

### Settings

macOS settings cover shortcuts, middle-click recording, sound feedback, mute/pause media, clipboard restore delay, paste method, recorder style, Power Mode behavior, privacy cleanup, backup import/export, diagnostics, launch at login, onboarding reset, and update checks.

Windows gaps:

- Most settings beyond model path, cleanup, and the supported global shortcut fields.
- Windows equivalents for launch at login, tray behavior, audio device selection, privacy cleanup, backup, diagnostics, and paste method.

### Audio Input

macOS audio input supports `System Default`, `Custom Device`, and `Prioritized` modes, refresh, active/unavailable states, priority ordering, and fallback behavior.

Windows gaps:

- Device listing and refresh.
- Custom device mode.
- Prioritized fallback mode.
- UI status for active/unavailable input devices.

### Onboarding

macOS first-run onboarding covers introduction, permissions, model download, and tutorial.

Windows gaps:

- First-run flow.
- Microphone permission guidance.
- Model import/download guidance.
- Shortcut setup.
- Basic usage tutorial.

### Packaging

Windows needs open-source friendly packaging.

Windows gaps:

- MSIX or installer project.
- Zip/dev distribution script.
- Native dependency placement.
- Uninstall behavior.
- Shortcut registration.

## Milestone Order

1. Core pipeline parity: dictionary, cleanup, expanded history status, paste-last primitives.
2. Settings/navigation shell: sidebar, settings pages, About/Open Source, persisted settings.
3. Floating recorder: mini recorder first, then notch-like Windows adaptation.
4. Shortcuts: configurable actions, key-up handling, secondary shortcuts, utility actions.
5. Model management: catalog, import/download, language selection, warmup.
6. AI enhancement: prompt templates, OpenAI-compatible provider, secure secrets, fallback.
7. Cloud transcription: provider metadata, custom provider, named adapters.
8. Power Mode: config model, matching, shortcuts, settings override.
9. History and metrics: full metadata, retry, CSV export, performance dashboards.
10. Context and onboarding: clipboard, selected text, active window/OCR, first-run flow.
11. Packaging: installer, zip/dev distribution, smoke tests.

## First Slice

Implement dictionary and text-cleanup core parity first because it is UI-independent, directly improves every transcription, and provides stable data contracts for later Dictionary UI, enhancement prompt rendering, import/export, Power Mode, and history retry work.

The first slice must:

- Add vocabulary and word replacement models in Core.
- Add a dictionary store contract.
- Add text cleanup options matching macOS defaults.
- Apply hallucination filtering, filler-word removal, word replacements, punctuation cleanup, lowercase, trim, and trailing-space handling in the Windows pipeline.
- Save richer original/final text and status metadata in history without breaking existing rows.

Status on 2026-05-24:

- Completed Core dictionary records, validation, vocabulary prompt rendering, and global longest-trigger-first replacement application.
- Completed persistent JSON dictionary storage.
- Completed basic shell add/remove controls for vocabulary and word replacements.
- Completed vocabulary prompt pass-through to local Whisper transcription.
- Completed cleanup options and processing, including macOS-style punctuation cleanup strings in JSON settings.
- Completed richer SQLite history metadata and MVP schema migration.
- Completed core CSV formatting for history export.
- Completed shell recent-history list/detail and local CSV export.
- Completed paste-last final and enhanced-preferred primitives with shell buttons.
- Completed configurable key+modifier global shortcuts for recording toggle, paste last, and paste last enhanced.
- Completed dictation pipeline wiring for cleanup settings and dictionary replacements.
- Completed basic shell controls for filler words, punctuation cleanup, lowercase output, and trailing-space settings.
- Remaining for this slice: dictionary edit/import/export/quick-add, secondary/push-to-talk shortcuts, retry-last, and history search/delete/audio playback.
- Add focused tests and docs.

## Verification

For every slice:

- Run the smallest targeted test first.
- Run the full Windows test suite after the slice.
- Run a Debug x64 build when app or project wiring changes.
- Document manual smoke status when a feature requires microphone/model/API keys/UI.
