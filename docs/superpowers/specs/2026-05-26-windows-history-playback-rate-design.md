# Windows History Playback Rate Design

## Context

The Windows History surfaces can play saved audio, but they do not expose playback speed controls. The macOS History player has richer playback affordances, and Windows can incrementally close that gap using the native `MediaPlayerElement.MediaPlayer.PlaybackSession.PlaybackRate` API.

Online grounding: Microsoft Learn documents `MediaPlaybackSession.PlaybackRate` as the supported way to control playback speed for `MediaPlayerElement`.

## Goals

- Add History audio playback rate choices.
- Support common dictation review speeds: 0.75x, 1.0x, 1.25x, 1.5x, and 2.0x.
- Apply the selected rate to both inline History playback and the dedicated History window player.
- Reset newly loaded audio to 1.0x by default.
- Keep the rate logic testable in Core.

## Non-goals

- Do not add waveform rendering in this slice.
- Do not add custom audio decoding libraries.
- Do not add batch audio playback.
- Do not add telemetry or commercial features.

## Architecture

Add `HistoryPlaybackRatePresenter` in Core to define supported rates, labels, and clamped selection behavior. Add `ComboBox` controls next to each `MediaPlayerElement` and apply selected rates through the playback session when an audio source is loaded or the selection changes.

## Verification

- Core tests cover supported choices, default selection, and clamping invalid rates to 1.0x.
- App project builds after XAML wiring.
- Full solution tests/build pass before commit.
