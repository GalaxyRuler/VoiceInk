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
| Core dictation pipeline | 88% | `[##################--]` | Local recording/transcription/insertion, Transcribe Audio picker/drag-drop/copy/save/per-file-enhance/restored-queue flow, cleanup, dictionary, enhancement fallback, prompt-trigger detection, completed/canceled/failed history writes, metrics, and canceling in-flight post-recording work are in place. Advanced formatting remains. |
| Shell and tray | 86% | `[#################---]` | Navigation shell, macOS-order Permissions route, tray icon, close-to-tray, open-source About/diagnostics, dedicated History window routing, and rich tray quick-setting submenus for model/provider/enhancement/language/audio/context/Power Mode are in place. Remaining work is visual polish and rare tray edge-case recovery. |
| Floating recorder | 90% | `[##################--]` | Mini and top-center Notch styles show recording/processing state, elapsed time, live microphone level bars, non-activating Stop/Cancel controls, hover-dismissable no-activate Prompt/Power chooser panels, and a gated live transcript preview panel now backed by Deepgram, AssemblyAI, Soniox, Speechmatics, and Cartesia interim results. Waveform polish and more streaming providers remain. |
| Shortcuts | 97% | `[###################-]` | Primary/secondary recording shortcuts now support key-based and modifier-only Toggle, Push to Talk, and Hybrid key-up modes; paste last, paste enhanced, retry, cancel, open history, quick add, toggle enhancement, cycle Power Mode, direct per-rule Power Mode selection, floating-recorder Ctrl/Alt digit prompt/Power Mode slots, and read-only recorder fields with explicit Record buttons. Remaining work is rare Windows-reserved-key conflict recovery. |
| Model management | 76% | `[###############-----]` | Local Whisper catalog cards, direct GGML downloads, imported `.bin` references, app-local model storage, default model selection, model path health checks, stale imported model cleanup, model-aware language selection, cloud provider metadata cards, and nonblocking warmup/preload exist. Deeper model lifecycle polish remains. |
| Cloud transcription | 99% | `[####################]` | OpenAI-compatible adapter with endpoint-query `response_format` support, secure key storage, Custom/Groq/Deepgram/AssemblyAI/Mistral/ElevenLabs/Soniox/Speechmatics/Gemini/xAI/Cartesia presets with provider metadata cards, metadata-only provider test requests for every named preset, direct Deepgram batch requests with advanced endpoint query option preservation, AssemblyAI upload/transcript polling, Soniox async upload/transcription polling plus realtime preview, Speechmatics Jobs API batch transcription plus realtime preview, Gemini inline-audio and Files API generateContent transcription, xAI Grok STT batch transcription, Cartesia Ink Whisper batch transcription, Mistral Voxtral batch transcription through the multipart adapter, ElevenLabs Scribe batch transcription, Deepgram/AssemblyAI/Soniox/Speechmatics/Cartesia live preview streaming, and a user-owned-key smoke runbook exist. Remaining cloud work is advanced provider-specific options. |
| AI enhancement | 90% | `[##################--]` | OpenAI-compatible enhancement, Anthropic Messages API enhancement, native Ollama chat enhancement, Local CLI hooks, macOS-aligned Default/Assistant/Chat/Email/Rewrite prompts, assistant context wrapping, custom prompts with icon/description metadata, trigger-word activation, context, retries/timeouts, secure keys, toggle-enhancement shortcut, and dynamic OpenRouter/Ollama model refresh exist. Remaining work is visual polish and advanced assistant niceties. |
| Context features | 73% | `[###############-----]` | Clipboard context, selected-text context with clipboard fallback, active-window process/title context, sanitized browser URL context, default-off local screen OCR context, region-aware OCR capture plumbing, visible numeric OCR region controls, and a visual OCR region picker exist with graceful degradation. Multi-monitor picker refinement remains. |
| Power Mode | 85% | `[#################---]` | Rule model, process/title/browser URL matching, default fallback, explicit recorder chooser selection, cycle shortcut, direct per-rule shortcuts, settings overlays, history metadata, editor UI, inline validation feedback, and post-insertion auto-send keys exist. Deeper visual parity polish remains. |
| Dictionary | 79% | `[################----]` | Vocabulary, replacements, sorting, quick add, import/export, pipeline integration, macOS-style section descriptions, counts, empty states, and disabled replacement row presentation exist. Remaining work is richer card/flow layout polish. |
| History | 90% | `[##################--]` | Dedicated History window, SQLite detail metadata, search, retry, re-enhance from saved original text, copy actions for original/final/enhanced/AI request, paste, audio playback/open with waveform and rate controls, single and batch delete, single and selected CSV export, and privacy cleanup operations exist. Remaining work is deeper visual polish and analysis overlays. |
| Metrics | 76% | `[###############-----]` | SQLite metrics, macOS-style hero/card dashboard presentation, filters, model performance, CSV export, and confirmed reset controls exist. Deeper model-performance panel styling and diagnostics expansion remain. |
| Settings | 74% | `[###############-----]` | Shortcut/cleanup/provider/audio controls, recorder style selection, Permissions readiness routing, local settings backup import/export, privacy cleanup, reset onboarding, clipboard restore delay, paste method choices, launch at login, diagnostic log export, recording feedback controls, and custom start/stop sound import/reset/test exist. Visual polish remains. |
| Audio input | 88% | `[##################--]` | Device refresh, System Default/Custom/Prioritized modes, priority ordering with unavailable-device fallback, custom persistence, endpoint-ID backed identity, saved-name rebinding, unavailable fallback, startup capture rebinding, live Core Audio device-change refresh, persistent status notices for active/rebound/priority-fallback/unavailable/no-device states, and a Permissions-page microphone privacy link exist. Richer device health UI remains. |
| Onboarding | 70% | `[##############------]` | First-run setup covers model path, recommended local model download, microphone health, microphone settings, readiness checklist, in-dialog audio input refresh, audio input choice, shortcut, basic usage, Settings reset, and a post-onboarding Permissions page. Richer setup polish remains. |
| Packaging | 62% | `[############--------]` | Source-run docs, runtime workaround, repeatable self-contained dev ZIP packaging, dev ZIP smoke validation, per-user Dev ZIP install/uninstall helpers with Start Menu shortcut creation, signed MSIX manifest/script foundation, certificate-free packaging preflight, non-installing MSIX artifact validation, optional signed-build artifact validation, and a gated signed install/uninstall smoke helper exist. Actual signed MSIX install smoke and release signing flow remain gated on maintainer-owned signing/trust setup. |

## Current Slice

```text
Windows dictionary page visual parity  [####################] 100%
```

Completed:

- Added a Core dictionary page presenter for macOS-style hero/section labels, count labels, empty-state text, and replacement row display.
- Updated the Windows Dictionary page with hero description, Vocabulary and Word Replacements descriptions/counts, empty guidance, and disabled replacement status text.
- Added focused Core tests for non-empty and empty dictionary page presentation states.

## Near-Term Priority

1. Continue packaging from per-user Dev ZIP install to actual signed MSIX install/uninstall smoke and release signing flow when a trusted signing setup is available.
2. Continue visual parity polish for Power Mode, Settings, Metrics, Dictionary, Enhancement, and onboarding pages.
3. Continue model lifecycle polish for local model discovery, validation, and repair flows.
4. Consider a future WASAPI capture backend if endpoint-native recording becomes necessary.
