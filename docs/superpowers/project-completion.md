# VoiceInk Windows Project Completion

Last updated: 2026-05-27

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
| Model management | 87% | `[#################---]` | Local Whisper catalog cards, direct GGML downloads, imported `.bin` references, app-local model storage, default model selection, local library overview, action rows for download/import/default/repair/warmup, storage/import/backup/warmup guidance rows, model path health checks with actionable repair guidance, selected-model health rows, a direct repair/warmup action button, stale imported model cleanup, model-aware language selection, cloud provider metadata cards, and nonblocking warmup/preload exist. Deeper model lifecycle polish remains. |
| Cloud transcription | 99% | `[####################]` | OpenAI-compatible adapter with endpoint-query `response_format` support, secure key storage, Custom/Groq/Deepgram/AssemblyAI/Mistral/ElevenLabs/Soniox/Speechmatics/Gemini/xAI/Cartesia presets with provider metadata cards, metadata-only provider test requests for every named preset, direct Deepgram batch requests with advanced endpoint query option preservation, AssemblyAI upload/transcript polling, Soniox async upload/transcription polling plus realtime preview, Speechmatics Jobs API batch transcription plus realtime preview, Gemini inline-audio and Files API generateContent transcription, xAI Grok STT batch transcription, Cartesia Ink Whisper batch transcription, Mistral Voxtral batch transcription through the multipart adapter, ElevenLabs Scribe batch transcription, Deepgram/AssemblyAI/Soniox/Speechmatics/Cartesia live preview streaming, and a user-owned-key smoke runbook exist. Remaining cloud work is advanced provider-specific options. |
| AI enhancement | 90% | `[##################--]` | OpenAI-compatible enhancement, Anthropic Messages API enhancement, native Ollama chat enhancement, Local CLI hooks, macOS-aligned Default/Assistant/Chat/Email/Rewrite prompts, assistant context wrapping, custom prompts with icon/description metadata, trigger-word activation, context, retries/timeouts, secure keys, toggle-enhancement shortcut, and dynamic OpenRouter/Ollama model refresh exist. Remaining work is visual polish and advanced assistant niceties. |
| Context features | 83% | `[#################---]` | Clipboard context, selected-text context with clipboard fallback, active-window process/title context, sanitized browser URL context, default-off local screen OCR context, region-aware OCR capture plumbing, visible numeric OCR region controls, a visual OCR region picker, display-targeted OCR region selection, Enhancement context readiness summary rows, provider-aware context privacy boundary rows, and context action guidance rows exist with graceful degradation. Deeper context visual polish remains. |
| Power Mode | 90% | `[##################--]` | Rule model, process/title/browser URL matching, default fallback, explicit recorder chooser selection, cycle shortcut, direct per-rule shortcuts, settings overlays, history metadata, editor UI, inline validation feedback, post-insertion auto-send keys, macOS-style page/empty-state copy, and rich rule rows for targets/overrides/shortcuts/status exist. Deeper visual layout polish remains. |
| Dictionary | 89% | `[##################--]` | Vocabulary, replacements, sorting, quick add, import/export, pipeline integration, macOS-style section descriptions, counts, summary rows, rule application guidance rows, empty states, local overview guidance, disabled replacement row presentation, and richer vocabulary/replacement row details with status badges exist. Remaining work is deeper flow/layout polish. |
| History | 92% | `[##################--]` | Dedicated History window, SQLite detail metadata, search, retry, re-enhance from saved original text, copy actions for original/final/enhanced/AI request, paste, audio playback/open with waveform and rate controls, selected-item local analysis rows, single and batch delete, single and selected CSV export, and privacy cleanup operations exist. Remaining work is deeper visual polish. |
| Metrics | 88% | `[##################--]` | SQLite metrics, macOS-style hero/card dashboard presentation, filters, local action and diagnostics summary rows, metric data/formula guidance rows, presenter-backed model performance guidance, templated model performance rows with primary values/status badges, CSV export, and confirmed reset controls exist. Deeper visual diagnostics polish remains. |
| Settings | 87% | `[#################---]` | Shortcut/cleanup/provider/audio controls, recorder style selection, Permissions readiness routing, local settings backup import/export, privacy cleanup, reset onboarding, clipboard restore delay, paste method choices, launch at login, diagnostic log export, recording feedback controls, custom start/stop sound import/reset/test, macOS-style section descriptions, local data-safety overview guidance, action summary rows, current-state rows for paste/clipboard/feedback/cleanup, backup/import guidance rows, and diagnostics privacy/export guidance rows exist. Deeper form layout polish remains. |
| Audio input | 91% | `[##################--]` | Device refresh, System Default/Custom/Prioritized modes, priority ordering with unavailable-device fallback, custom persistence, endpoint-ID backed identity, saved-name rebinding, unavailable fallback, startup capture rebinding, live Core Audio device-change refresh, persistent status notices, Device Health rows with Active/Available/Unavailable/Default badges, and a Permissions-page microphone privacy link exist. Remaining work is visual polish and deeper Windows device diagnostics. |
| Onboarding | 87% | `[#################---]` | First-run setup covers model path, recommended local model download, microphone health, microphone settings, compact readiness summary rows with Windows permission guidance, action rows, progress-aware readiness checklist, guided setup stages, macOS-style try-it-out tutorial steps with final insertion/History verification, in-dialog audio input refresh, audio input choice, shortcut, basic usage, Settings reset, and a post-onboarding Permissions page. Richer visual flow polish remains. |
| Packaging | 66% | `[#############-------]` | Source-run docs, runtime workaround, repeatable self-contained dev ZIP packaging, dev ZIP smoke validation, per-user Dev ZIP install/uninstall helpers with Start Menu shortcut creation, signed MSIX manifest/script foundation, certificate-free packaging preflight, non-installing MSIX artifact validation, optional signed-build artifact validation, a gated signed install/uninstall smoke helper, and a read-only release readiness report with signing/trust checklist exist. Actual signed MSIX install smoke and release signing flow remain gated on maintainer-owned signing/trust setup. |

## Current Slice

```text
Windows onboarding permission summary  [####################] 100%
```

Completed:

- Wrote the onboarding permission summary spec and implementation plan.
- Added failing onboarding presenter tests for visible and missing microphone permission guidance.
- Added a Windows Permission row to the onboarding summary.
- Ran focused onboarding tests, full solution tests, Debug x64 build, and whitespace checking.

## Near-Term Priority

1. Continue packaging toward actual signed MSIX install/uninstall smoke and release signing flow when a trusted maintainer signing setup is available.
2. Continue visual parity polish for Power Mode, Settings, Metrics, Dictionary, Enhancement, and onboarding pages.
3. Continue model lifecycle polish for local model discovery, validation, and repair flows.
4. Consider a future WASAPI capture backend if endpoint-native recording becomes necessary.
