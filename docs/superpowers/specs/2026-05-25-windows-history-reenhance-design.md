# Windows History Re-enhance Design

## Context

The macOS VoiceInk history surface includes a re-enhance-only action so users can rerun AI enhancement with the current prompt, dictionary, provider, and context settings without retranscribing the saved audio. The Windows history surface already supports search, CSV export, paste last, retry from audio, audio playback, and deletion, but retry currently performs a full transcription retry and requires an audio file.

Online grounding: upstream VoiceInk release notes mention history re-enhance and retry/retranscribe actions, so this is a real parity gap rather than a newly invented Windows workflow.

## Goals

- Add a Windows History action named "Re-enhance Selected".
- Reuse the selected history item's saved original transcription text.
- Apply the current enhancement settings, prompt templates, and dictionary vocabulary.
- Save the result as a new completed history item so the original row remains available for comparison.
- Preserve source transcription metadata where appropriate: provider, language, model, audio path, audio duration, transcription duration, and Power Mode labels.
- Store enhancement metadata from the new enhancement attempt: enhanced text, prompt name, provider/model, duration, and rendered request messages.
- Gracefully report disabled/missing provider/empty source failures without creating a new history item.

## Non-goals

- Do not update existing rows in place.
- Do not retranscribe audio.
- Do not require an audio file.
- Do not add commercial gates, accounts, telemetry, or paid-provider assumptions.
- Do not build the dedicated standalone History window in this slice.

## Architecture

Add a Core service, `HistoryReenhancementService`, near `HistoryRetryService`. It depends on `IHistoryStore`, `ISettingsStore`, optional `IDictionaryStore`, and `TextEnhancementPipeline`. The app shell wires a new History button to the service, refreshes the list, and selects the newly saved result.

The service treats re-enhancement as history derivation: a new row is created with a new id and timestamp. If the selected row is not completed, has no usable original/final text, enhancement is disabled, or provider configuration is missing, it returns a failure result and leaves history unchanged.

## Verification

- Core tests cover successful re-enhancement, provider-disabled/missing failure, non-completed source failure, and original text selection.
- UI wiring builds under WinUI.
- Focused Core History tests pass.
- Full solution tests pass.
- Debug x64 build passes.
