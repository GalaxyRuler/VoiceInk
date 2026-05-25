# VoiceInk Windows Project Completion

Last updated: 2026-05-26

This tracker is an approximate parity bar for the full free/open-source Windows fork, grounded in the current parity spec and implemented Windows slices. Percentages represent user-feature parity against the macOS app, not just buildability.

## Overall

```text
VoiceInk Windows parity  [##################--] 88%
```

## Area Bars

| Area | Completion | Bar | Status |
| --- | ---: | --- | --- |
| Core dictation pipeline | 84% | `[#################---]` | Local recording/transcription/insertion, cleanup, dictionary, enhancement fallback, prompt-trigger detection, completed/canceled/failed history writes, metrics, and canceling in-flight post-recording work are in place. Advanced formatting remains. |
| Shell and tray | 76% | `[###############-----]` | Navigation shell, tray icon, close-to-tray, open-source About/diagnostics, and dedicated History window routing are in place. Rich tray submenus remain. |
| Floating recorder | 84% | `[#################---]` | Mini and top-center Notch styles show recording/processing state, elapsed time, live microphone level bars, non-activating Stop/Cancel controls, hover-dismissable no-activate Prompt/Power chooser panels, and a gated live transcript preview panel now backed by Deepgram interim results. Waveform polish and more streaming providers remain. |
| Shortcuts | 67% | `[#############-------]` | Primary/secondary toggle, paste last, paste enhanced, retry, cancel, open history, quick add, toggle enhancement, and cycle Power Mode are configurable. Press-and-hold modes, per-rule Power Mode shortcuts, and shortcut recorder UI remain. |
| Model management | 70% | `[##############------]` | Local Whisper catalog cards, direct GGML downloads, imported `.bin` references, app-local model storage, default model selection, model-aware language selection, and nonblocking warmup/preload exist. Richer provider cards and deeper model lifecycle polish remain. |
| Cloud transcription | 72% | `[##############------]` | OpenAI-compatible adapter, secure key storage, Custom/Groq/Deepgram presets, direct Deepgram batch requests, and Deepgram live preview streaming exist. More provider-specific adapters, richer cards, and provider test requests remain. |
| AI enhancement | 73% | `[###############-----]` | OpenAI-compatible enhancement, presets, custom prompts, trigger-word activation, context, retries/timeouts, secure keys, and a toggle-enhancement shortcut exist. Local/Ollama-style hooks and richer assistant workflows remain. |
| Context features | 73% | `[###############-----]` | Clipboard context, selected-text context with clipboard fallback, active-window process/title context, sanitized browser URL context, default-off local screen OCR context, region-aware OCR capture plumbing, visible numeric OCR region controls, and a visual OCR region picker exist with graceful degradation. Multi-monitor picker refinement remains. |
| Power Mode | 71% | `[##############------]` | Rule model, process/title/browser URL matching, default fallback, explicit recorder chooser selection, cycle shortcut, settings overlays, history metadata, and editor UI exist. Auto-send keys and per-rule shortcuts remain. |
| Dictionary | 75% | `[###############-----]` | Vocabulary, replacements, sorting, quick add, import/export, and pipeline integration exist. Rich macOS-style page polish remains. |
| History | 90% | `[##################--]` | Dedicated History window, SQLite detail metadata, search, retry, re-enhance from saved original text, copy actions for original/final/enhanced/AI request, paste, audio playback/open with waveform and rate controls, single and batch delete, single and selected CSV export, and privacy cleanup operations exist. Remaining work is deeper visual polish and analysis overlays. |
| Metrics | 72% | `[##############------]` | SQLite metrics, dashboard summary, filters, model performance, CSV export, and confirmed reset controls exist. Visual parity and diagnostics expansion remain. |
| Settings | 73% | `[###############-----]` | Shortcut/cleanup/provider/audio controls, recorder style selection, local settings backup import/export, privacy cleanup, reset onboarding, clipboard restore delay, paste method choices, launch at login, diagnostic log export, recording feedback controls, and custom start/stop sound import/reset/test exist. Visual polish remains. |
| Audio input | 72% | `[##############------]` | Device refresh, system default/custom persistence, saved-name rebinding, unavailable fallback, startup capture rebinding, live Core Audio device-change refresh, and persistent status notices for active/rebound/unavailable/no-device states exist. Richer device health and permission UI remains. |
| Onboarding | 64% | `[#############-------]` | First-run setup covers model path, recommended local model download, microphone health, microphone settings, in-dialog audio input refresh, audio input choice, shortcut, basic usage, and Settings reset. Broader permission health checks and richer setup polish remain. |
| Packaging | 46% | `[#########-----------]` | Source-run docs, runtime workaround, repeatable self-contained dev ZIP packaging, dev ZIP smoke validation, signed MSIX manifest/script foundation, certificate-free packaging preflight, and non-installing MSIX artifact validation exist. End-to-end signed MSIX build smoke, shortcut registration, uninstall behavior validation, and release signing flow remain. |

## Current Slice

```text
Windows onboarding model catalog download  [####################] 100%
```

Completed:

- Added a tested onboarding recommended model catalog helper.
- Added first-run recommended model selection using the existing free/open-source Whisper catalog.
- Added `Download Recommended Model` to fetch a selected GGML model into app-local model storage.
- Filled the onboarding model path after download while leaving completion to the existing `Save Setup` action.

## Near-Term Priority

1. Continue packaging from artifact validation to signed MSIX build smoke, uninstall behavior, and release signing flow.
2. Expand streaming/live preview beyond Deepgram and continue recorder waveform visual polish.
3. Add broader onboarding permission health checks and setup polish.
4. Refine OCR region picker behavior across complex multi-monitor/DPI layouts.
