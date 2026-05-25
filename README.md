<div align="center">
  <img src="VoiceInk/Assets.xcassets/AppIcon.appiconset/256-mac.png" width="180" height="180" />
  <h1>VoiceInk</h1>
  <p>Free and open-source voice-to-text app source with an in-progress native Windows fork</p>

  [![License](https://img.shields.io/badge/License-GPL%20v3-blue.svg)](https://www.gnu.org/licenses/gpl-3.0)
  ![Platform](https://img.shields.io/badge/platform-macOS%2014.0%2B-brightgreen)
  ![GitHub stars](https://img.shields.io/github/stars/Beingpax/VoiceInk?style=social)
</div>

---

VoiceInk is a native macOS application that transcribes what you say to text almost instantly. This fork also contains an in-progress native Windows implementation under `VoiceInk.Windows/`.

![VoiceInk Mac App](https://github.com/user-attachments/assets/12367379-83e7-48a6-b52c-4488a6a04bba)

The Windows fork is developed as a free/open-source implementation. It has no licensing gates, trials, purchase prompts, paid feature locks, commercial telemetry, or private paid updater channel.

The project goal is to make privacy-focused voice-to-text software that is efficient, understandable, and source-runnable.

## Features

- 🎙️ **Accurate Transcription**: Local AI models that transcribe your voice to text with 99% accuracy, almost instantly
- 🔒 **Privacy First**: 100% offline processing ensures your data never leaves your device
- ⚡ **Power Mode**: Intelligent app detection automatically applies your perfect pre-configured settings based on the app/ URL you're on
- 🧠 **Context Aware**: Smart AI that understands your screen content and adapts to the context
- 🎯 **Global Shortcuts**: Configurable keyboard shortcuts for quick recording, with Windows secondary toggle support and macOS push-to-talk modes
- 📝 **Personal Dictionary**: Train the AI to understand your unique terminology with custom words, industry terms, and smart text replacements
- 🔄 **Smart Modes**: Instantly switch between AI-powered modes optimized for different writing styles and contexts
- 🤖 **AI Assistant**: Built-in voice assistant mode for a quick chatGPT like conversational assistant

## Get Started

### Build from Source

Build the macOS app by following [BUILDING.md](BUILDING.md). Build the Windows fork by following the Windows Fork Development section below.

## Requirements

- macOS 14.4 or later

## Documentation

- [Building from Source](BUILDING.md) - Detailed instructions for building the project
- [Contributing Guidelines](CONTRIBUTING.md) - How to contribute to VoiceInk
- [Code of Conduct](CODE_OF_CONDUCT.md) - Our community standards

## Windows Fork Development

The Windows implementation lives under `VoiceInk.Windows/` and is separate from the macOS SwiftUI app.

### Requirements

- Windows 11
- .NET 10 SDK
- Visual Studio with Windows App SDK support, or equivalent Build Tools
- A local whisper.cpp-compatible `.bin` model file

### Build

Run these commands from the repository root, so the `VoiceInk.Windows\...` paths resolve correctly:

```powershell
dotnet restore VoiceInk.Windows\VoiceInk.Windows.sln
dotnet build VoiceInk.Windows\VoiceInk.Windows.sln -c Debug -p:Platform=x64
```

### Run

Run this command from the repository root:

```powershell
dotnet run --project VoiceInk.Windows\src\VoiceInk.Windows.App\VoiceInk.Windows.App.csproj -c Debug -p:Platform=x64
```

If `dotnet` reports `NETSDK1045` because the system SDK is older than .NET 10, either install the .NET 10 SDK or run with a local .NET 10 SDK executable. In this worktree, the bundled SDK command is:

```powershell
& ..\.dotnet-sdk-10\dotnet.exe run --project VoiceInk.Windows\src\VoiceInk.Windows.App\VoiceInk.Windows.App.csproj -c Debug -p:Platform=x64
```

If the command exits without showing a window from a very long checkout path, the Windows App SDK bootstrapper may be hitting a path-length limit. Move the checkout to a shorter path, or map the long parent directory to a temporary drive with `subst` and run from there:

```powershell
cmd /c 'subst W: "C:\path\to\worktrees-parent"'
Set-Location W:\your-checkout
& W:\.dotnet-sdk-10\dotnet.exe run --project VoiceInk.Windows\src\VoiceInk.Windows.App\VoiceInk.Windows.App.csproj -c Debug -p:Platform=x64
```

Remove the temporary drive mapping with:

```powershell
cmd /c "subst W: /D"
```

### Package a Dev ZIP

The Windows fork includes a repo-local packaging script for an unpackaged, self-contained developer ZIP. Run it from the repository root:

```powershell
.\VoiceInk.Windows\scripts\package-dev-zip.ps1 -DotNetPath "..\.dotnet-sdk-10\dotnet.exe"
```

The ZIP is written to `VoiceInk.Windows\artifacts\dev-zip\VoiceInk-Windows-dev-win-x64.zip`. Extract it and launch `VoiceInk.Windows.App.exe`. On first run, configure a whisper.cpp-compatible GGML `.bin` model such as `C:\Models\ggml-base.en.bin`; app data remains under `%LocalAppData%\VoiceInk.Windows`.

This dev ZIP is for source-built testing. It does not create Start Menu shortcuts, register uninstall entries, sign binaries, or install an MSIX/package identity.

### Package a Signed MSIX

The Windows fork also includes the open-source MSIX packaging foundation: `VoiceInk.Windows\src\VoiceInk.Windows.App\Package.appxmanifest` plus a repo-local packaging script. MSIX packages must be signed. The repo does not create certificates, import certificates, or store signing secrets; provide a maintainer-owned `.pfx` when building a package:

Before using a real certificate, run the non-mutating preflight from the repository root to verify paths, MSIX publish properties, artifact containment, and the manual smoke command sequence:

```powershell
.\VoiceInk.Windows\scripts\package-msix.ps1 -Preflight -DotNetPath "..\.dotnet-sdk-10\dotnet.exe"
```

```powershell
.\VoiceInk.Windows\scripts\package-msix.ps1 -DotNetPath "..\.dotnet-sdk-10\dotnet.exe" -PackageCertificateKeyFile "C:\Path\VoiceInk.Windows.Signing.pfx"
```

If the certificate has a password, add `-PackageCertificatePassword "<password>"` from a secure local shell. Artifacts are written under `VoiceInk.Windows\artifacts\msix`. On a test machine where the certificate is trusted, smoke the package with:

```powershell
.\VoiceInk.Windows\scripts\test-msix-package.ps1 -PackagePath <path-to-msix>
Add-AppxPackage -Path <path-to-msix>
Get-AppxPackage VoiceInk.Windows
Remove-AppxPackage -Package <package-full-name>
```

`package-msix.ps1 -Preflight` does not create output folders, delete output folders, publish, sign, create certificates, import certificates, install packages, uninstall packages, or read certificate passwords. `test-msix-package.ps1` only inspects the artifact under `VoiceInk.Windows\artifacts`; it does not install, uninstall, launch, sign, create certificates, import certificates, or cryptographically verify the package signature.

On first launch, VoiceInk for Windows opens a setup dialog for the local whisper model path, microphone settings/input, primary shortcut, microphone health, and a short try-it flow. After setup, use the sidebar to move between Dashboard, Transcribe Audio, History, Metrics, AI Models, Enhancement, Power Mode, Audio Input, Dictionary, Settings, and About / Open Source. In AI Models, local Whisper catalog cards show language, size, speed, accuracy, description, downloaded/default state, direct download progress, set-default, and Show in Explorer actions. Catalog downloads use the open-source whisper.cpp GGML files from Hugging Face, save `.bin` files under `%LocalAppData%\VoiceInk.Windows\Models`, add them to imported local models, and select the downloaded model as the default path. The `Transcription Language` control follows the selected model: English-only `.en` models force English, while multilingual and imported local models offer auto-detect plus Whisper language codes. The `Prewarm Local Model` setting defaults on and schedules a conservative Whisper.net preload for the selected local model on startup, Windows resume, model import/selection, catalog download, and provider settings save; `Warm Up Selected Model` runs the same preload manually. `Show Live Transcript Preview` persists the macOS-style preview preference. When the Deepgram transcription preset is selected and a Deepgram key is saved locally, the recorder streams copied 16 kHz mono PCM chunks for interim preview text; final insertion/history still come from the stopped-recording transcription result. Windows skips macOS Core ML encoder downloads because Whisper.net uses the GGML `.bin` model directly. You can still enter a custom local whisper model path, use `Import Model` to add an existing whisper.cpp `.bin` file to the imported local models selector, and use `Open GGML Model Downloads` to open the upstream model page in the default browser. In Enhancement, you can enable the default-off AI enhancement pipeline, choose an AI provider preset, configure an OpenAI-compatible chat-completions endpoint/model, choose or edit a prompt, create local custom prompts, persist trigger words, turn on read-only Clipboard Context, set timeout/short-phrase/retry behavior, and save or clear the selected provider API key in Windows Credential Manager. Predefined prompt text stays source-controlled, but predefined trigger words can be edited locally; custom prompt title, instructions, trigger words, and system-instruction wrapping are stored in JSON settings. Provider presets currently include Custom OpenAI-compatible, Cerebras, Groq, Gemini, OpenAI, OpenRouter, Mistral, and Ollama. The hosted presets fill the macOS-aligned endpoint/model defaults and store keys independently per provider. Ollama uses the local OpenAI-compatible endpoint `http://localhost:11434/v1/chat/completions` and does not require an API key. Remote enhancement endpoints must use HTTPS; plain HTTP is accepted only for localhost development endpoints. VoiceInk rejects enhancement endpoint URLs that contain embedded credentials or common key/token query parameters so API keys stay out of JSON settings. When enhancement runs, VoiceInk also attempts read-only selected text capture through Windows UI Automation and appends available selected text inside `<CURRENTLY_SELECTED_TEXT>` tags. Clipboard Context reads current plain text from the clipboard only for the enhancement prompt, appends it inside `<CLIPBOARD_CONTEXT>` tags, skips empty or unavailable clipboard content, and does not modify clipboard contents. Screen OCR Context is default-off and can read local screen text into `<SCREEN_OCR_CONTEXT>`; use `Select Region` to drag a visual full-screen picker, or edit the numeric region fields directly, when OCR should be constrained to a specific area. In Power Mode, add enabled rules that match the active Windows process name and/or title bar text, optionally mark one rule as the default fallback, and set session-only overrides for model path, language, enhancement, prompt, trailing-space behavior, filler cleanup, punctuation cleanup, and lowercase output. VoiceInk captures the matching Power Mode at recording start and records the rule name/emoji in History without rewriting base settings. In Dashboard, click `Start Recording`, speak, then click `Stop And Insert`. You can also press `Ctrl+Alt+Space` to toggle recording by default when that global hotkey is available. During recording and processing, Settings `Interface` > `Recorder Style` switches the always-on-top recorder between the bottom mini pill and a Windows top-center notch-style pill. Both styles show state text, elapsed time, live microphone level bars while recording, and processing pulse bars otherwise. While recording, compact Stop and Cancel buttons route through the same guarded commands as shortcuts, tray commands, and the main window. The Prompt button opens a no-activate in-window chooser with an `AI Enhancement` toggle and prompt rows; selecting a prompt enables enhancement and persists the choice. The Power Mode button opens a matching chooser with Auto and enabled rules, and persists the explicit selection. The Prompt and Power chooser panels also open on pointer hover, stay visible while the pointer is over the button or panel, and close after a short delay when the pointer leaves both. Recorder changes made during capture affect the active result because completion reloads current Prompt/Power settings against the original recording target, and the recorder uses a no-activate mouse hook so clicking it does not take focus away from the dictation target. When live preview is enabled and provider partial text is available, the recorder expands to show that text near the controls; the text is in-memory only and clears at recording boundaries. In Transcribe Audio, choose one or more supported audio/video files, then start the queue to import them into app-owned WAV recordings, transcribe them with the selected transcription provider, optionally enhance the cleaned text when Enhancement is enabled/configured, apply dictionary/cleanup settings, and save completed rows to History. Completed recorder, Transcribe Audio, and retry rows also write local session metrics to `metrics.db`; the Metrics section shows sessions recorded, words dictated, words per minute, keystrokes saved, estimated time saved, and transcription/enhancement model performance. Metrics can be filtered by Last 7 Days, Last 30 Days, This Year, or All Time, and exported as a local CSV through the save picker. In Settings, `Export Settings` writes a local JSON backup for general settings, custom prompts, Power Mode rules, imported model references, and dictionary entries; `Import Settings` lets you choose categories before applying them. Provider API keys stay in Windows Credential Manager and are never written to the backup file, and any credential-bearing provider endpoint is cleared from the backup, so imported provider settings require reconfiguring keys locally. Settings also includes Clipboard controls for keeping the previous clipboard content, choosing the restore delay, and switching between the default clipboard paste method and Windows `Direct Text`. Clipboard restore uses a VoiceInk paste-session marker and restores only if the clipboard still contains VoiceInk's transient paste payload, so later user clipboard changes are left alone. Settings also includes local-only Privacy controls: transcript cleanup deletes old history rows and their app-owned audio files, audio cleanup deletes old app-owned audio files while preserving transcript text, and audio cleanup is hidden while transcript cleanup is enabled. When enabled, cleanup runs on launch, after completed recordings, and on a daily in-app timer; manual `Run ... Now` actions remain available. Cleanup only deletes root-level app-created WAV files in VoiceInk's recordings directory. The General group includes `Launch at Login`, which registers the current source-built executable under the current user's Windows `Run` key with a `--voiceink-startup` argument so login launches start hidden to the tray, plus `Reset Onboarding`, which marks the first-run setup to show again on next launch after confirmation. The Windows shell also exposes configurable key+modifier shortcuts for primary and secondary recording toggle, paste last, paste last enhanced, retry last transcription, cancel recording, open history, and quick add to dictionary. A Windows tray icon stays available while the app is running; its menu can show or hide the shell, start or stop recording, open Quick Add to Dictionary, focus History, or quit VoiceInk. Retry Last reuses the latest completed history row with a saved audio file, applies the current transcription provider, dictionary, and cleanup settings, saves the retried row, records a retry metric, and copies the retried text to the clipboard. Cancel Recording stops the active recorder, keeps the captured audio file, and saves a canceled history row without adding a session metric. Open History restores the main window and focuses the History section. Quick Add opens a small Vocabulary/Word Replacement dialog that mirrors the macOS quick-add panel intent.

About / Open Source includes local diagnostics actions for opening the app data folder, copying a safe diagnostics summary, and exporting a `.log` diagnostic report with system/app context, known file metadata, and recent generic in-app status labels. Diagnostic exports do not include API keys, Credential Manager values, environment variables, clipboard contents, transcript/history text, rendered AI prompt contents, user-authored dictionary/prompt text, or settings file contents.

Settings Recording Feedback controls cover local start/stop sound feedback, custom start/stop sound import, muting the default Windows output while recording, opt-in media pause/resume while recording, and the shared resume delay. Audio/media restore runs after capture stops so long transcription work does not keep output muted. Stop sound plays after successful text insertion. By default, sound feedback uses Windows system sounds; use `Choose` to import a local `.wav`, `.mp3`, `.aiff`, or `.aif` sound up to 3 seconds for either Start Sound or Stop Sound. Imported sounds are copied under `%LocalAppData%\VoiceInk.Windows\Sounds`, can be tested from Settings, can be reset back to System Default, and fall back to Windows system sounds if the custom file cannot be played. Media pause/resume is off by default and uses Windows Global System Media Transport Controls, pausing only when Windows reports a current playing session and resuming only that same session when supported. During long transcription, enhancement, or insertion work after capture stops, Cancel Recording cancels the active processing pipeline cooperatively, keeps VoiceInk responsive, and preserves the captured audio as a canceled History row when possible.

For cloud transcription, choose `OpenAI-compatible` in AI Models, then pick `Custom OpenAI-compatible`, `Groq`, or `Deepgram`. The custom preset lets you enter a full request endpoint such as `https://api.example.com/v1/audio/transcriptions` and model ID. The Groq preset fills `https://api.groq.com/openai/v1/audio/transcriptions` and offers `whisper-large-v3-turbo` and `whisper-large-v3`. The Deepgram preset fills `https://api.deepgram.com/v1/listen`, offers `nova-3` and `nova-3-medical`, sends direct Deepgram batch requests for final transcription, and can stream interim recorder preview text when `Show Live Transcript Preview` is enabled. API keys are stored per preset in Windows Credential Manager. Remote endpoints must use HTTPS; plain HTTP is accepted only for localhost development endpoints. VoiceInk rejects endpoint URLs that contain embedded credentials or common key/token query parameters so API keys stay out of JSON settings. Dictation, Transcribe Audio, and Retry Last then use that provider instead of local Whisper, sending provider-appropriate audio transcription requests with vocabulary prompt context when supported.

### Current Windows MVP Scope

- WinUI 3 shell with sidebar navigation for Dashboard, Transcribe Audio, History, Metrics, AI Models, Enhancement, Power Mode, Audio Input, Dictionary, Settings, and About / Open Source
- Windows tray icon with show/hide, recording toggle, Quick Add, History, and Quit commands
- Always-on-top floating recorder with Mini and Notch styles, state text, elapsed timer, live microphone level bars, non-activating Stop/Cancel controls, hover-dismissable no-activate Prompt/Power chooser panels, a gated live transcript preview panel for real partial sources, and pulse animation
- About / Open Source replacement for commercial VoiceInk Pro surfaces, with local-only diagnostics folder, summary, and diagnostic log export actions
- First-run setup dialog for local model path, microphone settings/input, primary shortcut, and basic usage
- Local Whisper catalog cards with direct GGML `.bin` downloads, app-local model storage, imported `.bin` references, model-aware transcription language selection, nonblocking warmup/preload, and default model selection from the shell
- Local whisper.cpp transcription through Whisper.net
- Default-off OpenAI-compatible cloud transcription provider with custom, Groq, and Deepgram presets, endpoint/model settings, provider-specific Windows Credential Manager API key storage, multipart OpenAI-compatible requests, direct Deepgram batch requests, Deepgram live recorder preview streaming, sanitized HTTP errors, and routing across Dictation, Transcribe Audio, and History Retry
- Transcribe Audio section with multi-file picker, in-memory pending/processing/completed/failed queue, cancel/clear/remove/retry controls, Media Foundation import to app-owned WAV recordings, selected-provider transcription, cleanup, and History save
- Default-off AI Enhancement section with Custom/Cerebras/Groq/Gemini/OpenAI/OpenRouter/Mistral/Ollama provider presets, OpenAI-compatible endpoint/model settings, provider-specific Windows Credential Manager API key storage, automatic read-only selected text context, optional read-only Clipboard Context, prompt selection and local custom prompt editing, trigger-word persistence, timeout/short-phrase/retry settings, output filtering, endpoint safety validation, and original-text fallback
- Dictation, Transcribe Audio, and history retry can run enhancement after local transcription cleanup and before insertion when enhancement is enabled and configured
- Power Mode section with ordered enabled/default rules, active-window quick fill from Win32 foreground-window process/title detection, session-only overrides for model/language/enhancement/prompt/cleanup settings, and history name/emoji metadata
- Microphone capture
- Clipboard-based text insertion into the active app
- JSON settings and SQLite transcription history
- Configurable global key+modifier shortcuts for primary and secondary recording toggle, paste last, paste last enhanced, retry last transcription, cancel recording, open history, and quick add to dictionary
- Core dictionary models for vocabulary words and word replacements
- Persistent JSON-backed Dictionary storage for vocabulary words and word replacements
- macOS-style transcription cleanup for hallucination markers, filler words, punctuation cleanup, lowercase output, trailing spaces, and word replacements
- Vocabulary prompt biasing for local Whisper transcription
- Expanded history metadata for original text, final text, enhanced text, status, language, model path, prompt name, Power Mode, enhancement provider/model, enhancement timing, AI request messages, and audio file path
- Shell controls for filler-word removal, punctuation cleanup, lowercase output, and trailing spaces
- Settings Clipboard controls for restore delay and default/direct-text paste methods
- Settings General `Launch at Login` control using the current user's Windows startup registry key
- Settings Privacy controls for macOS-style transcript cleanup, audio-only cleanup, and confirmed onboarding reset
- Shell controls for adding/removing/sorting vocabulary words and adding/editing/enabling/disabling/removing/sorting word replacements
- Quick-add Dictionary dialog for vocabulary words and word replacements
- Dictionary JSON import/export for vocabulary words and word replacements
- Shell audio input refresh and System Default/custom microphone selection
- Shell recent-history list/detail view for original, final, enhanced, status, timing, model, prompt, and error metadata
- Picker-based CSV history export
- Paste-last final and enhanced-preferred history actions in the shell
- History search, selected-row audio playback/open, selected-row retry, retry-last-to-clipboard, active-recording cancel history, and confirmed single-item delete
- Settings JSON backup export/import with category selection for General Settings, Custom Prompts, Power Mode, Dictionary, and Custom Model Definitions; Credential Manager API keys are intentionally excluded

The current shell does not yet expose Permissions, tray submenus for model/provider/enhancement/language/audio/context settings, named cloud provider adapters beyond Groq/Deepgram, streaming cloud transcription adapters beyond Deepgram live preview, provider-specific payloads for other non-compatible APIs, Anthropic Messages API enhancement, native Ollama generate and Local CLI enhancement hooks, dynamic OpenRouter/Ollama model refresh, prompt icon/description editing, selected-text clipboard fallback, OCR context, browser URL Power Mode matching, Power Mode shortcuts, Power Mode auto-send keys, toggle-enhancement shortcuts, push-to-talk/hybrid shortcut modes, shortcut key-up handling, prioritized audio input failover, canceling in-flight transcription/enhancement, Transcribe Audio drag/drop import, per-file Transcribe Audio save/copy controls, persistent Transcribe Audio queue restoration, a dedicated multi-window History surface, waveform/rate audio playback controls, AI re-enhance, or batch history actions. The first-run setup dialog, onboarding reset, sidebar navigation, About / Open Source diagnostics actions, local Whisper catalog downloads/imported model selector/language selector/warmup, OpenAI-compatible cloud transcription path with custom/Groq/Deepgram presets, Deepgram live recorder preview, tray command surface, floating recorder Mini/Notch styles with live microphone level bars, non-activating Stop/Cancel controls, hover-dismissable Prompt/Power chooser panels, and gated live transcript preview rendering for real partial sources, Enhancement settings/default prompt/provider/custom-prompt/selected-text/clipboard-context path, Power Mode process/title/default/recorder-selection rule path, Transcribe Audio picker/queue/provider-history path, Dictionary add/edit/sort/import/export/quick-add path, Settings backup import/export, Settings privacy cleanup controls, recent-history metadata view, paste-last actions, retry-last action, active-recording cancel action, open-history focus action, selected-row history retry/playback/open, history search/delete/export, configurable global key+modifier shortcuts, and System Default/custom microphone selection are source-runnable and wired into the dictation pipeline. Installer packaging is part of the Windows fork scope and is planned as a follow-on Windows subsystem after this source-built MVP.

## Contributing

This project is **not accepting pull requests** at this time. You're welcome to fork and modify VoiceInk for your own use.

You can still contribute by:
- Reporting bugs via [issues](https://github.com/Beingpax/VoiceInk/issues)
- Suggesting features or enhancements
- Improving documentation via issues

For more details, see our [Contributing Guidelines](CONTRIBUTING.md). For build instructions, see our [Building Guide](BUILDING.md).

## License

This project is licensed under the GNU General Public License v3.0 - see the [LICENSE](LICENSE) file for details.

## Support

If you encounter any issues or have questions, please:
1. Check the existing issues in the GitHub repository
2. Create a new issue if your problem isn't already reported
3. Provide as much detail as possible about your environment and the problem

## Acknowledgments

### Core Technology
- [whisper.cpp](https://github.com/ggerganov/whisper.cpp) - High-performance inference of OpenAI's Whisper model
- [FluidAudio](https://github.com/FluidInference/FluidAudio) - Used for Parakeet model implementation

### Essential Dependencies
- [Sparkle](https://github.com/sparkle-project/Sparkle) - Keeping VoiceInk up to date
- [KeyboardShortcuts](https://github.com/sindresorhus/KeyboardShortcuts) - User-customizable keyboard shortcuts
- [LaunchAtLogin](https://github.com/sindresorhus/LaunchAtLogin) - Launch at login functionality
- [MediaRemoteAdapter](https://github.com/ejbills/mediaremote-adapter) - Media playback control during recording
- [Zip](https://github.com/marmelroy/Zip) - File compression and decompression utilities
- [SelectedTextKit](https://github.com/tisfeng/SelectedTextKit) - A modern macOS library for getting selected text
- [Swift Atomics](https://github.com/apple/swift-atomics) - Low-level atomic operations for thread-safe concurrent programming


---

Made with ❤️ by Pax
