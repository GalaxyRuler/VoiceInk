# VoiceInk Windows Project Completion

Last updated: 2026-05-26

This tracker is an approximate parity bar for the full free/open-source Windows fork, grounded in the current parity spec and implemented Windows slices. Percentages represent user-feature parity against the macOS app, not just buildability.

## Overall

```text
VoiceInk Windows parity  [##################--] 89%
```

## Area Bars

| Area | Completion | Bar | Status |
| --- | ---: | --- | --- |
| Core dictation pipeline | 84% | `[#################---]` | Local recording/transcription/insertion, cleanup, dictionary, enhancement fallback, prompt-trigger detection, completed/canceled/failed history writes, metrics, and canceling in-flight post-recording work are in place. Advanced formatting remains. |
| Shell and tray | 76% | `[###############-----]` | Navigation shell, tray icon, close-to-tray, open-source About/diagnostics, and dedicated History window routing are in place. Rich tray submenus remain. |
| Floating recorder | 87% | `[#################---]` | Mini and top-center Notch styles show recording/processing state, elapsed time, live microphone level bars, non-activating Stop/Cancel controls, hover-dismissable no-activate Prompt/Power chooser panels, and a gated live transcript preview panel now backed by Deepgram, AssemblyAI, and Cartesia interim results. Waveform polish and more streaming providers remain. |
| Shortcuts | 72% | `[##############------]` | Primary/secondary toggle, paste last, paste enhanced, retry, cancel, open history, quick add, toggle enhancement, and cycle Power Mode are configurable with typed or captured shortcut entry. Press-and-hold modes and per-rule Power Mode shortcuts remain. |
| Model management | 72% | `[##############------]` | Local Whisper catalog cards, direct GGML downloads, imported `.bin` references, app-local model storage, default model selection, model-aware language selection, cloud provider metadata cards, and nonblocking warmup/preload exist. Deeper model lifecycle polish remains. |
| Cloud transcription | 93% | `[###################-]` | OpenAI-compatible adapter, secure key storage, Custom/Groq/Deepgram/AssemblyAI/Mistral/ElevenLabs/Soniox/Speechmatics/Gemini/xAI/Cartesia presets with provider metadata cards, metadata-only provider test requests for Custom/Groq/Mistral/xAI/Deepgram/AssemblyAI/ElevenLabs/Soniox/Gemini, direct Deepgram batch requests, AssemblyAI upload/transcript polling, Soniox async upload/transcription polling, Speechmatics Jobs API batch transcription, Gemini inline-audio generateContent transcription, xAI Grok STT batch transcription, Cartesia Ink Whisper batch transcription, Mistral Voxtral batch transcription through the multipart adapter, ElevenLabs Scribe batch transcription, and Deepgram, AssemblyAI, plus Cartesia live preview streaming exist. Speechmatics/Cartesia safe probe endpoints, Files API upload for long Gemini recordings, and more streaming providers remain. |
| AI enhancement | 73% | `[###############-----]` | OpenAI-compatible enhancement, presets, custom prompts, trigger-word activation, context, retries/timeouts, secure keys, and a toggle-enhancement shortcut exist. Local/Ollama-style hooks and richer assistant workflows remain. |
| Context features | 73% | `[###############-----]` | Clipboard context, selected-text context with clipboard fallback, active-window process/title context, sanitized browser URL context, default-off local screen OCR context, region-aware OCR capture plumbing, visible numeric OCR region controls, and a visual OCR region picker exist with graceful degradation. Multi-monitor picker refinement remains. |
| Power Mode | 71% | `[##############------]` | Rule model, process/title/browser URL matching, default fallback, explicit recorder chooser selection, cycle shortcut, settings overlays, history metadata, and editor UI exist. Auto-send keys and per-rule shortcuts remain. |
| Dictionary | 75% | `[###############-----]` | Vocabulary, replacements, sorting, quick add, import/export, and pipeline integration exist. Rich macOS-style page polish remains. |
| History | 90% | `[##################--]` | Dedicated History window, SQLite detail metadata, search, retry, re-enhance from saved original text, copy actions for original/final/enhanced/AI request, paste, audio playback/open with waveform and rate controls, single and batch delete, single and selected CSV export, and privacy cleanup operations exist. Remaining work is deeper visual polish and analysis overlays. |
| Metrics | 72% | `[##############------]` | SQLite metrics, dashboard summary, filters, model performance, CSV export, and confirmed reset controls exist. Visual parity and diagnostics expansion remain. |
| Settings | 73% | `[###############-----]` | Shortcut/cleanup/provider/audio controls, recorder style selection, local settings backup import/export, privacy cleanup, reset onboarding, clipboard restore delay, paste method choices, launch at login, diagnostic log export, recording feedback controls, and custom start/stop sound import/reset/test exist. Visual polish remains. |
| Audio input | 72% | `[##############------]` | Device refresh, system default/custom persistence, saved-name rebinding, unavailable fallback, startup capture rebinding, live Core Audio device-change refresh, and persistent status notices for active/rebound/unavailable/no-device states exist. Richer device health and permission UI remains. |
| Onboarding | 68% | `[##############------]` | First-run setup covers model path, recommended local model download, microphone health, microphone settings, readiness checklist, in-dialog audio input refresh, audio input choice, shortcut, basic usage, and Settings reset. Richer setup polish remains. |
| Packaging | 52% | `[##########----------]` | Source-run docs, runtime workaround, repeatable self-contained dev ZIP packaging, dev ZIP smoke validation, signed MSIX manifest/script foundation, certificate-free packaging preflight, non-installing MSIX artifact validation, optional signed-build artifact validation, and a gated signed install/uninstall smoke helper exist. Actual signed MSIX install smoke, shortcut registration, and release signing flow remain. |

## Current Slice

```text
Windows shortcut recorder fields  [####################] 100%
```

Completed:

- Added `GlobalShortcut.TryCreateFromKeyCapture` with normalized display formatting.
- Wired shortcut settings text boxes to capture modifier+key combinations through `KeyDown`.
- Added bare Escape clearing and modifier-only guidance.
- Preserved typed shortcut entry and existing global registration validation.

## Near-Term Priority

1. Add remaining grounded Speechmatics and Cartesia provider test probes if official safe metadata endpoints are available.
2. Continue packaging from signed-build artifact validation to actual signed MSIX install/uninstall smoke, shortcut registration, and release signing flow.
3. Expand streaming/live preview beyond Deepgram and continue recorder waveform visual polish.
4. Continue richer onboarding setup polish and any deeper microphone permission probes that can be implemented without mutating Windows settings.
