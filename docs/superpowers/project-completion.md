# VoiceInk Windows Project Completion

Last updated: 2026-05-25

This tracker is an approximate parity bar for the full free/open-source Windows fork, grounded in the current parity spec and implemented Windows slices. Percentages represent user-feature parity against the macOS app, not just buildability.

## Overall

```text
VoiceInk Windows parity  [##############------] 69%
```

## Area Bars

| Area | Completion | Bar | Status |
| --- | ---: | --- | --- |
| Core dictation pipeline | 78% | `[################----]` | Local recording/transcription/insertion, cleanup, dictionary, enhancement fallback, history writes, and metrics are in place. Prompt-trigger detection, advanced formatting, and canceling in-flight post-recording work remain. |
| Shell and tray | 72% | `[##############------]` | Navigation shell, tray icon, close-to-tray, and open-source About/diagnostics are in place. Rich tray submenus and dedicated History window remain. |
| Floating recorder | 58% | `[############--------]` | Compact recorder shows recording/processing state, elapsed time, live microphone level bars, and non-activating Stop/Cancel controls. Live transcript, notch style, prompt picker, and Power Mode popover remain. |
| Shortcuts | 58% | `[############--------]` | Primary/secondary toggle, paste last, paste enhanced, retry, cancel, open history, and quick add are configurable. Press-and-hold modes, toggle enhancement, Power Mode shortcuts, and shortcut recorder UI remain. |
| Model management | 70% | `[##############------]` | Local Whisper catalog cards, direct GGML downloads, imported `.bin` references, app-local model storage, default model selection, model-aware language selection, and nonblocking warmup/preload exist. Richer provider cards and deeper model lifecycle polish remain. |
| Cloud transcription | 65% | `[#############-------]` | OpenAI-compatible adapter, secure key storage, provider presets, and routing exist. Provider-specific edge behavior, streaming, and richer cards remain. |
| AI enhancement | 70% | `[##############------]` | OpenAI-compatible enhancement, presets, prompts, context, retries/timeouts, and secure keys exist. Trigger-word mode, local/Ollama-style hooks, and richer assistant workflows remain. |
| Context features | 38% | `[########------------]` | Clipboard and UI Automation selected-text context exist with graceful degradation. Clipboard-copy fallback, active window/browser URL context, and OCR remain. |
| Power Mode | 55% | `[###########---------]` | Rule model, process/title matching, default fallback, settings overlays, history metadata, and editor UI exist. Browser URL matching, auto-send keys, shortcuts, and recorder integration remain. |
| Dictionary | 75% | `[###############-----]` | Vocabulary, replacements, sorting, quick add, import/export, and pipeline integration exist. Rich macOS-style page polish remains. |
| History | 71% | `[##############------]` | SQLite detail metadata, search, retry, paste, audio playback/open, delete, CSV export, and privacy cleanup operations exist. Re-enhance, waveform/rate controls, batch actions, and dedicated window remain. |
| Metrics | 68% | `[##############------]` | SQLite metrics, dashboard summary, filters, model performance, and CSV export exist. Visual parity and reset controls remain. |
| Settings | 68% | `[##############------]` | Shortcut/cleanup/provider/audio controls, local settings backup import/export, privacy cleanup, reset onboarding, clipboard restore delay, paste method choices, launch at login, diagnostic log export, and recording feedback controls exist. Custom sound picker and visual polish remain. |
| Audio input | 52% | `[##########----------]` | Device refresh and system default/custom device persistence exist. Prioritized fallback, live device-change updates, and richer active/unavailable UI remain. |
| Onboarding | 55% | `[###########---------]` | First-run setup covers model path, microphone settings, audio input, shortcut, basic usage, and Settings reset. Model catalog/download and deeper permission health checks remain. |
| Packaging | 8% | `[##------------------]` | Source-run docs and runtime workaround exist. MSIX/installer, zip/dev distribution, native dependency layout, uninstall behavior, and installer smoke tests remain. |

## Current Slice

```text
Floating recorder controls  [####################] 100%
```

Completed:

- Added compact Stop and Cancel controls to the floating recorder.
- Kept controls enabled only while recording and disabled during busy/processing states.
- Routed controls through the existing guarded MainWindow stop/cancel commands.
- Added a Win32 no-activate hook so recorder clicks can be handled without stealing focus from the dictation target.

## Near-Term Priority

1. Finish floating recorder fidelity: live partial transcript, prompt and Power Mode controls.
2. Fill remaining Settings polish: custom sound picker/import and recorder style selection.
3. Add packaging path: zip/dev package first, then MSIX or installer.
4. Continue model-management polish with richer provider cards and lifecycle status.
