# Windows Transcribe Audio Copy Save Design

## Goal

Let users copy or save the text for a completed Transcribe Audio queue item, matching the original app's completed-row copy/save affordances without changing the queue processing pipeline.

## Grounding

- The macOS `AudioFileRow` shows copy and save controls only for completed rows with a transcription.
- The macOS copy/save action text uses the currently visible expanded tab, or enhanced text when collapsed; the Windows queue currently exposes a selected-item detail transcript, so the first faithful Windows adaptation uses selected-row actions.
- Microsoft WinUI clipboard docs use `DataPackage.SetText` and `Clipboard.SetContent`; the existing Windows app already routes user-copy operations through `ITextInjectionService.CopyAsync`.
- Microsoft Windows App SDK docs support `FileSavePicker` for WinUI save flows, and the existing Windows app already uses it for History and Metrics CSV exports.

## Behavior

- Add `Copy` and `Save` controls near the selected Transcribe Audio transcript detail.
- Enable both controls only when the selected queue item is completed and has transcript text.
- Copy writes the action text to the clipboard through the existing text injection abstraction and reports status.
- Save opens a Windows save picker with `Text file` and `Markdown file` choices.
- The suggested file name is derived from the first useful words of the action text, sanitized for Windows file names, and falls back to `transcription`.
- TXT saves plain text. MD saves a small local Markdown document with `# Transcription`, a date line, and the transcript text.
- Canceled picker operations report a neutral canceled status. Write/copy failures report the exception message without logging or exposing secrets.

## Out Of Scope

- Inline row icon-template polish.
- Independent original/enhanced tabs for queue details.
- Persistent queue restoration.

## Verification

- Add Core tests for action text selection, generated file names, and Markdown formatting.
- Run focused Core tests, full solution tests, Debug x64 build, and `git diff --check`.
