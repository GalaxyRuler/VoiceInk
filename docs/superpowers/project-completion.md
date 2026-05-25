# VoiceInk Windows Project Completion

Last updated: 2026-05-26

This tracker is an approximate parity bar for the full free/open-source Windows fork, grounded in the current parity spec and implemented Windows slices. Percentages represent user-feature parity against the macOS app, not just buildability.

## Overall

```text
VoiceInk Windows parity  [#################---] 87%
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
| Context features | 65% | `[#############-------]` | Clipboard context, selected-text context with clipboard fallback, active-window process/title context, sanitized browser URL context, and default-off local screen OCR context exist with graceful degradation. Picker/region OCR polish remains. |
| Power Mode | 71% | `[##############------]` | Rule model, process/title/browser URL matching, default fallback, explicit recorder chooser selection, cycle shortcut, settings overlays, history metadata, and editor UI exist. Auto-send keys and per-rule shortcuts remain. |
| Dictionary | 75% | `[###############-----]` | Vocabulary, replacements, sorting, quick add, import/export, and pipeline integration exist. Rich macOS-style page polish remains. |
| History | 90% | `[##################--]` | Dedicated History window, SQLite detail metadata, search, retry, re-enhance from saved original text, copy actions for original/final/enhanced/AI request, paste, audio playback/open with waveform and rate controls, single and batch delete, single and selected CSV export, and privacy cleanup operations exist. Remaining work is deeper visual polish and analysis overlays. |
| Metrics | 72% | `[##############------]` | SQLite metrics, dashboard summary, filters, model performance, CSV export, and confirmed reset controls exist. Visual parity and diagnostics expansion remain. |
| Settings | 73% | `[###############-----]` | Shortcut/cleanup/provider/audio controls, recorder style selection, local settings backup import/export, privacy cleanup, reset onboarding, clipboard restore delay, paste method choices, launch at login, diagnostic log export, recording feedback controls, and custom start/stop sound import/reset/test exist. Visual polish remains. |
| Audio input | 60% | `[############--------]` | Device refresh, system default/custom persistence, saved-name rebinding, unavailable fallback, and startup capture rebinding exist. Live device-change updates and richer active/unavailable UI remain. |
| Onboarding | 55% | `[###########---------]` | First-run setup covers model path, microphone settings, audio input, shortcut, basic usage, and Settings reset. Model catalog/download and deeper permission health checks remain. |
| Packaging | 32% | `[######--------------]` | Source-run docs, runtime workaround, repeatable self-contained dev ZIP packaging, and signed MSIX manifest/script foundation exist. End-to-end signed package smoke, shortcut registration, uninstall behavior validation, and release signing flow remain. |

## Current Slice

```text
Windows local screen OCR reader  [####################] 100%
```

Completed:

- Added a local virtual-desktop PNG capture provider.
- Added `Windows.Media.Ocr` recognition over captured screen snapshots.
- Wired the default Windows enhancement context provider to use local OCR when `UseOcrContext` is enabled.
- Kept OCR default-off and graceful on unsupported/failing OCR paths.

## Near-Term Priority

1. Add OCR picker/region polish so users can constrain screen context.
2. Expand streaming/live preview beyond Deepgram and continue recorder waveform visual polish.
3. Continue Settings visual parity and remaining macOS preferences.
4. Continue packaging from MSIX foundation to signed smoke, uninstall behavior, and release signing flow.
