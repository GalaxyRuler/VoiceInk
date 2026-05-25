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
- Compact always-on-top floating mini-recorder during recording and processing, with status text, elapsed timer, pulse animation, and Prompt/Power Mode affordance labels.
- Transcribe Audio navigation section with multi-file picker, in-memory queue, Media Foundation import to app-owned WAV recordings, local Whisper transcription, text cleanup, and History save.
- Default-off AI Enhancement section with prompt catalog, OpenAI-compatible endpoint/model settings, Windows Credential Manager API key storage, output filtering, retry/timeout controls, automatic read-only selected text context, optional read-only clipboard context, original-text fallback, and successful enhancement insertion.
- Power Mode navigation section with ordered enabled/default process/title rules, Win32 active-window quick fill, session-only model/language/enhancement/prompt/cleanup overrides, and History name/emoji metadata.
- First-run setup dialog for local model path, microphone settings/input, primary shortcut, and basic usage.
- Imported local Whisper `.bin` model references with shell selection for the default model path.
- Configurable global key+modifier shortcuts for primary and secondary recording toggle, paste last, paste last enhanced, retry last transcription, cancel recording, open history, and quick add to dictionary.
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
- Keeps stop on global shortcuts, tray commands, and main-window controls, and keeps cancel on global shortcuts and main-window controls in this slice so the floating recorder cannot steal focus from the dictation target before paste insertion.
- Uses a Core `FloatingRecorderPresenter` for state-to-UI mapping.
- Leaves live partial transcript, real audio meter waveform, notch style, prompt picker behavior, and Power Mode behavior for later slices.

Windows gaps:

- Waveform/level visualization.
- Live partial transcript.
- Non-activating mouse controls for stop/cancel that preserve the paste target.
- Prompt and Power Mode controls in recorder.

### Shortcuts

macOS supports primary and secondary recording shortcuts, toggle/push-to-talk/hybrid modes, paste last original, paste last enhanced, retry last transcription, cancel recording, open history, quick add to dictionary, toggle enhancement, per-Power Mode shortcuts, and mini-recorder numeric shortcuts.

Implemented:

- Configurable key+modifier shortcuts for primary and secondary recording toggle, paste last transcription, paste last enhanced transcription, retry last transcription, cancel active recording, open history, and quick add to dictionary.
- Validation for unsupported keys, Windows-key reservations, missing modifiers, and duplicate assignments.

Windows gaps:

- Press-and-hold key-up handling.
- Toggle-enhancement and Power Mode shortcuts.
- Canceling in-flight transcription/enhancement after recording has already stopped.
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

Implemented core:

- Hallucination marker cleanup, filler-word filtering, whitespace normalization, word replacements, punctuation cleanup, lowercase output, and trailing-space handling.
- History model/storage/export support for completed, failed, and canceled statuses.

Windows gaps:

- macOS-style formatting pass beyond cleanup preferences.
- Prompt trigger detection.
- Prompt-triggered toggle-enhancement shortcut and canceling in-flight enhancement after recording has already stopped.
- Metrics persistence and views.
- Dictation-controller history writes for failed recording/transcription sessions and canceled in-flight transcription/enhancement sessions.

### Model Management

macOS has local Whisper cards, imported Whisper models, Parakeet/FluidAudio cards, native Apple model, cloud model cards, language selection, custom cloud models, API key management, model download/import, default model selection, and prewarm on wake. The macOS local model flow scans model files, imports Whisper ggml `.bin` models, displays imported model cards, and lets the user set a default transcription model.

Local-model-library slice completed on 2026-05-24:

- Persist imported local Whisper `.bin` model references in Windows JSON settings.
- Add an imported-model selector to the Windows shell and set the selected model as the default local model path.
- Add an import picker for `.bin` files and a link to open the open-source whisper.cpp GGML model downloads page.
- Keep model catalog cards, direct in-app download, language capability UI, warmup/preload, and cloud provider cards as later model-management work.

Windows gaps:

- Model catalog and cards beyond imported local model references.
- Direct model download flow.
- Language picker bound to model capabilities.
- Rich default model management views beyond the imported-model selector and raw path field.
- Warmup/preload.
- Provider cards and secure API key storage.

Windows adaptation:

- Skip Native Apple transcription.
- Treat Parakeet/FluidAudio as optional only if a compatible Windows runtime is practical.

### Transcribe Audio

macOS has a dedicated `Transcribe Audio` workflow for queued audio/video files. It supports drag/drop or file choosing, pending/processing/completed/failed states, start/cancel/clear controls, retry, copy/save, and optional AI enhancement.

Transcribe Audio slice completed on 2026-05-25:

- Added a Windows `Transcribe Audio` navigation section after Dashboard.
- Added multi-file picker import using Windows App SDK picker APIs and supported audio/video extensions backed by Windows Media Foundation where codecs are available.
- Added a source-runnable in-memory queue with pending, processing, completed, failed, remove, retry, clear, start, and cancel behavior.
- Convert/import selected media into app-owned WAV recordings before transcription, then run the existing local Whisper transcription path with dictionary prompt biasing and text cleanup.
- Save completed file transcriptions into the existing SQLite history with original/final text, provider/model/language metadata, transcription duration, audio duration, and app-owned audio file path.
- Leave drag/drop, per-file save/copy buttons, persistent queue restoration, and richer batch actions for later slices.

Windows gaps:

- Drag/drop import.
- Per-file save/copy controls in the queue.
- Persistent queue restoration across launches.
- Per-file enhancement controls independent of the global Enhancement setting.

### Cloud Transcription

macOS provider catalog includes Groq, ElevenLabs, Deepgram, Mistral, Gemini, Soniox, Speechmatics, AssemblyAI, xAI, Cartesia, and custom OpenAI-compatible models. Some providers support streaming only.

Cloud Transcription Windows MVP target:

- Add a default-off `OpenAI-compatible` transcription provider option alongside local Whisper.
- Keep the provider user-configured: endpoint URL, model ID, and API key supplied by the user.
- Store the API key in Windows Credential Manager, not JSON settings.
- Send request-based multipart audio transcription requests with `file`, `model`, `response_format=json`, optional ISO language when not `auto`, and optional vocabulary/prompt context.
- Parse JSON responses with a `text` property and return clear sanitized errors without logging secrets or provider response bodies.
- Use the provider in dictation, Transcribe Audio, and history retry through the existing Core transcription interface.
- Keep named provider cards, streaming providers, and provider-specific payloads for later slices.

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

Windows gaps:

- Named provider cards beyond Groq.
- Streaming adapters.
- Provider-specific payloads.
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

Windows gaps:

- Prompt template persistence.
- Ollama and Local CLI hooks.
- Trigger detection.
- Clipboard-copy fallback for selected text, screen/OCR, browser URL, and app-specific context capture.
- Named provider cards and dynamic model lists.
- Toggle-enhancement shortcut and recorder prompt picker activation.
- AI re-enhance from History.

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
- Deliberately did not add clipboard-copy fallback for selected text in this slice, preserving clipboard contents.

Windows gaps:

- Clipboard-copy fallback for selected text when UI Automation does not expose selection.
- Active window title/process.
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
- Defer browser URL detection, auto-send keys, Power Mode global shortcuts, and recorder popover selection until the next Power Mode slice.

Power Mode slice completed on 2026-05-25:

- Added Core Power Mode rules, target matching, default fallback, and settings overlay behavior.
- Added native Windows active-window process/title detection behind a Core interface.
- Captured the matching rule at recording start and used its effective settings for transcription, cleanup, enhancement, insertion, and canceled-history metadata.
- Added Power Mode name/emoji to SQLite history, CSV export, and the History detail view.
- Added a WinUI Power Mode section for ordered rules, active-window quick fill, enabled/default toggles, and model/language/enhancement/prompt/cleanup overrides.

Windows Win32 grounding:

- `GetForegroundWindow` is the active-window primitive for the window the user is working with.
- `GetWindowThreadProcessId` maps that window to the owning process id.
- `GetWindowTextW` reads the title bar text when the window exposes one; empty titles must degrade gracefully.

Windows gaps:

- Browser URL matching.
- Auto-send keys.
- Power Mode shortcuts.
- Recorder popover integration.

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

Implemented:

- Device listing and refresh through NAudio `WaveIn`.
- System Default mode.
- Custom device selection persisted in JSON settings and applied before recording.
- Missing saved device fallback to System Default with a status warning.

Windows gaps:

- Prioritized fallback mode.
- Live device-change notifications.
- Rich active/unavailable badges and priority ordering UI.

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
- Remaining for this slice: dedicated Dictionary navigation page/richer layout, push-to-talk and hybrid shortcut modes, shortcut key-up handling, prioritized audio input failover, canceling in-flight transcription/enhancement, waveform/rate controls, AI re-enhance, and history batch actions.
- Add focused tests and docs.

## Verification

For every slice:

- Run the smallest targeted test first.
- Run the full Windows test suite after the slice.
- Run a Debug x64 build when app or project wiring changes.
- Document manual smoke status when a feature requires microphone/model/API keys/UI.
