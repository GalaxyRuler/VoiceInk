# Windows Stale Model Cleanup Design

## Goal

Let users remove imported local Whisper model references that no longer point to usable `.bin` files.

## Behavior

- Add a Core cleanup helper that removes imported models whose path health is not usable.
- Keep the helper UI-independent with injected file existence and length delegates.
- Add an AI Models action to remove unavailable imported models.
- If the removed model was selected as the default model, clear the selected path so the app does not keep trying to warm it up.
- Do not delete, move, or modify any user model files.

## Verification

- Add Core tests for removing stale imported models and preserving usable imported models.
- Run focused model tests, full solution tests, Debug x64 build, and `git diff --check`.
