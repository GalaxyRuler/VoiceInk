# Windows Model Repair Validation Guidance Plan

Date: 2026-05-27

## Goal

Clarify that local model repair validates a user-selected whisper.cpp `.bin` file and does not scan folders automatically.

## Steps

1. Add a failing model health presenter assertion for a `Repair Picker` guidance row.
2. Add the row to `LocalWhisperModelHealthPresenter` repair guidance.
3. Run the focused model health presenter test.
4. Update the project completion tracker and slice documentation.
5. Run the full verification gate:
   - `dotnet test VoiceInk.Windows\VoiceInk.Windows.sln`
   - `dotnet build VoiceInk.Windows\VoiceInk.Windows.sln -c Debug -p:Platform=x64`
   - `git diff --check`
6. Commit the completed slice.

## Review Notes

- Keep repair guidance explicit and user-owned.
- Do not imply model files are copied, deleted, or scanned automatically.
- Keep ready-state guidance unchanged.
