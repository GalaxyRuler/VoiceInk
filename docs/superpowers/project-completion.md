# VoiceInk Windows Project Completion

Last updated: 2026-05-25

This tracker is an approximate parity bar for the full free/open-source Windows fork, grounded in the current parity spec and implemented Windows slices. Percentages represent user-feature parity against the macOS app, not just buildability.

## Overall

```text
VoiceInk Windows parity  [##############------] 70%
```

## Area Bars

| Area | Completion | Bar | Status |
| --- | ---: | --- | --- |
| Core dictation pipeline | 78% | `[################----]` | Local recording/transcription/insertion, cleanup, dictionary, enhancement fallback, history writes, and metrics are in place. Prompt-trigger detection, advanced formatting, and canceling in-flight post-recording work remain. |
| Shell and tray | 72% | `[##############------]` | Navigation shell, tray icon, close-to-tray, and open-source About/diagnostics are in place. Rich tray submenus and dedicated History window remain. |
| Floating recorder | 63% | `[#############-------]` | Compact recorder shows recording/processing state, elapsed time, live microphone level bars, non-activating Stop/Cancel controls, and functional Prompt/Power cycling controls. Live transcript, notch style, and richer hover popovers remain. |
| Shortcuts | 58% | `[############--------]` | Primary/secondary toggle, paste last, paste enhanced, retry, cancel, open history, and quick add are configurable. Press-and-hold modes, toggle enhancement, Power Mode shortcuts, and shortcut recorder UI remain. |
| Model management | 70% | `[##############------]` | Local Whisper catalog cards, direct GGML downloads, imported `.bin` references, app-local model storage, default model selection, model-aware language selection, and nonblocking warmup/preload exist. Richer provider cards and deeper model lifecycle polish remain. |
| Cloud transcription | 65% | `[#############-------]` | OpenAI-compatible adapter, secure key storage, provider presets, and routing exist. Provider-specific edge behavior, streaming, and richer cards remain. |
| AI enhancement | 70% | `[##############------]` | OpenAI-compatible enhancement, presets, prompts, context, retries/timeouts, and secure keys exist. Trigger-word mode, local/Ollama-style hooks, and richer assistant workflows remain. |
| Context features | 38% | `[########------------]` | Clipboard and UI Automation selected-text context exist with graceful degradation. Clipboard-copy fallback, active window/browser URL context, and OCR remain. |
| Power Mode | 58% | `[############--------]` | Rule model, process/title matching, default fallback, explicit recorder selection, settings overlays, history metadata, and editor UI exist. Browser URL matching, auto-send keys, shortcuts, and richer recorder popover integration remain. |
| Dictionary | 75% | `[###############-----]` | Vocabulary, replacements, sorting, quick add, import/export, and pipeline integration exist. Rich macOS-style page polish remains. |
| History | 71% | `[##############------]` | SQLite detail metadata, search, retry, paste, audio playback/open, delete, CSV export, and privacy cleanup operations exist. Re-enhance, waveform/rate controls, batch actions, and dedicated window remain. |
| Metrics | 68% | `[##############------]` | SQLite metrics, dashboard summary, filters, model performance, and CSV export exist. Visual parity and reset controls remain. |
| Settings | 68% | `[##############------]` | Shortcut/cleanup/provider/audio controls, local settings backup import/export, privacy cleanup, reset onboarding, clipboard restore delay, paste method choices, launch at login, diagnostic log export, and recording feedback controls exist. Custom sound picker and visual polish remain. |
| Audio input | 52% | `[##########----------]` | Device refresh and system default/custom device persistence exist. Prioritized fallback, live device-change updates, and richer active/unavailable UI remain. |
| Onboarding | 55% | `[###########---------]` | First-run setup covers model path, microphone settings, audio input, shortcut, basic usage, and Settings reset. Model catalog/download and deeper permission health checks remain. |
| Packaging | 8% | `[##------------------]` | Source-run docs and runtime workaround exist. MSIX/installer, zip/dev distribution, native dependency layout, uninstall behavior, and installer smoke tests remain. |

## Current Slice

```text
Floating recorder Prompt/Power controls  [####################] 100%
```

Completed:

- Added functional Prompt and Power controls to the floating recorder.
- Prompt enables AI enhancement first, then cycles through available prompts without opening a focus-stealing popup.
- Power Mode cycles Auto and enabled Power Mode rules and persists the explicit selection.
- Dictation completion now uses the original recording target with latest Prompt/Power settings so in-recording changes affect the active result.

## Near-Term Priority

1. Finish floating recorder fidelity: live partial transcript, richer Prompt/Power popovers, and notch-style recorder.
2. Fill remaining Settings polish: custom sound picker/import and recorder style selection.
3. Add packaging path: zip/dev package first, then MSIX or installer.
4. Continue model-management polish with richer provider cards and lifecycle status.
