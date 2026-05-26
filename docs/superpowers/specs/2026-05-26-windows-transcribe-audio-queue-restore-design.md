# Windows Transcribe Audio Queue Restore Design

## Goal

Restore unfinished Transcribe Audio queue work after an app restart so users do not lose selected files that were waiting, failed, or interrupted while processing.

## Grounding

- Microsoft .NET docs support `System.Text.Json` for local JSON serialization and `File.ReadAllTextAsync`/async file APIs for local persistence.
- The Windows app already persists settings and dictionary data as local JSON under `%LocalAppData%\VoiceInk.Windows`.
- The current Transcribe Audio queue uses Core validation for supported extensions, missing files, and duplicate active paths.

## Behavior

- Persist a small queue snapshot under app data whenever the queue changes.
- Restore pending and failed items at startup.
- Reset any persisted processing item to pending, because app shutdown interrupts in-flight transcription work.
- Do not restore completed items; completed Transcribe Audio results already live in History.
- Skip missing, unsupported, duplicate, or malformed paths during restore.
- Keep failed items failed with their saved error message so users can retry them.
- Clear the snapshot when the queue is cleared or when no restorable items remain.
- If reading the snapshot fails, start with an empty queue and show a nonfatal status message.
- If saving the snapshot fails, keep the in-memory queue and show a nonfatal status message.

## Out Of Scope

- Persisting completed queue presentation rows.
- Persisting live processing progress.
- Cross-machine file path migration.

## Verification

- Add Core tests for snapshot creation and restoration.
- Add Infrastructure tests for JSON snapshot load/save/delete behavior.
- Run focused tests, full solution tests, Debug x64 build, and `git diff --check`.
