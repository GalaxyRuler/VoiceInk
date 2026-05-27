# Windows Transcribe Audio Format Parity

## Goal

Stop filtering out macOS-supported Transcribe Audio files before Windows gets a chance to decode them.

## Source Of Truth

- `VoiceInk/Services/SupportedMedia.swift` accepts `wav`, `mp3`, `m4a`, `aiff`, `mp4`, `mov`, `aac`, `flac`, `caf`, `amr`, `ogg`, `oga`, `opus`, and `3gp`.

## Online Grounding

- Microsoft Media Foundation documents built-in file sources for common Windows media containers such as WAV, MP3, AAC/ADTS, MPEG-4/MOV/3GP, AVI, ASF/WMA/WMV, and notes that codec availability can vary by installed components: https://learn.microsoft.com/windows/win32/medfound/supported-media-formats-in-media-foundation
- Microsoft Windows Media Player support documentation lists AIFF extensions such as `.aif`, `.aifc`, and `.aiff`, while also noting additional formats may depend on installed codecs: https://support.microsoft.com/help/316992

## Windows Design

- Expand the queue/picker allow-list to include macOS-supported `.aiff`, `.caf`, `.amr`, `.ogg`, `.oga`, and `.opus`.
- Also include `.aif` and `.aifc` as Windows AIFF aliases for the same format family.
- Keep decode responsibility in `MediaFoundationAudioFileImportService`; if Windows cannot decode a selected file, the existing failed queue item/error flow remains the graceful fallback.

## Non-Goals

- No bundled codec installation.
- No FFmpeg dependency.
- No claim that every extension decodes on every Windows machine.
