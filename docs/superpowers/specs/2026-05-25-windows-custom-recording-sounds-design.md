# Windows Custom Recording Sounds Design

## Status

Approved by continuous execution directive.

## Goal

Port the macOS custom start/stop recording sound workflow to the Windows fork as a free/open-source, local-only Settings feature.

## Source Of Truth

Mac references:

- `VoiceInk/CustomSoundManager.swift`
- `VoiceInk/SoundPlaybackEngine.swift`
- `VoiceInk/Views/Settings/CustomSoundSettingsView.swift`

Windows references:

- Microsoft WinUI desktop file picker guidance: pickers must be initialized with the owner window handle before use in desktop apps.
- NAudio playback guidance: use `AudioFileReader` with an output device such as `WaveOutEvent`, then dispose both after playback completes.

## Product Behavior

Settings > Recording Feedback gains custom sound controls for the start and stop feedback sounds.

Each sound has:

- A selection between `System Default` and the imported custom sound when one exists.
- `Test`, `Choose`, and `Reset` actions.
- A label showing the imported file name or `No custom sound`.

Choosing a custom sound:

- Opens a Windows file picker for `.wav`, `.mp3`, `.aiff`, and `.aif`.
- Validates that the file exists, is decodable, has a finite positive duration, and is no longer than 3 seconds.
- Copies the file into the app-local sound folder under `%LocalAppData%\VoiceInk.Windows\Sounds`.
- Uses stable file names, `CustomStartSound.<ext>` and `CustomStopSound.<ext>`, replacing any previous custom file for that kind.
- Saves the matching setting immediately so recording feedback uses the custom sound on the next recording.

Testing a sound:

- Plays the currently selected start or stop sound without starting a recording.
- Falls back to the Windows system default sound if the selected custom file is missing or cannot be played.
- Does not upload, log, or export audio.

Resetting a sound:

- Deletes the app-owned custom sound file for that sound kind when present.
- Restores `System Default`.
- Saves settings immediately.

## Windows Adaptation

The macOS app ships built-in sounds named `sound1` through `sound7`; the current Windows fork does not have equivalent source-controlled audio assets. This slice therefore keeps the existing Windows system start/stop sounds as the built-in option and adds the custom-import workflow around them. Future visual/audio fidelity work can add bundled open-source sound assets without changing the Core settings model.

## Data Model

Add to `AppSettings`:

- `StartSoundMode`, default `systemDefault`
- `StopSoundMode`, default `systemDefault`
- `CustomStartSoundPath`, default empty
- `CustomStopSoundPath`, default empty

Supported sound modes:

- `systemDefault`
- `custom`

Invalid or blank modes normalize to `systemDefault`.

Backups include these settings in the General Settings category. API keys and secrets remain excluded as before.

## Architecture

Core remains UI-independent:

- `RecordingSoundKind` identifies start vs. stop sounds.
- `RecordingSoundModeSettings` normalizes persisted mode strings.
- `RecordingSoundPlaybackSettings` captures the selected mode and custom path for a playback event.
- `RecordingSoundSettings` projects start/stop playback settings from `AppSettings`.
- `CustomRecordingSoundImporter` owns validation, stable destination naming, replacement, and reset policy through injected file-system/probe interfaces.
- `IRecordingSoundFeedback` receives a playback settings snapshot so `RecordingFeedbackCoordinator` can preserve the start-of-recording configuration through completion.

Native Windows code handles platform details:

- `WindowsRecordingSoundFileProbe` uses NAudio to validate and read duration.
- `LocalRecordingSoundFileSystem` copies/deletes app-local files.
- `WindowsRecordingSoundFeedback` plays custom files through NAudio and falls back to `SystemSounds.Asterisk` / `SystemSounds.Exclamation`.

WinUI handles user interaction:

- The existing Settings `Recording Feedback` group gains start/stop sound controls.
- Existing file picker owner-window initialization is reused.
- Settings are saved after choose/reset actions and by the existing Apply Recording Feedback button.

## Error Handling

Validation failures return user-facing messages:

- Missing source file.
- Unsupported extension.
- Audio file cannot be read.
- Audio file has no valid duration.
- Audio file is longer than 3 seconds.

Playback failures are swallowed after fallback because sound feedback must never block recording.

Reset deletion failures are reported as a settings status message and do not change settings unless the reset succeeds.

## Tests

Core tests cover:

- Mode normalization.
- AppSettings equality/persistence round-trip for the new fields.
- Backup export/merge of the new General Settings fields.
- Recording feedback coordinator passing the start/stop sound settings captured at recording start.
- Import validation, stable destination naming, replacement, and reset behavior through fake file-system/probe adapters.

Native audio playback and WinUI pickers require manual smoke testing because they depend on OS audio codecs, speakers, and a UI file picker.

## Open-Source And Privacy

This feature is fully local. It does not add licensing gates, telemetry, accounts, paid channels, network calls, or commercial prompts. Imported sound files stay on disk under the app data directory and are not included in diagnostic logs.
