# VoiceInk Windows Project Completion

Last updated: 2026-05-25

This tracker is an approximate parity bar for the full free/open-source Windows fork, grounded in the current parity spec and implemented Windows slices. Percentages represent user-feature parity against the macOS app, not just buildability.

## Overall

```text
VoiceInk Windows parity  [###############-----] 75%
```

## Area Bars

| Area | Completion | Bar | Status |
| --- | ---: | --- | --- |
| Core dictation pipeline | 78% | `[################----]` | Local recording/transcription/insertion, cleanup, dictionary, enhancement fallback, history writes, and metrics are in place. Prompt-trigger detection, advanced formatting, and canceling in-flight post-recording work remain. |
| Shell and tray | 72% | `[##############------]` | Navigation shell, tray icon, close-to-tray, and open-source About/diagnostics are in place. Rich tray submenus and dedicated History window remain. |
| Floating recorder | 84% | `[#################---]` | Mini and top-center Notch styles show recording/processing state, elapsed time, live microphone level bars, non-activating Stop/Cancel controls, hover-dismissable no-activate Prompt/Power chooser panels, and a gated live transcript preview panel now backed by Deepgram interim results. Waveform polish and more streaming providers remain. |
| Shortcuts | 58% | `[############--------]` | Primary/secondary toggle, paste last, paste enhanced, retry, cancel, open history, and quick add are configurable. Press-and-hold modes, toggle enhancement, Power Mode shortcuts, and shortcut recorder UI remain. |
| Model management | 70% | `[##############------]` | Local Whisper catalog cards, direct GGML downloads, imported `.bin` references, app-local model storage, default model selection, model-aware language selection, and nonblocking warmup/preload exist. Richer provider cards and deeper model lifecycle polish remain. |
| Cloud transcription | 72% | `[##############------]` | OpenAI-compatible adapter, secure key storage, Custom/Groq/Deepgram presets, direct Deepgram batch requests, and Deepgram live preview streaming exist. More provider-specific adapters, richer cards, and provider test requests remain. |
| AI enhancement | 70% | `[##############------]` | OpenAI-compatible enhancement, presets, prompts, context, retries/timeouts, and secure keys exist. Trigger-word mode, local/Ollama-style hooks, and richer assistant workflows remain. |
| Context features | 38% | `[########------------]` | Clipboard and UI Automation selected-text context exist with graceful degradation. Clipboard-copy fallback, active window/browser URL context, and OCR remain. |
| Power Mode | 60% | `[############--------]` | Rule model, process/title matching, default fallback, explicit recorder chooser selection, settings overlays, history metadata, and editor UI exist. Browser URL matching, auto-send keys, and shortcuts remain. |
| Dictionary | 75% | `[###############-----]` | Vocabulary, replacements, sorting, quick add, import/export, and pipeline integration exist. Rich macOS-style page polish remains. |
| History | 71% | `[##############------]` | SQLite detail metadata, search, retry, paste, audio playback/open, delete, CSV export, and privacy cleanup operations exist. Re-enhance, waveform/rate controls, batch actions, and dedicated window remain. |
| Metrics | 68% | `[##############------]` | SQLite metrics, dashboard summary, filters, model performance, and CSV export exist. Visual parity and reset controls remain. |
| Settings | 73% | `[###############-----]` | Shortcut/cleanup/provider/audio controls, recorder style selection, local settings backup import/export, privacy cleanup, reset onboarding, clipboard restore delay, paste method choices, launch at login, diagnostic log export, recording feedback controls, and custom start/stop sound import/reset/test exist. Visual polish remains. |
| Audio input | 52% | `[##########----------]` | Device refresh and system default/custom device persistence exist. Prioritized fallback, live device-change updates, and richer active/unavailable UI remain. |
| Onboarding | 55% | `[###########---------]` | First-run setup covers model path, microphone settings, audio input, shortcut, basic usage, and Settings reset. Model catalog/download and deeper permission health checks remain. |
| Packaging | 20% | `[####----------------]` | Source-run docs, runtime workaround, and repeatable self-contained dev ZIP packaging exist. MSIX/installer, signing, shortcut registration, uninstall behavior, and installer smoke tests remain. |

## Current Slice

```text
Windows custom recording sounds  [####################] 100%
```

Completed:

- Added Core sound mode/import/reset models and recording-session playback snapshots.
- Added custom start/stop sound Settings controls with Test, Choose, and Reset actions.
- Added NAudio duration validation and playback for imported `.wav`, `.mp3`, `.aiff`, and `.aif` sounds.
- Kept imported sounds local under `%LocalAppData%\VoiceInk.Windows\Sounds` with Windows system sound fallback.

## Near-Term Priority

1. Expand streaming/live preview beyond Deepgram and continue waveform visual polish.
2. Continue Settings visual parity and remaining macOS preferences.
3. Continue packaging from dev ZIP to MSIX or installer with uninstall behavior and smoke tests.
4. Continue model-management polish with richer provider cards and lifecycle status.
