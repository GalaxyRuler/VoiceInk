# VoiceInk Windows Project Completion

Last updated: 2026-05-26

This tracker is an approximate parity bar for the full free/open-source Windows fork, grounded in the current parity spec and implemented Windows slices. Percentages represent user-feature parity against the macOS app, not just buildability.

## Overall

```text
VoiceInk Windows parity  [###################-] 95%
```

## Area Bars

| Area | Completion | Bar | Status |
| --- | ---: | --- | --- |
| Core dictation pipeline | 84% | `[#################---]` | Local recording/transcription/insertion, cleanup, dictionary, enhancement fallback, prompt-trigger detection, completed/canceled/failed history writes, metrics, and canceling in-flight post-recording work are in place. Advanced formatting remains. |
| Shell and tray | 78% | `[################----]` | Navigation shell, macOS-order Permissions route, tray icon, close-to-tray, open-source About/diagnostics, and dedicated History window routing are in place. Rich tray submenus remain. |
| Floating recorder | 90% | `[##################--]` | Mini and top-center Notch styles show recording/processing state, elapsed time, live microphone level bars, non-activating Stop/Cancel controls, hover-dismissable no-activate Prompt/Power chooser panels, and a gated live transcript preview panel now backed by Deepgram, AssemblyAI, Soniox, Speechmatics, and Cartesia interim results. Waveform polish and more streaming providers remain. |
| Shortcuts | 91% | `[##################--]` | Primary/secondary recording shortcuts now support Toggle, Push to Talk, and Hybrid key-up modes; paste last, paste enhanced, retry, cancel, open history, quick add, toggle enhancement, cycle Power Mode, direct per-rule Power Mode selection, and floating-recorder Ctrl/Alt digit prompt/Power Mode slots are supported. Modifier-only shortcuts remain. |
| Model management | 72% | `[##############------]` | Local Whisper catalog cards, direct GGML downloads, imported `.bin` references, app-local model storage, default model selection, model-aware language selection, cloud provider metadata cards, and nonblocking warmup/preload exist. Deeper model lifecycle polish remains. |
| Cloud transcription | 98% | `[####################]` | OpenAI-compatible adapter, secure key storage, Custom/Groq/Deepgram/AssemblyAI/Mistral/ElevenLabs/Soniox/Speechmatics/Gemini/xAI/Cartesia presets with provider metadata cards, metadata-only provider test requests for every named preset, direct Deepgram batch requests, AssemblyAI upload/transcript polling, Soniox async upload/transcription polling plus realtime preview, Speechmatics Jobs API batch transcription plus realtime preview, Gemini inline-audio and Files API generateContent transcription, xAI Grok STT batch transcription, Cartesia Ink Whisper batch transcription, Mistral Voxtral batch transcription through the multipart adapter, ElevenLabs Scribe batch transcription, and Deepgram, AssemblyAI, Soniox, Speechmatics, plus Cartesia live preview streaming exist. Remaining cloud work is advanced provider-specific options and live credential smoke documentation. |
| AI enhancement | 73% | `[###############-----]` | OpenAI-compatible enhancement, presets, custom prompts, trigger-word activation, context, retries/timeouts, secure keys, and a toggle-enhancement shortcut exist. Local/Ollama-style hooks and richer assistant workflows remain. |
| Context features | 73% | `[###############-----]` | Clipboard context, selected-text context with clipboard fallback, active-window process/title context, sanitized browser URL context, default-off local screen OCR context, region-aware OCR capture plumbing, visible numeric OCR region controls, and a visual OCR region picker exist with graceful degradation. Multi-monitor picker refinement remains. |
| Power Mode | 85% | `[#################---]` | Rule model, process/title/browser URL matching, default fallback, explicit recorder chooser selection, cycle shortcut, direct per-rule shortcuts, settings overlays, history metadata, editor UI, inline validation feedback, and post-insertion auto-send keys exist. Deeper visual parity polish remains. |
| Dictionary | 75% | `[###############-----]` | Vocabulary, replacements, sorting, quick add, import/export, and pipeline integration exist. Rich macOS-style page polish remains. |
| History | 90% | `[##################--]` | Dedicated History window, SQLite detail metadata, search, retry, re-enhance from saved original text, copy actions for original/final/enhanced/AI request, paste, audio playback/open with waveform and rate controls, single and batch delete, single and selected CSV export, and privacy cleanup operations exist. Remaining work is deeper visual polish and analysis overlays. |
| Metrics | 72% | `[##############------]` | SQLite metrics, dashboard summary, filters, model performance, CSV export, and confirmed reset controls exist. Visual parity and diagnostics expansion remain. |
| Settings | 74% | `[###############-----]` | Shortcut/cleanup/provider/audio controls, recorder style selection, Permissions readiness routing, local settings backup import/export, privacy cleanup, reset onboarding, clipboard restore delay, paste method choices, launch at login, diagnostic log export, recording feedback controls, and custom start/stop sound import/reset/test exist. Visual polish remains. |
| Audio input | 82% | `[################----]` | Device refresh, System Default/Custom/Prioritized modes, priority ordering with unavailable-device fallback, custom persistence, saved-name rebinding, unavailable fallback, startup capture rebinding, live Core Audio device-change refresh, persistent status notices for active/rebound/priority-fallback/unavailable/no-device states, and a Permissions-page microphone privacy link exist. Richer device health UI and endpoint-ID backed priority remain. |
| Onboarding | 70% | `[##############------]` | First-run setup covers model path, recommended local model download, microphone health, microphone settings, readiness checklist, in-dialog audio input refresh, audio input choice, shortcut, basic usage, Settings reset, and a post-onboarding Permissions page. Richer setup polish remains. |
| Packaging | 62% | `[############--------]` | Source-run docs, runtime workaround, repeatable self-contained dev ZIP packaging, dev ZIP smoke validation, per-user Dev ZIP install/uninstall helpers with Start Menu shortcut creation, signed MSIX manifest/script foundation, certificate-free packaging preflight, non-installing MSIX artifact validation, optional signed-build artifact validation, and a gated signed install/uninstall smoke helper exist. Actual signed MSIX install smoke and release signing flow remain gated on maintainer-owned signing/trust setup. |

## Current Slice

```text
Windows audio input priority mode  [####################] 100%
```

Completed:

- Added Core System Default/Custom/Prioritized audio input mode settings.
- Added priority-list add/remove/reorder helpers and tests.
- Added prioritized microphone selection with next-available and system-default fallback notices.
- Added WinUI Audio Input mode and priority controls.
- Persisted audio input mode/priority settings through JSON settings and backup.

## Near-Term Priority

1. Continue packaging from per-user Dev ZIP install to actual signed MSIX install/uninstall smoke and release signing flow when a trusted signing setup is available.
2. Add modifier-only shortcut compatibility where Windows can support it safely.
3. Add endpoint-ID backed audio input identity if/when the capture backend moves from WaveIn numbering to MMDevice/WASAPI.
4. Add richer cloud-provider live smoke documentation for optional user-owned API keys and models.
5. Continue visual parity polish for Power Mode, Settings, Metrics, Dictionary, and onboarding pages.
