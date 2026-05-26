# Windows Open-Source Parity Design

## Status

Approved by user directive for continuous execution.

## Goal

Bring the Windows fork to faithful user-feature parity with the macOS VoiceInk app while keeping the Windows app fully free and open source.

## Source Of Truth

Use the Swift app under `VoiceInk/` as the behavioral and language reference. The Windows app remains a native .NET / WinUI 3 implementation under `VoiceInk.Windows/`.

Key macOS references inspected:

- `VoiceInk/Views/ContentView.swift`: main information architecture.
- `VoiceInk/MenuBarManager.swift` and `VoiceInk/Views/MenuBarView.swift`: menu-bar shell, menu-bar-only mode, window reopen behavior, and menu command language.
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
- `VoiceInk/Views/Onboarding/*`: first-run welcome, permissions, model download, and try-it tutorial flow.

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
- Native Windows tray icon with show/hide, recording toggle, Quick Add, History, and Quit commands.
- Always-on-top floating recorder with Mini and Notch styles during recording and processing, with status text, elapsed timer, live microphone level bars, non-activating Stop/Cancel controls, no-activate Prompt/Power chooser panels, and pulse animation.
- Transcribe Audio navigation section with multi-file picker, in-memory queue, Media Foundation import to app-owned WAV recordings, local Whisper transcription, text cleanup, and History save.
- Default-off AI Enhancement section with prompt catalog, OpenAI-compatible endpoint/model settings, Windows Credential Manager API key storage, output filtering, retry/timeout controls, automatic read-only selected text context, optional read-only clipboard context, original-text fallback, and successful enhancement insertion.
- Cloud transcription with Custom OpenAI-compatible, Groq, and Deepgram presets, provider-specific Credential Manager keys, direct Deepgram batch transcription, and Deepgram live recorder preview streaming.
- Power Mode navigation section with ordered enabled/default process/title rules, Win32 active-window quick fill, session-only model/language/enhancement/prompt/cleanup overrides, and History name/emoji metadata.
- Metrics navigation section backed by local SQLite `metrics.db`, with session totals, words dictated, words per minute, estimated keystrokes/time saved, transcription model performance, and enhancement model performance.
- First-run setup dialog for local model path, microphone settings/input, primary shortcut, and basic usage.
- Imported local Whisper `.bin` model references with shell selection for the default model path.
- Local Whisper catalog cards with direct GGML `.bin` downloads into the app data models folder and default model selection.
- Configurable global key+modifier shortcuts for primary and secondary recording toggle, paste last, paste last enhanced, retry last transcription, cancel recording, open history, quick add to dictionary, and toggle enhancement.
- README notes for repo root commands, local .NET 10 SDK, and Windows App SDK short-path workaround.
- Core dictionary models and replacement logic.
- Persistent JSON-backed dictionary storage.
- Shell add/remove/sort controls for vocabulary words and add/edit/enable/disable/remove/sort controls for word replacements.
- Quick-add dialog with Vocabulary and Word Replacement modes.
- Dictionary JSON import/export for vocabulary words and word replacements.
- Vocabulary prompt biasing for local Whisper transcription.
- Text cleanup for hallucination markers, filler words, punctuation cleanup, lowercase output, trailing-space handling, and dictionary replacements.
- Expanded history metadata and migration for original/final text, status, language, model path, prompt, and enhancement timing.
- Recent history list/detail shell view for original, final, enhanced, status, timing, model, prompt, and error metadata.
- Picker-based CSV history export.
- Paste-last final and enhanced-preferred history actions.
- History search and confirmed single-item delete.
- Settings JSON backup export/import with category selection for General Settings, Custom Prompts, Power Mode, Dictionary, and Custom Model Definitions, with Credential Manager API keys intentionally excluded.
- Settings custom start/stop sound import, local app-owned sound storage, test playback, reset, and Windows system sound fallback.
- Shortcut parser validation and duplicate detection for supported global shortcut actions.
- Refreshable audio input list with System Default/custom microphone selection.

Fresh baseline verification on 2026-05-24:

```powershell
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" test VoiceInk.Windows\VoiceInk.Windows.sln
```

Result: 121 Core tests and 35 Infrastructure tests passed after the first-run onboarding slice.

## Parity Inventory

### Shell And Navigation

macOS has a sidebar app with Dashboard, Transcribe Audio, History, AI Models, Enhancement, Power Mode, Permissions, Audio Input, Dictionary, Settings, and VoiceInk Pro. Windows should keep the same information architecture except replace VoiceInk Pro with About/Open Source.

macOS also has a menu-bar utility shell through `MenuBarManager` and `MenuBarView`. It can keep the app alive after windows close, toggle menu-bar-only mode, focus or hide the main window, open History, and expose recorder/model/enhancement/audio/context/settings/help/quit commands from the menu bar. Windows should adapt this as a tray icon rather than a Dock/menu-bar mode.

Implemented:

- Sidebar navigation with Dashboard, Transcribe Audio, History, AI Models, Enhancement, Audio Input, Dictionary, Settings, and About / Open Source sections.
- Tray icon with Show VoiceInk, Hide VoiceInk, Start/Stop Recording, Quick Add to Dictionary, History, and Quit VoiceInk commands.
- Close-to-tray behavior for the main window, with explicit Quit from the tray.
- Tray state presenter for recording/busy/loading labels and enabled states.
- About / Open Source section with version/source information and local-only diagnostics folder/summary actions.

Windows gaps:

- Dedicated settings pages beyond grouped existing shortcut/cleanup controls.
- Dedicated multi-window History surface.
- Power Mode and Permissions sections.
- Tray submenus for model/provider/enhancement/language/audio/context/settings once those Windows subsystems exist.

Tray-shell slice completed on 2026-05-24:

- Native Windows tray icon with menu entries for Show VoiceInk, Hide VoiceInk, Start/Stop Recording, Quick Add to Dictionary, History, and Quit VoiceInk.
- Menu state synchronized with dictation state so recording and busy states use correct labels/enabled states.
- macOS commercial updater/support items omitted. Future open-source replacements belong in About/Open Source and diagnostics export, not the tray shell.
- Model/provider/enhancement/language/audio context submenus remain later work until those Windows subsystems and view-models exist.

Navigation-settings-shell slice completed on 2026-05-25:

- Use WinUI `NavigationView` as the Windows-native adaptation of macOS `NavigationSplitView`.
- Move existing source-runnable controls into macOS-aligned active sections: Dashboard, History, AI Models, Audio Input, Dictionary, and Settings.
- Replace the commercial `VoiceInk Pro` sidebar destination with `About / Open Source`.
- Add local-only diagnostics actions under About/Open Source.
- Keep Power Mode and Permissions as later slices until their real Windows subsystems have usable controls.

### Floating Recorder

macOS has mini and notch recorder styles with record/stop states, waveform, processing indicators, live partial transcript, AI prompt picker, Power Mode button, cancel behavior, and compact keyboard-driven controls.

Floating-recorder slice completed on 2026-05-25:

- Added a compact always-on-top Windows mini-recorder adapted from `MiniRecorderPanel` and `MiniRecorderView`.
- Shows while recording, transcribing, inserting, or during an explicit operation status such as starting, stopping, or canceling.
- Shows state title, detail text, elapsed timer, animated pulse bars, and disabled Prompt/Power Mode affordance labels for design continuity.
- Kept stop on global shortcuts, tray commands, and main-window controls, and kept cancel on global shortcuts and main-window controls in this slice while the no-activate recorder command path was deferred.
- Uses a Core `FloatingRecorderPresenter` for state-to-UI mapping.
- Leaves live partial transcript, real audio meter waveform, notch style, prompt picker behavior, and Power Mode behavior for later slices.

Floating-recorder level-meter slice completed on 2026-05-25:

- Added Core PCM16 peak-level calculation for local microphone buffers.
- Added NAudio capture level events from the same recording buffers written to disk, with subscriber failures isolated from recording.
- Added live five-bar microphone level rendering in the compact Windows recorder while recording.
- Kept processing states on a lightweight pulse animation when live microphone input is unavailable.

Floating-recorder controls slice completed on 2026-05-25:

- Added compact Stop and Cancel controls to the Windows floating recorder.
- Controls are enabled only for the recording state and remain disabled while starting, stopping, canceling, transcribing, inserting, idle, or error.
- Routed recorder controls through the existing guarded `StopCurrentRecordingAsync` and `CancelCurrentRecordingAsync` commands so tray, shortcut, main-window, and recorder behavior stays consistent.
- Kept `AppWindow.Show(activateWindow: false)` and added a Win32 `WM_MOUSEACTIVATE` subclass returning `MA_NOACTIVATE`, allowing recorder clicks to process without activating the recorder window or stealing the paste target.

Floating-recorder Prompt/Power controls slice completed on 2026-05-25:

- Added a Core `FloatingRecorderControlPresenter` for test-covered Prompt and Power control labels, enabled states, and selected choices.
- Prompt control now enables AI enhancement on first click and cycles through available prompts after enhancement is enabled, using the same app-level enhancement prompt settings as the Enhancement page.
- Power control now cycles `Auto` plus enabled Power Mode rules through an explicit selected Power Mode rule id, using the same rules shown on the Power Mode page.
- `PowerModeMatcher` prefers the explicit selected rule when it is enabled, then falls back to active-window/default matching.
- `DictationController` still captures the recording-start target window, but reloads current settings on stop/cancel so Prompt and Power changes made from the recorder during capture affect the active transcription without changing the paste target.
- This Windows slice intentionally uses click-to-cycle controls instead of hover popovers because WinUI flyouts can steal focus from the target app; richer no-activate popovers remain a visual fidelity follow-up.

Floating-recorder no-activate popovers slice completed on 2026-05-25:

- Match the macOS `EnhancementPromptPopover` and `PowerModePopover` structure without using WinUI `Flyout`, because Microsoft documents flyouts as light-dismiss controls that trap keyboard focus until dismissed.
- Host Prompt and Power chooser panels inside the existing floating recorder window so the current `AppWindow.Show(false)` plus `WM_MOUSEACTIVATE -> MA_NOACTIVATE` no-activation behavior still applies.
- Resize the floating recorder window upward while a chooser is open, keeping the recorder chrome anchored to the bottom-center work area position so it does not slide below the taskbar.
- Prompt chooser shows an `AI Enhancement` toggle, selected prompt row, prompt rows, disabled styling when enhancement is off, and selecting any prompt enables enhancement before persisting the prompt.
- Power chooser shows `Select Power Mode`, `Auto`, enabled Power Mode rows, selected-row checkmarks, and `No Power Modes Available` when there are no enabled rules.
- Stop and Cancel must wait for any in-flight chooser persistence before completing the recording, preserving the race fix from the click-to-cycle slice.

Floating-recorder hover-dismissal slice completed on 2026-05-25:

- Prompt and Power chooser panels now open on pointer hover over their recorder buttons.
- The open chooser remains visible while the pointer is over either the button or the chooser panel.
- The chooser closes after a 250 ms delay once the pointer leaves both regions, matching the macOS recorder popover timing.
- Manual click toggles still work, and all behavior stays inside the existing no-activate recorder window.

Floating-recorder live transcript preview plumbing slice completed on 2026-05-25:

- Added the macOS-aligned `Show Live Transcript Preview` setting under AI Models, persisted in JSON settings and General Settings backup/import.
- Added Core partial transcript state to the dictation controller, with updates accepted only while recording and cleared at start/stop/cancel/error boundaries.
- Added presenter gating so live transcript text is shown only while recording, the preview setting is enabled, and a real partial transcript source has provided non-empty text.
- Added a compact live transcript panel above the Windows floating recorder chrome. The panel expands the existing no-activate recorder window upward and collapses when no live text is available.
- This slice intentionally does not generate fake partial text. Deepgram live preview adds the first real provider source; more Windows streaming transcription providers remain later provider slices.

Floating-recorder recorder-style slice completed on 2026-05-25:

- Added a macOS-compatible `RecorderStyle` setting with `mini` default and `notch` option, persisted in JSON settings and General Settings backup/import.
- Added a Settings `Interface` control named `Recorder Style` with `Mini` and `Notch` choices.
- Extended the Core floating recorder view state with normalized recorder style so WinUI renders style from presenter output instead of reading settings directly.
- Kept the existing mini recorder as the default bottom-center style.
- Added a Windows notch-style adaptation that moves the no-activate always-on-top recorder to the top center of the current work area, uses a black top-edge pill shape, and expands downward for live transcript preview and Prompt/Power chooser panels.
- Preserved no-activate mouse behavior, Stop/Cancel commands, Prompt/Power controls, live microphone meter, processing pulse, and live transcript gating across both styles.

Deepgram live preview slice completed on 2026-05-25:

- Added Core audio chunk publishing and live preview session contracts so recorder partials are provider-supplied rather than inferred from local state.
- Extended NAudio capture to publish copied 16 kHz mono PCM chunks while still writing the complete WAV file for final transcription.
- Added a Deepgram live preview service that starts only when the Deepgram preset, saved Deepgram key, and `Show Live Transcript Preview` are all present.
- Sends Deepgram streaming websocket audio through a single serialized send loop with `encoding=linear16`, `sample_rate=16000`, `channels=1`, `interim_results=true`, and `smart_format=true`.
- Parses Deepgram interim/final messages into an in-memory recorder preview, clears preview text at recording boundaries, and keeps stopped-recording transcription as the final insertion/history source.
- Keeps preview failures best-effort so recording and final transcription continue with sanitized warning text.

Windows gaps:

- Additional real streaming partial transcript sources beyond Deepgram.
- Pixel-perfect macOS physical notch geometry is intentionally not implemented because Windows does not expose macOS safe-area notch metrics; the Windows equivalent is a top-center notch-style recorder.

### Shortcuts

macOS supports primary and secondary recording shortcuts, toggle/push-to-talk/hybrid modes, paste last original, paste last enhanced, retry last transcription, cancel recording, open history, quick add to dictionary, toggle enhancement, per-Power Mode shortcuts, and mini-recorder numeric shortcuts.

Implemented:

- Configurable key+modifier shortcuts for primary and secondary recording toggle, paste last transcription, paste last enhanced transcription, retry last transcription, cancel active recording, open history, quick add to dictionary, and toggle enhancement.
- Modifier-only primary and secondary recording shortcuts with Toggle, Push to Talk, and Hybrid key-up behavior.
- Power Mode cycle and direct per-rule shortcuts.
- Mini-recorder Ctrl/Alt digit prompt and Power Mode slots.
- Validation for unsupported keys, Windows-key reservations, missing modifiers, and duplicate assignments.
- Read-only shortcut recorder fields with explicit `Record` buttons for Settings and Power Mode rule shortcuts.

Windows gaps:

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

Implemented core:

- Hallucination marker cleanup, filler-word filtering, whitespace normalization, word replacements, punctuation cleanup, lowercase output, trailing-space handling, and prompt trigger detection.
- History model/storage/export support for completed, failed, and canceled statuses.

Windows gaps:

- macOS-style formatting pass beyond cleanup preferences.

Failed dictation history slice completed on 2026-05-25:

- When audio capture has already stopped, post-capture dictation failures now save a best-effort `Failed` History row.
- Failed rows preserve provider, language, model metadata, audio duration/path, Power Mode metadata, and the error message.
- Failed rows do not insert text and do not record session metrics.
- Capture-start and capture-stop failures without a stopped audio result still avoid History writes.

Toggle Enhancement Shortcut slice completed on 2026-05-25:

- Added `ToggleEnhancementHotkey` to settings, global shortcut registration, JSON persistence, and settings backup/import.
- Added a Settings > Shortcuts `Toggle Enhancement` field and a `GlobalShortcutAction.ToggleEnhancement` dispatch path.
- Pressing the shortcut flips the persisted `IsEnhancementEnabled` setting and updates the Enhancement checkbox/status.

Cancel-processing slice completed on 2026-05-25:

- `Cancel Recording` now cancels the active post-capture stop pipeline while VoiceInk is transcribing, enhancing, or inserting.
- `DictationController` preserves the captured audio as a canceled History row when cancellation happens after capture stops and before completed history/metrics are saved.
- Completed session metrics are not written for canceled processing.
- Cancellation remains cooperative through .NET cancellation tokens; adapters that ignore tokens may still finish their current call before the UI can return to idle.

### Model Management

macOS has local Whisper cards, imported Whisper models, Parakeet/FluidAudio cards, native Apple model, cloud model cards, language selection, custom cloud models, API key management, model download/import, default model selection, and prewarm on wake. The macOS local model flow scans model files, imports Whisper ggml `.bin` models, displays imported model cards, and lets the user set a default transcription model.

The macOS local Whisper catalog includes `ggml-tiny`, `ggml-tiny.en`, `ggml-base`, `ggml-base.en`, `ggml-large-v2`, `ggml-large-v3`, `ggml-large-v3-turbo`, and `ggml-large-v3-turbo-q5_0`. Model cards show display name, language, size, speed, accuracy, description, download progress, and actions for Download, Set as Default, Delete Model, and Show in Finder. Downloads use the open-source whisper.cpp GGML Hugging Face URL shape `https://huggingface.co/ggerganov/whisper.cpp/resolve/main/{name}.bin`. macOS also downloads Core ML encoder zips for non-quantized models; Windows skips that macOS-only optimization and downloads only the `.bin` model file for Whisper.net/whisper.cpp.

Local-model-library slice completed on 2026-05-24:

- Persist imported local Whisper `.bin` model references in Windows JSON settings.
- Add an imported-model selector to the Windows shell and set the selected model as the default local model path.
- Add an import picker for `.bin` files and a link to open the open-source whisper.cpp GGML model downloads page.
- Keep model catalog cards, direct in-app download, language capability UI, warmup/preload, and cloud provider cards as later model-management work.

Model catalog downloads Windows target:

- Add macOS-aligned local Whisper catalog metadata and recommended ordering for `ggml-base.en` and `ggml-large-v3-turbo-q5_0`.
- Display local Whisper model cards in AI Models with language, size, speed, accuracy, description, downloaded/default state, and download progress.
- Download `.bin` files directly from the whisper.cpp Hugging Face catalog into `%LocalAppData%\VoiceInk.Windows\Models`.
- Add downloaded files to the existing imported local model list and set the downloaded model as the default local model path after a successful download.
- Keep user-imported `.bin` paths supported, and allow showing downloaded or imported files in Explorer.
- Do not download Core ML encoder archives on Windows; document that they are macOS-only acceleration assets.

Model catalog downloads slice completed on 2026-05-25:

- Added a Core local Whisper catalog with macOS metadata, recommended ordering, whisper.cpp Hugging Face download URLs, and test-covered downloaded/default card state.
- Added an Infrastructure streaming downloader that writes to a `.download` temp file, reports progress, preserves existing complete model files on failure, and cleans temp files on failure or cancellation.
- Added WinUI AI Models catalog cards with language, size, speed, accuracy, description, downloaded/default status, download progress, set-default, and Show in Explorer actions.
- Downloaded catalog models are saved under `%LocalAppData%\VoiceInk.Windows\Models`, replace existing entries with the same GGML name, persist in JSON settings, and become the default local model path after successful download.
- Core ML encoder downloads remain omitted because they are macOS-only acceleration assets.

Model language selection Windows target:

- Add a macOS-style `Transcription Language` control to AI Models.
- Use local Whisper model capability metadata so `.en` catalog models force `en`, while multilingual catalog models and imported `.bin` models offer `auto` plus Whisper language codes.
- Keep the current language if it is supported when the default model changes. Fall back to `en` for English-only models and `auto` for multilingual models when the saved language is blank or incompatible.
- Persist the selected language in JSON settings so dictation, Transcribe Audio, cloud transcription, history retry, and Power Mode overlays keep using the shared `AppSettings.Language` flow.
- Keep native Apple language asset downloads omitted on Windows.

Model language selection slice completed on 2026-05-25:

- Added a Core Whisper language catalog using macOS language names and Whisper language-code support.
- Added model-aware language fallback rules: English-only `.en` catalog models force `en`; multilingual catalog and imported local models offer `auto` plus Whisper language codes.
- Added an AI Models `Transcription Language` combo that updates when the selected model path changes, disables selection for English-only models, and persists user selections to JSON settings.
- Reused the existing `AppSettings.Language` flow, so dictation, Transcribe Audio, cloud transcription, history retry, and Power Mode overlays consume the selected language without platform-specific branching.

Model warmup/preload slice completed on 2026-05-25:

- Added a Core warmup coordinator that schedules nonblocking local Whisper warmups, skips cloud providers or missing model paths, prevents duplicate warmups, normalizes language through the same configuration path as transcription, and reports UI-safe status.
- Added a native Whisper.net warmup adapter that validates the configured GGML model file, creates a `WhisperFactory` and processor, applies the configured language/prompt settings, and disposes everything after the preload.
- Added AI Models controls for `Prewarm Local Model`, `Warm Up Selected Model`, and model warmup status.
- Scheduled warmup after startup, after local model import/default selection, after successful catalog download/default selection, after transcription provider settings save, and after Windows resume through `SystemEvents.PowerModeChanged`.
- Kept this as a conservative preload rather than a persistent shared model cache; actual transcription still owns its processor lifetime.

Windows gaps:

- Provider cards and secure API key storage.

Windows adaptation:

- Skip Native Apple transcription.
- Treat Parakeet/FluidAudio as optional only if a compatible Windows runtime is practical.

### Transcribe Audio

macOS has a dedicated `Transcribe Audio` workflow for queued audio/video files. It supports drag/drop or file choosing, pending/processing/completed/failed states, start/cancel/clear controls, retry, copy/save, and optional AI enhancement.

Transcribe Audio slice completed on 2026-05-25:

- Added a Windows `Transcribe Audio` navigation section after Dashboard.
- Added multi-file picker import using Windows App SDK picker APIs, plus drag-and-drop import from local dropped files, with supported audio/video extensions backed by Windows Media Foundation where codecs are available.
- Added a source-runnable in-memory queue with pending, processing, completed, failed, remove, retry, clear, start, and cancel behavior.
- Convert/import selected media into app-owned WAV recordings before transcription, then run the existing local Whisper transcription path with dictionary prompt biasing and text cleanup.
- Save completed file transcriptions into the existing SQLite history with original/final text, provider/model/language metadata, transcription duration, audio duration, and app-owned audio file path.
- Added selected completed-item copy and save actions, with plain text clipboard copy plus TXT/Markdown file export.
- Added local JSON queue snapshot restoration for pending, failed, and interrupted processing items when their source files still exist.
- Added selected completed-item enhancement that forces a one-off enhancement run without changing the global Enhancement toggle.
- Leave richer batch actions for later slices.

Windows gaps:

- Richer batch actions.

### Cloud Transcription

macOS provider catalog includes Groq, ElevenLabs, Deepgram, Mistral, Gemini, Soniox, Speechmatics, AssemblyAI, xAI, Cartesia, and custom OpenAI-compatible models. Some providers support streaming only.

Cloud Transcription Windows MVP target:

- Add a default-off `OpenAI-compatible` transcription provider option alongside local Whisper.
- Keep the provider user-configured: endpoint URL, model ID, and API key supplied by the user.
- Store the API key in Windows Credential Manager, not JSON settings.
- Send request-based multipart audio transcription requests with `file`, `model`, `response_format=json`, optional ISO language when not `auto`, and optional vocabulary/prompt context.
- Parse JSON responses with a `text` property and return clear sanitized errors without logging secrets or provider response bodies.
- Use the provider in dictation, Transcribe Audio, and history retry through the existing Core transcription interface.
- Keep additional named provider cards, streaming providers beyond Deepgram, and provider-specific payloads for later slices.

Cloud Transcription slice completed on 2026-05-25:

- Added `OpenAI-compatible` as a default-off transcription provider selection in AI Models.
- Added Core provider-aware settings, validation, transcription options, and history model metadata for dictation, Transcribe Audio, and history retry.
- Added an Infrastructure multipart HTTP adapter that sends `file`, `model`, `response_format=json`, optional language, and optional vocabulary/prompt context to a user-supplied endpoint, with bearer auth from Windows Credential Manager.
- Require HTTPS for remote cloud transcription endpoints, allow HTTP only for loopback/local development endpoints, and reject endpoint URLs that embed credentials or common key/token query parameters.
- Added sanitized configuration, HTTP, and JSON response errors that do not include provider response bodies or API keys.
- Added a router between local Whisper and OpenAI-compatible transcription so the existing UI workflows use the selected provider.
- Added WinUI endpoint/model/key controls and key save/clear/status actions without storing API keys in JSON settings.

Cloud provider preset slice completed on 2026-05-25:

- Added a Core preset catalog with `Custom OpenAI-compatible` and `Groq`.
- Added Groq endpoint/model defaults for `https://api.groq.com/openai/v1/audio/transcriptions`, `whisper-large-v3-turbo`, and `whisper-large-v3`.
- Added provider-specific Credential Manager secret names so custom and Groq keys are stored independently.
- Added provider ID persistence in JSON settings without storing keys in JSON.
- Added WinUI preset and preset-model selectors that prefill compatible endpoint/model defaults while leaving custom configuration editable.
- Saved Groq cloud transcription rows with provider metadata `groq`.

Deepgram cloud transcription and live preview slice completed on 2026-05-25:

- Added Deepgram to the transcription preset catalog with `https://api.deepgram.com/v1/listen`, `nova-3`, and `nova-3-medical`.
- Added provider-specific Credential Manager secret name `VoiceInk.Windows.Transcription.OpenAICompatible.Deepgram.ApiKey`.
- Added a direct Deepgram batch transcription adapter using `Authorization: Token ...`, `smart_format=true`, optional language, raw WAV content, sanitized HTTP/JSON errors, and Deepgram transcript extraction.
- Routed Deepgram final transcription separately from the generic OpenAI-compatible multipart adapter while preserving the shared Core transcription interface for dictation, Transcribe Audio, and History Retry.
- Added a Deepgram websocket live preview adapter for recorder partials when live preview is enabled.

Windows gaps:

- Named provider cards beyond Groq and Deepgram.
- Streaming adapters beyond Deepgram live preview.
- Provider-specific payloads for other non-compatible APIs.
- In-app provider test requests and API-key verification.

### AI Enhancement

macOS has providers for Cerebras, Groq, Gemini, Anthropic, OpenAI, OpenRouter, Mistral, Ollama, Local CLI, custom, and several speech providers where applicable. Enhancement supports prompt templates, assistant mode, custom prompts, trigger words, clipboard context, selected text context, screen/OCR context, retries, timeout, and output filtering.

AI Enhancement slice completed on 2026-05-25:

- Add Core prompt catalog, prompt rendering, trigger detection, output filtering, and provider-agnostic enhancement orchestration.
- Add an OpenAI-compatible chat-completions HTTP adapter with sanitized errors and no secret logging.
- Store the provider API key in Windows Credential Manager rather than JSON settings.
- Add an Enhancement navigation section for enable/disable, endpoint/model, prompt selection, timeout, skip-short, retry-on-timeout settings, and key save/clear.
- Run enhancement after local transcription cleanup and before insertion. Store original cleaned text as history `Text`, successful enhancement as `EnhancedText`, plus enhancement provider/model, prompt, duration, and rendered request messages for local diagnostics. Paste the enhanced text. On enhancement failure, paste original cleaned text and keep enhanced text empty so paste-last-enhanced never pastes an error string.
- Add a default-off `Clipboard Context` setting that reads the current text clipboard without modifying it, appends it to the enhancement system message inside `<CLIPBOARD_CONTEXT>` tags, and gracefully skips empty, non-text, or unavailable clipboard content.
- Add best-effort selected text capture through read-only Windows UI Automation when enhancement runs, append it to the enhancement system message inside `<CURRENTLY_SELECTED_TEXT>` tags, and gracefully skip unavailable controls, empty selections, or failed reads.

AI enhancement provider preset Windows target:

- Match the macOS provider picker terminology for enhancement providers where the current Windows adapters can make a faithful request: Custom OpenAI-compatible, Cerebras, Groq, Gemini, Anthropic, OpenAI, OpenRouter, Mistral, Ollama, and Local CLI.
- Keep speech/transcription-only providers as later slices because they need transcription-oriented adapters rather than the existing enhancement request paths.
- Persist a provider ID in JSON settings while continuing to store API keys only in Windows Credential Manager.
- Store API keys per provider preset so choosing Groq, Gemini, OpenAI, or another provider does not overwrite the custom provider key.
- Preserve the legacy single enhancement secret name as a custom-provider fallback so existing local settings keep working.
- Use provider display names and model choices from the macOS source of truth, while documenting that users may still type any compatible model ID.
- Reject remote enhancement endpoints that use plain HTTP, contain embedded credentials, or contain common key/token query parameters. Plain HTTP remains allowed only for local loopback providers such as Ollama.
- Keep this as local configuration only: no bundled keys, no sign-up flow, no provider account prompt, no telemetry, and no commercial upgrade surface.

AI enhancement provider preset slice completed on 2026-05-25:

- Added a Core enhancement provider preset catalog for Custom OpenAI-compatible, Cerebras, Groq, Gemini, OpenAI, OpenRouter, Mistral, and Ollama, using macOS display names, endpoints, default models, and static model lists where available.
- Added `EnhancementProviderId` JSON settings persistence and provider-aware enhancement requests/history metadata.
- Added provider-specific Windows Credential Manager secret names with legacy custom-key fallback. Ollama is treated as keyless local configuration.
- Hardened enhancement endpoint validation to reject plain HTTP remote endpoints, embedded credentials, and common key/token query parameters while allowing loopback HTTP for local development.
- Added WinUI Enhancement provider and preset-model pickers, provider-specific API-key save/clear/status text, and disabled API-key controls for keyless providers.
- Added dynamic Ollama local model refresh through the Ollama `/api/tags` endpoint, surfaced as an Enhancement action only when the Ollama preset is selected.
- Added dynamic OpenRouter model refresh through the OpenRouter `/api/v1/models` endpoint, sharing the provider-aware Enhancement refresh action.
- Added Anthropic as a native Messages API enhancement provider with provider-specific Credential Manager keys and macOS-aligned Claude model choices.
- Added Local CLI enhancement hooks that execute user-configured Windows commands with VoiceInk prompt environment variables and use stdout as the enhanced text.
- Added prompt icon and description editing for custom prompts while keeping predefined prompt metadata read-only.

Windows gaps:

- Screen/OCR, browser URL, and deeper app-specific context capture.
- Recorder prompt picker activation.
- AI re-enhance from History.

Custom prompt persistence Windows target:

- Persist user-created AI enhancement prompts in JSON settings using the existing Core `EnhancementPrompt` shape: title, prompt text, icon, optional description, trigger words, and `UseSystemInstructions`.
- Keep predefined prompt text source-controlled while allowing trigger-word overrides to persist for predefined prompts, matching the macOS behavior where predefined prompts are refreshed from source but trigger words can survive.
- Add Enhancement-section controls for selecting, creating, updating, and deleting prompt templates. Predefined prompts can have trigger words edited but cannot be deleted or have their source prompt text overwritten.
- Ensure the dictation and Transcribe Audio enhancement pipeline reads the current prompt library after prompt edits without needing to restart the app.
- Keep prompt persistence local only. No prompt marketplace, account sync, commercial template upsell, telemetry, or bundled paid prompt catalog.

Custom prompt persistence slice completed on 2026-05-25:

- Added a Core prompt library that merges source-controlled predefined prompts with locally persisted custom prompts and predefined trigger-word overrides.
- Added JSON settings persistence for local custom enhancement prompts.
- Changed the enhancement pipeline to read the current prompt library dynamically, so prompt edits apply to dictation, Transcribe Audio, and retry without restarting the app.
- Added WinUI Enhancement controls for new/save/delete prompt actions, prompt title, prompt instructions, trigger words, and system-instruction wrapping.
- Kept predefined prompt text source-controlled and non-deletable while allowing local trigger-word edits.

### Context

macOS can use clipboard context, selected text, active window/screen OCR, browser URL, and app-specific context.

Clipboard enhancement context Windows MVP target:

- Add a default-off `UseClipboardContext` setting matching macOS `useClipboardContext`.
- Capture current plain-text clipboard content only when enhancement is about to run, never modifying clipboard contents.
- Add captured text to the enhancement system message inside `<CLIPBOARD_CONTEXT>` tags, matching the macOS prompt contract.
- Gracefully skip context when the clipboard is empty, unavailable, non-text, or cannot be read.
- Keep context capture local to the enhancement request and persist only the rendered AI request messages already saved for local History diagnostics.
- Leave selected-text context, screen/OCR context, and browser URL context for later Windows context slices.

Clipboard enhancement context slice completed on 2026-05-25:

- Added `UseClipboardContext` JSON setting and an Enhancement section `Clipboard Context` toggle.
- Added Core `EnhancementContext` and `IEnhancementContextProvider` contracts so context capture remains UI-independent and testable.
- Added read-only Windows clipboard text capture through the native adapter, with empty/unavailable/non-text/failure fallback to no context.
- Added `<CLIPBOARD_CONTEXT>` rendering before vocabulary in the enhancement system message, matching the macOS prompt contract.

Selected-text enhancement context Windows MVP target:

- Match macOS's best-effort selected-text context by attempting read-only UI Automation selected-text capture when enhancement is about to run.
- Add selected text to the enhancement system message inside `<CURRENTLY_SELECTED_TEXT>` tags, before clipboard and vocabulary sections, matching macOS prompt ordering.
- Gracefully skip context when the focused element has no selected text, does not expose UI Automation `TextPattern`, cannot be read, or the read is canceled.
- Avoid clipboard-copy fallback in this slice so selected-text capture never modifies user clipboard contents.
- Keep selected text local to the enhancement request and persist only the rendered AI request messages already saved for local History diagnostics.

Selected-text enhancement context slice completed on 2026-05-25:

- Added selected text to Core enhancement context rendering inside `<CURRENTLY_SELECTED_TEXT>` tags before clipboard and vocabulary context.
- Changed enhancement context providers to receive a request describing which sources to read, so clipboard remains gated by `Clipboard Context` while selected text is best-effort when enhancement runs.
- Added Windows UI Automation selected-text capture through `AutomationElement.FocusedElement`, `TextPattern.GetSelection()`, and `TextPatternRange.GetText(...)`.
- Added a Windows context aggregator that isolates selected-text and clipboard read failures and degrades each source independently.

Selected-text clipboard fallback slice completed on 2026-05-25:

- Added a guarded clipboard-copy fallback for selected text when UI Automation returns no selection.
- The fallback captures the current clipboard, sends Ctrl+C, reads text, truncates it for prompt context, and restores the prior clipboard state in a best-effort finally block.
- When both selected text and clipboard context are requested, ordinary clipboard context is read only after fallback restoration so it sees the user's original clipboard text.

Active-window enhancement context slice completed on 2026-05-25:

- Added active-window process/title fields to Core enhancement context.
- Prompt rendering now emits an `<ACTIVE_WINDOW_CONTEXT>` section before selected text, clipboard, and vocabulary sections.
- Windows context capture reuses the existing foreground-window Power Mode provider and gracefully degrades to no active-window context on failure.

Windows gaps:

- OCR via Windows OCR APIs if available.
- Browser URL detection.

Windows adaptation:

- Implement graceful degradation with explicit status when permissions or APIs are unavailable.

### Power Mode

macOS Power Mode supports app and URL rules, configurable model/language/enhancement/prompt/paste/session preferences, emoji/name display, auto-send keys, enabled states, ordering, validation, shortcut integration, and persistence choice.

Power Mode Windows MVP target:

- Add a Core `PowerModeRule` model persisted with normal JSON settings.
- Match enabled rules in stored order against the active Windows process name and title bar text.
- Apply a default rule only when no app/window rule matches.
- Capture the active window at recording start and keep the chosen Power Mode for the whole dictation session.
- Override model path, language, enhancement enabled state, selected enhancement prompt, trailing-space paste cleanup, filler removal, punctuation cleanup, and lowercase cleanup for that session without rewriting the user's base settings.
- Store Power Mode name and emoji in history and CSV export for completed and canceled rows.
- Add a WinUI Power Mode section with rule list, add/update/remove, enable toggle, active-window quick fill, ordering, and basic override controls.
- Defer browser URL detection, auto-send keys, and Power Mode global shortcuts until later Power Mode slices.

Power Mode slice completed on 2026-05-25:

- Added Core Power Mode rules, target matching, default fallback, and settings overlay behavior.
- Added native Windows active-window process/title detection behind a Core interface.
- Captured the matching rule at recording start and used its effective settings for transcription, cleanup, enhancement, insertion, and canceled-history metadata.
- Added Power Mode name/emoji to SQLite history, CSV export, and the History detail view.
- Added a WinUI Power Mode section for ordered rules, active-window quick fill, enabled/default toggles, and model/language/enhancement/prompt/cleanup overrides.
- Added floating-recorder Power Mode chooser integration with Auto plus enabled rule selection.

Windows Win32 grounding:

- `GetForegroundWindow` is the active-window primitive for the window the user is working with.
- `GetWindowThreadProcessId` maps that window to the owning process id.
- `GetWindowTextW` reads the title bar text when the window exposes one; empty titles must degrade gracefully.

Windows gaps:

- Browser URL matching.
- Auto-send keys.
- Power Mode shortcuts.

### Dictionary

macOS Dictionary has Word Replacements and Vocabulary sections. Vocabulary stores words for enhancement prompts. Word replacements support comma-separated originals, duplicate detection across variants, enabled state, edit/delete, sorting, quick add, and replacement application in the transcription pipeline.

Implemented core:

- Dictionary models.
- JSON dictionary store.
- Word replacement application.
- Vocabulary prompt rendering.
- Vocabulary prompt pass-through to local Whisper transcription.
- Shell add/delete/sort controls for vocabulary.
- Shell add/edit/delete/enable/disable/sort controls for word replacements.
- Dictionary-only JSON backup export/import using macOS backup field names for vocabulary and word replacements.
- Quick-add dialog with Vocabulary and Word Replacement modes, opened by shell button or global shortcut.

Windows gaps:

- Dedicated macOS-style Dictionary navigation page and richer layout/guidance.

### History

macOS history stores original and enhanced text, timestamp, audio duration, audio file URL, transcription model, enhancement model, prompt name, transcription and enhancement duration, AI request messages, Power Mode name/emoji, and status pending/completed/failed/canceled. UI supports list/detail, retry, copy/paste, audio playback, inline history, and CSV export.

Implemented core:

- SQLite schema now stores original text, final text, enhanced text, status, language, model path, prompt name, enhancement duration, error message, and audio file path.
- SQLite schema now stores enhancement provider/model and rendered AI request system/user messages for local diagnostics.
- Existing MVP history databases migrate in place and map legacy text to original/final text.
- Core CSV export formatting for stored metadata, including audio file path.
- Core selected-history retry service for existing audio files using current local transcription settings, dictionary prompt, replacements, and cleanup options.
- Core retry-latest service for the newest completed history item with a saved audio file.
- Active-recording cancellation saves a canceled history row with the captured audio file.
- Shell recent-history list/detail and picker-based CSV export.
- Paste-last final and enhanced-preferred Core primitives with shell buttons.
- Search, selected-row audio playback/open, selected-row retry, retry-last-to-clipboard, active-recording cancel history, and confirmed single-item delete in the shell.

Windows gaps:

- Power Mode name/emoji.
- Waveform/rate controls and AI re-enhance from the macOS audio player.
- Batch actions.

### Metrics

macOS metrics include session metrics, model speed factors, enhancement timing, dashboard summaries, model performance panels, and system diagnostics.

Session metrics Windows MVP target:

- Add separate session metric persistence rather than deriving all dashboard state directly from recent History rows.
- Record one metric for each completed recorder, Transcribe Audio, or retry history item, keyed by transcription/history id so retries create their own rows and duplicate saves do not double-count.
- Store transcription id, timestamp, source, word count, audio duration, transcription model name, transcription duration, speed factor, Power Mode name, enhancement model name, and enhancement duration.
- Count words from enhanced text only when enhancement ran and produced non-empty enhanced text; otherwise count the final cleaned transcription text.
- Keep canceled and failed history rows visible in History but out of session metrics, matching macOS recorder metric behavior.
- Add a Metrics sidebar section with sessions recorded, words dictated, words per minute, keystrokes saved, time saved, transcription model performance, and enhancement model performance.
- Keep diagnostics copy/export open-source and local-only.

Session Metrics slice completed on 2026-05-25:

- Added Core `SessionMetric`, dashboard summary, model performance stats, pure aggregation, and a `SessionMetricRecorder` matching macOS completed-session behavior.
- Added `ISessionMetricStore` and `SqliteSessionMetricStore` with idempotent `transcription_id` persistence, schema migration, summary queries, and filtered model/enhancement performance aggregation.
- Completed recorder, Transcribe Audio, and retry flows now record metrics after successful History saves; canceled, failed, and pending rows do not count toward metrics.
- Added a WinUI `Metrics` sidebar section after History showing dashboard totals, metrics database path, transcription model performance, and enhancement model performance.

Metrics Filters and Export slice completed on 2026-05-25:

- Added macOS-aligned filter choices for Last 7 Days, Last 30 Days, This Year, and All Time, with Last 7 Days as the default Metrics filter.
- Filtered dashboard summaries, transcription model performance, and enhancement model performance by the selected time range.
- Added non-destructive local Metrics CSV export through a Windows save picker.

Metrics Reset slice completed on 2026-05-25:

- Added a confirmed local-only Metrics reset command in the Metrics section.
- Added `ISessionMetricStore.ClearAsync` with SQLite `session_metrics` row deletion and no-op disabled-store behavior.
- Reset deletes only local metrics rows; History, recordings, models, settings, and diagnostics remain unchanged.

Windows gaps:

- Rich macOS visual dashboard cards and slide-over model performance panel styling.
- Diagnostics copy/export expansion beyond the existing About/Open Source local diagnostics actions.

### Settings

macOS settings cover shortcuts, middle-click recording, sound feedback, mute/pause media, clipboard restore delay, paste method, recorder style, Power Mode behavior, privacy cleanup, backup import/export, diagnostics, launch at login, onboarding reset, and update checks.

Settings backup Windows MVP target:

- Add local-only JSON export from the Settings section using the same user language as macOS: `Export Settings` and `Import Settings`.
- Export one backup containing the current Windows settings, custom enhancement prompts, Power Mode rules, imported model references, and dictionary vocabulary/replacements.
- Let import choose the same categories as macOS where Windows has equivalent data: General Settings, Custom Prompts, Power Mode, Dictionary, and Custom Model Definitions.
- Merge dictionary imports non-destructively using the existing duplicate-aware dictionary import behavior.
- Apply imported settings to the current UI state after import so later saves do not overwrite imported values with stale controls.
- Validate and register imported global shortcuts before saving them; if registration fails, keep the previous shortcuts and settings active.
- Never export Credential Manager API keys or any other secret material. Clear credential-bearing provider endpoints from backups rather than serializing embedded credentials or key/token query parameters. After importing provider settings or prompts, show a reminder that API keys must be reconfigured locally.
- Keep history and metrics exports separate CSV workflows rather than adding them to settings backup, because they are user data rather than app configuration.

Settings backup slice completed on 2026-05-25:

- Added a Core backup schema that exports general Windows settings, custom prompts, Power Mode rules, imported local model references, vocabulary words, and word replacements to one local JSON file.
- Added category merge behavior for General Settings, Custom Prompts, Power Mode, Dictionary, and Custom Model Definitions.
- Added Settings section `Export Settings` and `Import Settings` buttons using Windows App SDK file pickers and a category-selection import dialog.
- Import validates and registers imported global shortcuts before saving settings, and restores prior registrations if saving fails.
- Backup files include a secret-exclusion notice and do not export Credential Manager API keys, tokens, credentials, or other secret material. Provider endpoint fields with embedded credentials or key/token query parameters are cleared during export.
- History and metrics remain separate local CSV exports rather than part of the settings backup.

Settings privacy/onboarding Windows target:

- Add `Reset Onboarding` in Settings, matching macOS by setting the local onboarding completion flag back to false after a confirmation. The first-run setup should appear again on the next launch.
- Add macOS-aligned privacy cleanup settings: `Auto-delete Transcripts`, `Delete After` with immediately/1 hour/1 day/3 days/7 days, `Auto-delete Audio Files`, and `Keep Audio For` with 1/3/7/14/30 days.
- Persist cleanup settings in JSON with macOS defaults: transcript cleanup off, transcript retention 1440 minutes, audio cleanup off, audio retention 7 days.
- Run transcript cleanup by deleting history rows older than the configured retention and deleting their associated audio files when present.
- Run audio cleanup by deleting only old audio files and clearing their history audio-file references while preserving transcript text.
- Hide or disable standalone audio cleanup while transcript cleanup is enabled, because deleting transcripts also removes their audio files and macOS treats the two modes as mutually exclusive.
- Keep cleanup local-only and non-commercial. Do not upload audio, transcripts, metrics, or diagnostics.

Settings privacy/onboarding slice completed on 2026-05-25:

- Added JSON settings for macOS-style transcript/audio cleanup defaults and retention choices.
- Added a Core cleanup service that computes UTC cutoffs, deletes app-owned audio files only under the recordings directory, deletes old transcript rows for transcript cleanup, and clears audio references while preserving transcript text for audio-only cleanup.
- Hardened cleanup so it only deletes direct app-created recording WAV files, rejects reparse-point paths, and keeps transcript rows when app-owned audio deletion fails so later cleanup can retry.
- Added SQLite history queries for cutoff-based cleanup and audio-reference clearing.
- Added WinUI Settings controls for Privacy cleanup and a confirmed `Reset Onboarding` action that shows first-run setup again on the next launch.
- Automatic cleanup now runs on launch, after completed recordings, and on a daily in-app timer while the relevant setting is enabled.

Settings recording feedback Windows target:

- Add a Settings `Recording Feedback` group matching the macOS settings language for `Sound Feedback`, `Mute Audio While Recording`, `Pause Media While Recording`, and `Resume Delay`.
- Persist the macOS defaults in Windows JSON settings and General Settings backup/import: sound feedback enabled, mute audio while recording enabled, pause media while recording disabled, and audio resumption delay `0.0` seconds.
- Play local start/stop feedback sounds only when `Sound Feedback` is enabled. Use Windows system sounds as the built-in default, with a validated custom sound picker/import workflow for user-selected start and stop sounds.
- When `Mute Audio While Recording` is enabled, mute the default Windows render endpoint for the recording session and restore it after the configured resume delay only if VoiceInk muted it.
- When `Pause Media While Recording` is enabled, use Windows Global System Media Transport Controls as an opt-in local equivalent. Pause only when Windows reports a current playing session and resume only that same session after the configured delay. Keep it disabled by default because Windows media session support varies by app.
- Keep all recording feedback local-only. Do not persist or export media titles, app names, volume levels, or audio session metadata.

Settings recording feedback slice completed on 2026-05-25:

- Added `IsSoundFeedbackEnabled`, `IsSystemMuteEnabled`, `IsPauseMediaEnabled`, and `AudioResumptionDelaySeconds` to Windows JSON settings and General Settings backup/import.
- Added a Core recording feedback coordinator that snapshots settings at recording start, restores system/media feedback immediately after capture stops, and also restores on cancel, start failure, and window close.
- Added Settings `Recording Feedback` controls for `Sound Feedback`, `Mute Audio While Recording`, `Pause Media While Recording`, and `Resume Delay`.
- Added native Windows adapters for start/stop system sounds, default render endpoint mute/restore through Core Audio, and opt-in Global System Media Transport Controls pause/resume.
- Gated stop sound to successful text insertion, matching the macOS intent that stop feedback follows completed paste rather than mere capture stop.
- Documented that Windows system sounds remain the built-in default until source-controlled open-source sound assets are added.

Settings custom recording sounds slice completed on 2026-05-25:

- Added `StartSoundMode`, `StopSoundMode`, `CustomStartSoundPath`, and `CustomStopSoundPath` to Windows JSON settings and General Settings backup/import.
- Added Core sound settings projection so recording feedback uses the start-of-recording sound configuration snapshot.
- Added a validated custom sound importer that accepts `.wav`, `.mp3`, `.aiff`, and `.aif`, requires finite positive duration no longer than 3 seconds, copies sounds into `%LocalAppData%\VoiceInk.Windows\Sounds`, and replaces/resets app-owned custom files by sound kind.
- Added NAudio validation/playback for custom sounds with Windows system sound fallback.
- Added Settings `Start Sound` and `Stop Sound` controls with `System Default`, imported custom sound selection, `Test`, `Choose`, and `Reset` actions.
- Kept sound files local-only and excluded from diagnostics/secrets flows.

Settings clipboard/paste Windows target:

- Add Settings controls matching the macOS intent for `Keep Clipboard Content`, `Restore Delay`, and `Paste Method`.
- Persist clipboard restore settings with macOS defaults: restore enabled and a 2.0 second delay, clamped to at least 250ms when restoring.
- Keep the default paste method as clipboard-based paste using simulated Ctrl+V, because it works across rich text targets and preserves the existing Windows behavior.
- Add a Windows-native `Direct Text` paste method that sends Unicode text input directly without using the clipboard. Document it as a compatibility option, not a replacement for the default.
- When clipboard restore is enabled, restore only if the clipboard still contains VoiceInk's transient paste payload and session marker, so VoiceInk does not overwrite a user clipboard change made after insertion.
- Keep clipboard behavior local-only. Never upload clipboard contents or include clipboard snapshots in diagnostics/backups.

Settings clipboard/paste slice completed on 2026-05-25:

- Added JSON settings for clipboard restore delay and paste method with macOS-aligned defaults.
- Added Settings `Clipboard` controls for `Keep Clipboard Content`, `Restore Delay`, and `Paste Method`.
- Changed Windows text insertion to read settings at paste time instead of hardcoding clipboard restore behavior.
- Added session-marked clipboard restore so VoiceInk restores only when its own transient paste payload is still present.
- Added the Windows `Direct Text` paste method, which sends Unicode text input without touching the clipboard.

Settings launch-at-login Windows target:

- Add a Settings `General` toggle named exactly `Launch at Login`, matching the macOS Settings and menu-bar language.
- Persist launch-at-login as part of Windows `AppSettings` so local settings backup export/import includes it with General Settings.
- Because the current Windows app is unpackaged (`WindowsPackageType=None`), register startup through the current user's `HKCU\Software\Microsoft\Windows\CurrentVersion\Run` value rather than packaged `StartupTask`.
- Register a quoted executable path plus a `--voiceink-startup` argument. The argument lets source-run and future packaged builds start hidden to the tray instead of stealing focus at sign-in.
- If the stored Run entry points somewhere else, surface that as an unavailable or warning state rather than silently claiming VoiceInk is registered.
- On settings save or backup import, apply the OS startup registration and only persist the enabled flag after registration succeeds, so JSON settings and Windows startup state stay aligned.
- Keep this open-source and local-only: no updater, account, telemetry, or commercial startup channel.

Settings launch-at-login slice completed on 2026-05-25:

- Added `LaunchAtLogin` to Windows JSON settings and General Settings backup/import.
- Added Settings `General` control named `Launch at Login`.
- Added a native HKCU Run-key registration service for the current unpackaged source build.
- Registered startup commands use a quoted executable path and `--voiceink-startup`.
- Login-started launches hide the main shell to the tray after initialization, preserving tray and shortcut availability without taking focus.

Windows gaps:

- Settings surfaces beyond supported shortcut, cleanup, backup, provider, audio, onboarding, and clipboard controls.
- Windows equivalents for tray behavior and richer diagnostics.

Settings diagnostics Windows target:

- Match macOS `Export Logs` intent with an open-source, local-only Windows diagnostic log export.
- Keep existing `Open Diagnostics Folder` and `Copy Diagnostics Summary` actions, and add `Export Diagnostic Logs` in About / Open Source.
- Export a plain UTF-8 `.log` file chosen by the user through a Windows save picker.
- Include safe system and app context: export time, app version, OS/runtime, architecture, app data paths, known local database/settings file existence and sizes, active section, dictation state, selected model path, and recent generic in-app status labels.
- Never include API keys, Credential Manager values, environment variables, clipboard contents, transcript/history text, rendered AI prompt content, user-authored dictionary/prompt text, or settings JSON contents.
- Redact key/token/credential/secret-looking query parameters from any diagnostic line that could include a URL-like or leading key-value value.
- Keep diagnostics useful for open-source issue reporting rather than commercial support or telemetry.

Settings diagnostics slice completed on 2026-05-25:

- Added Core diagnostic report formatting with safe system/app context, file inventory, state, recent generic event-label, and privacy notice sections.
- Added redaction for key/token/secret/credential query values and leading key-value diagnostic lines.
- Added an in-memory recent status event list capped at 200 generic labels, with user-authored status payloads stripped before recording.
- Added About / Open Source `Export Diagnostic Logs` using a user-selected `.log` save picker.
- Reused the safe diagnostic report for `Copy Diagnostics Summary` so copy/export share privacy boundaries.

### Audio Input

macOS audio input supports `System Default`, `Custom Device`, and `Prioritized` modes, refresh, active/unavailable states, priority ordering, and fallback behavior.

Implemented:

- Device listing and refresh through NAudio `WaveIn`.
- System Default mode.
- Custom device selection persisted in JSON settings and applied before recording.
- Prioritized mode with ordered microphone names, next-available fallback, and System Default fallback when no priority entries are available.
- Audio Input page priority list controls for adding, removing, and reordering microphones.
- Missing saved device fallback to System Default with a status warning.
- Live Core Audio endpoint-change refresh while idle, with deferred refresh during recording/processing.

Windows gaps:

- Endpoint-ID backed priority matching if the capture backend moves from WaveIn numbering to MMDevice/WASAPI.
- Richer active/unavailable device health badges.

### Onboarding

macOS first-run onboarding covers introduction, microphone/device/accessibility/screen/shortcut permissions, model download, and tutorial.

Implemented:

- First-run setup dialog after settings and audio devices load.
- Local JSON `HasCompletedOnboarding` flag.
- Local whisper model path selection with `.bin` picker.
- Windows microphone privacy settings link and audio input selection.
- Primary shortcut setup and basic try-it instructions.

Windows gaps:

- Model catalog/download cards and imported-model reuse inside onboarding.
- Permission health checks beyond opening Windows microphone settings.
- Reset-onboarding setting.

Onboarding slice completed on 2026-05-24:

- Added a Windows first-run setup dialog after settings and audio devices load.
- Guided local whisper model path selection, microphone privacy/settings, audio input selection, primary shortcut, and a short try-it path.
- Persisted onboarding completion locally in JSON settings.
- Kept direct model download/import cards, deeper permission health checks, and reset-onboarding settings as later model-management/settings work.

### Packaging

Windows needs open-source friendly packaging.

Dev ZIP packaging slice completed on 2026-05-25:

- Added a repo-local PowerShell packaging script for an unpackaged developer ZIP.
- The script publishes the WinUI app for `win-x64` as a self-contained .NET output with `WindowsPackageType=None`, `WindowsAppSDKSelfContained=true`, and `PublishSingleFile=false`.
- Generated package contents are staged under ignored `VoiceInk.Windows/artifacts/dev-zip` output, with a README next to `VoiceInk.Windows.App.exe`.
- The dev ZIP is explicitly a source-built testing distribution. It does not register package identity, Start Menu shortcuts, uninstall entries, signing, auto-update behavior, or commercial channels.

Windows gaps:

- MSIX or installer project.
- Installer/native dependency layout validation.
- Uninstall behavior.
- Shortcut registration.
- Signing/release smoke tests.

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
- Completed shell add/remove/sort controls for vocabulary and add/edit/remove/enable/disable/sort controls for word replacements.
- Completed dictionary JSON import/export for vocabulary words and word replacements.
- Completed vocabulary prompt pass-through to local Whisper transcription.
- Completed cleanup options and processing, including macOS-style punctuation cleanup strings in JSON settings.
- Completed richer SQLite history metadata and MVP schema migration.
- Completed history audio file path persistence and core CSV formatting for history export.
- Completed shell recent-history list/detail and local CSV export.
- Completed picker-based CSV export location selection.
- Completed paste-last final and enhanced-preferred primitives with shell buttons.
- Completed selected-row history audio playback/open, selected-row retry through the local transcription pipeline, retry-last-to-clipboard through the local transcription pipeline, active-recording cancel history, history search, and confirmed single-item delete.
- Completed configurable key+modifier global shortcuts for primary and secondary recording toggle, paste last, paste last enhanced, retry last transcription, cancel active recording, open history, and quick add to dictionary.
- Completed Open History Window shortcut as a Windows MVP adaptation that restores/focuses the main shell and inline History area until a dedicated history window exists.
- Completed Quick Add to Dictionary as a Windows MVP dialog adaptation of the macOS floating quick-add panel, with Vocabulary and Word Replacement modes.
- Completed Windows audio input refresh and System Default/custom microphone selection.
- Completed native Windows tray shell with show/hide, recording toggle, Quick Add, History, Quit, and close-to-tray behavior.
- Completed WinUI sidebar navigation for Dashboard, History, AI Models, Audio Input, Dictionary, Settings, and About / Open Source.
- Completed commercial VoiceInk Pro replacement with neutral About / Open Source and local-only diagnostics actions.
- Completed first-run setup dialog for local model path, microphone settings/input, primary shortcut, and basic usage.
- Completed imported local Whisper `.bin` model references, shell default-model selection, `.bin` import picker, and open-source GGML model downloads link.
- Completed dictation pipeline wiring for cleanup settings and dictionary replacements.
- Completed basic shell controls for filler words, punctuation cleanup, lowercase output, and trailing-space settings.
- Remaining for this slice: dedicated Dictionary navigation page/richer layout, endpoint-ID backed audio input identity, waveform polish, and richer visual parity.
- Add focused tests and docs.

## Verification

For every slice:

- Run the smallest targeted test first.
- Run the full Windows test suite after the slice.
- Run a Debug x64 build when app or project wiring changes.
- Document manual smoke status when a feature requires microphone/model/API keys/UI.
