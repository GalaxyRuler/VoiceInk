# Windows Model Language Selection Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add model-aware transcription language selection to the Windows AI Models page.

**Architecture:** Keep language capabilities and fallback rules in Core. The WinUI shell binds a `ComboBox` to Core language choices, persists `AppSettings.Language`, and reconciles the selected language whenever the selected model path changes.

**Tech Stack:** .NET 10, WinUI 3 `ComboBox`, existing JSON settings, Whisper.net language option wiring, xUnit.

---

## Context

macOS references:

- `VoiceInk/Views/AI Models/LanguageSelectionView.swift` shows `Transcription Language`, forces English for English-only models, and offers a picker for multilingual models.
- `VoiceInk/Models/LanguageDictionary.swift` defines Whisper language codes and names; `auto` means auto-detect.
- `VoiceInk/Models/TranscriptionModelRegistry.swift` marks `.en` Whisper models as English-only and other GGML models as multilingual.

Online grounding:

- Whisper/whisper.cpp uses language codes and `auto` for auto-detect.
- Whisper.net already maps `auto` to `WithLanguageDetection()` and language codes to `WithLanguage(...)`.
- Microsoft WinUI `ComboBox` supports `ItemsSource`, `DisplayMemberPath`, and `SelectionChanged` for dynamic selections.

## Files

- Create: `VoiceInk.Windows/src/VoiceInk.Windows.Core/Models/TranscriptionLanguageChoice.cs`
- Create: `VoiceInk.Windows/src/VoiceInk.Windows.Core/Models/WhisperLanguageCatalog.cs`
- Modify: `VoiceInk.Windows/tests/VoiceInk.Windows.Core.Tests/Models/LocalWhisperModelServiceTests.cs`
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.App/MainWindow.xaml`
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.App/MainWindow.xaml.cs`
- Modify: `README.md`
- Modify: `docs/superpowers/specs/2026-05-24-windows-open-source-parity-design.md`
- Modify: `docs/superpowers/project-completion.md`

### Task 1: Core Language Catalog

- [ ] Write failing Core tests for:
  - English-only catalog model `ggml-base.en` returns only `en`.
  - Multilingual catalog model `ggml-base` returns `auto` first and includes `en`, `fr`, `de`, `ja`, and `zh`.
  - Imported/unknown `.bin` models are treated as multilingual.
  - Fallback keeps a compatible language, maps incompatible English-only choices to `en`, and maps blank multilingual choices to `auto`.
- [ ] Run the filtered Core model test and confirm it fails because the language catalog does not exist.
- [ ] Add `TranscriptionLanguageChoice` and `WhisperLanguageCatalog`.
- [ ] Run the filtered Core model test and confirm it passes.

### Task 2: WinUI Language Selector

- [ ] Add `LanguageComboBox` and `LanguageDescriptionTextBlock` under AI Models in `MainWindow.xaml`.
- [ ] Add `languageChoices`, selection suppression, and refresh helpers in `MainWindow.xaml.cs`.
- [ ] Load `settings.Language` into the selector in `ApplySettingsToUiAsync`.
- [ ] Save `SelectedLanguageCode()` into `GatherSettings`.
- [ ] Reconcile language after model path changes, imported model selection, catalog set-default, and catalog download.
- [ ] Disable language selection only while operation gating disables model controls.

### Task 3: Review, Verification, Docs, Commit

- [ ] Update README/spec/completion tracker to show model language selection is complete.
- [ ] Run focused Core tests, full solution tests, Debug x64 build, and `git diff --check`.
- [ ] Request review; fix all Critical/Important findings.
- [ ] Commit as `feat(windows): add model language selection`.
