# Windows Cancel Processing Design

## Status

Approved by continuous execution directive.

## Goal

Let users cancel post-recording processing after capture has stopped, including transcription, enhancement, and insertion, without leaving VoiceInk stuck busy.

## Source Of Truth

The macOS app exposes cancel behavior around active recording and processing states. The Windows fork already cancels while recording; this slice extends that intent to the post-capture pipeline.

Reference behavior:

- Cancel during recording stops capture and saves a canceled history row.
- Cancel during processing should stop pending cooperative work, restore UI to idle, avoid text insertion when insertion has not happened, avoid completed metrics, and preserve the captured audio as a canceled history row where possible.

## Architecture

Core:

- `DictationController.StopAsync` already passes a cancellation token through transcription, enhancement, insertion, history, and metrics operations.
- Add best-effort canceled-history persistence when cancellation happens after audio capture has been stopped and before completion is saved.
- Use a non-canceled token for the best-effort canceled-history save so the user's cancel request does not prevent the local cancellation record from being written.
- Keep cancellation cooperative. Do not terminate threads or abandon unmanaged resources.

App:

- Add a stop-operation `CancellationTokenSource` linked to the window lifetime.
- Pass that linked token to `controller.StopAsync`.
- Allow the existing Cancel button/shortcut to cancel this token while the controller is `Transcribing` or `Inserting`.
- Keep normal recording cancellation unchanged.

## User Experience

- During transcription or insertion, `Cancel Recording` becomes a processing cancel action.
- The status changes to `Processing canceled` when cooperative cancellation completes.
- If cancellation happens before text insertion, no text is inserted.
- A canceled History row is saved with the captured audio file path when the history store is available.

## Error Handling

- If canceled-history save fails, keep cancellation successful and surface a warning where possible.
- If shutdown cancels the window lifetime, keep existing close behavior and do not block shutdown.
- If an adapter ignores cancellation, VoiceInk can only finish when that adapter returns; this slice passes the token consistently but does not forcefully kill external work.

## Tests

- Add Core tests that cancel during transcription after capture stop and assert:
  - `OperationCanceledException` propagates to the caller.
  - Controller returns to `Idle`.
  - Capture has been released.
  - No text is inserted.
  - A canceled history row is saved with the captured audio path and metadata.
  - No completed session metric is recorded.

Manual UI smoke remains required for pressing the Cancel button during a real long-running provider call.
