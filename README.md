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

On first launch, VoiceInk for Windows opens a setup dialog for the local whisper model path, microphone settings/input, primary shortcut, and a short try-it flow. After setup, enter or confirm a local whisper model path, click `Start Recording`, speak, then click `Stop And Insert`. You can also press `Ctrl+Alt+Space` to toggle recording by default when that global hotkey is available. The Windows shell also exposes configurable key+modifier shortcuts for primary and secondary recording toggle, paste last, paste last enhanced, retry last transcription, cancel recording, open history, and quick add to dictionary. A Windows tray icon stays available while the app is running; its menu can show or hide the shell, start or stop recording, open Quick Add to Dictionary, focus History, or quit VoiceInk. Retry Last reuses the latest completed history row with a saved audio file, applies the current local model, dictionary, and cleanup settings, saves the retried row, and copies the retried text to the clipboard. Cancel Recording stops the active recorder, keeps the captured audio file, and saves a canceled history row. Open History restores the main window and focuses the inline History area. Quick Add opens a small Vocabulary/Word Replacement dialog that mirrors the macOS quick-add panel intent.

### Current Windows MVP Scope

- WinUI 3 shell
- Windows tray icon with show/hide, recording toggle, Quick Add, History, and Quit commands
- First-run setup dialog for local model path, microphone settings/input, primary shortcut, and basic usage
- Local whisper.cpp transcription through Whisper.net
- Microphone capture
- Clipboard-based text insertion into the active app
- JSON settings and SQLite transcription history
- Configurable global key+modifier shortcuts for primary and secondary recording toggle, paste last, paste last enhanced, retry last transcription, cancel recording, open history, and quick add to dictionary
- Core dictionary models for vocabulary words and word replacements
- Persistent JSON-backed Dictionary storage for vocabulary words and word replacements
- macOS-style transcription cleanup for hallucination markers, filler words, punctuation cleanup, lowercase output, trailing spaces, and word replacements
- Vocabulary prompt biasing for local Whisper transcription
- Expanded history metadata for original text, final text, status, language, model path, prompt name, enhancement timing, and audio file path
- Shell controls for filler-word removal, punctuation cleanup, lowercase output, and trailing spaces
- Shell controls for adding/removing/sorting vocabulary words and adding/editing/enabling/disabling/removing/sorting word replacements
- Quick-add Dictionary dialog for vocabulary words and word replacements
- Dictionary JSON import/export for vocabulary words and word replacements
- Shell audio input refresh and System Default/custom microphone selection
- Shell recent-history list/detail view for original, final, enhanced, status, timing, model, prompt, and error metadata
- Picker-based CSV history export
- Paste-last final and enhanced-preferred history actions in the shell
- History search, selected-row audio playback/open, selected-row retry, retry-last-to-clipboard, active-recording cancel history, and confirmed single-item delete

The current shell does not yet expose the full macOS-style Dictionary navigation page, tray submenus for model/provider/enhancement/language/audio/context settings, model catalog/download/import cards, push-to-talk/hybrid shortcut modes, shortcut key-up handling, prioritized audio input failover, canceling in-flight transcription/enhancement, a dedicated multi-window History surface, waveform/rate audio playback controls, AI re-enhance, or batch history actions. The first-run setup dialog, tray command surface, Dictionary add/edit/sort/import/export/quick-add path, recent-history metadata view, paste-last actions, retry-last action, active-recording cancel action, open-history focus action, selected-row history retry/playback/open, history search/delete/export, configurable global key+modifier shortcuts, and System Default/custom microphone selection are source-runnable and wired into the dictation pipeline. Cloud transcription providers, AI text enhancement, and installer packaging are part of the Windows fork scope and are planned as follow-on Windows subsystems after this source-built MVP.

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
