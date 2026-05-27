# Windows Recorder Processing Footer Guidance

## Goal

Make the floating recorder's post-recording processing states clearer by replacing the static footer hint with presenter-driven guidance.

## Source Of Truth

The macOS recorder communicates record, stop, processing, and insertion states in the compact recorder surface. The Windows recorder already shows title/detail changes; this slice adds a second line of lightweight footer guidance for transcribing and inserting states.

## Requirements

- Floating recorder view state must carry a footer hint.
- Recording state must explain Stop and Cancel behavior.
- Transcribing state must explain that transcription is finishing before insertion.
- Inserting state must explain that final text is being pasted into the active app.
- The WinUI floating recorder must render the presenter-provided footer hint.

## Non-Goals

- This slice does not change dictation pipeline timing.
- This slice does not add determinate transcription progress.
